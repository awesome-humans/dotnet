---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-12
sources:
  [
    ms-learn,
    dotnet-blog,
    andrew-lock,
    damien-bowden,
    meziantou,
    steve-gordon,
    milan-jovanovic,
    code-with-mukesh,
    khalid,
  ]
---

# Security

What this repository should say about security on .NET 10. No opinion file covers security. It's split between [aspnet-core.md](../opinions/aspnet-core.md), which covers passkeys, API challenge responses, rate limiting, CSRF, and the reverse proxy, and [ci.md](../opinions/ci.md), which covers SHA pinning, script injection, token permissions, pinned builds, signing, and Renovate. This topic checks both against the roster and maps the ground between them.

**This is the second pass.** The first, on 2026-09-11, ended by nominating seven sources, because the ground it couldn't cover was mostly application security. The `vet-source` pass of 2026-09-12 (#67) settled them: `damien-bowden` admitted, unmarked; `khalid` widened to his Duende byline; and `barry-dorrans`, `duende-blog`, and `scott-brady` watch-listed. This pass sweeps what became citable.

The first pass's three findings stand:

1. **The house warnings rule now fails restore on every new advisory.** For `net10.0` projects, NuGet Audit checks transitive packages by default, and [templates/Directory.Build.props](../templates/Directory.Build.props) sets `TreatWarningsAsErrors` unconditionally. Reproduced on SDK 10.0.400 with a fresh project: `error NU1903: Warning As Error: Package 'Newtonsoft.Json' 12.0.1 has a known high severity vulnerability`, and restore exits with code 1. An advisory against anything in the graph turns a passing commit red with no change to the repository. Keep that behaviour, and document it ([NuGet Audit under the house warnings rule](#nuget-audit-under-the-house-warnings-rule)).
2. **ci.md's publishing advice is out of date.** It offers "trusted publishing or a scoped API key stored as a repository secret" as equals. Trusted publishing has been GA since 2025-09-22. From 2026-08-17, nuget.org caps new API keys at 30 days, and every older key expires on 2026-11-01. The opinion should name trusted publishing, with a key as the fallback ([Publishing credentials](#publishing-credentials)).
3. **The container hosting opinion leads straight into a Data Protection problem.** aspnet-core.md says to ship a container image, and says nothing about where the keys that protect authentication cookies and antiforgery tokens live. `ms-learn` says a container must keep them on a persistent volume or in an external store, and documents a trap: choosing an explicit location turns off encryption at rest without warning ([Data Protection in containers](#data-protection-in-containers)).

This pass changes:

- **Browser authentication and BFF are fully sourced.** `damien-bowden` is the unmarked depth source the first pass lacked, and `khalid` adds the .NET 11 consequence ([Browser clients](#browser-clients-cookies-not-tokens)).
- **Security headers have a baseline.** `damien-bowden` publishes a concrete header set and a CSP nonce approach for Blazor, and independently recommends the library the first pass could reach only through its maintainer ([Output encoding, CSP, and security headers](#output-encoding-csp-and-security-headers)).
- **The passkey `amr` defect is citable and confirmed open,** with a filed ASP.NET Core issue, and it affects an opinion this repository publishes ([section 1](#1-what-the-repository-already-says-checked)).
- **Password-based key derivation has a current source,** replacing a Learn page reviewed in 2021, though not a current iteration count ([Cryptography on GA APIs](#cryptography-on-ga-apis)).
- **Token binding is sourced, but not an opinion yet.** DPoP, PAR, and client assertions come only from `damien-bowden`, with nothing on the roster supporting or contesting them ([Tokens for APIs](#tokens-for-apis)).
- **A correction.** The first pass ranked Barry Dorrans the strongest candidate. idunno.org has been a blog only since 2025-11-18, so the strongest name it nominated is the one that can't be cited ([item 9](#what-this-reveals-about-the-repository)).
- **Still blocked:** API-key handling, and which analyzer security rules to enable.

## Sources swept

| id                 | Standing                            | Used for                                                                                                                                                                                                                                                                                                                                                         |
| ------------------ | ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ms-learn`         | Citable                             | ASP.NET Core security pages, NuGet auditing and source mapping, cryptography, containers, and servicing. Each page's review date is its own `ms.date`, listed under [Freshness](#freshness)                                                                                                                                                                      |
| `dotnet-blog`      | Citable                             | .NET 10 post-quantum status, NuGet supply-chain posts, BinaryFormatter removal, the support lifecycle, and .NET 11 previews. The NuGet blog now redirects to this blog's NuGet category, so none of it is unvetted                                                                                                                                               |
| `andrew-lock`      | Citable                             | CSRF and Fetch Metadata, CVE-2025-55315, trusted publishing, SBOM and provenance attestations, and Trusted Types                                                                                                                                                                                                                                                 |
| `damien-bowden`    | Citable, admitted 2026-09-12        | **New this pass.** BFF and browser authentication, the passkey `amr` defect, the security-header baseline and CSP nonces, client assertions, DPoP and PAR, and password hashing. His notes record two conflicts: he maintains `angular-auth-oidc-client` and sells application-security workshops, so every sample cited here was checked for what it configures |
| `meziantou`        | Citable                             | NuGet auditing, source mapping, lock files, and deserialization. Most of the security catalogue is from 2019 and 2020, before .NET 8                                                                                                                                                                                                                             |
| `steve-gordon`     | Citable                             | AES-GCM nonce handling (2026). The rest of his security tag is from 2016                                                                                                                                                                                                                                                                                         |
| `milan-jovanovic`  | Citable, conflict of interest noted | Token storage, JWT validation, permissions, and identity providers                                                                                                                                                                                                                                                                                               |
| `code-with-mukesh` | Citable, **Corroborate.**           | Token lifetime, API keys, rate limiting across replicas, and permission revocation                                                                                                                                                                                                                                                                               |
| `khalid`           | Citable, scope widened 2026-09-12   | **Scope widened this pass,** from one 2022 post to his Duende byline: .NET 11 CSRF and OpenID Connect flows, cookie properties, and Data Protection in distributed deployments. His Duende posts are cited for mechanism, never alone on adopting a Duende product                                                                                               |
| `aspnet-blog`      | Citable                             | Swept with `dotnet-blog`. No BFF or Data Protection post from 2024 to 2026 (negative result)                                                                                                                                                                                                                                                                     |
| `jetbrains-dotnet` | Citable                             | Discovery only: the September 2026 roundup linked to Lock, Khalid, and Bowden. No original security writing (negative result)                                                                                                                                                                                                                                    |
| `jon-skeet`        | Citable                             | Nothing on this ground (negative result)                                                                                                                                                                                                                                                                                                                         |
| `barry-dorrans`    | Watch list, 2026-09-12              | Not citable. SBOM, package signing, SSRF, and CVE triage, held for discovery and cross-checking until the blog turns two                                                                                                                                                                                                                                         |
| `duende-blog`      | Watch list, 2026-09-12              | Not citable. Khalid's byline reaches this repository through his own ID                                                                                                                                                                                                                                                                                          |
| `scott-brady`      | Watch list, 2026-09-12              | Not citable: dormant                                                                                                                                                                                                                                                                                                                                             |
| `chris-sainty`     | Watch list                          | Blazor auth series, 2018 to 2023. Nothing new, not cited                                                                                                                                                                                                                                                                                                         |

One unvetted source still carries a claim, flagged where it appears: the [OWASP Cheat Sheet Series](https://cheatsheetseries.owasp.org/), which the `vet-source` pass didn't take up. Philippe De Ryck, checked in the first pass, is out of scope: no .NET-specific writing, and nothing since 2022.

## 1. What the repository already says, checked

**Passkeys first holds, and the caveat the first pass could only report as rumour is now citable.** `damien-bowden` reports that a passkey sign-in with ASP.NET Core Identity doesn't set the `amr` claim to `pop`, the RFC 8176 value for proof of possession, and calls it a framework defect: "At present, this is not implemented correctly in ASP.NET Core (.NET 10)." ([Bowden: Set the amr claim when using passkeys authentication in ASP.NET Core](https://damienbod.com/2026/01/05/set-the-amr-claim-when-using-passkeys-authentication-in-asp-net-core/), 2026-01-05). It's filed as [dotnet/aspnetcore#64881](https://github.com/dotnet/aspnetcore/issues/64881), "The amr claim is set incorrectly when authenticating using passkeys", opened 2025-12-28 and still open on 2026-09-12. His workaround signs out to clear the cookie, then signs in again through `SignInWithClaimsAsync()` with the corrected claim, because the claims collection can't be changed in place. The editor hasn't reproduced the behaviour. What's verified is the post, the issue, and that the issue is open.

**aspnet-core.md's passkeys opinion should carry this caveat.** Anything that reads `amr` to decide whether a sign-in met a step-up or multi-factor bar misjudges a passkey until the fix ships. The opinion still stands: the passkey is what it claims to be, and the defect is in how the sign-in is described afterwards.

**Rate limiting holds,** and `code-with-mukesh` corroborates the per-node framing: "If your deployment has more than one replica, your in-memory rate limiter is not enforcing what you think" ([Mukesh: Rate Limiting in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/rate-limiting-aspnet-core/), 2026-05-21). Partition keys "must be validated and bounded", because every distinct key allocates a limiter. `milan-jovanovic`'s older advice that "Rate limiting by IP address can be a good layer of security for unauthenticated users" ([Jovanović: Advanced Rate Limiting Use Cases In .NET](https://milanjovanovic.tech/blog/advanced-rate-limiting-use-cases-in-dotnet), 2023-08-19) seems to contradict the opinion's "Never partition by a value the client controls, such as a spoofable IP header", but they're compatible. On the editor's reading, the client doesn't control the connection's remote address, but does control a forwarded header, and the forwarded-headers middleware the hosting opinion requires decides which one `RemoteIpAddress` reports. The opinion should say so.

**The CSRF opinion and its "Coming next" note are accurate,** and Lock's follow-up sharpens the note. The .NET 11 middleware is on by default for apps built with `WebApplicationBuilder`. Minimal APIs and Blazor SSR can drop `UseAntiforgery()` and rely on it, but MVC and Razor Pages keep token validation: "There's basically no simple way today to opt into _only_ using the new approach." It rejects a cross-origin request unless the origin is in the CORS allowed-origins list, and allows a request with no browser signals through. ([Lock: Automatic CSRF protection based on Fetch Metadata headers](https://andrewlock.net/exploring-the-dotnet-11-preview-6-automatic-csrf-protection-based-on-fetch-metadata-http-headers/), 2026-08-04). .NET 11 reached RC 1 on 2026-09-08, with GA expected in November 2026 ([Microsoft Learn: What's new in .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/overview), reviewed 2026-09-08), so the note stays a preview for now. In .NET 11, the CORS policy becomes part of CSRF configuration, which [CORS](#cors) covers.

**`khalid` shows what that change does to an identity flow, the strongest new argument in this pass.** A SPA that exchanges an authorization code at the token endpoint makes a cross-origin POST: "It's cross-origin. It's a POST. It will be rejected." He flags an open question too: a provider callback with `response_mode=form_post` is "a cross-origin form POST from the provider's domain", and whether the middleware accepts it isn't settled. ([Abuhakmeh: Understanding .NET 11 Automatic CSRF Protection](https://duendesoftware.com/blog/20260901-understanding-dotnet-11-automatic-csrf-protection), 2026-09-01). His fix is Duende BFF, a product recommendation his roster notes say never to rely on alone. This repository takes the mechanism: a same-origin backend "sidesteps most of these issues".

**A .NET 8 rule is missing beside the CSRF opinion.** Since ASP.NET Core 8, "Minimal API endpoints that bind a parameter from the form via IFormFile or IFormFileCollection require anti-forgery validation" ([Microsoft Learn: Minimal APIs antiforgery checks](https://learn.microsoft.com/aspnet/core/breaking-changes/8/antiforgery-checks), reviewed 2023-12-05).

**The hosting opinion says "The base image stays patched with the SDK".** That's true only at build time. An image nobody rebuilds keeps the runtime it was built with. [Servicing](#3-servicing) covers the cadence.

**ci.md's pinning, injection, and permissions opinions** drew no new evidence. Its signing section is the second finding above.

## 2. Supply chain

### NuGet Audit under the house warnings rule

From `ms-learn`: audit runs during restore, on by default, and "`NuGetAuditMode` defaults to `all` when a project targets `net10.0` or higher. Otherwise `NuGetAuditMode` defaults to `direct`." A multi-targeted project uses `all` if any of its frameworks selects it ([Microsoft Learn: Auditing package dependencies for security vulnerabilities](https://learn.microsoft.com/nuget/concepts/auditing-packages), reviewed 2025-10-01). Severities map to NU1901 (low) through NU1904 (critical). A .NET 9 preview briefly shipped `all`, and the 9.0.101 SDK reverted it ([Microsoft Learn: NuGetAudit audits transitive packages](https://learn.microsoft.com/dotnet/core/compatibility/sdk/10.0/nugetaudit-transitive-packages), reviewed 2025-03-28).

`dotnet-blog` measures the noise. .NET 10 prunes platform-provided packages from the graph before auditing it, and "Projects with these defaults have 70% fewer transitive vulnerability reports compared to projects using the previous defaults" ([Kolev: NuGet Package Pruning: Cleaner Dependencies and Actionable Vulnerability Reports](https://devblogs.microsoft.com/dotnet/nuget-package-pruning-in-dotnet-10/), 2026-05-18). `meziantou` covered switching audit on while it was opt-in ([Meziantou: Enable NuGet auditing for your .NET projects](https://www.meziantou.net/enable-nuget-auditing-for-your-dotnet-projects.htm), 2024-07-15), which .NET 10 makes unnecessary.

The interaction with this repository is documented: "If these warnings are causing restore to fail because you are using `TreatWarningsAsErrors`, you can add `<WarningsNotAsErrors>NU1901;NU1902;NU1903;NU1904</WarningsNotAsErrors>` to allow these codes to remain as warnings." ([Microsoft Learn: NuGet Warnings NU1901, NU1902, NU1903, NU1904](https://learn.microsoft.com/nuget/reference/errors-and-warnings/nu1901-nu1904), reviewed 2024-07-19). The reproduction above confirms it on the SDK this repository pins.

**Recommendation: keep NU1901 to NU1904 as errors, and say so in the template.** The **House:** warnings rule makes a problem fail where it's introduced, and a known vulnerability in the shipped graph is such a problem. The opt-out already meets the house rule's requirement for every suppression: `NuGetAuditSuppress` takes an advisory URL ([Microsoft Learn: Excluding advisories](https://learn.microsoft.com/nuget/concepts/auditing-packages#excluding-advisories)), and an entry with its reason in a comment is what the rule asks of `#pragma` and `<NoWarn>`. Document two consequences, because they surprise people:

- An unchanged release branch stops restoring the day an advisory lands against its graph. That's intended, but servicing a release branch now starts with triage.
- `WarningsNotAsErrors` is the one-line way out, and it switches off the house rule for exactly the warnings where failing loudly matters most.

`ms-learn` offers one more option, which the house rule rejects: "If you would like to run NuGet Audit on developer machines, but disable it on CI pipelines, you can take advantage of MSBuild importing environment variables, and create a NuGetAudit environment variable set to `false` in your pipeline definition." That contradicts ci.md's own argument that a warning that fails in only one place reaches a pull request before anyone sees it.

### Publishing credentials

Two `dotnet-blog` posts overtake ci.md's "trusted publishing or a scoped API key stored as a repository secret":

- **Trusted publishing is GA.** A workflow uses "a short-lived GitHub OIDC token to request a temporary, single-use NuGet API key", and "These keys expire quickly (≈1 hour)" ([Tvorun and Iyer: New Trusted Publishing enhances security on NuGet.org](https://devblogs.microsoft.com/dotnet/enhanced-security-is-here-with-the-new-trust-publishing-on-nuget-org/), 2025-09-22).
- **Long-lived keys are going away.** "Starting August 17, 2026, new API keys will be limited to 30 days. All existing API keys created before that date will expire on November 1, 2026." The post strongly recommends moving to trusted publishing ([.NET Team: Strengthening NuGet Supply Chain Security: Reducing API Key Lifetime](https://devblogs.microsoft.com/dotnet/strengthening-nuget-supply-chain-security-reducing-api-key-lifetime/), 2026-08-03).

`andrew-lock` walks through the setup, and gives the reason: "managing the lifecycle of long-lived secrets is notoriously difficult" ([Lock: Publishing NuGet packages from GitHub actions the easy way with Trusted Publishing](https://andrewlock.net/easily-publishing-nuget-packages-from-github-actions-with-trusted-publishing/), 2025-09-30). The `ms-learn` page has two review dates, 2025-07-01 and 2026-07-29 ([Microsoft Learn: Trusted Publishing](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)).

**Recommendation: trusted publishing is the opinion.** An API key is the fallback for a host that can't present an OIDC token, and it now expires in 30 days, so it needs an owner for rotation. The ci.md sentence cites `meziantou` and `ms-learn` today. A rewrite would cite `dotnet-blog` and `andrew-lock`, which adds to the file's `sources:`.

### Package source mapping and lock files

**Source mapping prevents dependency confusion.** `meziantou` describes the attack: "If you have a package published on an internal feed... with the same name as a public package, the public package can be used instead." His fix: "You can use package source mapping to prevent this issue." ([Meziantou: NuGet Packages: security risks and best practices](https://www.meziantou.net/nuget-packages-security-risks-and-best-practices.htm), 2024-12-02). `ms-learn` agrees, and scopes it: mapping "can be used to improve your supply chain security, especially if you use a mix of public and private package sources", set up in one root `nuget.config` with `<clear/>` and a pattern that covers every package ([Microsoft Learn: Package Source Mapping](https://learn.microsoft.com/nuget/consume-packages/package-source-mapping), reviewed 2026-07-16). It names a limit: mapping filters restore, but not `dotnet package add` or the `--vulnerable` listing. The templates have no `nuget.config`, which is where this would go.

**The sources describe lock files three ways.** The template comment presents `packages.lock.json` as optional protection "against feed drift". `ms-learn` presents it as repeatability for CI. `meziantou` presents it as integrity: "The lock file contains the package hash. If a corrupted or malicious package is downloaded, NuGet detects it and fails." ([Meziantou: Faster and Safer NuGet restore using Source Mapping and Lock files](https://www.meziantou.net/faster-and-safer-nuget-restore-using-source-mapping-and-lock-files.htm), 2022-08-01). His claim about the mechanism is correct: CPM pins a version, and only the lock file pins content.

What the hash protects against depends on the feeds. On the editor's reading, not a sourced claim: nobody can overwrite a version published to nuget.org, so on nuget.org alone the hash adds little, but behind a mirror or private feed it's the only check on content. So source mapping and lock files are one decision for multi-feed repositories, and not urgent for single-feed ones.

### SBOM and provenance

`andrew-lock` is still the only citable source, in a three-post series: an SBOM for a NuGet package ([Lock: Creating a software bill of materials (SBOM) for an open-source NuGet package](https://andrewlock.net/creating-a-software-bill-of-materials-sbom-for-an-open-source-nuget-package/), 2025-03-25), provenance attestations ([Lock: Creating provenance attestations for NuGet packages in GitHub Actions](https://andrewlock.net/creating-provenance-attestations-for-nuget-packages-in-github-actions/), 2025-03-18), and signed SBOM attestations with `actions/attest-sbom` ([Lock: Creating SBOM attestations in GitHub Actions](https://andrewlock.net/creating-sbom-attestations-in-github-actions/), 2025-04-01). `ms-learn` has no page on generating an SBOM for .NET builds, and `damien-bowden` doesn't cover supply chain (both negative results).

**The first pass expected `barry-dorrans` to fill this gap, and the watch-listing keeps it open.** His SBOM and package-signing posts are the second source this section needs, available for cross-checking but not citation until the blog turns two. One citable author is thin for an opinion, and nothing swept says who consumes the SBOM.

### An SDK that reports its own vulnerabilities, documented but not yet shipped here

`ms-learn` documents a property that would complement the pinned `global.json`, as Renovate complements pinned action SHAs: "To detect builds running on an unsupported SDK, set the `CheckSdkVulnerabilities` MSBuild property to `true` in your project. The build then emits warning NETSDK1239 when the resolved .NET SDK is end of life." ([Microsoft Learn: .NET releases, patches, and support](https://learn.microsoft.com/dotnet/core/releases-and-support), reviewed 2026-05-15). A search summary reports that it also enables NETSDK1238, for an SDK with known vulnerabilities, and NETSDK1240, for a discontinued feature band. The editor hasn't seen that on a Learn page.

**The SDK this repository pins doesn't have it.** The installed 10.0.400 SDK has no reference to `CheckSdkVulnerabilities` or any of the three codes, and the Learn page doesn't say which band adds it. Nothing to act on until it ships.

### Security analyzers

`ms-learn` documents `AnalysisModeSecurity`, which sets the analysis mode for the Security category alone and otherwise inherits `AnalysisMode` ([Microsoft Learn: MSBuild reference for .NET SDK projects](https://learn.microsoft.com/dotnet/core/project-sdk/msbuild-props), reviewed 2026-03-27). The template's `latest-recommended` level enables some security rules, but the docs don't list which, and point to the SDK's `analysislevel_*_recommended.globalconfig` instead. No source in either pass recommends a setting, so this is mechanism, not a pending opinion.

## 3. Servicing

Security fixes ship on Patch Tuesday, "always the second Tuesday of the month", and `ms-learn` says to "Regularly install servicing updates to ensure that your apps are in a secure and supported state" ([Microsoft Learn: .NET releases, patches, and support](https://learn.microsoft.com/dotnet/core/releases-and-support), reviewed 2026-05-15). After STS support grew to 24 months, ".NET 8 and .NET 9 will reach end of support on the same day – November 10, 2026" ([Damkewala: .NET STS releases supported for 24 months](https://devblogs.microsoft.com/dotnet/dotnet-sts-releases-supported-for-24-months/), 2025-09-16). The repository targets `net10.0`, supported to November 2028, so the date matters to readers with older projects, and to [verify-project](../skills/verify-project/SKILL.md), for which a target below `net10.0` is out of support in two months.

`andrew-lock`'s write-up of CVE-2025-55315 makes the case for cadence. It's an ASP.NET Core request-smuggling flaw that "received Microsoft's highest-ever CVSS score of 9.9", and exploiting it could let an attacker "Login as a different user", "Make an internal request (SSRF)", or "Bypass CSRF checks" ([Lock: Understanding the worst .NET vulnerability ever: request smuggling and CVE-2025-55315](https://andrewlock.net/understanding-the-worst-dotnet-vulnerability-request-smuggling-and-cve-2025-55315/), 2025-10-28).

**Recommendation: tie the hosting opinion to the calendar.** A container image is patched only when it's rebuilt from a patched base, so rebuild and redeploy after every Patch Tuesday, whether or not the code changed. That's the editor's combination of the `ms-learn` servicing rule with the hosting opinion, not a source's prescription, so `resolve-research` should look for one that states it.

## 4. Application security

### Authorization by default

`ms-learn` recommends a fallback policy that requires an authenticated user: "Setting the fallback authorization policy to require users to be authenticated protects newly added Razor Pages and controllers. Having authorization required by default is more secure than relying on new controllers and Razor Pages to include the `[Authorize]` attribute." ([Microsoft Learn: Create an ASP.NET Core web app with user data protected by authorization](https://learn.microsoft.com/aspnet/core/security/authorization/secure-data), reviewed 2026-05-11). Endpoints then opt out with `[AllowAnonymous]`, or name a stricter policy.

```csharp
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());
```

It matches aspnet-core.md's validation rule: secure by default, with per-endpoint opt-outs. `andrew-lock` covered the `DefaultPolicy` and `FallbackPolicy` split for ASP.NET Core 3.x, which is too old to cite for current behaviour. This is the topic's strongest candidate opinion: one source, but uncontested and mechanical.

### Browser clients: cookies, not tokens

**This section changed most in this pass.** The first pass had `ms-learn` and `milan-jovanovic` for the rule and nothing citable for depth. `damien-bowden` supplies the depth, and states the principle more plainly than either: "No authentication security logic should be implemented in a client application running in the browser", with "No JWT shared in the public (accessible from JS)" ([Bowden: Implement BFF using Auth0, Angular and ASP.NET Core](https://damienbod.com/2026/08/10/implement-bff-using-auth0-angular-and-asp-net-core/), 2026-08-10). His roster notes say to check what a sample configures. This one is Angular against Auth0, and this topic takes only the cookie-and-proxy arrangement, which doesn't depend on the front end or the provider.

He shows the mechanism in code. The session travels in a `__Host-` prefixed cookie, HttpOnly and secure, with YARP putting the SPA and the API on one origin, and the downstream call uses a separate token: "DPoP used for the all access tokens". An earlier post states the rule that makes it a token-mediating BFF, not a proxy that forwards what the browser holds: "The downstream API does not accept the user delegated access tokens from the UI application" ([Bowden: BFF secured ASP.NET Core application using downstream API and an OAuth client credentials JWT](https://damienbod.com/2024/04/08/bff-secured-asp-net-core-application-using-downstream-api-and-an-oauth-client-credentials-jwt/), 2024-04-08).

`ms-learn` states the rule twice. "We recommend using cookies for browser-based applications, because, by default, the browser automatically handles them without exposing them to JavaScript", with tokens for clients that can't use cookies, and then "you are responsible for ensuring the tokens are kept secure" ([Microsoft Learn: Use Identity to secure a Web API backend for SPAs](https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization), reviewed 2026-03-23). For Blazor, it documents the BFF pattern with YARP proxying API calls "with the `access_token` stored in the authentication cookie" ([Microsoft Learn: Secure an ASP.NET Core Blazor Web App with OpenID Connect (OIDC)](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?pivots=bff-pattern), reviewed 2025-12-18).

`milan-jovanovic` gives the attacker's view: "Avoid localStorage, which any XSS payload can read. HttpOnly cookies are safer for browser apps" ([Jovanović: JWT Authentication in ASP.NET Core](https://milanjovanovic.tech/blog/jwt-authentication-aspnetcore), 2026-07-04). `khalid` adds the property that removes a whole class of attack, "`SameSite=Strict` cookies are not sent on cross-site requests, which eliminates CSRF as an attack vector without requiring anti-forgery tokens", and "`HttpOnly` cookies cannot be read by JavaScript, so even a successful XSS attack cannot exfiltrate the user's access or refresh token" ([Abuhakmeh: The Cookie Apocalypse Already Happened](https://duendesoftware.com/blog/20260414-the-cookie-apocalypse-already-happened), 2026-04-14). That post ends on the Duende BFF product, so this repository takes only the cookie properties.

**Recommendation: browser apps authenticate with cookies, and a SPA or Blazor WebAssembly client calls APIs through a BFF that holds the tokens server-side.** Four roster sources support it, two unmarked, and one of those is independent and shows working code. The .NET 11 CSRF change adds urgency, not support: a design where the browser holds tokens depends on a cross-origin token exchange, and that's what stops working ([section 1](#1-what-the-repository-already-says-checked)).

The pattern has one operational caveat. A cookie-bound session grows, and browsers cap a cookie at about 4 KB: "The cleanest fix is to stop storing session data in the cookie entirely... The cookie becomes a thin key that points to the server state rather than containing that state itself." ([Abuhakmeh: ASP.NET Core Cookie Size Limits in Production](https://duendesoftware.com/blog/20260429-aspnet-core-cookie-size-limits), 2026-04-29).

### Tokens for APIs

`milan-jovanovic` sets the validation minimum, "Always validate issuer, audience, lifetime, and signing key", and adds "Rotate refresh tokens on every use (issue a new one, revoke the old)" (same post). `code-with-mukesh` gives a lifetime range: "15 to 60 minutes is reasonable. Since you cannot revoke a JWT once it is issued, a short lifetime limits the damage if one leaks" ([Mukesh: JWT Authentication in ASP.NET Core](https://codewithmukesh.com/blog/jwt-authentication-in-aspnet-core/), 2026-06-07).

Both put permissions on the server, and Mukesh shows what fails when they're in the token. Jovanović: "keep tokens lean and resolve permissions server-side with IClaimsTransformation" ([Jovanović: Building Secure APIs with Role-Based Access Control in ASP.NET Core](https://milanjovanovic.tech/blog/building-secure-apis-with-role-based-access-control-in-aspnetcore), 2025-09-27). Mukesh: "A revoked permission keeps working until the token expires, which I reproduced as a 201 that should have been a 403" ([Mukesh: Permission-Based Authorization in ASP.NET Core](https://codewithmukesh.com/blog/permission-based-authorization-in-aspnet-core/), 2026-08-06).

**`damien-bowden` goes further.** The other two stop at validating a bearer token. He binds the token, and replaces the client secret:

- **DPoP** binds the token to a key the client holds, so a stolen token isn't enough: "The access tokens should only be used for what the access tokens are intended for. OAuth DPoP helps force this." ([Bowden: Securing APIs using ASP.NET Core and OAuth 2.0 DPoP](https://damienbod.com/2023/08/14/securing-apis-using-asp-net-core-and-oauth-2-0-dpop/), 2023-08-14)
- **Pushed authorization requests** move the request parameters out of the browser redirect, with `PushedAuthorizationBehavior.Require`, new in .NET 9 ([Bowden: Implement ASP.NET Core OpenID Connect OAuth PAR client with Keycloak using .NET Aspire](https://damienbod.com/2024/09/02/implement-asp-net-core-openid-connect-oauth-par-client-with-keycloak-using-net-aspire/), 2024-09-02).
- **Client assertions** replace the shared client secret with a private-key JWT, in a series from 2025-02 to 2026-02 ([Bowden: Use client assertions in ASP.NET Core using OpenID Connect, OAuth DPoP and OAuth PAR](https://damienbod.com/2026/02/02/use-client-assertions-in-asp-net-core-using-openid-connect-oauth-dpop-and-oauth-par/), 2026-02-02).

`milan-jovanovic` contradicts himself, and an opinion should settle it. His Keycloak post argues for "a battle-tested identity provider" ([Jovanović: Integrate Keycloak with ASP.NET Core Using OAuth 2.0](https://milanjovanovic.tech/blog/integrate-keycloak-with-aspnetcore-using-oauth-2), 2026-02-07), while his and Mukesh's most prominent posts teach issuing your own JWTs. The editor would put the opinion at the identity provider, with self-issued tokens as the exception. `damien-bowden`'s whole catalogue assumes an identity provider, and focuses on how the client proves itself to it.

**API keys are still blocked.** Mukesh asks for "at least 128 bits of entropy, ideally 256 bits. Generated with RandomNumberGenerator.GetBytes()", storage "as SHA-256 hashes, never plaintext", and comparison with `CryptographicOperations.FixedTimeEquals` ([Mukesh: API Key Authentication in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/api-key-authentication-aspnet-core/), 2026-05-08). Jovanović's 2023 post covers neither hashing nor comparison, and `damien-bowden` doesn't write about custom API-key authentication (negative result, checked this pass). A **Corroborate.** source can't carry an opinion alone.

### Secrets

`ms-learn`: "Never store passwords or other sensitive data in source code or configuration files. Production secrets shouldn't be used for development or test. Secrets shouldn't be deployed with the app. Production secrets should be accessed through a controlled means like Azure Key Vault." The Secret Manager "doesn't encrypt the stored secrets and shouldn't be treated as a trusted store. It's for development purposes only." ([Microsoft Learn: Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/app-secrets), reviewed 2026-05-13). The security overview adds managed identities as the most secure way to authenticate to Azure services.

`damien-bowden` follows the same pattern, and shows the deployed half: user secrets locally, and a system-assigned managed identity with `ChainedTokenCredential` to reach the vault ([Bowden: Using ASP.NET Core with Azure Key Vault](https://damienbod.com/2024/12/02/using-asp-net-core-with-azure-key-vault/), 2024-12-02). `meziantou` covers how secrets leak through logs and diagnostics ([Meziantou: Prevent accidental disclosure of configuration secrets](https://www.meziantou.net/prevent-accidental-disclosure-of-configuration-secrets.htm), 2023-02-13). .NET 10 closed one such path by default: EF Core now redacts inlined constants from its logs ([.NET Team: Announcing .NET 10](https://devblogs.microsoft.com/dotnet/announcing-dotnet-10/), 2025-11-11).

**Recommendation: user secrets in development, a managed identity to a vault in production, and nothing secret in `appsettings*.json`.** Two roster sources, uncontested. The redaction part links to [logging.md](../opinions/logging.md).

### Data Protection in containers

The Data Protection key ring encrypts authentication cookies and antiforgery tokens. `khalid` explains why a deployment can't ignore it: the keys let an app "work in a distributed environment like Windows Azure, AWS, Google Cloud, or another production environment as you scale past a single host machine" ([Abuhakmeh: Data Protection for ASP.NET Core Developers](https://duendesoftware.com/blog/20250313-data-protection-aspnetcore-duende-identityserver), 2025-03-13). An earlier personal post shows the same keys shared on purpose across apps behind one proxy ([Abuhakmeh: Sharing Auth Cookies With YARP, IdentityServer, and ASP.NET Core](https://khalidabuhakmeh.com/sharing-auth-cookies-with-yarp-identityserver-and-aspnet-core), 2022-08-23).

`ms-learn` says where the ring must live in a container: "When hosting in a Docker container, keys should be maintained in either: A folder that's a Docker volume that persists beyond the container's lifetime... or An external provider, such as Azure Blob Storage... or Redis." ([Microsoft Learn: Configure ASP.NET Core Data Protection](https://learn.microsoft.com/aspnet/core/security/data-protection/configuration/overview), reviewed 2025-10-08).

The key-storage page adds two traps ([Microsoft Learn: Key storage providers in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/data-protection/implementation/key-storage-providers), reviewed 2025-11-07):

- "If you specify an explicit key persistence location, the data protection system deregisters the default key encryption at rest mechanism, so keys are no longer encrypted at rest." Persisting the ring without a `ProtectKeysWith*` call swaps a restart bug for plaintext keys.
- "Redis doesn't persist data by default when restarting. This can cause Data Protection to issue new keys, invalidating previously protected data."

The configuration page adds that encrypting keys at rest "doesn't prevent cyberattackers from creating new keys", so the store's permissions matter as much as its encryption.

**Recommendation: with more than one instance, or a container that restarts, persist the key ring to shared storage and protect it at rest, both explicitly.** It belongs beside the container hosting opinion, which as written produces exactly this failure. `damien-bowden`'s material on this is from 2016 to 2019, too old to cite (negative result), so `ms-learn` carries the container mechanics and `khalid` supports the distributed case.

### CORS

`ms-learn`: "Specifying AllowAnyOrigin and AllowCredentials is an insecure configuration and can result in cross-site request forgery", and "Bypassing the built-in checks by using SetIsOriginAllowed(\_ => true) together with AllowCredentials is also an insecure configuration." Named policies applied with `[EnableCors]` give the finest control ([Microsoft Learn: Enable Cross-Origin Requests (CORS) in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cors), reviewed 2026-05-12).

In .NET 11, the allowed-origins list also decides which cross-origin requests the CSRF middleware admits (Lock, in [section 1](#1-what-the-repository-already-says-checked)), so a permissive CORS policy becomes a CSRF hole too. That depends on a preview, but with `khalid`'s account of what the middleware rejects, it's a reason to add a CORS opinion before .NET 11 ships.

### HTTPS behind a proxy

`ms-learn` extends the hosting opinion's proxy guidance to TLS: "If the proxy also handles HTTPS redirection, there's no need to use HTTPS redirection middleware. If the proxy server also handles writing HSTS headers... then the app doesn't require HSTS middleware." `UseHsts` "isn't recommended in development because the HSTS settings are highly cacheable by browsers." ([Microsoft Learn: Enforce HTTPS in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/enforcing-ssl), reviewed 2026-07-29). The layer that terminates TLS owns redirection and HSTS. That's a sentence for the hosting opinion, not an opinion of its own.

### Output encoding, CSP, and security headers

Razor "automatically encodes all output sourced from variables, unless you work to prevent this behavior", and `HtmlString` "should never be used in combination with untrusted input because it exposes an XSS vulnerability" ([Microsoft Learn: Prevent Cross-Site Scripting (XSS) in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cross-site-scripting), reviewed 2026-05-13). For Blazor, "CSP is recommended for Blazor apps". Blazor Web Apps on .NET 8 and later send `frame-ancestors 'self'` automatically. On .NET 8 to 10, a strict policy without `unsafe-inline` styles breaks the `Virtualize` component, which .NET 11 fixes ([Microsoft Learn: Enforce a Content Security Policy for ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/security/content-security-policy), reviewed 2026-08-18). `andrew-lock` adds Trusted Types for client-side script, under which "APIs like `innerHTML` must be passed a `TrustedHTML` object" ([Lock: Preventing client-side cross-site-scripting vulnerabilities with Trusted Types](https://andrewlock.net/preventing-client-side-cross-site-scripting-vulnerabilities-with-trusted-types/), 2025-02-11).

**The baseline the first pass couldn't source now has one.** `damien-bowden` publishes a header set for a Blazor Web application:

- `X-Frame-Options`: deny
- `X-Content-Type-Options`: nosniff
- `Referrer-Policy`: strict-origin-when-cross-origin
- The three Cross-Origin-\* policies
- A CSP with a nonce
- `Permissions-Policy`
- HSTS outside development

([Bowden: Implement a secure Blazor Web application using OpenID Connect and security headers](https://damienbod.com/2024/04/15/implement-a-secure-blazor-web-application-using-openid-connect-and-security-headers/), 2024-04-15). He revisits the CSP part for interactive circuits, preferring a nonce and falling back to hashes ([Bowden: Revisiting using a Content Security Policy (CSP) nonce in Blazor](https://damienbod.com/2025/05/26/revisiting-using-a-content-security-policy-csp-nonce-in-blazor/), 2025-05-26).

**It also removes the conflict that blocked the first pass.** That pass could reach `NetEscapades.AspNetCore.SecurityHeaders` only through `andrew-lock`, who maintains it. Bowden uses it as a third party, with no interest to declare: "The NetEscapades.AspNetCore.SecurityHeaders nuget package is used to implement the security headers." The recommendation no longer rests on the library's author.

### Open redirects

.NET 10 closed a minimal-API gap: `RedirectHttpResult.IsLocalUrl` works without MVC, as in `if (RedirectHttpResult.IsLocalUrl(url)) return Results.LocalRedirect(url);`, and `LocalRedirect` "throws an exception if a non-local URL is specified" ([Microsoft Learn: Prevent open redirect attacks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/preventing-open-redirects), reviewed 2026-08-26). Small, mechanical, and uncontested.

### SQL injection in EF Core

"The FromSql and FromSqlInterpolated methods are safe against SQL injection, and always integrate parameter data as a separate SQL parameter. However, the FromSqlRaw method can be vulnerable to SQL injection attacks, if improperly used." The same split holds for `ExecuteSql` against `ExecuteSqlRaw`, and `SqlQuery` against `SqlQueryRaw` ([Microsoft Learn: SQL Queries](https://learn.microsoft.com/ef/core/querying/sql-queries), reviewed 2022-09-19). The review is four years old and predates EF Core 10, but the API split hasn't changed since `FromSql` arrived in EF Core 7.

[data-access.md](../opinions/data-access.md) says nothing about raw SQL, and its TODO doesn't list it. One line would do: interpolated methods only, and never a `*Raw` method with input. It would also give that file its first `ms-learn` citation, which its source-redundancy note already suggests.

## 5. Runtime and cryptography

### Deserialization

`BinaryFormatter` is gone: "Starting in .NET 9, the in-box BinaryFormatter implementation throws exceptions on use, even with the settings that previously enabled its use." ([Microsoft Learn: BinaryFormatter disabled across all project types](https://learn.microsoft.com/dotnet/core/compatibility/serialization/9.0/binaryformatter-removal), reviewed 2024-08-06). An unsupported compatibility package keeps it alive for code that can't migrate. `dotnet-blog` states the general rule: "Any deserializer, binary or text, that allows its input to carry information about the objects to be created is a security problem waiting to happen." ([Landwerth: BinaryFormatter removed from .NET 9](https://devblogs.microsoft.com/dotnet/binaryformatter-removed-from-dotnet-9/), 2024-08-28). `meziantou` shows the attack end to end, from a crafted payload to remote code execution ([Meziantou: Deserialization can be dangerous](https://www.meziantou.net/deserialization-can-be-dangerous.htm), 2020-01-05): old, but the mechanism hasn't changed. [ui-frameworks.md](../opinions/ui-frameworks.md) already mentions the removal as an Avalonia 12 migration cost.

**Recommendation: never reference the `System.Runtime.Serialization.Formatters` compatibility package, and never deserialize untrusted input in a format that names its own types.** Landwerth's sentence is the rule.

### Cryptography on GA APIs

`ms-learn`'s recommended-algorithms table names `Aes` for privacy, `HMACSHA256` or `HMACSHA512` for integrity, `ECDsa` or `RSA` for signatures, `RandomNumberGenerator` for random numbers, and `Rfc2898DeriveBytes.Pbkdf2` for password-based key derivation ([Microsoft Learn: .NET cryptography model](https://learn.microsoft.com/dotnet/standard/security/cryptography-model), reviewed 2021-02-26). The page is five years old, gives no iteration count, and doesn't mention AES-GCM, so it can't carry an opinion alone. `steve-gordon` gives the practical AES-GCM rule: the nonce must be unique per operation, filled from `RandomNumberGenerator`, and "There is, over time, a very small chance of generating the same nonce, which can render AES-GCM insecure" ([Gordon: Encrypting Properties with System.Text.Json and a TypeInfoResolver Modifier (Part 2)](https://www.stevejgordon.co.uk/encrypting-properties-with-system-text-json-and-a-typeinforesolver-modifier-part-2), 2026-02-05).

**Password hashing has a current source, but not a current iteration count.** `damien-bowden` gives the parameters the 2021 Learn page omits, "a salt of 8 bytes or more... and more than 10000 iterations" for `Rfc2898DeriveBytes.Pbkdf2`, and names the better default for anyone on Identity: its own `PasswordHasher`, not a hand-written derivation ([Bowden: Creating hashes in .NET](https://damienbod.com/2024/07/01/creating-hashes-in-net/), 2024-07-01). His minimum is ten times lower than the framework's own: `ms-learn` documents `PasswordHasherOptions.IterationCount` as "the number of iterations used when hashing passwords using PBKDF2. Default is 100,000." ([Microsoft Learn: PasswordHasherOptions.IterationCount](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.passwordhasheroptions.iterationcount), generated API reference for `aspnetcore-10.0`, so it has no `ms.date` for [Freshness](#freshness)). So this topic takes his routing, not his numbers. The passkeys-first opinion already avoids passwords, so the recommendation is short: let Identity hash passwords at its default. Use `Rfc2898DeriveBytes.Pbkdf2` directly only to derive a key from a password outside Identity, and then the iteration count is an open question. OWASP's [Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html) prefers Argon2id (**unvetted**), which the BCL doesn't ship. That's recorded as a disagreement below.

### Post-quantum cryptography

**Experimental as of 2026-09-12, not yet an opinion,** except ML-KEM and ML-DSA, and those only on some platforms. `dotnet-blog` is the primary source. "For `MLKem` and `MLDsa` we've removed `[Experimental]` from the classes, but it remains on a few methods": `ExportEncryptedPkcs8PrivateKey`, `ExportPkcs8PrivateKey`, `ExportSubjectPublicKeyInfo`, and `ImportFromPem`, under diagnostic `SYSLIB5006`. And "we've decided to release the `SlhDsa` and `CompositeMLDsa` classes with `[Experimental]`" ([Barton: Post-Quantum Cryptography in .NET](https://devblogs.microsoft.com/dotnet/post-quantum-cryptography-in-dotnet/), 2025-11-18). Linux needs OpenSSL 3.5 or later, and Apple platforms, Android, and the browser have none of these algorithms ([Microsoft Learn: Cross-platform cryptography](https://learn.microsoft.com/dotnet/standard/security/cross-platform-cryptography), reviewed 2026-08-03). The two sources disagree on Windows; see below.

At GA, the guidance is narrow: use ML-KEM and ML-DSA where the operating system provides them, and gate every use on `IsSupported`.

### Containers run as non-root

With SDK container publish, which the hosting opinion requires, "If you're targeting .NET 8 or higher and using the Microsoft runtime images, then: on Linux, the rootless user `app` is used" ([Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration), reviewed 2026-08-04). Setting `ContainerUser` to `root` undoes it. `andrew-lock` gives the reason: "if an attacker manages to compromise your container, they'll be able to do pretty much anything in the container" ([Lock: Updates to Docker images in .NET 8](https://andrewlock.net/exploring-the-dotnet-8-preview-updates-to-docker-images-in-dotnet-8/), 2023-10-17). The repository already gets this by default, so the opinion is one line: never set `ContainerUser` to `root`. The chiseled images page was last reviewed on 2024-08-28, too stale to cite for .NET 10 image choices.

## 6. Preview, not yet an opinion

All of this is .NET 11, which reached RC 1 on 2026-09-08, with GA expected in November 2026 ([Microsoft Learn: What's new in .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/overview), reviewed 2026-09-08).

- **Automatic CSRF protection from Fetch Metadata headers**, covered in section 1 and in aspnet-core.md's "Coming next" note. Watch `khalid`'s account of what it rejects: a SPA token exchange, and possibly a `form_post` provider callback.
- **`X25519DiffieHellman` and unpadded AES key wrap on `Aes`**, plus "TLS handshake hardening, certificate-validation alerts on Linux, channel binding validation on Unix".
- **A redesign of `unsafe` in C#** as a compiler-enforced contract between callers and API authors: an opt-in C# 16 feature in .NET 11 previews, targeted for production in .NET 12 ([Lander: Improving C# Memory Safety](https://devblogs.microsoft.com/dotnet/improving-csharp-memory-safety/), 2026-05-21).
- **A CSP-compliant `Virtualize` component** in Blazor, which removes the .NET 8 to 10 conflict noted under output encoding.

## Where the sources disagree

1. **Post-quantum support on Windows.** `ms-learn`'s cross-platform page, reviewed 2026-08-03, still lists ML-KEM and ML-DSA on Windows as "Windows 11 Insiders (Latest)". `dotnet-blog` said on 2025-11-18 that "Windows support arrived this month". The blog is a later primary statement by the engineer who shipped the feature, and the Learn table looks unrevised, so this topic follows the blog.
2. **What a lock file is for.** Integrity (`meziantou`), repeatability (`ms-learn`), or drift protection (this repository's template). All three are true. The template's is the weakest framing, and the only one that makes the file sound optional in every case.
3. **Where NuGet Audit runs.** `ms-learn` presents switching it off in CI as supported. The **House:** rule says a check runs everywhere or reports late. The house wins, and the sourced option would be recorded as the community default this repository declines.
4. **Password-based key derivation and its parameters.** The roster agrees on the algorithm: `ms-learn` and `damien-bowden` both use PBKDF2 through `Rfc2898DeriveBytes.Pbkdf2`. OWASP prefers Argon2id (**unvetted**), which isn't in the BCL, so PBKDF2 stands. The parameters are open. Bowden's 2024 minimum of "more than 10000 iterations" is ten times below ASP.NET Core Identity's own default of 100,000, and no roster source gives a current figure for a hand-written derivation on .NET 10. This topic leaves the count open instead of carrying a number nothing supports.
5. **Partitioning rate limits by IP.** `milan-jovanovic` (2023) endorses it for anonymous traffic, and aspnet-core.md forbids partitioning on spoofable values. They're compatible once the opinion separates the connection address from a forwarded header (section 1).
6. **Issuing your own tokens.** `milan-jovanovic` argues for an identity provider in one post and teaches self-issued JWTs in another, and `code-with-mukesh` teaches the latter. `damien-bowden`'s catalogue assumes an identity provider throughout, which settles it by weight, not argument.

## Freshness

`ms-learn` review dates are each page's `ms.date`, per the roster rule. The editor re-read four pages directly (NU1901 to NU1904, releases and support, the Blazor OIDC page, and cross-platform cryptography). The rest are `ms.date` values reported by worker agents.

| Page                                              | `ms.date`              |
| ------------------------------------------------- | ---------------------- |
| NuGet Warnings NU1901 to NU1904                   | 2024-07-19             |
| Auditing package dependencies                     | 2025-10-01             |
| NuGetAudit audits transitive packages             | 2025-03-28             |
| Package Source Mapping                            | 2026-07-16             |
| Trusted Publishing                                | 2025-07-01, 2026-07-29 |
| MSBuild reference for .NET SDK projects           | 2026-03-27             |
| .NET releases, patches, and support               | 2026-05-15             |
| Minimal APIs antiforgery checks                   | 2023-12-05             |
| User data protected by authorization              | 2026-05-11             |
| Use Identity to secure a Web API backend for SPAs | 2026-03-23             |
| Blazor Web App with OIDC (BFF)                    | 2025-12-18             |
| Safe storage of app secrets                       | 2026-05-13             |
| Configure Data Protection                         | 2025-10-08             |
| Key storage providers                             | 2025-11-07             |
| CORS                                              | 2026-05-12             |
| Enforce HTTPS                                     | 2026-07-29             |
| Prevent XSS                                       | 2026-05-13             |
| Blazor Content Security Policy                    | 2026-08-18             |
| Prevent open redirect attacks                     | 2026-08-26             |
| EF Core SQL Queries                               | **2022-09-19**         |
| BinaryFormatter disabled                          | 2024-08-06             |
| .NET cryptography model                           | **2021-02-26**         |
| Cross-platform cryptography                       | 2026-08-03             |
| Containerize a .NET app reference                 | 2026-08-04             |
| Container images (chiseled)                       | **2024-08-28**         |
| What's new in .NET 11                             | 2026-09-08             |

Weigh the bold dates. The EF Core page describes an API unchanged since EF Core 7, so its age doesn't weaken the claim. The cryptography model page predates AES-GCM's prominence and every post-quantum type, and `damien-bowden`'s 2024 hashing post now covers the one part of it this topic relies on. The container-images page is why chiseled images get no recommendation here.

On the roster blogs: `meziantou`'s security catalogue is mostly from 2019 and 2020, and only the 2022 and 2024 NuGet posts are recent enough to rely on. `andrew-lock`'s security writing splits into a current run from 2025 to 2026, and a 2016 to 2023 back catalogue that predates .NET 8. `steve-gordon`'s AES-GCM series is from 2026. `damien-bowden`'s material runs from 2023 to 2026, and is current on every topic cited, except two older strands recorded in place: the DPoP post is from 2023-08-14, before PAR and client assertions built on it, and his Data Protection writing, from 2016 to 2019, isn't cited. `khalid`'s Duende posts run from 2025-03 to 2026-09, and his personal blog published nothing between 2025-04-22 and this pass, so the widened scope is what makes him current. His one personal-blog citation here is the 2022 Data Protection post, whose mechanism hasn't changed.

Preview-gated: everything in section 6, plus `SlhDsa`, `CompositeMLDsa`, and the experimental members of `MLKem` and `MLDsa`.

## What this reveals about the repository

1. **Security needs a home, and `resolve-research` should choose between two.** Supply chain fits [ci.md](../opinions/ci.md) and the templates. Application and runtime security fits no existing file. [aspnet-core.md](../opinions/aspnet-core.md) could absorb authorization defaults, cookies and BFF, Data Protection, CORS, HTTPS, security headers, and open redirects, but secrets, deserialization, and cryptography aren't ASP.NET Core topics. The editor recommends a new `opinions/security.md` for those, with aspnet-core.md keeping the HTTP pipeline items and linking across. Either way, the new file needs its README entries in the same pull request.

2. **Document the template's NuGet Audit consequence.** A comment in [templates/Directory.Build.props](../templates/Directory.Build.props) should say that NU1901 to NU1904 fail restore on purpose, and that the only accepted opt-out is `NuGetAuditSuppress` with a stated reason. The house rule already implies this, so `resolve-research` can add it as the rule applied, not a new opinion.

3. **ci.md's signing section is out of date on credentials.** Trusted publishing is GA, and nuget.org API keys now last 30 days. This is `harvest-sources` work, because `dotnet-blog` and `andrew-lock` published it, and it should land before 2026-11-01, when every older nuget.org key expires.

4. **aspnet-core.md has three gaps, one new this pass.** It ships containers without saying where the Data Protection keys live. It credits SDK container publish with patching that only happens on rebuild. And its passkeys opinion should carry the `amr` caveat while [dotnet/aspnetcore#64881](https://github.com/dotnet/aspnetcore/issues/64881) is open, because step-up authentication built on the claim gets the wrong answer.

5. **Two small opinions are ready, each on one uncontested `ms-learn` page:** parameterised raw SQL for [data-access.md](../opinions/data-access.md), and `RedirectHttpResult.IsLocalUrl` for minimal-API redirects. Both are as the first pass left them.

6. **For the next `refresh-dotnet-versions` run:** .NET 11 GA in November 2026 turns the CSRF note into an opinion, and makes the CORS policy part of CSRF configuration. Adopt `CheckSdkVulnerabilities` once a pinned SDK has it. .NET 8 and 9 reach end of support on 2026-11-10, after which `verify-project` should report either as a finding.

7. **One admission moved three areas from blocked to ready, and sourced a fourth.** Browser authentication and BFF, the security header baseline, and password hashing now have a recommendation. Token binding (DPoP, PAR, and client assertions) is sourced, but has no recommendation, because nothing else on the roster supports or contests it. All four come from `damien-bowden`, who filled more of this topic than the whole first-pass sweep.

8. **Two narrow gaps remain.** API-key handling still rests on one **Corroborate.** source, and `damien-bowden`, the likeliest remaining source, doesn't cover it. No source recommends an `AnalysisModeSecurity` setting. Both stay open instead of being filled from general knowledge.

9. **A correction, and what the `vet-source` pass settled.** The first pass ranked Barry Dorrans the strongest of seven candidates for his subject-matter authority. The roster checked the assumption behind the ranking and found it wrong: idunno.org has been a blog only since 2025-11-18, and was a static conference page for the decade before, so the record that counts is under a year. The second-ranked candidate, `damien-bowden`, is the one admission, and the one that moved this topic. **For the next pass: a research topic ranks candidates by the gap they'd fill, which isn't the question `vet-source` asks, so say which question the ranking answers.** The rest: `khalid` widened, `duende-blog` and `scott-brady` watch-listed, and the OWASP Cheat Sheet Series and Philippe De Ryck not taken up, the first still unvetted and the second out of scope.

10. **One lead for a future `vet-source` pass.** The passkey and antiforgery posts on the Duende blog carry Maarten Balliauw's byline, not Khalid's, so the widened scope doesn't reach them. He has a long personal publishing record this topic hasn't examined, and the passkeys opinion has only one source. Look at him the next time the roster is open, on the same terms that admitted `khalid`'s byline.
