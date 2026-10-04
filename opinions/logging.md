---
targets: [net10.0, csharp-14]
last-reviewed: 2026-08-18
last-used: 2026-09-25
sources: [ms-learn, nicholas-blumhardt]
---

# Logging & tracing

Use one structured pipeline, exported over OTLP. Treat logs and traces as one telemetry stream, not two systems.

> **Conflict of interest:** The Serilog guidance below cites Nicholas Blumhardt, who wrote Serilog and SerilogTracing and sells Seq through Datalust. The recommendations point at the open-source libraries and the BCL `Activity` APIs, not at any product. The fallback example uses an OTLP endpoint, not Seq, on purpose.

## Opinions

- **Log through `ILogger` or `ILogger<T>`, and declare every event as a source-generated `[LoggerMessage]` partial method.** `logger.LogInformation($"...")` boxes, allocates, and formats the string even when the level is disabled. The generator removes all that work. It also keeps the message template, so structured sinks get named properties instead of a flat string. It's better than hand-written `LoggerMessage.Define` calls too: there's no six-parameter limit, the level can be dynamic, and duplicate event IDs are reported at compile time. ([Microsoft Learn: Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator))

  ```csharp
  public partial class OrderService(ILogger<OrderService> logger)
  {
      [LoggerMessage(EventId = 100, Level = LogLevel.Information,
          Message = "Order {OrderId} accepted for {CustomerId}")]
      public partial void OrderAccepted(Guid orderId, string customerId);
  }
  ```

  Never use string interpolation in a log call. `LogInformation($"Order {orderId} accepted")` throws away the structure, and formats the string whether or not anything is listening.

- **Create spans with `ActivitySource.StartActivity`, not with log lines that record a duration.** `Activity` is the BCL span type. ASP.NET Core and `System.Net.Http` already create and propagate W3C `traceparent` context without any code from you. One instrumented tree gives you both the timing and the parent-child structure, which stopwatch logging can't. ([Microsoft Learn: Distributed tracing concepts](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts)) For work that crosses a process or time boundary, propagate the context explicitly, as [aspnet-core.md](aspnet-core.md#observability-propagate-trace-context-across-async-boundaries) describes.

- **Control tracing cost in the `ActivityListener.Sample` callback, not by filtering at the sink.** Sampling happens when the activity is created. The listener can decline the activity, create an ID-only activity that still propagates trace context, or populate it fully. A recorded activity costs about a microsecond, and one rejected at sampling costs under 100 ns. Dropping spans at the exporter pays the full construction cost first, and breaks trace trees, because children can't know that their parent was discarded. ([Microsoft Learn: Distributed tracing concepts](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts), [Blumhardt: .NET's `ActivityListener` sampling API](https://nblumhardt.com/2024/10/activity-listener-sampling/))

- **Export over OTLP to a collector, and keep vendor SDKs out of application code.** The default pipeline is `Microsoft.Extensions.Logging` with the OpenTelemetry exporter. It has one wire format, and the backend becomes a deployment choice instead of a code dependency.
  - Choose Serilog when you want message-template logging and its ecosystem of sinks. `SerilogTracing` then sends `Activity` spans through the same Serilog pipeline, so traces and logs share enrichers and sinks.
  - Serilog isn't the default only because it adds a second configuration surface next to the one that `ILogger` already provides. ([Blumhardt: SerilogTracing](https://nblumhardt.com/2024/01/serilog-tracing/))

- **Give the log pipeline a durable local fallback, and expect it to lag.** A network collector will be unavailable at some point, and that's exactly when the logs matter. Chain a rolling file behind the network destination, so nothing is lost. Plan for the delay: network sinks retry with backoff before they report failure, so the fallback usually starts receiving events about ten minutes into an outage. To change that window, set `BatchingOptions.RetryTimeLimit`. ([Blumhardt: Serilog fallback sinks](https://nblumhardt.com/2024/10/fallback-logging/), [Blumhardt: Visualizing the Serilog 4.1 batch retry algorithm](https://nblumhardt.com/2024/10/retry-time-limit/))

  ```csharp
  Log.Logger = new LoggerConfiguration()
      .WriteTo.FallbackChain(
          wt => wt.OpenTelemetry(endpoint: "https://otlp.example/v1/logs"),
          wt => wt.File("logs/app-.txt", rollingInterval: RollingInterval.Day))
      .CreateLogger();
  ```

  **Run a collector on localhost, and let it own the durable queue. The default pipeline has no equivalent of the chain above.** `AddOtlpExporter` batches in memory and has no on-disk queue to replay, so a failed export or a killed process loses whatever was in flight. When the app exports to a collector on the loopback address, the export succeeds even while the backend is down. Retry and disk buffering then belong to the one component built for them, and you configure them there, not in the app.

  The alternative is to use Serilog for the logging pipeline, with the chain above. Doing neither is acceptable for a service whose logs are for diagnostics, not an audit trail, as long as someone made that decision on purpose. ([Microsoft Learn: .NET observability with OpenTelemetry](https://learn.microsoft.com/dotnet/core/diagnostics/observability-with-otel))

## Levels

- **Use `Information` for events an operator wants in production, and `Debug` for developers.** Set the default minimum level to `Information`. Raise noisy framework categories, such as `Microsoft.AspNetCore`, to `Warning` in configuration, instead of deleting the log calls. ([Microsoft Learn: Logging in .NET and ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0))
- **Pass an exception as the `exception` argument, never in the message.** The source generator treats the first `Exception` parameter specially, and structured sinks record the type, message, and stack trace as separate fields. ([Microsoft Learn: Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator))
- **Redact classified data in the pipeline, not at the call site.** `Microsoft.Extensions.Compliance.Redaction` selects redactors by data classification. Once you call `EnableRedaction()` on the logging builder, a parameter annotated with one of your classifications is masked everywhere it's logged. A `?? "***"` at the call site is one refactor away from a leak. ([Microsoft Learn: Compile-time logging source generation](https://learn.microsoft.com/dotnet/core/extensions/logger-message-generator))
