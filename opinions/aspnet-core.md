---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources:
  [dotnet-blog, aspnet-blog, ms-learn, meziantou, andrew-lock, milan-jovanovic]
---

# ASP.NET Core

Target ASP.NET Core 10, and use minimal APIs for new HTTP APIs.

## Opinions

### Hosting and deployment

**Run Kestrel directly, in a container image that `dotnet publish /t:PublishContainer` builds without a Dockerfile.** Kestrel is supported as an edge server. If you run behind a proxy or ingress for port sharing, TLS termination, or load balancing, enable host filtering and forwarded headers. The proxy setup is incomplete without them.

Container publish builds an OCI image from MSBuild properties such as `ContainerRepository` and `ContainerRegistry`. The base image stays patched with the SDK, and you don't need a Docker daemon to produce a tarball for a scanner. Write a Dockerfile only when you need custom image layers, because a hand-written one drifts from the SDK's. ([Microsoft Learn: When to use a reverse proxy with Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/when-to-use-a-reverse-proxy), [Microsoft Learn: Containerize an app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish))

### Caching with `HybridCache`

**Use `HybridCache` instead of writing cache-aside code over `IMemoryCache` or `IDistributedCache`.** `GetOrCreateAsync()` gives you stampede protection, an in-memory L1 over a distributed L2, and tag-based invalidation. It ships in the `Microsoft.Extensions.Caching.Hybrid` package.

```csharp
builder.Services.AddHybridCache();
```

```csharp
app.MapGet("/api/orders/{id}", async (string id, HybridCache cache, CancellationToken ct) =>
    await cache.GetOrCreateAsync(
        $"order:{id}",
        async token => await LoadOrderAsync(id, token),
        cancellationToken: ct));
```

Concurrent callers for the same key share one factory call. A distributed backend, such as Redis or Postgres, plugs in through your existing `IDistributedCache` registration, and the call sites don't change. ([.NET Blog: Hello HybridCache! Streamlining Cache Management for ASP.NET Core Applications](https://devblogs.microsoft.com/dotnet/hybrid-cache-is-now-ga/), [.NET Blog: High-Performance Distributed Caching with .NET and Postgres on Azure](https://devblogs.microsoft.com/dotnet/high-performance-distributed-caching-dotnet-postgres-azure/))

### Request validation

**Validate requests with `AddValidation()` and data annotations.** An invalid request gets a 400 with problem details, and the handler doesn't run. The validation APIs live in `Microsoft.Extensions.Validation`, so you can reuse them outside HTTP.

```csharp
builder.Services.AddValidation();
```

```csharp
app.MapPost("/api/orders", (CreateOrder order) =>
    TypedResults.Created($"/api/orders/1", order));

record CreateOrder(
    [property: Required] string Sku,
    [property: Range(1, 100)] int Quantity);
```

Validation applies to every endpoint. To exclude one, call `.DisableValidation()` on it. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### OpenAPI and API versioning

**Generate the OpenAPI document at build time with `Microsoft.AspNetCore.OpenApi`.** It produces OpenAPI 3.1, and the Web API template already references and calls it.

- Set `<GenerateDocumentationFile>true</GenerateDocumentationFile>`, so the source generator copies your XML doc comments into the document.
- Use named methods as handlers. A lambda's doc comments don't reach the document.
- Version APIs with `Asp.Versioning` v10 (`Asp.Versioning.Http`, `Asp.Versioning.Mvc.ApiExplorer`, and `Asp.Versioning.OpenApi`). Call `WithDocumentPerVersion()` to get one document per version:

```csharp
builder.Services.AddOpenApi();
builder.Services
    .AddApiVersioning()
    .AddApiExplorer(options => options.GroupNameFormat = "'v'VVV")
    .AddOpenApi(); // the Asp.Versioning.OpenApi overload

var app = builder.Build();

app.MapOpenApi().WithDocumentPerVersion(); // /openapi/v1.json, /openapi/v2.json, ...

var orders = app.NewVersionedApi("Orders");
var v1 = orders.MapGroup("/api/orders").HasApiVersion(1.0);
```

([.NET Blog: Combining API versioning with OpenAPI in .NET 10 applications](https://devblogs.microsoft.com/dotnet/api-versioning-in-dotnet-10-applications/), [Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Typed clients

**Generate typed clients with Kiota from your OpenAPI document at build time, instead of hand-writing `HttpClient` wrappers.** The client is rebuilt from the contract on every build, so it can't drift from it. ([Meziantou: Kiota client at build time](https://www.meziantou.net/generate-a-kiota-client-at-build-time-from-an-asp-net-core-openapi-file.htm))

### HTTP caching defaults

**Send `Cache-Control: no-cache, no-store, must-revalidate` from APIs by default, and opt individual endpoints into caching.** A cacheable default leads to stale data and cache poisoning. ([Meziantou: Disable HTTP caching by default](https://www.meziantou.net/disable-http-caching-by-default-in-asp-net-core-apis.htm))

### Server push

**Use Server-Sent Events (`TypedResults.ServerSentEvents`) for one-way server push.** ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Authentication: passkeys first

**Sign users in with ASP.NET Core Identity's built-in passkey (WebAuthn and FIDO2) support, not passwords or a third-party FIDO library.** In .NET 10, Identity and the Blazor Web App template include passkey management and sign-in. Passkeys resist phishing, leave no server-side secret to leak, and add no dependency. Keep a second factor or another recovery path, but don't create a password table in a new app. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [Microsoft Learn: Passkeys in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/passkeys/))

**Return 401 or 403 from API endpoints, never a sign-in redirect.** ASP.NET Core 10 skips cookie redirects for known API endpoints. Make your custom authentication handlers do the same. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Request timeouts

**Give every endpoint a timeout, and pass the cancellation token to the work it starts.** ASP.NET Core sets no request timeout by default. A stalled query keeps using resources after the response stops being useful, and the proxy in front of your app decides when to give up and what the client sees.

- `AddRequestTimeouts` only registers the services. Set the limit with a named policy, or with `WithRequestTimeout` on the endpoint.
- The middleware cancels `HttpContext.RequestAborted` and waits. A handler that ignores the token runs to completion and returns 200. A 504 means the cancellation reached the middleware, not that the database stopped.
- Give reads and exports separate policies instead of one global value.
- Exclude streaming responses with `DisableRequestTimeout()`. After a response starts, the middleware can't replace it with a 504. ([Microsoft Learn: Request timeouts middleware](https://learn.microsoft.com/aspnet/core/performance/timeouts), [Jovanović: Your ASP.NET Core endpoints don't have a timeout](https://www.milanjovanovic.tech/blog/your-aspnetcore-endpoints-dont-have-a-timeout))

```csharp
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("api-read", TimeSpan.FromSeconds(3));
    options.AddPolicy("report-export", TimeSpan.FromSeconds(30));
});

app.UseRequestTimeouts();

// The token reaches EF Core, so the timeout stops the query.
app.MapGet("/orders/{id:guid}", async (Guid id, AppDbContext db, CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id, cancellationToken))
    .WithRequestTimeout("api-read");

app.MapGet("/events", StreamEvents).DisableRequestTimeout();
```

To test a timeout, run without a debugger attached. Timeouts don't fire under one.

### Rate limiting

**Put named `Microsoft.AspNetCore.RateLimiting` policies on public endpoints, and reject with 429.** The default rejection status is 503, so set `RejectionStatusCode`. ([Microsoft Learn: RateLimiterOptions.RejectionStatusCode](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.ratelimiting.ratelimiteroptions.rejectionstatuscode))

- Partition by a stable identity, such as the authenticated user or the API key tier. Never partition by a value the client controls, such as a spoofable IP header.
- Use the concurrency limiter on expensive endpoints, where in-flight work is the limit that matters.

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

var app = builder.Build();
app.UseRateLimiter();

app.MapGet("/api/orders", () => Results.Ok()).RequireRateLimiting("api");
```

Each node enforces its own limits, so this controls fairness and abuse, not DDoS. Put DDoS protection at the edge, in a WAF or CDN. Load-test your limits before you ship them. ([Microsoft Learn: Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit))

### Observability: propagate trace context across async boundaries

**When work crosses a process or time boundary, capture the OpenTelemetry context and restore it in the consumer.** Automatic propagation covers only synchronous HTTP calls. Message queues, background jobs, outbox tables, and scheduled work need it done by hand.

1. Serialize `Activity.Current`'s context with `Propagators.DefaultTextMapPropagator`, and store it with the message.
2. In the consumer, start the new activity with the extracted `ActivityContext` as its parent. When many messages feed one operation, attach each context as an `ActivityLink` instead.

Don't propagate `Baggage` by default. It inflates every payload and leaks whatever an upstream service put in it. ([Meziantou: Propagating OpenTelemetry context in .NET](https://www.meziantou.net/propagating-opentelemetry-context-in-dotnet.htm))

### CSRF defence in depth

**Check Fetch Metadata headers, such as `Sec-Fetch-Site`, alongside token-based antiforgery.** ([Lock: Understanding the Fetch Metadata headers](https://andrewlock.net/understanding-the-fetch-metadata-http-headers-sec-fetch-site-and-friends/))

### Localization

**Register `AddLocalization`, call `UseRequestLocalization` early in the pipeline, and read the culture from a cookie, not `Accept-Language`.** The header reflects the user's OS settings, not a choice they made. For provider order, `SupportedCultures` versus `SupportedUICultures`, and resource key policy, see [globalization.md](globalization.md).

## Coming next (preview, not yet the opinion)

.NET 11 previews add automatic CSRF protection based on Fetch Metadata headers. It removes the need for `UseAntiforgery()` in minimal APIs and Blazor SSR. ([Lock: Automatic CSRF protection based on Fetch Metadata headers](https://andrewlock.net/exploring-the-dotnet-11-preview-6-automatic-csrf-protection-based-on-fetch-metadata-http-headers/))
