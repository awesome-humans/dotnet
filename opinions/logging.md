---
targets: [net11.0, csharp-15]
last-reviewed: 2026-11-14
sources: [ms-learn, nicholas-blumhardt]
---

# Logging & tracing

Use one structured pipeline, exported over OTLP. Treat logs and traces as one telemetry stream.

> **Conflict of interest:** The Serilog guidance cites Nicholas Blumhardt, who wrote Serilog and SerilogTracing and sells Seq through Datalust. The recommendations name the open-source libraries and the BCL `Activity` APIs, not a product, and the fallback example deliberately targets an OTLP endpoint, not Seq.

## Opinions

- **Log through `ILogger<T>`, and declare every event as a source-generated `[LoggerMessage]` partial method.** `logger.LogInformation($"...")` boxes, allocates, and formats the string even when the level is off. The generator skips that work, and keeps the message template, so structured sinks get named properties. Unlike `LoggerMessage.Define`, it has no six-parameter limit, allows a dynamic level, and reports duplicate event IDs at compile time. ([Microsoft Learn: Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator))

  ```csharp
  public partial class OrderService(ILogger<OrderService> logger)
  {
      [LoggerMessage(EventId = 100, Level = LogLevel.Information,
          Message = "Order {OrderId} accepted for {CustomerId}")]
      public partial void OrderAccepted(Guid orderId, string customerId);
  }
  ```

  Never interpolate a string in a log call. `LogInformation($"Order {orderId} accepted")` also loses the structure: the sink gets one string, with no `OrderId` property.

- **Create spans with `ActivitySource.StartActivity`, not log lines that record durations.** `Activity` is the BCL span type, and ASP.NET Core and `System.Net.Http` already create it and propagate W3C `traceparent` context. A span records its timing and its parent, which a logged stopwatch reading can't. ([Microsoft Learn: Distributed tracing concepts](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts)) For work that crosses a process or time boundary, propagate the context explicitly, as [aspnet-core.md](aspnet-core.md#observability-propagate-trace-context-across-async-boundaries) describes.

- **Control tracing cost in the `ActivityListener.Sample` callback, not with a sink filter.** Sampling runs when the activity is created. The listener can decline it, create an ID-only activity that still propagates context, or record it fully. Dropping spans at the exporter instead pays for creating and recording each one first. ([Microsoft Learn: Distributed tracing concepts](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts), [Blumhardt: .NET's `ActivityListener` sampling API](https://nblumhardt.com/2024/10/activity-listener-sampling/))

- **Export over OTLP to a collector, and keep vendor SDKs out of application code.** The default pipeline is `Microsoft.Extensions.Logging` with the OpenTelemetry exporter. With one wire format, the backend is a deployment choice, not a code dependency.
  - Serilog isn't the default because it adds a second configuration surface beside `ILogger`'s. If you need one of its sinks, `SerilogTracing` sends `Activity` spans through the same pipeline as the logs. ([Blumhardt: SerilogTracing](https://nblumhardt.com/2024/01/serilog-tracing/))

- **Give the log pipeline a durable local fallback, and expect it to lag.** A network collector goes down eventually, and an outage is when you need the logs most. Chain a rolling file behind the network sink. Network sinks retry with backoff before they report failure, so the file usually starts receiving events about ten minutes into an outage. To change that, set `BatchingOptions.RetryTimeLimit`. ([Blumhardt: Serilog fallback sinks](https://nblumhardt.com/2024/10/fallback-logging/), [Blumhardt: Visualizing the Serilog 4.1 batch retry algorithm](https://nblumhardt.com/2024/10/retry-time-limit/))

  ```csharp
  Log.Logger = new LoggerConfiguration()
      .WriteTo.FallbackChain(
          wt => wt.OpenTelemetry(endpoint: "https://otlp.example/v1/logs"),
          wt => wt.File(
              "logs/app-.txt",
              rollingInterval: RollingInterval.Day,
              formatProvider: CultureInfo.InvariantCulture))
      .CreateLogger();
  ```

  **With the default pipeline, run a collector on localhost, and let it own the durable queue.** `AddOtlpExporter` doesn't retry a failed export by default. Its in-memory and disk retry are experimental, behind the `OTEL_DOTNET_EXPERIMENTAL_OTLP_RETRY` environment variable, so a failed export or a killed process loses what was in flight. An export to the loopback address succeeds while the backend is down. Configure retry and disk buffering in the collector, not the app. ([OpenTelemetry .NET: OTLP exporter](https://github.com/open-telemetry/opentelemetry-dotnet/blob/main/src/OpenTelemetry.Exporter.OpenTelemetryProtocol/README.md), [Microsoft Learn: .NET observability with OpenTelemetry](https://learn.microsoft.com/dotnet/core/diagnostics/observability-with-otel))

  If you can't run a local collector, use Serilog with the chain above. Skip both only when the logs are for diagnostics, not an audit trail, and record that decision.

## Levels

- **Use `Information` for events an operator wants in production, and `Debug` for developers.** Set the default minimum level to `Information`. Raise noisy framework categories, such as `Microsoft.AspNetCore`, to `Warning` in configuration instead of deleting the calls. ([Microsoft Learn: Logging in .NET and ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0))
- **Pass an exception as the `exception` argument, never in the message.** The generator treats the first `Exception` parameter specially, and structured sinks record its type, message, and stack trace as separate fields. ([Microsoft Learn: Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator))
- **Redact classified data in the pipeline, not at the call site.** `Microsoft.Extensions.Compliance.Redaction` picks redactors by data classification. It takes two calls: `AddRedaction` on the service collection, to "Register and configure redactors for each classification in your DI container", and `EnableRedaction()` on the logging builder. After both, a parameter annotated with one of your classifications is masked wherever it's logged. A `?? "***"` at the call site leaks the value as soon as someone logs it elsewhere. ([Microsoft Learn: Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator))
