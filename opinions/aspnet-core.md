---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-25
last-used: 2026-10-13
sources:
  [
    andrew-lock,
    aspnet-blog,
    code-with-mukesh,
    damien-bowden,
    dotnet-blog,
    khalid,
    meziantou,
    milan-jovanovic,
    ms-learn,
  ]
---

# ASP.NET Core

Target ASP.NET Core 10, and use minimal APIs for new HTTP APIs.

## Opinions

### Hosting and deployment

**Run Kestrel directly, in a container image that `dotnet publish /t:PublishContainer` builds without a Dockerfile.** Kestrel is supported as an edge server. If you run behind a proxy or ingress for port sharing, TLS termination, or load balancing, enable host filtering and forwarded headers. Without forwarded headers, the app sees the proxy's scheme and IP address instead of the client's.

Container publish builds an OCI image from MSBuild properties such as `ContainerRepository` and `ContainerRegistry`. The base image stays patched with the SDK, and you don't need a Docker daemon to produce a tarball for a scanner. Write a Dockerfile only when you need custom image layers, because a hand-written one drifts from the SDK's. ([Microsoft Learn: When to use a reverse proxy with Kestrel](https://learn.microsoft.com/aspnet/core/fundamentals/servers/kestrel/when-to-use-a-reverse-proxy), [Microsoft Learn: Containerize an app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish))

**Never set `ContainerUser` to `root`.** The Microsoft runtime images run as the rootless `app` user on Linux. As root, "if an attacker manages to compromise your container, they'll be able to do pretty much anything in the container". ([Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration), [Lock: Updates to Docker images in .NET 8](https://andrewlock.net/exploring-the-dotnet-8-preview-updates-to-docker-images-in-dotnet-8/))

An image keeps the runtime it was built with until you rebuild it. For how often to rebuild, see [security.md: Servicing](security.md#servicing-take-the-monthly-patch).

**Let the layer that terminates TLS own HTTPS redirection and HSTS.** If the proxy redirects to HTTPS, "there's no need to use HTTPS redirection middleware", so drop `UseHttpsRedirection()`, and leave the HSTS header to the proxy too. Don't call `UseHsts()` in development, because "the HSTS settings are highly cacheable by browsers". ([Microsoft Learn: Enforce HTTPS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl))

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

**Generate the OpenAPI document with `Microsoft.AspNetCore.OpenApi`.** It produces OpenAPI 3.1 on .NET 10, and the Web API template already references and calls it. It generates the document at run time, and `MapOpenApi()` serves it at `/openapi/{documentName}.json`.

- To also write the document to disk during `dotnet build`, add `Microsoft.Extensions.ApiDescription.Server`. Do this when a build step consumes the file, such as the Kiota client in [Typed clients](#typed-clients). Set `<OpenApiDocumentsDirectory>` to choose where the file goes. The build runs the app's entry point against a mock server, so guard startup code that needs live infrastructure.
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

([Microsoft Learn: Generate OpenAPI documents](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/aspnetcore-openapi), [.NET Blog: Combining API versioning with OpenAPI in .NET 10 applications](https://devblogs.microsoft.com/dotnet/api-versioning-in-dotnet-10-applications/), [Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Typed clients

**Generate typed clients with Kiota from your OpenAPI document at build time, instead of hand-writing `HttpClient` wrappers.** The client is rebuilt from the contract on every build, so it can't drift from it. ([Meziantou: Kiota client at build time](https://www.meziantou.net/generate-a-kiota-client-at-build-time-from-an-asp-net-core-openapi-file.htm))

### HTTP caching defaults

**Send `Cache-Control: no-cache, no-store, must-revalidate` from APIs by default, and opt individual endpoints into caching.** A cacheable default leads to stale data and cache poisoning. ([Meziantou: Disable HTTP caching by default](https://www.meziantou.net/disable-http-caching-by-default-in-asp-net-core-apis.htm))

### Server push

**Use Server-Sent Events (`TypedResults.ServerSentEvents`) for one-way server push.** ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

### Authentication: passkeys first

**Sign users in with ASP.NET Core Identity's built-in passkey (WebAuthn and FIDO2) support, not passwords or a third-party FIDO library.** In .NET 10, Identity and the Blazor Web App template include passkey management and sign-in. Passkeys resist phishing, leave no server-side secret to leak, and add no dependency. Keep a second factor or another recovery path, but don't create a password table in a new app. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [Microsoft Learn: Passkeys in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/passkeys/))

**Return 401 or 403 from API endpoints, never a sign-in redirect.** ASP.NET Core 10 skips cookie redirects for known API endpoints. Make your custom authentication handlers do the same. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0))

**Review by 2027-03-31.** Don't trust the `amr` claim after a passkey sign-in. .NET 10 sets it to `pwd` instead of `pop`, the proof-of-possession value that OpenID Connect EAP ACR Values 1.0 defines. Bowden reports that this "is not implemented correctly in ASP.NET Core (.NET 10)", and it's filed as [dotnet/aspnetcore#64881](https://github.com/dotnet/aspnetcore/issues/64881), which is still open. Authorization that reads `amr` for a step-up or multi-factor decision misjudges a passkey. Do one of these instead:

- Take the credential type from your own sign-in path.
- Reissue the claim. You can't change the existing claims collection, so sign the user out and back in with `SignInWithClaimsAsync()`.

Drop this note when a shipped release fixes the issue. ([Bowden: Set the amr claim when using passkeys authentication in ASP.NET Core](https://damienbod.com/2026/01/05/set-the-amr-claim-when-using-passkeys-authentication-in-asp-net-core/))

### Authorization by default

**Set a fallback policy that requires an authenticated user, and let endpoints opt out.** A new endpoint is then private until someone marks it public, the same default as [Request validation](#request-validation). "Having authorization required by default is more secure than relying on new controllers and Razor Pages to include the `[Authorize]` attribute". ([Microsoft Learn: Create an ASP.NET Core web app with user data protected by authorization](https://learn.microsoft.com/aspnet/core/security/authorization/secure-data))

```csharp
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
```

Mark public endpoints with `[AllowAnonymous]`, or call `.AllowAnonymous()` on a minimal API endpoint or group. Give an endpoint that needs more a stricter named policy.

### Browser clients: cookies, with a BFF holding the tokens

**Authenticate a browser app with a cookie. A SPA or Blazor WebAssembly client calls its APIs through a backend for frontend (BFF) that keeps the tokens on the server.** Microsoft Learn recommends cookies for browser apps "because, by default, the browser automatically handles them without exposing them to JavaScript", and reserves tokens for clients that can't use cookies. ([Microsoft Learn: Use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization))

The difference is what one XSS bug exposes. Any injected script can read a token in `localStorage`. ([Jovanović: JWT Authentication in ASP.NET Core](https://milanjovanovic.tech/blog/jwt-authentication-aspnetcore)) By contrast, "`HttpOnly` cookies cannot be read by JavaScript, so even a successful XSS attack cannot exfiltrate the user's access or refresh token". ([Abuhakmeh: The Cookie Apocalypse Already Happened](https://duendesoftware.com/blog/20260414-the-cookie-apocalypse-already-happened)) Bowden states the client-side rule: "No authentication security logic should be implemented in a client application running in the browser". ([Bowden: Implement BFF using Auth0, Angular and ASP.NET Core](https://damienbod.com/2026/08/10/implement-bff-using-auth0-angular-and-asp-net-core/))

Set it up in three parts:

- **The session cookie:** Use a `__Host-` prefixed cookie marked `HttpOnly` and `Secure`. Add `SameSite=Strict` where the flow allows it, because such cookies "are not sent on cross-site requests". This adds to the [CSRF defence](#csrf-defence-in-depth) below, and doesn't replace it. ([Abuhakmeh: The Cookie Apocalypse Already Happened](https://duendesoftware.com/blog/20260414-the-cookie-apocalypse-already-happened))
- **One origin:** Put a reverse proxy in front, so the front end and the API share an origin.
- **A separate downstream token:** The BFF gets its own token for the downstream API, instead of forwarding the browser's. In Bowden's design, "The downstream API does not accept the user delegated access tokens from the UI application". ([Bowden: BFF secured ASP.NET Core application using downstream API and an OAuth client credentials JWT](https://damienbod.com/2024/04/08/bff-secured-asp-net-core-application-using-downstream-api-and-an-oauth-client-credentials-jwt/))

Blazor's documented BFF pattern has the same shape. It proxies API calls "with the `access_token` stored in the authentication cookie". ([Microsoft Learn: Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=bff-pattern))

**Keep session state on the server, and make the cookie a key to it.** Browsers cap a cookie at about 4 KB. Abuhakmeh's fix: "stop storing session data in the cookie entirely... The cookie becomes a thin key that points to the server state rather than containing that state itself". ([Abuhakmeh: ASP.NET Core Cookie Size Limits in Production](https://duendesoftware.com/blog/20260429-aspnet-core-cookie-size-limits))

### Tokens for APIs

**Get tokens from an identity provider, validate the issuer, audience, lifetime, and signing key, and keep permissions out of the token.** Hand identity to "a battle-tested identity provider" instead of issuing your own JWTs, which would make your service responsible for key management and revocation. ([Jovanović: Integrate Keycloak with ASP.NET Core Using OAuth 2.0](https://milanjovanovic.tech/blog/integrate-keycloak-with-aspnetcore-using-oauth-2))

- **Resolve permissions on the server** with `IClaimsTransformation`, and keep tokens small. A permission in a token outlives its revocation: "A revoked permission keeps working until the token expires, which I reproduced as a 201 that should have been a 403". ([Jovanović: Building Secure APIs with Role-Based Access Control in ASP.NET Core](https://milanjovanovic.tech/blog/building-secure-apis-with-role-based-access-control-in-aspnetcore), [Mukesh: Permission-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/permission-based-authorization-in-aspnet-core/))
- **Keep access-token lifetimes short,** because you can't revoke a JWT after you issue it. ([Mukesh: JWT Authentication in ASP.NET Core](https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/))
- **Rotate refresh tokens on every use:** issue a new one, and revoke the old one. ([Jovanović: JWT Authentication in ASP.NET Core](https://milanjovanovic.tech/blog/jwt-authentication-aspnetcore))

**Bind each token to its client, and authenticate the client with a key instead of a secret.** Anyone who holds a bearer token can use it. Each of these needs support from your identity provider, so check what it implements before you design around them:

- **DPoP:** OAuth DPoP binds the token to a key that the client proves it holds. "The access tokens should only be used for what the access tokens are intended for. OAuth DPoP helps force this." ([Bowden: Securing APIs using ASP.NET Core and OAuth 2.0 DPoP](https://damienbod.com/2023/08/14/securing-apis-using-asp-net-core-and-oauth-2-0-dpop/))
- **Pushed authorization requests (PAR):** Set `PushedAuthorizationBehavior.Require`, available since .NET 9, to move the authorization request parameters out of the browser redirect. ([Bowden: Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/))
- **Client assertions:** Authenticate a confidential client with a private-key JWT assertion. It replaces the shared secret, the one long-lived credential the client would otherwise keep in configuration. ([Bowden: Use client assertions in ASP.NET Core using OpenID Connect, OAuth DPoP and OAuth PAR](https://damienbod.com/2026/02/02/use-client-assertions-in-asp-net-core-using-openid-connect-oauth-dpop-and-oauth-par/))

### The Data Protection key ring outlives the container

**If you run more than one instance, or a container that restarts, persist the key ring to shared storage and protect it at rest. Configure both explicitly.** The ring encrypts authentication cookies and antiforgery tokens. If it doesn't survive a restart, every user is signed out. If replicas don't share it, they can't read each other's cookies. In a container, keep the keys in "A folder that's a Docker volume that persists beyond the container's lifetime... or An external provider, such as Azure Blob Storage... or Redis". ([Microsoft Learn: Configure ASP.NET Core Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview))

```csharp
// Azure.Extensions.AspNetCore.DataProtection.Blobs and .Keys; both extension
// methods live in the Microsoft.AspNetCore.DataProtection namespace.
builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(blobUri, credential)
    .ProtectKeysWithAzureKeyVault(keyIdentifier, credential);
```

Make both calls. When you name a location, "the data protection system deregisters the default key encryption at rest mechanism, so keys are no longer encrypted at rest". Redis "doesn't persist data by default when restarting", so a Redis restart gives the app new keys and invalidates everything the old ones protected. ([Microsoft Learn: Key storage providers in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/data-protection/implementation/key-storage-providers))

Encryption at rest "doesn't prevent cyberattackers from creating new keys", so lock down the store's permissions as carefully as you encrypt it. Share one ring between applications behind a single proxy only when you configure it on purpose. ([Abuhakmeh: Data Protection for ASP.NET Core Developers](https://duendesoftware.com/blog/20250313-data-protection-aspnetcore-duende-identityserver))

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
- To partition anonymous traffic, use `HttpContext.Connection.RemoteIpAddress`, not `X-Forwarded-For`. The client controls the header but not the connection address. The forwarded-headers handling in [Hosting and deployment](#hosting-and-deployment) decides which address that property reports.
- Bound the number of partitions, because every distinct key allocates its own limiter. ([Mukesh: Rate Limiting in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/rate-limiting-aspnet-core/))
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

**If a minimal API endpoint binds a form file, register `UseAntiforgery()`.** Since ASP.NET Core 8, "Minimal API endpoints that bind a parameter from the form via IFormFile or IFormFileCollection require anti-forgery validation". An upload endpoint that worked on .NET 7 returns 400 until you register the middleware and the caller sends the token. ([Microsoft Learn: Minimal APIs antiforgery checks](https://learn.microsoft.com/aspnet/core/breaking-changes/8/antiforgery-checks))

### CORS

**Name every CORS policy, and never combine a wildcard origin with credentials.** Microsoft Learn calls two combinations insecure: "Specifying AllowAnyOrigin and AllowCredentials", and "Bypassing the built-in checks by using SetIsOriginAllowed(\_ => true) together with AllowCredentials". ([Microsoft Learn: Enable Cross-Origin Requests (CORS) in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cors))

Apply a named policy to each endpoint or group with `[EnableCors]`, instead of one permissive default. Otherwise, a policy you widen for one endpoint widens what the whole app accepts. In .NET 11, the allowed-origins list also decides which cross-origin requests the CSRF middleware admits. For details, see [Coming next](#coming-next-preview-not-yet-the-opinion).

### Security headers and output encoding

**Send a baseline of security headers with `NetEscapades.AspNetCore.SecurityHeaders`, and give the Content Security Policy (CSP) a nonce instead of `unsafe-inline`.** Bowden's baseline is:

- `X-Frame-Options: DENY`
- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: strict-origin-when-cross-origin`
- The three `Cross-Origin-*` policies
- `Permissions-Policy`
- HSTS outside development

For an interactive Blazor circuit, prefer the nonce, and fall back to hashes. ([Bowden: Implement a secure Blazor Web application using OpenID Connect and security headers](https://damienbod.com/2024/04/15/implement-a-secure-blazor-web-application-using-openid-connect-and-security-headers/), [Bowden: Revisiting using a Content Security Policy (CSP) nonce in Blazor](https://damienbod.com/2025/05/26/revisiting-using-a-content-security-policy-csp-nonce-in-blazor/))

```csharp
app.UseSecurityHeaders(new HeaderPolicyCollection()
    .AddDefaultSecurityHeaders()
    .AddCrossOriginOpenerPolicy(policy => policy.SameOrigin())
    .AddContentSecurityPolicy(csp =>
    {
        csp.AddDefaultSrc().Self();
        csp.AddScriptSrc().Self().WithNonce();
        csp.AddObjectSrc().None();
        csp.AddFrameAncestors().None();
    }));
```

Razor encodes output "sourced from variables, unless you work to prevent this behavior". Never pass untrusted input to `HtmlString`, because that "exposes an XSS vulnerability". ([Microsoft Learn: Prevent Cross-Site Scripting (XSS) in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cross-site-scripting))

Blazor Web Apps send `frame-ancestors 'self'` without extra code. On .NET 8 to 10, a CSP without `unsafe-inline` styles breaks the `Virtualize` component, and .NET 11 fixes it. ([Microsoft Learn: Enforce a Content Security Policy for ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/security/content-security-policy))

If client-side script writes to the DOM, enable Trusted Types, which moves the check into the browser: "APIs like `innerHTML` must be passed a `TrustedHTML` object". ([Lock: Preventing client-side cross-site-scripting vulnerabilities with Trusted Types](https://andrewlock.net/preventing-client-side-cross-site-scripting-vulnerabilities-with-trusted-types/))

### Open redirects

**Before you redirect to a URL from the request, check that it's local.** On .NET 10, `RedirectHttpResult.IsLocalUrl` works outside MVC. `LocalRedirect` "throws an exception if a non-local URL is specified". ([Microsoft Learn: Prevent open redirect attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/preventing-open-redirects))

```csharp
app.MapGet("/redirect", (string url) =>
    RedirectHttpResult.IsLocalUrl(url)
        ? Results.LocalRedirect(url)
        : Results.BadRequest());
```

### Localization

**Register `AddLocalization`, call `UseRequestLocalization` early in the pipeline, and read the culture from a cookie, not `Accept-Language`.** The header reflects the user's OS settings, not a choice they made. For provider order, `SupportedCultures` versus `SupportedUICultures`, and resource key policy, see [globalization.md](globalization.md).

## Coming next (preview, not yet the opinion)

.NET 11 previews add automatic CSRF protection based on Fetch Metadata headers. It's on by default for apps built with `WebApplicationBuilder`, and it removes the need for `UseAntiforgery()` in minimal APIs and Blazor SSR.

- MVC and Razor Pages still validate tokens: "There's basically no simple way today to opt into _only_ using the new approach."
- A cross-origin request is rejected unless its origin is in the CORS allowed-origins list.
- A request that carries no browser signals passes through. ([Lock: Automatic CSRF protection based on Fetch Metadata headers](https://andrewlock.net/exploring-the-dotnet-11-preview-6-automatic-csrf-protection-based-on-fetch-metadata-http-headers/))

The rejection breaks one identity flow. A SPA that exchanges an authorization code at the token endpoint sends a cross-origin POST: "It's cross-origin. It's a POST. It will be rejected." It isn't settled yet whether a provider callback that uses `response_mode=form_post` counts as legitimate. ([Abuhakmeh: Understanding .NET 11 Automatic CSRF Protection](https://duendesoftware.com/blog/20260901-understanding-dotnet-11-automatic-csrf-protection)) A same-origin BFF, as [Browser clients](#browser-clients-cookies-with-a-bff-holding-the-tokens) recommends, never sends that request from the browser.
