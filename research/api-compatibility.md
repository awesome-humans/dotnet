---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-25
sources:
  [
    ms-learn,
    dotnet-blog,
    aaron-stannard,
    jon-skeet,
    meziantou,
    andrew-lock,
    mark-seemann,
    house,
  ]
---

# Public API compatibility for .NET libraries

Treat a published library's public API as a contract that the pack step checks.
Turn on package validation **with a baseline version**, since validation without a baseline let a binary break through in the test below.
Track the API surface in the repository so every change to it shows up in review.
Design by extension only: add overloads and types rather than changing the ones consumers compiled against.
Signal breaks through SemVer on the package and a major-only `AssemblyVersion`, and retire APIs through `[Obsolete]` over successive major versions rather than deleting them.

It applies to anyone who ships a package, or a library another team consumes as a binary.
An application that recompiles everything it references on each build pays no binary cost, only source and behavioural ones.

**The topic has no opinion file.** [ci.md](../opinions/ci.md) asks NuGet packages to ship "with package validation enabled", which this research finds insufficient on its own.
Neither [templates/Directory.Build.props](../templates/Directory.Build.props) nor [templates/projects/Example.Library.csproj](../templates/projects/Example.Library.csproj) enables validation at all.

## Three kinds of break, and which ones a tool can see

`ms-learn` defines the taxonomy that everything else here uses ([Microsoft Learn: Breaking changes and .NET libraries](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)):

- **Source:** existing code fails to compile after an upgrade, for example through a new ambiguous overload or a renamed parameter that callers name. It is "the least disruptive", because the consumer finds out at build time and can fix their own code.
- **Behavioural:** it compiles and runs, but differently. It is "the most common type", and "even a bug fix can qualify as a breaking change if users relied on the previously broken behavior."
- **Binary:** assemblies compiled against the old version "are no longer able to call the API". It surfaces as `MissingMethodException` at runtime, often in someone else's library.

The binary kind is the one that reaches people who did nothing wrong.
Jon Skeet's diamond-dependency case shows why: an application references two libraries that each depend on a third, at different major versions, and only one copy can load.
"If Lib1 does call IClock.Now, the runtime will throw a MissingMethodException. Ouch." ([Skeet: Versioning limitations in .NET](https://codeblog.jonskeet.uk/2019/06/30/versioning-limitations-in-net/), 2019-06-30)
Tooling sees binary breaks well, source breaks partly, and behavioural breaks not at all, which is why design discipline carries the rest.

## Verified: the optional-parameter trap, and what catches it

Adding an optional parameter to a public method is the canonical break that looks safe.
`ms-learn` uses it as the worked example for baseline validation ([Microsoft Learn: Baseline package validator](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator)).
This research reproduced it on SDK 10.0.400 with a package `Compat.Demo`, changing `Greet(string name)` in 1.0.0 to `Greet(string name, string greeting = "Hello")` in 1.1.0:

| Check                                                                        | Result                                                                                               |
| ---------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| Consumer recompiled against 1.1.0                                            | Builds and runs: a **source-compatible** change                                                      |
| Consumer built against 1.0.0, run on 1.1.0                                   | `MissingMethodException: Method not found: 'System.String Compat.Demo.Greeter.Greet(System.String)'` |
| `dotnet pack` with `EnablePackageValidation` only                            | **Packs without error:** 1.1.0 ships                                                                 |
| `dotnet pack` with `PackageValidationBaselineVersion=1.0.0`                  | `error CP0002: Member 'string Compat.Demo.Greeter.Greet(string)' exists on [Baseline] ...`           |
| Extend-only fix: keep `Greet(string)`, add `Greet(string, string)` beside it | Baseline validation passes, and the 1.0.0-built consumer runs                                        |

The third row is the finding that matters for `ci.md`.
Package validation without a baseline checks a package against itself: consistent surfaces across target frameworks, and no missing assets.
Only the baseline compares it with what consumers already have.
Package validation has shipped in the SDK since .NET 6 ([.NET Blog: Package Validation](https://devblogs.microsoft.com/dotnet/package-validation/), 2021-06-22), and it's still opt-in.
Meziantou's walkthrough covers the same setup, including `EnableStrictModeForBaselineValidation` ([Meziantou: Detecting breaking changes between two versions of a NuGet package at packaging time](https://www.meziantou.net/detecting-breaking-changes-between-two-versions-of-a-nuget-package-at-packaging.htm), 2023-01-09).

**Recommendation:** in every packable project, set `EnablePackageValidation` and `PackageValidationBaselineVersion`, and roll the baseline forward after each release.
When a break is intentional, generate the suppression file with `GenerateCompatibilitySuppressionFile=true` and commit `CompatibilitySuppressions.xml`, which "should be checked into source control to document and review the breaking changes made in a PR" ([Microsoft Learn: Baseline package validator](https://learn.microsoft.com/dotnet/fundamentals/apicompat/package-validation/baseline-version-validator)).

## Make the API surface visible in review

Baseline validation fails the pack. It doesn't show a reviewer that the surface changed while the pull request is still open, and it says nothing about additions.
Three tools write the surface into the repository, and they differ in where the check runs:

- **`Microsoft.CodeAnalysis.PublicApiAnalyzers`** (5.6.0 as of 2026-09-25, now maintained in `dotnet/roslyn`): `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt` beside the project. RS0016 fires on any public symbol missing from the files, and RS0017 on an entry that no longer exists. With `#nullable enable` at the top of the files, RS0036 and RS0037 track nullable annotations too.
- **PublicApiGenerator with Verify:** a test renders the surface as C# text and snapshots it, and the test fails with a diff when it changes. Andrew Lock prefers this approach. He is candid that it detects rather than prevents: "Neither of these approaches will prevent breaking changes to your API, but they both provide a way for you to document and track what your public API is." ([Andrew Lock: Preventing breaking changes in public APIs with PublicApiGenerator](https://andrewlock.net/preventing-breaking-changes-in-public-apis-with-publicapigenerator/), 2024-09-10)
- **Meziantou.Framework.PublicApiGenerator:** a generator that emits compilable C# for the surface, verified in CI with `--verify-no-change`. "The generated file becomes a small contract that lives next to your source code. If the file changes, reviewers know the public API changed too." ([Meziantou: Generate and review the public API of a .NET library](https://www.meziantou.net/generate-and-review-the-public-api-of-a-dotnet-library.htm), 2026-08)

**Recommendation: PublicApiAnalyzers, against Lock's stated preference.**
His objections are specific: "I find it tedious that the analyzer breaks the build _every_ time I add a public type", the Shipped/Unshipped split seems "completely unnecessary" to him, and he dislikes the file format, where PublicApiGenerator's output "looks a lot like 'normal' C#."
The first objection is the reason to choose the analyzer here.
The **House:** rule already makes warnings errors on every workstation, and a new public member is a commitment worth one deliberate line in a file.
The Shipped/Unshipped split also records which additions haven't been released yet, and they can still change freely.
The analyzer also has dedicated rules, RS0036 and RS0037, for nullable annotations in the declared surface.
Meziantou's generator is the choice for someone who shares Lock's taste but wants the check outside the test run.
All three are detection. None of them replaces baseline validation, which is the only check that compares against what shipped.

## Design rules: extend, don't modify

**Previous shapes are immutable. New capability arrives through new members, ideally opt-in.**
Aaron Stannard's formulation: "Previous functionality, schema, or behavior is immutable and not open for modification. New functionality, schema, or behavior can be introduced through new constructs only and ideally those should be opt-in." Removal "can only be removed after a long period of time and that is measured in years." ([Stannard: Professional Open Source: Extend-Only Design](https://aaronstannard.com/extend-only-design/), 2021-12-27)
Mark Seemann gives the same rule a formal shape: "In order to keep backwards compatibility, you can weaken preconditions or strengthen postconditions." ([Seemann: Backwards compatibility as a profunctor](https://blog.ploeh.dk/2021/12/13/backwards-compatibility-as-a-profunctor/), 2021-12-13) **Limit:** the framing is language-agnostic, so it corroborates Stannard rather than standing alone.

The `ms-learn` rules table is the reference for individual changes ([Microsoft Learn: .NET API changes that affect compatibility](https://learn.microsoft.com/dotnet/core/compatibility/library-change-rules), updated 2026-04-10). Microsoft says library authors "can also use these criteria to evaluate changes to their libraries".
The entries most likely to surprise someone who knows the obvious ones:

- **Parameter names are contract.** Renaming one, even changing its case, is disallowed because callers use named arguments and late binding.
- **Adding `virtual` to a member is disallowed**, as is removing it: callers compiled earlier may call it non-virtually.
- **Sealing a type is disallowed** if it has accessible constructors.
- **Adding an interface member is a judgement call.** A default implementation avoids compile failures. But since C# 13, a `ref struct` that implements the interface must implement every instance member explicitly, so a new default member is a source break for it. `ms-learn` suggests considering an abstract base class where the contract is expected to grow ([Microsoft Learn: Breaking changes and .NET libraries](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes)).
- **Adding an instance field to a struct with no non-public fields** breaks callers that declare it without a constructor call. It's a binary break for callers using `[SkipLocalsInit]`.
- **Changing a parameter default value isn't a binary break**, because the old value is baked into callers, but recompiled callers silently get the new behaviour. Removing a default is a source break.
- **Changing an overload set is where binary and source compatibility come apart.** Adding an overload that "precludes an existing overload and defines different behavior" is disallowed, even though existing binaries keep calling the old one.

Wire and persistence formats follow the same rule with higher stakes: Stannard calls wire compatibility "the most important type of compatibility if you need it" ([Stannard: Maintaining API, Binary, and Wire Compatibility](https://aaronstannard.com/oss-compatibility-standards/), 2021-05-04).
Serialization formats belong to a separate topic.

## .NET 10 and C# 14 hazards, verified

Checked on SDK 10.0.400:

- **A new `ReadOnlySpan<T>` overload captures array callers.** With `Describe(IEnumerable<int>)` shipped, adding `Describe(ReadOnlySpan<int>)` gives two different results for the call `Describe(int[])`:
  - A C# 13 caller **fails to compile** with CS0121, an ambiguous call, which is a source break.
  - A C# 14 caller **binds to the span overload**, a silent behavioural change on recompile. The first-class span conversions behind it are listed among the C# 14 breaking changes, as [csharp.md](../opinions/csharp.md) notes.
- **`[OverloadResolutionPriority]` is the tool built for this.** With `[OverloadResolutionPriority(1)]` on the span overload, C# 13 and C# 14 callers both bind to it without ambiguity. `ms-learn`: "Its primary use case is for library authors to write better performing overloads while still supporting existing code without breaks", with the caution that authors "should use this attribute as a last resort" ([Microsoft Learn: OverloadResolutionPriority attribute](https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/general#overloadresolutionpriority-attribute)). Adding it to an existing overload, or changing its value, is itself a possible source break under the rules table. Baseline validation sees none of this, because a new overload is an addition.
- **Changing a `params` collection type**, say from `params T[]` to `params ReadOnlySpan<T>`, changes the IL signature and is a binary break.
- **Converting classic extension methods to C# 14 extension blocks is safe**: "Both forms generate identical IL, so callers can't distinguish between them." The rules table marks it allowed, so [csharp.md](../opinions/csharp.md)'s extension-members opinion costs a library nothing to adopt.
- **The `field` keyword is safe for callers.** Its synthesised backing field is named `<Name>k__BackingField`, exactly like an auto-property. The one hazard is replacing a hand-written backing field such as `_name`: anything that reflected over that private field by name, a field-based serializer included, loses it. Several third-party write-ups describe the name as newly mangled, and they are wrong.

## Versioning: SemVer on the package, major only on the assembly

- **Package version:** SemVer 2.0.0, with a prerelease suffix for anything unstable.
- **`AssemblyVersion`:** "CONSIDER only including a major version in the AssemblyVersion", so 1.0 and 1.0.1 both carry 1.0.0.0 and fewer binding redirects are needed. Keep its major in step with the package major, and never freeze it.
- **`AssemblyInformationalVersion`:** leave it to Source Link to generate.
- **Assembly name and strong-naming key:** never change them, and never add or remove the key.

([Microsoft Learn: Versioning and .NET libraries](https://learn.microsoft.com/dotnet/standard/library-guidance/versioning), [Microsoft Learn: Breaking changes and .NET libraries](https://learn.microsoft.com/dotnet/standard/library-guidance/breaking-changes))

**Where the sources disagree: how strict SemVer should be.**

- **Seemann:** "I find Semantic Versioning useful as it is." He goes as far as treating deprecation itself as a possible major bump ([Seemann: Phased breaking changes](https://blog.ploeh.dk/2025/03/17/phased-breaking-changes/), 2025-03-17).
- **Stannard:** "Strict SemVer is hilariously impractical". Breaks to experimental, deprecated or low-impact APIs can ship without a major bump, because "the right thing is always setting the user's expectations correctly" ([Stannard: Practical vs. Strict Semantic Versioning](https://aaronstannard.com/oss-semver/), 2021-05-31).
- **Skeet** sides with strictness for a structural reason: SemVer "makes the version number a purely technical decision, not a marketing one", and the runtime can't isolate two majors of one package in a process.

**Weighing it: strict for everything baseline validation can see, with Stannard's carve-out made explicit.**
Tooling now makes strictness cheap: a binary break fails the pack, and a suppression file forces the decision into review.
Stannard's legitimate case, unstable APIs that shouldn't freeze the major version, is exactly what `[Experimental]` exists for. `ms-learn`: "If you want to ship a stable package that contains some preview quality APIs, you should mark those APIs using `[Experimental]`. Make sure to use your own diagnostic ID" ([Microsoft Learn: Preview APIs](https://learn.microsoft.com/dotnet/fundamentals/runtime-libraries/preview-apis)).
Consumers must opt into each diagnostic ID, so a break there was agreed to in advance and the carve-out is visible in the code rather than in the maintainer's judgement.

## Deprecation: phase it across majors

**Mark, then escalate, then delete, one major version apart.**
Seemann's sequence, worked in C#: `[Obsolete]` as a warning in major N, `error: true` in N+1, deletion in N+2. "Only in version 5.0.0 can you entirely delete it... this whole process may take years. I find that appropriate." ([Seemann: Phased breaking changes](https://blog.ploeh.dk/2025/03/17/phased-breaking-changes/))
Give each obsoletion its own `DiagnosticId` and a `UrlFormat` pointing at migration notes. Consumers can then suppress one retirement without silencing all of CS0618, and every warning links to what replaces it ([Microsoft Learn: ObsoleteAttribute](https://learn.microsoft.com/dotnet/api/system.obsoleteattribute)).
For low- and mid-level libraries (serializers, ORMs, frameworks), `ms-learn` suggests keeping obsolete members indefinitely, since removal is a binary break for consumers who can't control their transitive graph.
That is Stannard's "measured in years" taken to its end.

When a whole major is a rewrite, Skeet's alternative deserves consideration: publish it as a new package ID, so that both majors can load side by side ([Skeet: Options for .NET's versioning issues](https://codeblog.jonskeet.uk/2019/10/25/options-for-nets-versioning-issues/), 2019-10-25).

## Closing the loop

- **Ready for `resolve-research` as a new opinion file,** likely `opinions/library-compatibility.md` or similar. The roster carries it: `ms-learn` for the rules and tooling, `aaron-stannard` and `mark-seemann` for design and deprecation, `jon-skeet` for the runtime reasons, `meziantou` and `andrew-lock` for tooling practice. A new file needs its Scope and layout entries in `README.md`.
- **`ci.md` needs a correction** in that pass: "package validation enabled" should become package validation with a baseline version, per the reproduction above.
- **Template candidate:** a packable-library block for `templates/Directory.Build.props` or `Example.Library.csproj`, with `EnablePackageValidation`, the baseline, and PublicApiAnalyzers pinned in `Directory.Packages.props`.
- **Not verified in this pass:** the rules table's exact wording on the C# 14 span conversion break (the compiler breaking-changes page moved, and the behaviour was verified by compiling instead), and `Microsoft.DotNet.ApiCompat.Tool` for non-packable assemblies.
- **Unvetted leads, not used:** Bill Wagner's and Phil Haack's 2010 posts on optional-parameter versioning, and Claire Novotny's writing on strong naming. The roster covered the topic without them.
