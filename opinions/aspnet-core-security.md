---
targets: [net11.0, csharp-15]
last-reviewed: 2026-11-14
sources:
  [
    andrew-lock,
    code-with-mukesh,
    damien-bowden,
    khalid,
    milan-jovanovic,
    ms-learn,
  ]
---

# ASP.NET Core security

This file covers security in the HTTP pipeline: authentication, authorization, browser sessions, tokens, the Data Protection key ring, CSRF, CORS, security headers, and open redirects.

- For hosting, request validation, error responses, and the rest of ASP.NET Core, see [aspnet-core.md](aspnet-core.md).
- For secrets, deserialization, cryptography, and servicing cadence, see [security.md](security.md).

## Opinions

### Authentication: passkeys first

**Sign users in with ASP.NET Core Identity's built-in passkey (WebAuthn and FIDO2) support, not passwords or a third-party FIDO library.** Since .NET 10, Identity and the Blazor Web App template include passkey management and sign-in. Passkeys resist phishing, leave no server-side secret to leak, and add no dependency. Don't offer a password as the backup method in a new app. Have users register more than one passkey, prompt for another when `UserPasskeyInfo.IsBackedUp` is false, and back them with an external provider or an email-based recovery flow. Identity's built-in recovery codes stand in for a second factor after a first sign-in, so they can't recover a passkey-only account. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [Microsoft Learn: Passkeys in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/passkeys/))

**Return 401 or 403 from API endpoints, never a sign-in redirect.** Since ASP.NET Core 10, the cookie handler returns the status code instead of redirecting when an endpoint has `IDisableCookieRedirectMetadata`. Microsoft Learn's What's new page still gives its pre-release name, `IApiEndpointMetadata`. Four kinds of endpoint get it automatically: `[ApiController]` actions, minimal APIs that read or write JSON, SignalR hubs, and endpoints whose declared return type is a status-code `TypedResults` type, such as `Ok<T>` or `NotFound<T>`. The declared type decides it: a handler declared to return `IResult` doesn't get it, even when it returns `TypedResults.Ok()`. The non-generic `NotFound` and `InternalServerError` are exceptions too: they don't add it. Call `.DisableCookieRedirect()` on any other API endpoint, such as one that returns a `string`, `IResult`, `Results.Text`, `TypedResults.Text`, or only `TypedResults.NotFound()`. The change covers only the cookie handler: in a custom handler, return 401 from the challenge action and 403 from the forbid action for API endpoints. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), [aspnet/Announcements#525: Cookie login redirects are disabled for known API endpoints](https://github.com/aspnet/Announcements/issues/525), [Microsoft Learn: Overview of ASP.NET Core Authentication](https://learn.microsoft.com/aspnet/core/security/authentication/), [Microsoft Learn: API endpoint authentication behavior](https://learn.microsoft.com/aspnet/core/security/authentication/api-endpoint-auth), [dotnet/aspnetcore: NotFound.cs](https://github.com/dotnet/aspnetcore/blob/5d3f090fddf40048816db21708a5764279996624/src/Http/Http.Results/src/NotFound.cs#L48), [dotnet/aspnetcore: CookieRedirectEndpointConventionBuilderExtensions.cs](https://github.com/dotnet/aspnetcore/blob/5d3f090fddf40048816db21708a5764279996624/src/Http/Http.Extensions/src/CookieRedirectEndpointConventionBuilderExtensions.cs#L24))

**Review by 2027-03-31.** Don't trust the `amr` claim after a passkey sign-in. .NET 10 sets it to `pwd` instead of `pop`, the proof-of-possession value that OpenID Connect EAP ACR Values 1.0 defines. Bowden reports that this "is not implemented correctly in ASP.NET Core (.NET 10)", and it's filed as [dotnet/aspnetcore#64881](https://github.com/dotnet/aspnetcore/issues/64881), which is still open. Authorization that reads `amr` for a step-up or multi-factor decision misjudges a passkey. Do one of these instead:

- Take the credential type from your own sign-in path.
- Reissue the claim. You can't change the existing claims collection, so sign the user out and back in with `SignInWithClaimsAsync()`.

Drop this note when a shipped release fixes the issue.

([Bowden: Set the amr claim when using passkeys authentication in ASP.NET Core](https://damienbod.com/2026/01/05/set-the-amr-claim-when-using-passkeys-authentication-in-asp-net-core/))

### Authorization by default

**Set a fallback policy that requires an authenticated user, and let endpoints opt out.** A new endpoint is then private until someone marks it public, the same default as [Request validation](aspnet-core.md#request-validation). "Having authorization required by default is more secure than relying on new controllers and Razor Pages to include the `[Authorize]` attribute". ([Microsoft Learn: Create an ASP.NET Core web app with user data protected by authorization](https://learn.microsoft.com/aspnet/core/security/authorization/secure-data))

```csharp
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy("Admin", policy => policy.RequireRole("admin"));

app.MapDelete("/orders/{id}", (int id) => TypedResults.NoContent())
    .RequireAuthorization("Admin");
```

Mark public endpoints with `[AllowAnonymous]`, or call `.AllowAnonymous()` on a minimal API endpoint or group. Give an endpoint that needs more than a signed-in user, such as a role or a claim, its own named policy. Register the policy with `AddPolicy`, and apply it with `[Authorize("<policy>")]` or `.RequireAuthorization("<policy>")`. ([Microsoft Learn: Policy-based authorization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authorization/policies))

### Browser clients: cookies, with a BFF holding the tokens

**Authenticate a browser app with a cookie. A SPA or Blazor WebAssembly client calls its APIs through a backend for frontend (BFF) that keeps the tokens on the server.** Microsoft Learn recommends cookies for browser apps "because, by default, the browser automatically handles them without exposing them to JavaScript", and reserves tokens for clients that can't use cookies. ([Microsoft Learn: Use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization))

The reason is XSS. A token in `localStorage` is readable by any script an attacker injects. ([Jovanović: JWT Authentication in ASP.NET Core](https://milanjovanovic.tech/blog/jwt-authentication-aspnetcore)) By contrast, "`HttpOnly` cookies cannot be read by JavaScript, so even a successful XSS attack cannot exfiltrate the user's access or refresh token". ([Abuhakmeh: The Cookie Apocalypse Already Happened](https://duendesoftware.com/blog/20260414-the-cookie-apocalypse-already-happened)) Bowden states the client-side rule: "No authentication security logic should be implemented in a client application running in the browser". ([Bowden: Implement BFF using Auth0, Angular and ASP.NET Core](https://damienbod.com/2026/08/10/implement-bff-using-auth0-angular-and-asp-net-core/))

Set it up in three parts:

- **The session cookie:** Use a `__Host-` prefixed cookie marked `HttpOnly` and `Secure`. Add `SameSite=Strict` where the flow allows it, because such cookies "are not sent on cross-site requests". This adds to the [CSRF defence](#csrf-defence-in-depth) below, and doesn't replace it. ([Abuhakmeh: The Cookie Apocalypse Already Happened](https://duendesoftware.com/blog/20260414-the-cookie-apocalypse-already-happened))
- **One origin:** Put a reverse proxy in front, so the front end and the API share an origin.
- **A separate downstream token:** The BFF gets its own token for the downstream API, instead of forwarding the browser's. In Bowden's design, "The downstream API does not accept the user delegated access tokens from the UI application". That token identifies the BFF, not the user: "The app-to-app security has nothing to do with the delegated client from the web application". The downstream API can't tell one user from another, so authorise the user in the BFF before it makes the call. ([Bowden: BFF secured ASP.NET Core application using downstream API and an OAuth client credentials JWT](https://damienbod.com/2024/04/08/bff-secured-asp-net-core-application-using-downstream-api-and-an-oauth-client-credentials-jwt/))

Blazor's documented BFF sample differs in two ways, so change both if you start from it. It proxies API calls "with the `access_token` stored in the authentication cookie", which keeps the tokens in the browser's cookie instead of on the server. It also forwards the user's access token to the API, where the BFF should send its own. ([Microsoft Learn: Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=bff-pattern))

**Keep session state on the server, and make the cookie a key to it.** Browsers cap a cookie at about 4 KB. Abuhakmeh's fix: "stop storing session data in the cookie entirely... The cookie becomes a thin key that points to the server state rather than containing that state itself". ([Abuhakmeh: ASP.NET Core Cookie Size Limits in Production](https://duendesoftware.com/blog/20260429-aspnet-core-cookie-size-limits))

### Tokens for APIs

**Get tokens from an identity provider, validate the issuer, audience, lifetime, and signing key, and keep permissions out of the token.** Hand identity to "a battle-tested identity provider" instead of issuing your own JWTs, which would make your service responsible for key management and revocation. ([Jovanović: Integrate Keycloak with ASP.NET Core Using OAuth 2.0](https://milanjovanovic.tech/blog/integrate-keycloak-with-aspnetcore-using-oauth-2))

- **Resolve permissions on the server** with `IClaimsTransformation`, and keep tokens small. A revoked permission in a token "keeps working until the token expires", so a request that should get a 403 still succeeds. ([Jovanović: Building Secure APIs with Role-Based Access Control in ASP.NET Core](https://milanjovanovic.tech/blog/building-secure-apis-with-role-based-access-control-in-aspnetcore), [Mukesh: Permission-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/permission-based-authorization-in-aspnet-core/))
- **Keep access-token lifetimes short,** because you can't revoke a JWT after you issue it. ([Mukesh: JWT Authentication in ASP.NET Core](https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/))
- **Rotate refresh tokens on every use:** issue a new one, and revoke the old one. ([Jovanović: JWT Authentication in ASP.NET Core](https://milanjovanovic.tech/blog/jwt-authentication-aspnetcore))

**Bind each token to its client, and authenticate the client with a key instead of a secret.** Anyone who holds a bearer token can use it. Use the three OAuth extensions below. Each one works only if your identity provider supports it, so check which of them it implements before you design around them:

- **DPoP:** OAuth DPoP binds the token to a key that the client proves it holds. "The access tokens should only be used for what the access tokens are intended for. OAuth DPoP helps force this." ([Bowden: Securing APIs using ASP.NET Core and OAuth 2.0 DPoP](https://damienbod.com/2023/08/14/securing-apis-using-asp-net-core-and-oauth-2-0-dpop/))
- **Pushed authorization requests (PAR):** Set `PushedAuthorizationBehavior.Require`, available since .NET 9, to move the authorization request parameters out of the browser redirect. ([Bowden: Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/))
- **Client assertions:** Authenticate a confidential client with a private-key JWT assertion. It replaces the shared secret, the one long-lived credential the client would otherwise keep in configuration. ([Bowden: Use client assertions in ASP.NET Core using OpenID Connect, OAuth DPoP and OAuth PAR](https://damienbod.com/2026/02/02/use-client-assertions-in-asp-net-core-using-openid-connect-oauth-dpop-and-oauth-par/))

### The Data Protection key ring outlives the container

**If you run more than one instance, or a container that restarts, persist the key ring to shared storage and protect it at rest. Configure both explicitly.** The ring encrypts authentication cookies and antiforgery tokens. If it doesn't survive a restart, every user is signed out. If replicas don't share it, they can't read each other's cookies. In a container, keep the keys in a Docker volume that outlives the container, or in an external store such as Azure Blob Storage. ([Microsoft Learn: Configure ASP.NET Core Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview))

```csharp
// Azure.Extensions.AspNetCore.DataProtection.Blobs and .Keys; both extension
// methods live in the Microsoft.AspNetCore.DataProtection namespace.
builder.Services.AddDataProtection()
    .PersistKeysToAzureBlobStorage(blobUri, credential)
    .ProtectKeysWithAzureKeyVault(keyIdentifier, credential);
```

Make both calls. When you name a location, "the data protection system deregisters the default key encryption at rest mechanism, so keys are no longer encrypted at rest". Don't use Redis unless you've turned on its persistence. It "doesn't persist data by default when restarting", so a restart gives the app new keys and invalidates everything the old ones protected. ([Microsoft Learn: Key storage providers in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/data-protection/implementation/key-storage-providers))

Encryption at rest "doesn't prevent cyberattackers from creating new keys", so lock down the store's permissions as carefully as you encrypt it. ([Microsoft Learn: Configure ASP.NET Core Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview))

**Give each app its own key store, and call `SetApplicationName` with a fixed name.** On a shared store, the application discriminator keeps each app's payloads apart, but it isn't a security boundary: "a compromise of one is functionally equivalent to a compromise of both". It defaults to the content root path, and in a container "the app always has the same path and, therefore, the same application discriminator", so the payloads aren't even kept apart. To share sign-in cookies between apps behind one proxy, point them at the same store and give each the same `SetApplicationName` value. ([Microsoft Learn: Configure ASP.NET Core Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview))

### CSRF defence in depth

**Leave the automatic CSRF middleware on, and keep `UseAntiforgery()` in an app that renders forms with antiforgery tokens.** In .NET 11, `WebApplication.CreateBuilder` adds a middleware that reads the `Sec-Fetch-Site` and `Origin` headers, which the browser sets and page script can't forge. It records a verdict on each request, and "Components that process submitted form data enforce that verdict, rejecting cross-origin form posts that aren't explicitly trusted." The two protections "coexist and can both be active on the same endpoint". Razor Pages, MVC, and Blazor SSR apps that use tokens "gain a header-based check that runs before token validation, without changing the token flow", and when both run, "The token result is authoritative". ([Microsoft Learn: Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-11.0#automatic-csrf-protection-in-aspnet-core), [Lock: Understanding the Fetch Metadata headers](https://andrewlock.net/understanding-the-fetch-metadata-http-headers-sec-fetch-site-and-friends/))

Know what the middleware doesn't cover:

- **Only unsafe methods are checked.** `GET`, `HEAD`, `OPTIONS`, and `TRACE` pass, so never change state on a `GET`.
- **Only an endpoint that reads a form enforces the verdict:** an MVC action protected by antiforgery, a minimal API that binds a form parameter, or a Blazor SSR form. An endpoint that binds its body from JSON "isn't rejected automatically on a cross-origin request".
- **A request with no browser signal passes.** One with neither `Sec-Fetch-Site` nor `Origin` is "almost certainly a non-browser client". The middleware is a CSRF defence, and it doesn't authenticate an API's callers.
- **A cross-origin form post needs a CORS policy that names its origin and calls `.AllowCredentials()`.** "AllowAnyOrigin is intentionally not honored as a CSRF trust signal", and "A policy that lists specific origins with WithOrigins but omits .AllowCredentials() isn't honored for CSRF trust either."

([Microsoft Learn: Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-11.0#automatic-csrf-protection-in-aspnet-core))

**Opt an endpoint out only when a browser can't reach it or it doesn't use cookies, and never turn the middleware off for the whole app.** Call `.DisableAntiforgery()` on a minimal API, or put `[IgnoreAntiforgeryToken]` on an MVC action or controller. That fits an endpoint that "isn't browser-reachable or is secured by a non-cookie mechanism, such as a bearer token or API key". The `DisableCsrfProtection` configuration key removes the middleware everywhere, and Microsoft Learn says to "prefer per-endpoint opt-outs". ([Microsoft Learn: Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-11.0#opting-an-endpoint-out))

**On .NET 11, a minimal API that binds a form or a form file doesn't need `UseAntiforgery()`.** On .NET 8 to 10, "Minimal API endpoints that bind a parameter from the form via IFormFile or IFormFileCollection require anti-forgery validation", and the endpoint throws on every request until you register the token middleware. Now "The automatic CSRF middleware also satisfies the antiforgery requirement for endpoints that require validation, even when an app doesn't call app.UseAntiforgery()". Two cases still need `builder.Services.AddAntiforgery()` and `app.UseAntiforgery()`: a host that isn't built with `WebApplication`, where the middleware isn't added, and an app that sets `DisableCsrfProtection`. Without either middleware, the request throws. ([Microsoft Learn: Minimal APIs antiforgery checks](https://learn.microsoft.com/aspnet/core/breaking-changes/8/antiforgery-checks), [Microsoft Learn: Prevent Cross-Site Request Forgery (XSRF/CSRF) attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-11.0#disabling-globally))

**If your identity provider runs on .NET 11, check its cross-origin endpoints.** A SPA exchanges its authorization code by posting to the provider's token endpoint from its own origin, and the provider's check rejects that request: "It's cross-origin. It's a POST. It will be rejected." ([Abuhakmeh: Understanding .NET 11 Automatic CSRF Protection](https://duendesoftware.com/blog/20260901-understanding-dotnet-11-automatic-csrf-protection)) A same-origin BFF, as [Browser clients](#browser-clients-cookies-with-a-bff-holding-the-tokens) recommends, makes the token request from the server, so the browser never sends it.

**Review by 2027-02-28.** A second case is still open. With `response_mode=form_post`, the provider posts back to your app's `/signin-oidc` from its own domain, and it's not yet known whether the check lets that through. ([Abuhakmeh: Understanding .NET 11 Automatic CSRF Protection](https://duendesoftware.com/blog/20260901-understanding-dotnet-11-automatic-csrf-protection)) When it's known, state the answer, and drop this note.

### CORS

**Name every CORS policy, and never combine a wildcard origin with credentials.** Microsoft Learn calls two combinations insecure: "Specifying AllowAnyOrigin and AllowCredentials", and "Bypassing the built-in checks by using SetIsOriginAllowed(\_ => true) together with AllowCredentials". ([Microsoft Learn: Enable Cross-Origin Requests (CORS) in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cors))

Apply a named policy to each endpoint or group with `[EnableCors("<policy>")]` or `.RequireCors("<policy>")`, instead of one permissive default. Otherwise, a policy you widen for one endpoint widens what the whole app accepts. A policy that calls `.AllowCredentials()` also decides which cross-origin requests the CSRF middleware trusts: the ones from its allowed origins. For details, see [CSRF defence in depth](#csrf-defence-in-depth).

### Security headers and output encoding

**Send a baseline of security headers with `NetEscapades.AspNetCore.SecurityHeaders`, and give the Content Security Policy (CSP) a nonce instead of `unsafe-inline`.** Bowden's baseline is:

- `X-Frame-Options: DENY`
- `X-Content-Type-Options: nosniff`
- `Referrer-Policy: strict-origin-when-cross-origin`
- The three `Cross-Origin-*` policies
- `Permissions-Policy`
- HSTS outside development

For an interactive Blazor circuit, prefer the nonce, and fall back to hashes.

([Bowden: Implement a secure Blazor Web application using OpenID Connect and security headers](https://damienbod.com/2024/04/15/implement-a-secure-blazor-web-application-using-openid-connect-and-security-headers/), [Bowden: Revisiting using a Content Security Policy (CSP) nonce in Blazor](https://damienbod.com/2025/05/26/revisiting-using-a-content-security-policy-csp-nonce-in-blazor/), [NetEscapades.AspNetCore.SecurityHeaders: README](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders#readme))

```csharp
app.UseSecurityHeaders(new HeaderPolicyCollection()
    .AddDefaultSecurityHeaders()
    .AddCrossOriginOpenerPolicy(policy => policy.SameOrigin())
    .AddPermissionsPolicy(policy =>
    {
        policy.AddDefaultSecureDirectives();
        // The secure defaults block WebAuthn, which passkeys need.
        policy.AddPublickeyCredentialsGet().Self();
        policy.AddPublickeyCredentialsCreate().Self();
    })
    .AddContentSecurityPolicy(csp =>
    {
        csp.AddDefaultSrc().Self();
        csp.AddScriptSrc().Self().WithNonce();
        csp.AddObjectSrc().None();
        csp.AddFormAction().Self();
        csp.AddFrameAncestors().None();
    }));
```

Your `AddContentSecurityPolicy` call replaces the policy that `AddDefaultSecurityHeaders()` sets, so restate its `form-action 'self'`. `default-src` doesn't cover where a form submits. `AddDefaultSecurityHeaders()` doesn't send `Permissions-Policy`, so add it, and allow the two `publickey-credentials` features that the secure defaults turn off. In a WebAssembly render mode, add `.WasmUnsafeEval()` to `script-src`, or the browser won't run the .NET runtime. Blazor's `ImportMap` renders an inline script, so give it the request's nonce. ([NetEscapades.AspNetCore.SecurityHeaders: README](https://github.com/andrewlock/NetEscapades.AspNetCore.SecurityHeaders#readme), [Microsoft Learn: Enforce a Content Security Policy for ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/security/content-security-policy), [Bowden: Revisiting using a Content Security Policy (CSP) nonce in Blazor](https://damienbod.com/2025/05/26/revisiting-using-a-content-security-policy-csp-nonce-in-blazor/))

```razor
<ImportMap AdditionalAttributes="@(new Dictionary<string, object> { ["nonce"] = HttpContext?.GetNonce() ?? "" })" />
```

Razor encodes output "sourced from variables, unless you work to prevent this behavior". Never pass untrusted input to `HtmlString`, because that "exposes an XSS vulnerability". ([Microsoft Learn: Prevent Cross-Site Scripting (XSS) in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cross-site-scripting))

Blazor Web Apps send `frame-ancestors 'self'` without extra code. On .NET 8 to 10, a CSP without `unsafe-inline` styles breaks the `Virtualize` component. .NET 11 fixes it: the component no longer renders inline `style` attributes on its spacer and placeholder elements. ([Microsoft Learn: Enforce a Content Security Policy for ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/security/content-security-policy), [Microsoft Learn: What's new in ASP.NET Core in .NET 11](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-11))

If client-side script writes to the DOM, enable Trusted Types, which moves the check into the browser: "APIs like `innerHTML` must be passed a `TrustedHTML` object". ([Lock: Preventing client-side cross-site-scripting vulnerabilities with Trusted Types](https://andrewlock.net/preventing-client-side-cross-site-scripting-vulnerabilities-with-trusted-types/))

### Open redirects

**Before you redirect to a URL from the request, check that it's local.** On .NET 10, `RedirectHttpResult.IsLocalUrl` works outside MVC. `LocalRedirect` "throws an exception if a non-local URL is specified". ([Microsoft Learn: Prevent open redirect attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/preventing-open-redirects))

```csharp
app.MapGet("/redirect", (string url) =>
    RedirectHttpResult.IsLocalUrl(url)
        ? Results.LocalRedirect(url)
        : Results.BadRequest());
```
