---
targets: [net11.0, csharp-15]
last-reviewed: 2026-11-14
sources:
  [
    andrew-lock,
    aspnet-blog,
    code-with-mukesh,
    dotnet-blog,
    meziantou,
    milan-jovanovic,
    ms-learn,
  ]
---

# ASP.NET Core

Target ASP.NET Core 11, and use minimal APIs for new HTTP APIs.

- For authentication, authorization, CSRF, CORS, security headers, and the rest of security in the HTTP pipeline, see [aspnet-core-security.md](aspnet-core-security.md).
- For secrets, deserialization, cryptography, and servicing cadence, see [security.md](security.md).

## Opinions

### Hosting and deployment

**Run Kestrel directly, in a container image that `dotnet publish /t:PublishContainer` builds without a Dockerfile.** Kestrel can face the internet on its own, without a reverse proxy. If you do run behind a proxy or ingress for port sharing, TLS termination, or load balancing, enable host filtering and forwarded headers. Without forwarded headers, the app sees the proxy's scheme and IP address instead of the client's.

Container publish builds an OCI image from MSBuild properties such as `ContainerRepository` and `ContainerRegistry`. The base image stays patched with the SDK, and you don't need a Docker daemon to produce a tarball for a scanner. Write a Dockerfile only when you need custom image layers, because a hand-written one drifts from the SDK's. ([Microsoft Learn: When to use a reverse proxy with Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/when-to-use-a-reverse-proxy), [Microsoft Learn: Containerize an app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish))

An image keeps the runtime it was built with, so rebuild it after every Patch Tuesday, as [security.md: Servicing](security.md#servicing-take-the-monthly-patch) says.

**Never set `ContainerUser` to `root`.** Container publish defaults it to the rootless `app` user on Linux, "If you're targeting .NET 8 or higher and using the Microsoft runtime images". A Dockerfile gets no such default: except on the chiseled images, "you will still run as root by default". As root, "if an attacker manages to compromise your container, they'll be able to do pretty much anything in the container". ([Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration), [Lock: Updates to Docker images in .NET 8](https://andrewlock.net/exploring-the-dotnet-8-preview-updates-to-docker-images-in-dotnet-8/))

**Redirect to HTTPS and send HSTS wherever TLS terminates.** If the proxy redirects to HTTPS, "there's no need to use HTTPS redirection middleware", so drop `UseHttpsRedirection()`, and leave the HSTS header to the proxy too. Don't call `UseHsts()` in development, because "the HSTS settings are highly cacheable by browsers". ([Microsoft Learn: Enforce HTTPS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl))

### Request timeouts

**Give every endpoint a timeout, and pass the request's `CancellationToken` to every async call the handler makes.** ASP.NET Core sets no request timeout by default. Without one, a stalled query keeps running after the client has stopped waiting, and whatever sits in front of the app, a proxy or the client itself, decides when to give up and what the client sees.

- `AddRequestTimeouts` only registers the services. Set the limit with a named policy, or with `WithRequestTimeout` on the endpoint.
- On timeout, the middleware cancels `HttpContext.RequestAborted` but doesn't stop the handler. A handler that ignores the token runs to completion and returns 200. A 504 only means the handler threw on cancellation, not that the database stopped the query.
- Size each policy to its endpoints instead of setting one global value. A 3-second API read and a 30-second report export, as in the sample below, need separate policies.
- Exclude streaming responses with `DisableRequestTimeout()`. After a response starts, the middleware can't replace it with a 504.

([Microsoft Learn: Request timeouts middleware](https://learn.microsoft.com/aspnet/core/performance/timeouts), [Jovanović: Your ASP.NET Core endpoints don't have a timeout](https://www.milanjovanovic.tech/blog/your-aspnetcore-endpoints-dont-have-a-timeout))

```csharp
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("api-read", TimeSpan.FromSeconds(3));
    options.AddPolicy("report-export", TimeSpan.FromSeconds(30));
});

WebApplication app = builder.Build();
app.UseRequestTimeouts();

// The token reaches EF Core, so the timeout stops the query.
app.MapGet("/orders/{id:guid}", async (Guid id, AppDbContext db, CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id, cancellationToken))
    .WithRequestTimeout("api-read");

app.MapGet("/events", StreamEvents).DisableRequestTimeout();
```

Timeouts don't fire while a debugger is attached, so test them without one. ([dotnet/aspnetcore: RequestTimeoutsMiddleware.cs](https://github.com/dotnet/aspnetcore/blob/6e62760191a731f63d79b49fad7eda0aaf855cf3/src/Http/Http/src/Timeouts/RequestTimeoutsMiddleware.cs#L31))

### Rate limiting

**Put named `Microsoft.AspNetCore.RateLimiting` policies on public endpoints, and reject with 429.** The default rejection status is 503, so set `RejectionStatusCode`. ([Microsoft Learn: RateLimiterOptions.RejectionStatusCode](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.ratelimiting.ratelimiteroptions.rejectionstatuscode))

- Partition by a stable identity, such as the authenticated user or the API key tier. Never partition by a value the client controls, such as a spoofable IP header: "If you create partitions by using user input, your app becomes vulnerable to Denial of Service (DoS) attacks". ([Microsoft Learn: Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit#testing-endpoints-with-rate-limiting))
- To partition anonymous traffic, use `HttpContext.Connection.RemoteIpAddress`, never the raw `X-Forwarded-For` header. The forwarded headers middleware from [Hosting and deployment](#hosting-and-deployment) sets that property from the header, and accepts it only from a proxy in `KnownProxies` or a known network, because otherwise "IP spoofing attacks are possible". ([Microsoft Learn: Configure ASP.NET Core to work with proxy servers and load balancers](https://learn.microsoft.com/aspnet/core/host-and-deploy/proxy-load-balancer))
- Bound the number of partitions, because "Each distinct key creates and caches its own limiter". ([Microsoft Learn: Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit#chain-limiters-in-a-named-policy), [Mukesh: Rate Limiting in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/rate-limiting-aspnet-core/))
- Use the concurrency limiter on expensive endpoints, such as uploads and report generation, to cap how many run at once. It "doesn't cap the number of requests in a time period", so it doesn't replace a time-based limiter on a public endpoint. ([Microsoft Learn: Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit#rate-limiter-algorithms), [Mukesh: Rate Limiting in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/rate-limiting-aspnet-core/))

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("api", limiter =>
    {
        limiter.PermitLimit = 100;
        limiter.Window = TimeSpan.FromMinutes(1);
    });
});

WebApplication app = builder.Build();
app.UseRateLimiter();

app.MapGet("/api/orders", () => Results.Ok()).RequireRateLimiting("api");
```

The limiters keep their counts in memory, so each app instance enforces its own limits. That makes rate limiting a guard against unfair use and abuse, not DDoS. Put DDoS protection in front of the app, in a WAF or CDN. Load-test your limits before you ship them. ([Microsoft Learn: Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit))

### Observability: propagate trace context across async boundaries

**When work crosses a process or time boundary, capture the OpenTelemetry context and restore it in the consumer.** Automatic propagation covers only HTTP: ASP.NET Core reads the trace context from incoming requests, and `HttpClient` writes it on outgoing ones, "with no special coding by the app developer". ([Microsoft Learn: Distributed tracing concepts](https://learn.microsoft.com/dotnet/core/diagnostics/distributed-tracing-concepts#coordinate-across-process-boundaries)) Message queues, background jobs, outbox tables, and scheduled work lose the context unless you inject it into the message and extract it in the consumer:

```csharp
TextMapPropagator propagator = Propagators.DefaultTextMapPropagator;

// Producer: write the trace context into the message headers.
propagator.Inject(
    new PropagationContext(Activity.Current?.Context ?? default, default),
    message.Headers,
    (headers, key, value) => headers[key] = value);

// Consumer: read it back, and parent the new activity on it.
PropagationContext parent = propagator.Extract(
    default,
    message.Headers,
    (headers, key) => headers.TryGetValue(key, out string? value) ? [value] : []);

using Activity? activity = source.StartActivity(
    "orders process", ActivityKind.Consumer, parent.ActivityContext);
```

With no current activity, the `default` context injects nothing instead of throwing. When many messages feed one operation, attach each extracted context to the new activity as an `ActivityLink` instead of a parent.

Propagate only the trace context. Never put `Baggage` on a message: it inflates every payload, and it "can expose sensitive information" that an upstream service put in it. "In most cases, propagating just the trace context is sufficient." ([Meziantou: Propagating OpenTelemetry context in .NET](https://www.meziantou.net/propagating-opentelemetry-context-in-dotnet.htm), for the procedure and `Baggage`)

### Request validation

**Validate requests with `AddValidation()` and data annotations.** An invalid request gets a 400 with problem details, and the handler doesn't run. The validation APIs live in `Microsoft.Extensions.Validation`, so you can reuse them outside HTTP. From .NET 11, a rule that needs I/O can be asynchronous: derive from `AsyncValidationAttribute` or implement `IAsyncValidatableObject`, so "a validator can await a database lookup or a remote call instead of blocking a thread".

```csharp
builder.Services.AddValidation();

WebApplication app = builder.Build();

app.MapPost("/api/orders", (CreateOrder order) =>
    TypedResults.Created("/api/orders/1", order));

internal sealed record CreateOrder(
    [property: Required] string Sku,
    [property: Range(1, 100)] int Quantity);
```

Validation applies to every endpoint. To exclude one, call `.DisableValidation()` on it.

([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [.NET Blog: Announcing .NET 11](https://devblogs.microsoft.com/dotnet/announcing-dotnet-11/), [Microsoft Learn: What's new in .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/overview))

### Error responses

**Return every API error as problem details: call `AddProblemDetails()`, then `UseExceptionHandler()` and `UseStatusCodePages()` first in the pipeline.** The exception handler then writes problem details for an unhandled exception. The status code pages middleware writes them for any 400 to 599 response that has no body yet, including a routing 404. A client parses one error shape instead of guessing per endpoint.

```csharp
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
```

Neither middleware replaces a body that a handler already wrote. So return a handler's own errors as `TypedResults.Problem()` or `TypedResults.ValidationProblem()`, not as `TypedResults.BadRequest(new { error = "..." })`.

The middleware writes problem details only when the request's `Accept` header allows JSON: `application/json`, `application/problem+json`, or a wildcard. A client that asks only for another type, such as `text/html`, doesn't get them.

([Microsoft Learn: Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api))

### OpenAPI and API versioning

**Generate the OpenAPI document with `Microsoft.AspNetCore.OpenApi`.** It produces OpenAPI 3.2 by default on .NET 11, up from 3.1 on .NET 10, and the Web API template already references and calls it. It generates the document at run time, and `MapOpenApi()` serves it at `/openapi/{documentName}.json`.

- If a client generator or another tool can't read OpenAPI 3.2 yet, set `options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1` in `AddOpenApi`.
- To also write the document to disk during `dotnet build`, add `Microsoft.Extensions.ApiDescription.Server`. Do this when a build step consumes the file, such as the Kiota client in [Typed clients](#typed-clients). Set `<OpenApiDocumentsDirectory>` to choose where the file goes. The build runs the app's entry point against a mock server, so guard startup code that needs live infrastructure.
- Set `<GenerateDocumentationFile>true</GenerateDocumentationFile>`, so the source generator copies your XML doc comments into the document.
- Use named methods as handlers, because the generator reads doc comments only from methods, not from lambdas.
- Version APIs with `Asp.Versioning` 10.2 or later (`Asp.Versioning.Http`, `Asp.Versioning.Mvc.ApiExplorer`, and `Asp.Versioning.OpenApi`). Its analyzers ship from 10.2, so an earlier version reports none of the three rules below:
  - Call the package's `AddOpenApi()` after `AddApiVersioning()`, and drop the `builder.Services.AddOpenApi()` call. The versioned call replaces the services that call registers, and the package reports AV0029 when both are there.
  - Name the version reader. Left to the default, the package looks in both the query string and the URL segment, and reports AV0015 when every route uses only one. On an API whose clients already send both forms, keep the default: naming one reader stops the other form from being accepted.
  - Set `<Description>` in the project file. Each versioned document takes its description from the assembly, and the package reports AV0025 without one.
  - Call `WithDocumentPerVersion()` to get one document per version.

**Review by 2027-02-28.** On .NET 11, pin `Microsoft.AspNetCore.OpenApi` to 10.0.15 in a project that references `Asp.Versioning.OpenApi`. Version 10.2.3 of the versioning package needs `Microsoft.OpenApi` below 3.0.0, and `Microsoft.AspNetCore.OpenApi` 11.0.0 needs 3.11.0 or later, so restore fails with NU1107 when both are referenced. With the pin, the sample below builds on `net11.0` and serves OpenAPI 3.1. [templates/Directory.Packages.props](../templates/Directory.Packages.props) holds that pin, so until then every project built from the templates gets OpenAPI 3.1, whether or not it versions its API. For the 3.2 default in a project that doesn't reference `Asp.Versioning.OpenApi`, leave the shared pin alone, and set `VersionOverride="11.0.0"` on that project's `PackageReference`. Drop the pin when [dotnet/aspnet-api-versioning#1239](https://github.com/dotnet/aspnet-api-versioning/issues/1239) ships a release for .NET 11.

```csharp
using Asp.Versioning;
using Asp.Versioning.Builder;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiVersioning(options => options.ApiVersionReader = new QueryStringApiVersionReader())
    .AddApiExplorer(options => options.GroupNameFormat = "'v'VVV")
    .AddOpenApi(); // the Asp.Versioning.OpenApi overload

WebApplication app = builder.Build();

app.MapOpenApi().WithDocumentPerVersion(); // /openapi/v1.json, /openapi/v2.json, ...

IVersionedEndpointRouteBuilder orders = app.NewVersionedApi("Orders");
RouteGroupBuilder v1 = orders.MapGroup("/api/orders").HasApiVersion(1.0);
```

([Microsoft Learn: Generate OpenAPI documents](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi), [.NET Blog: Combining API versioning with OpenAPI in .NET 10 applications](https://devblogs.microsoft.com/dotnet/api-versioning-in-dotnet-10-applications/), [Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [Asp.Versioning: AV0015](https://dotnet.github.io/aspnet-api-versioning/diagnostic/av0015.html), [Asp.Versioning: AV0025](https://dotnet.github.io/aspnet-api-versioning/diagnostic/av0025.html), [Asp.Versioning: AV0029](https://dotnet.github.io/aspnet-api-versioning/diagnostic/av0029.html), [Asp.Versioning: 10.2.0 release notes](https://github.com/dotnet/aspnet-api-versioning/releases/tag/v10.2.0))

### Typed clients

**Generate typed clients with Kiota from your OpenAPI document at build time, instead of hand-writing `HttpClient` wrappers.** The client is rebuilt from the contract on every build, so it can't drift from it. ([Meziantou: Kiota client at build time](https://www.meziantou.net/generate-a-kiota-client-at-build-time-from-an-asp-net-core-openapi-file.htm))

### Server push

**Use Server-Sent Events (`TypedResults.ServerSentEvents`) for one-way server push.** ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Caching with `HybridCache`

**Use `HybridCache` instead of writing cache-aside code over `IMemoryCache` or `IDistributedCache`.** `GetOrCreateAsync()` gives you stampede protection, an in-memory L1 over a distributed L2, and tag-based invalidation. It ships in the `Microsoft.Extensions.Caching.Hybrid` package.

```csharp
builder.Services.AddHybridCache();

WebApplication app = builder.Build();

app.MapGet("/api/orders/{id}", async (string id, HybridCache cache, CancellationToken ct) =>
    await cache.GetOrCreateAsync(
        $"order:{id}",
        async token => await LoadOrderAsync(id, token),
        cancellationToken: ct));
```

Concurrent callers for the same key share one factory call. A distributed backend, such as Redis or Postgres, plugs in through your existing `IDistributedCache` registration, and the call sites don't change.

([.NET Blog: Hello HybridCache! Streamlining Cache Management for ASP.NET Core Applications](https://devblogs.microsoft.com/dotnet/hybrid-cache-is-now-ga/), [.NET Blog: High-Performance Distributed Caching with .NET and Postgres on Azure](https://devblogs.microsoft.com/dotnet/high-performance-distributed-caching-dotnet-postgres-azure/))

### HTTP caching defaults

**Send `Cache-Control: no-cache, no-store, must-revalidate` from APIs by default, and opt individual endpoints into caching.** Unintended caching leads to stale data, security vulnerabilities, and bugs that are hard to reproduce. ([Meziantou: Disable HTTP caching by default](https://www.meziantou.net/disable-http-caching-by-default-in-asp-net-core-apis.htm))

### Localization

**Register `AddLocalization`, call `UseRequestLocalization` early in the pipeline, and read the culture from a cookie, not `Accept-Language`.** The header reflects the user's OS settings, not a choice they made. For provider order, `SupportedCultures` versus `SupportedUICultures`, and resource key policy, see [globalization.md](globalization.md).
