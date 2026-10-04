---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources:
  [dotnet-blog, aspnet-blog, ms-learn, meziantou, andrew-lock, milan-jovanovic]
---

# ASP.NET Core

Target ASP.NET Core 10. Minimal APIs are the default for new HTTP APIs.

## Opinions

### Hosting and deployment

**Run Kestrel directly, in a container image that `dotnet publish /t:PublishContainer` builds without a Dockerfile.** Kestrel is fully supported as the edge server. When you do run behind a proxy or ingress for port sharing, TLS termination, or load balancing, enable host filtering and forwarded headers. They're part of the proxy contract, not optional hardening.

The SDK's container publish builds an OCI image from MSBuild properties such as `ContainerRepository` and `ContainerRegistry`. It keeps the base image patched along with the SDK, and it doesn't need a daemon to produce a tarball for scanning pipelines. Hand-written Dockerfiles drift, so use one only when you need custom image layers. ([Microsoft Learn: When to use a reverse proxy with Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/when-to-use-a-reverse-proxy), [Microsoft Learn: Containerize an app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish))

### Caching with `HybridCache`

**Use `HybridCache` instead of writing your own cache-aside code over `IMemoryCache` or `IDistributedCache`.** `GetOrCreateAsync()` handles stampede protection, a two-level cache (in-memory L1 and distributed L2), and tag-based invalidation. It ships in the `Microsoft.Extensions.Caching.Hybrid` package.

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

Under concurrency, the factory runs once per key. A distributed backend, such as Redis or Postgres, plugs in through the existing `IDistributedCache` registration, with no change to call sites. ([.NET Blog: Hello HybridCache! Streamlining Cache Management for ASP.NET Core Applications](https://devblogs.microsoft.com/dotnet/hybrid-cache-is-now-ga/), [.NET Blog: High-Performance Distributed Caching with .NET and Postgres on Azure](https://devblogs.microsoft.com/dotnet/high-performance-distributed-caching-dotnet-postgres-azure/))

### Request validation

**Validate requests with minimal API validation: `AddValidation()` and data annotations.** An invalid request returns 400 with problem details automatically. The validation APIs live in `Microsoft.Extensions.Validation`, so you can reuse them outside HTTP.

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

To exclude an endpoint, call `.DisableValidation()` on it. Don't opt the app in one endpoint at a time. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### OpenAPI and API versioning

**Generate the OpenAPI document at build time with Microsoft's own package.** `Microsoft.AspNetCore.OpenApi` generates OpenAPI 3.1, and the Web API template already references and calls it.

- Set `<GenerateDocumentationFile>true</GenerateDocumentationFile>`, so the source generator adds your XML doc comments to the document.
- Name your handlers, because lambdas lose their comments.
- Version APIs with `Asp.Versioning` v10 (`Asp.Versioning.Http`, `Asp.Versioning.Mvc.ApiExplorer`, and `Asp.Versioning.OpenApi`), and call `WithDocumentPerVersion()` to get one document per version:

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

**Generate typed clients from your own OpenAPI document at build time with Kiota,** instead of writing `HttpClient` wrappers by hand. The contract stays versioned, and the client can't drift from it. ([Meziantou: Kiota client at build time](https://www.meziantou.net/generate-a-kiota-client-at-build-time-from-an-asp-net-core-openapi-file.htm))

### HTTP caching defaults

**Send `Cache-Control: no-cache, no-store, must-revalidate` from APIs by default,** and opt individual endpoints into caching on purpose. The opposite default causes stale-data bugs and cache poisoning. ([Meziantou: Disable HTTP caching by default](https://www.meziantou.net/disable-http-caching-by-default-in-asp-net-core-apis.htm))

### Server push

**Use Server-Sent Events (`TypedResults.ServerSentEvents`) for one-way server push.** ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Authentication: passkeys first

**Sign users in with the built-in passkey (WebAuthn and FIDO2) support in ASP.NET Core Identity, instead of passwords or a third-party FIDO library.** In .NET 10, Identity and the Blazor Web App template include passkey management and sign-in. Passkeys resist phishing, leave no server-side secret to leak, and add no dependency to vet. Keep a second factor or another path for account recovery, but don't start a password table in a new app. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [Microsoft Learn: Passkeys in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/passkeys/))

**Return 401 or 403 from API endpoints, never a sign-in redirect.** ASP.NET Core 10 skips cookie redirects for known API endpoints. Make your custom authentication handlers do the same. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Request timeouts

**Give every endpoint a deadline, and pass the cancellation token to the work it starts.** ASP.NET Core has no application timeout of its own. A stalled query or dependency keeps using resources long after the response stops being useful, and whatever sits in front of your app ends up owning both the deadline and the response body.

- `AddRequestTimeouts` only registers the services. The limit comes from a named policy, or from `WithRequestTimeout` on the endpoint.
- The middleware is cooperative: it cancels `HttpContext.RequestAborted` and then waits. A handler that never passes the token on runs to completion and returns 200. No 504 appears, because nothing threw. So a 504 tells you that cancellation reached the middleware, not that the database stopped.
- Give reads and exports separate policies, instead of one global value.
- Exclude streaming responses with `DisableRequestTimeout()`. Once a response has started, it can't be replaced with a clean 504. ([Microsoft Learn: Request timeouts middleware](https://learn.microsoft.com/aspnet/core/performance/timeouts), [Jovanović: Your ASP.NET Core endpoints don't have a timeout](https://www.milanjovanovic.tech/blog/your-aspnetcore-endpoints-dont-have-a-timeout))

```csharp
builder.Services.AddRequestTimeouts(options =>
{
    options.AddPolicy("api-read", TimeSpan.FromSeconds(3));
    options.AddPolicy("report-export", TimeSpan.FromSeconds(30));
});

app.UseRequestTimeouts();

// The token reaches EF Core, so the timeout actually stops the query.
app.MapGet("/orders/{id:guid}", async (Guid id, AppDbContext db, CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == id, cancellationToken))
    .WithRequestTimeout("api-read");

app.MapGet("/events", StreamEvents).DisableRequestTimeout();
```

Timeouts don't fire while a debugger is attached, so test them without one.

### Rate limiting

**Put named `Microsoft.AspNetCore.RateLimiting` policies on your public endpoints, and reject with 429.** The default rejection status is 503, so always override it. ([Microsoft Learn: RateLimiterOptions.RejectionStatusCode](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.ratelimiting.ratelimiteroptions.rejectionstatuscode))

- Partition by a stable identity, such as the authenticated user or the API key tier. Never partition by raw input that the client controls, such as an IP header that can be spoofed.
- Use the concurrency limiter for expensive endpoints, where in-flight work is the real constraint.

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

Rate limiting controls fairness and abuse on each node. It isn't DDoS protection, which belongs at the edge in a WAF or CDN. Load-test your limits before you ship them. ([Microsoft Learn: Rate limiting middleware in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/rate-limit))

### Observability: propagate trace context across async boundaries

**When work crosses a process or time boundary, capture the OpenTelemetry context explicitly and restore it on the consumer side.** Automatic propagation only covers synchronous HTTP. Message queues, background jobs, outbox tables, and scheduled work all cross such a boundary.

1. Serialize the context of `Activity.Current` with `Propagators.DefaultTextMapPropagator`, and store it with the message.
2. When you consume the message, start the new activity with the extracted `ActivityContext` as its parent for a linear flow. When many messages fan into one operation, attach each context as an `ActivityLink` instead.

Don't propagate `Baggage` by default. It bloats payloads and leaks whatever anyone upstream put in it. ([Meziantou: Propagating OpenTelemetry context in .NET](https://www.meziantou.net/propagating-opentelemetry-context-in-dotnet.htm))

### CSRF defence in depth

**Add Fetch Metadata headers, such as `Sec-Fetch-Site`, to your CSRF defence,** alongside token-based antiforgery. .NET 11 will automate this. ([Lock: Understanding the Fetch Metadata headers](https://andrewlock.net/understanding-the-fetch-metadata-http-headers-sec-fetch-site-and-friends/))

### Localization

**Register `AddLocalization`, call `UseRequestLocalization` early in the pipeline, and take the culture from a cookie instead of `Accept-Language`.** The browser header reflects the user's OS, not a choice they made. For provider order, the split between `SupportedCultures` and `SupportedUICultures`, and resource key policy, see [globalization.md](globalization.md).

## Coming next (preview, not yet the opinion)

.NET 11 previews add automatic CSRF protection based on Fetch Metadata headers, which removes the need for `UseAntiforgery()` in minimal APIs and Blazor SSR. ([Lock: Automatic CSRF protection based on Fetch Metadata headers](https://andrewlock.net/exploring-the-dotnet-11-preview-6-automatic-csrf-protection-based-on-fetch-metadata-http-headers/))
