---
targets: [net10.0, csharp-14]
last-reviewed: 2026-10-31
sources:
  [andrew-lock, damien-bowden, dotnet-blog, meziantou, ms-learn, steve-gordon]
---

# Security

This file covers the security rules outside the HTTP pipeline: secrets, deserialization, cryptography, and servicing cadence.

- For authorization defaults, cookies and BFF, tokens, the Data Protection key ring, CORS, HTTPS, security headers, and open redirects, see [aspnet-core.md](aspnet-core.md).
- For supply-chain rules, see [ci.md](ci.md).
- For parameterised SQL, see [data-access.md](data-access.md).

## Opinions

### Secrets

**Use the Secret Manager in development and a managed identity to a vault in production. Never put a secret in `appsettings*.json`.** The Secret Manager "doesn't encrypt the stored secrets and shouldn't be treated as a trusted store. It's for development purposes only". Production secrets "should be accessed through a controlled means like Azure Key Vault". ([Microsoft Learn: Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/app-secrets)) A managed identity removes the last secret from the deployment, because you don't store a vault credential either.

```csharp
// Azure.Extensions.AspNetCore.Configuration.Secrets + Azure.Identity.
// The managed identity works in the deployed app and the second credential is the
// developer's own sign-in, so one code path covers both. Pass the id explicitly:
// the parameterless ManagedIdentityCredential constructor is obsolete as of
// Azure.Identity 1.18.0, and CS0618 is an error under TreatWarningsAsErrors.
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["VaultName"]}.vault.azure.net/"),
    new ChainedTokenCredential(
        new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned),
        new AzureCliCredential()));
```

([Bowden: Using ASP.NET Core with Azure Key Vault](https://damienbod.com/2024/12/02/using-asp-net-core-with-azure-key-vault/), and for the obsolete constructor, [Azure.Identity: CHANGELOG, 1.18.0-beta.1](https://github.com/Azure/azure-sdk-for-net/blob/main/sdk/identity/Azure.Identity/CHANGELOG.md#1180-beta1-2025-11-14))

A secret can also leave the process through a log or a diagnostic dump, so review what the app writes. ([Meziantou: Prevent accidental disclosure of configuration secrets](https://www.meziantou.net/prevent-accidental-disclosure-of-configuration-secrets.htm)) In .NET 10, EF Core redacts inlined constants from the SQL it logs. ([.NET Team: Announcing .NET 10](https://devblogs.microsoft.com/dotnet/announcing-dotnet-10/)) For the logging rules, see [logging.md](logging.md).

### Deserialization

**Never reference the `System.Runtime.Serialization.Formatters` compatibility package, and never deserialize untrusted input with a format that names its own types.** "Starting in .NET 9, the in-box BinaryFormatter implementation throws exceptions on use, even with the settings that previously enabled its use." ([Microsoft Learn: BinaryFormatter disabled across all project types](https://learn.microsoft.com/dotnet/core/compatibility/serialization/9.0/binaryformatter-removal)) The compatibility package brings that behaviour back, and the vulnerability with it.

The flaw is type information in the payload, not the binary encoding: "Any deserializer, binary or text, that allows its input to carry information about the objects to be created is a security problem waiting to happen." ([Landwerth: BinaryFormatter removed from .NET 9](https://devblogs.microsoft.com/dotnet/binaryformatter-removed-from-dotnet-9/)) Keep `System.Text.Json` on its default contract-based binding. Never let a converter or a polymorphic setting take a type name from the caller. A crafted payload that reaches a type-naming deserializer is remote code execution. For an end-to-end demonstration, see [Meziantou: Deserialization can be dangerous](https://www.meziantou.net/deserialization-can-be-dangerous.htm).

### Cryptography: stay on the GA algorithm set

**Choose the algorithm from the platform's recommended set, and never implement a primitive yourself.** The set below is Microsoft's. ([Microsoft Learn: .NET cryptography model](https://learn.microsoft.com/dotnet/standard/security/cryptography-model)) That page was last reviewed in 2021 and doesn't cover AES-GCM or password parameters, so the rules for those two cite their own current sources.

| Purpose                       | API                                                                                                             |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Symmetric encryption          | `AesGcm`. `Aes` in CBC mode only for a format that requires it, with an HMAC that you verify before you decrypt |
| Integrity                     | `HMACSHA256` or `HMACSHA512`                                                                                    |
| Signatures                    | `ECDsa` or `RSA`                                                                                                |
| Random values                 | `RandomNumberGenerator`                                                                                         |
| Password-based key derivation | `Rfc2898DeriveBytes.Pbkdf2`                                                                                     |

**Encrypt with `AesGcm`, an authenticated mode, by default.** Padded CBC leaks plaintext to an attacker who can tell a padding failure from a successful decryption: "to use padded CBC block ciphers safely, you must combine them with an HMAC (or another data integrity check) that you validate using a constant time comparison before trying to decrypt the data". The alternative is to "change the cipher mode to an authenticated encryption (AE) mode such as GCM or CCM". ([Microsoft Learn: Timing vulnerabilities with CBC-mode symmetric decryption using padding](https://learn.microsoft.com/dotnet/standard/security/vulnerabilities-cbc-mode))

**Use each GCM nonce once.** Generate it for each operation from `RandomNumberGenerator`, and store it beside the ciphertext. Never derive it from a counter you might reset. "There is, over time, a very small chance of generating the same nonce, which can render AES-GCM insecure". ([Gordon: Encrypting Properties with System.Text.Json and a TypeInfoResolver Modifier (Part 2)](https://www.stevejgordon.co.uk/encrypting-properties-with-system-text-json-and-a-typeinforesolver-modifier-part-2)) Rotate a key once it has encrypted enough data to make a 12-byte nonce collision plausible.

```csharp
static byte[] Encrypt(byte[] key, byte[] plaintext)
{
    // A fresh nonce for every call, stored with the ciphertext.
    byte[] nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
    byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize];
    byte[] ciphertext = new byte[plaintext.Length];

    using var aes = new AesGcm(key, tag.Length);
    aes.Encrypt(nonce, plaintext, ciphertext, tag);

    return [.. nonce, .. tag, .. ciphertext];
}
```

**Let ASP.NET Core Identity hash passwords.** Its `PasswordHasher` runs PBKDF2, and the `IterationCount` "Default is 100,000". ([Microsoft Learn: PasswordHasherOptions.IterationCount](https://learn.microsoft.com/dotnet/api/microsoft.aspnetcore.identity.passwordhasheroptions.iterationcount)) That's an order of magnitude above what a hand-rolled derivation usually gets. Under the passkeys-first opinion in [aspnet-core.md](aspnet-core.md#authentication-passkeys-first), most new apps never hash a password at all.

To derive a key from a password outside Identity, call `Rfc2898DeriveBytes.Pbkdf2` with a salt of at least 8 bytes from `RandomNumberGenerator`. ([Bowden: Creating hashes in .NET](https://damienbod.com/2024/07/01/creating-hashes-in-net/)) Bowden's 2024 figure of "more than 10000 iterations" is a floor, not a target. No current source states a figure for .NET 10, so justify the count for each application. Argon2id lost because the BCL doesn't ship it.

### Post-quantum: `IsSupported` gates every use

**Use `MLKem` and `MLDsa`, and check `IsSupported` at every call site.** The algorithms come from the operating system, not the runtime. Linux needs OpenSSL 3.5 or later, and Apple platforms, Android, and the browser have none of them. ([Microsoft Learn: Cross-platform cryptography in .NET](https://learn.microsoft.com/dotnet/standard/security/cross-platform-cryptography))

Both classes shipped without `[Experimental]`, but four export and import members keep it under diagnostic `SYSLIB5006`. So code that moves a key out of the process still opts into an experimental API. Windows support arrived with the same release, and the Learn compatibility table doesn't reflect it yet. `SlhDsa` and `CompositeMLDsa` ship with `[Experimental]` on the classes themselves, so they stay out of the GA set. ([Barton: Post-Quantum Cryptography in .NET](https://devblogs.microsoft.com/dotnet/post-quantum-cryptography-in-dotnet/))

### Servicing: take the monthly patch

**Rebuild and redeploy after every Patch Tuesday, whether or not your code changed.** Security fixes ship on "always the second Tuesday of the month". The platform's instruction is to "Regularly install servicing updates to ensure that your apps are in a secure and supported state". ([Microsoft Learn: .NET releases, patches, and support](https://learn.microsoft.com/dotnet/core/releases-and-support)) A container image keeps the runtime it was built with until something rebuilds it. ([aspnet-core.md: Hosting and deployment](aspnet-core.md#hosting-and-deployment))

CVE-2025-55315 shows what a slow cadence costs. It's a request-smuggling flaw in ASP.NET Core, and Microsoft gave it "a CVSS score of 9.9, their highest ever". An attacker could "Login as a different user", "Make an internal request (SSRF)", or "Bypass CSRF checks". ([Lock: Understanding the worst .NET vulnerability ever: request smuggling and CVE-2025-55315](https://andrewlock.net/understanding-the-worst-dotnet-vulnerability-request-smuggling-and-cve-2025-55315/)) The fix shipped on a Tuesday. Your pipeline decides when it reaches production.

**Target a supported framework.** .NET 8 and .NET 9 both reach end of support on 2026-11-10. After that date, a project on either gets no more patches. ([Damkewala: .NET STS releases supported for 24 months](https://devblogs.microsoft.com/dotnet/dotnet-sts-releases-supported-for-24-months/)) `net10.0` is supported until November 2028.

## Coming next (preview, not yet the opinion)

.NET 11 adds:

- `X25519DiffieHellman`
- Unpadded AES key wrap on `Aes`
- TLS handshake hardening
- Certificate-validation alerts on Linux
- Channel binding validation on Unix

([Microsoft Learn: What's new in .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/overview))

C# 16 previews a redesign of `unsafe` as a compiler-enforced contract between callers and API authors, targeted for production use in .NET 12. ([Lander: Improving C# Memory Safety](https://devblogs.microsoft.com/dotnet/improving-csharp-memory-safety/))
