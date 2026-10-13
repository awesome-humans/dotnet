---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-25
sources: [andrew-lock, barry-dorrans, code-with-mukesh, ms-learn]
---

# Security: the blocked remainder

**Partially promoted on 2026-09-25.** Git history keeps the topic's two passes, from 2026-09-11 and 2026-09-12, at `research/security.md`. Everything they sourced is now opinion. What's left here is what no citable source can carry yet, and each item says what would unblock it.

## What promoted

| Went to                                                               | What                                                                                                                                                                                                                                                                                                     |
| --------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [opinions/security.md](../opinions/security.md), new file             | Secrets, deserialization, the GA cryptography set with AES-GCM nonce handling, password hashing through Identity, post-quantum gated on `IsSupported`, and servicing cadence                                                                                                                             |
| [opinions/aspnet-core.md](../opinions/aspnet-core.md)                 | The passkey `amr` caveat, the authorization fallback policy, cookies and BFF, tokens for APIs including DPoP, PAR, and client assertions, the Data Protection key ring, CORS, security headers, open redirects, the form-binding antiforgery rule, non-root containers, and TLS ownership behind a proxy |
| [opinions/ci.md](../opinions/ci.md)                                   | NuGet Audit under the warnings rule, package source mapping, and trusted publishing instead of an API key                                                                                                                                                                                                |
| [opinions/data-access.md](../opinions/data-access.md)                 | Parameterised raw SQL: interpolated methods only, and never a `*Raw` method with input                                                                                                                                                                                                                   |
| [templates/Directory.Build.props](../templates/Directory.Build.props) | NU1901 to NU1904 failing restore on purpose. #123 had already reframed the lock file as a content pin, not drift protection.                                                                                                                                                                             |

## Still blocked on a source

**API key authentication.** `code-with-mukesh` asks for "at least 128 bits of entropy, ideally 256 bits. Generated with RandomNumberGenerator.GetBytes()". It also asks for storage "as SHA-256 hashes, never plaintext", and comparison with `CryptographicOperations.FixedTimeEquals`. ([Mukesh: API Key Authentication in ASP.NET Core (.NET 10)](https://codewithmukesh.com/blog/api-key-authentication-aspnet-core/), 2026-05-08) The advice is mechanical and looks right, but that ID is marked **Corroborate.**, so it can't carry the claim alone. `milan-jovanovic`'s 2023 post covers neither hashing nor comparison, and `damien-bowden` doesn't write about custom API key authentication (negative result, 2026-09-12). **Unblocked by:** a second citable source on hashing and comparing API keys.

**Which security analyzer rules to enable.** `AnalysisModeSecurity` sets the analysis mode for the Security category alone, and otherwise inherits `AnalysisMode`. ([Microsoft Learn: MSBuild reference for .NET SDK projects](https://learn.microsoft.com/dotnet/core/project-sdk/msbuild-props), reviewed 2026-03-27) The template's `latest-recommended` turns on some security rules. The documentation doesn't list which, and points at the SDK's `analysislevel_*_recommended.globalconfig` instead. No source in either pass recommends a setting. **Unblocked by:** a source that recommends a level or names the rules, so this repository doesn't invent one.

**SBOM and provenance.** `andrew-lock` is the only citable coverage. Three posts, from 2025-03 to 2025-04, cover generating an SBOM, provenance attestations, and signed SBOM attestations with `actions/attest-sbom`. `ms-learn` has no page on SBOM generation for .NET builds, and `damien-bowden` doesn't cover supply chain (both negative results). No source says who consumes the SBOM, which is the question an opinion would have to answer. `barry-dorrans` covers exactly this, and is on the watch list until idunno.org has been a blog for two years, on 2027-11-18. **Unblocked by:** that date, or another citable source.

**The PBKDF2 iteration count.** [opinions/security.md](../opinions/security.md) sends password hashing to Identity's hasher, at its default of 100,000 iterations. It leaves the count for a hand-rolled derivation to each application. `damien-bowden`'s 2024 floor of "more than 10000 iterations" is an order of magnitude lower, and no roster source states a current figure for .NET 10. The OWASP Password Storage Cheat Sheet prefers Argon2id, which is **unvetted** and not in the BCL, so it changed nothing. **Unblocked by:** a roster source that states a figure for .NET 10.

## Waiting on a release, not a source

- **.NET 11 GA, expected November 2026:** The CSRF note in aspnet-core.md becomes the opinion, and the CORS allowed-origins list becomes part of CSRF configuration. The CSP-compliant `Virtualize` component removes the .NET 8 to 10 conflict recorded under security headers. This is for `refresh-dotnet-versions`.
- **`CheckSdkVulnerabilities`:** `ms-learn` documents the property, and warning NETSDK1239 for an end-of-life SDK. ([Microsoft Learn: .NET releases, patches, and support](https://learn.microsoft.com/dotnet/core/releases-and-support), reviewed 2026-05-15) The installed 10.0.400 SDK doesn't reference it, NETSDK1238, or NETSDK1240. Adopt it when an SDK this repository pins carries it.
- **.NET 8 and 9 end of support, 2026-11-10:** [opinions/security.md](../opinions/security.md) records the date. After it, `verify-project` should report either target framework as a finding.

## A lead for `vet-source`

On the Duende blog, the passkey and antiforgery deep dives carry Maarten Balliauw's byline, not Khalid's, so the wider `khalid` scope doesn't reach them. He has a long personal publishing record that nobody here has examined. This repository's passkeys opinion rests on a single source, so run `vet-source` on him, on the same terms that admitted Khalid's byline instead of the publication.
