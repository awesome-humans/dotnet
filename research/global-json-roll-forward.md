---
targets: [net10.0, csharp-14, fsharp-10]
last-reviewed: 2026-09-14
sources: [ms-learn]
---

# global.json roll-forward: when to narrow past `latestFeature`

`latestFeature` stays the default, and `latestPatch` is the right choice in exactly one situation: the build must not pick up new SDK tooling mid-release, **and** the floor sits in a feature band that keeps getting serviced.
For .NET 10 that means `10.0.1xx` or `10.0.4xx` and nothing in between, because `10.0.2xx` and `10.0.3xx` are already out of support.
Narrowing to a band that has aged out trades a hypothetical build break for a real one: no more security patches.

Two things follow, and they are the practical heart of this topic.
`latestPatch` costs you no security fixes **provided** the floor sits on a band that's still serviced and a current patch is published in that band for the policy to resolve to; the policy on its own carries no such promise.
And when a band update does break a build, `SdkAnalysisLevel` is the first lever only where the behaviour that broke is one the property documents. It isn't a general "make this SDK behave like the older band" switch.
Outside that set, the lever is whatever compatibility switch governs the specific behaviour, and narrowing `rollForward` is what is left when there's no such switch.

## Why the feature band is where compatibility stops

The SDK version `x.y.znn` splits into major, minor, feature band (`z`, the hundreds digit) and patch (`nn`).
Bands ship roughly quarterly to line up with Visual Studio; patches ship monthly on the second Tuesday.
([Microsoft Learn: Releases and support for .NET](https://learn.microsoft.com/dotnet/core/releases-and-support), [Microsoft Learn: .NET SDK, MSBuild, and Visual Studio versioning](https://learn.microsoft.com/dotnet/core/porting/versioning-sdk-msbuild-vs))

The two carry different promises, stated plainly in the docs:

- **Patches:** "Servicing updates maintain compatibility."
- **Bands:** SDK updates "sometimes include new features or new versions of components like MSBuild and NuGet. These new features or components **might be incompatible** with the versions that shipped in previous SDK updates for the same major or minor version."

So a band boundary is the documented place where the toolchain is allowed to change under you, and `latestFeature` is an explicit agreement to cross it unattended.
That agreement is usually the right one, but choose it rather than inheriting it.

The breaking-change catalogue confirms it from the other side: its "Version introduced" column is indexed by band.
`dotnet sln add` tightened file-name validation in 9.0.2xx.
MSBuild's custom-culture resource handling was introduced in SDK 9.0.200 and then made opt-in, behind `EnableCustomCulture`, in SDK 9.0.300.
One `"version": "9.0.100", "rollForward": "latestFeature"` covered both behaviours, so two machines a quarter apart built the same commit differently.
([Microsoft Learn: .NET 9 breaking changes](https://learn.microsoft.com/dotnet/core/compatibility/9.0), [Microsoft Learn: MSBuild custom culture resource handling](https://learn.microsoft.com/dotnet/core/compatibility/sdk/10.0/msbuild-custom-culture))

Patches aren't perfectly inert either, so the accurate rule is that they are lower risk rather than no risk.
`NuGetAuditMode` had its default changed to `all` during .NET 9 previews and reverted to `direct` in the **9.0.101 SDK**, a patch-level behavioural change.
Servicing regressions happen too: .NET 10.0.4 shipped a debugger crash on macOS under VS Code, fixed by an out-of-band 10.0.5 on 2026-03-12.
([Microsoft Learn: `dotnet restore` audits transitive packages](https://learn.microsoft.com/dotnet/core/compatibility/sdk/10.0/nugetaudit-transitive-packages), [dotnet/core: .NET 10 known issues](https://github.com/dotnet/core/blob/main/release-notes/10.0/known-issues.md), **unvetted**: first-party release notes on GitHub rather than a roster source)

## The cost of narrowing is a support cliff, not lost patches

Feature bands don't all get the same support window.
The `1xx` band is serviced for the life of the major version, the final band of a major version is serviced for the life of the matching runtime, and the bands in between are supported only until roughly the next band ships.
As of 2026-09-14 ([Microsoft Learn: .NET SDK, MSBuild, and Visual Studio versioning](https://learn.microsoft.com/dotnet/core/porting/versioning-sdk-msbuild-vs)):

| SDK band   | Ship date | Supported until |
| ---------- | --------- | --------------- |
| `10.0.1xx` | Nov 2025  | Nov 2028        |
| `10.0.2xx` | Mar 2026  | May 2026        |
| `10.0.3xx` | May 2026  | Aug 2026        |
| `10.0.4xx` | Aug 2026  | Nov 2028        |

A `latestPatch` floor of `10.0.200` is therefore not a conservative choice.
It's a pin to an SDK that stopped receiving security fixes in May 2026.

Where the floor is on a serviced band, the security objection disappears entirely.
The 10.0.11 servicing release on 2026-08-11 shipped **three** SDKs at once: 10.0.400, 10.0.303 and 10.0.111, carrying the same ten CVE fixes.
A repository pinned `latestPatch` at `10.0.100` took 10.0.111 and got every fix while declining the MSBuild and NuGet changes in the 4xx band.
([dotnet/core: .NET 10.0.11 release notes](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.11/10.0.11.md), **unvetted** as above)

The next servicing release makes the same point from the other side.
10.0.12 on 2026-09-08 shipped only **two** SDKs, 10.0.401 and 10.0.112.
The 3xx band, which the table above gives as supported until August, had aged out and got nothing, so a repository pinned `latestPatch` at `10.0.300` went from fully patched to frozen in the space of one month without changing a line.
That's the support cliff as an observed event rather than a projection, and it's the reason the floor's band matters more than the policy on it.
([dotnet/core: .NET 10.0.12 release notes](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.12/10.0.12.md), **unvetted** as above)

Note what this rules out. `rollForward: disable` pins a single patch, and a servicing update **removes the patch it supersedes** within the same band.
The pinned SDK stops existing on the machine that updates, while band installs do sit side by side.
Visual Studio makes it worse: it keeps one SDK copy and replaces it on upgrade, across bands and even across major versions, unless the SDK was installed stand-alone.
([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))

## Where the risk lies: CI

`actions/setup-dotnet` honours the `latest*` policies and installs the SDK the policy resolves to.
Under `latestFeature`, the version CI builds with changes on the day a new band ships, with no commit and no review.
That's the mechanism behind "green yesterday, red today", so choose the policy deliberately even if the answer stays `latestFeature`.
([actions/setup-dotnet: README](https://github.com/actions/setup-dotnet#using-the-global-json-file-input), **unvetted**: first-party action documentation)

One reported case, useful as a shape rather than as a settled fact: SDK 10.0.300 was reported to roughly double `.slnx` build times against 10.0.204, and the reporter's mitigation was a narrower `global.json` pin.
([dotnet/sdk#54344](https://github.com/dotnet/sdk/issues/54344), **unvetted**: an unresolved issue report, not verified here)

## The better lever, where it applies: `SdkAnalysisLevel`

Introduced in .NET 9, `SdkAnalysisLevel` takes a feature band as its value and reverts a newer SDK to older behaviour **only for those behaviours its documentation lists**, not for the band as a whole.
Its design document names the exact scenario this topic is about: users "are often not in control of the versions of the SDK used to build their code".

```xml
<PropertyGroup>
  <SdkAnalysisLevel>10.0.100</SdkAnalysisLevel>
</PropertyGroup>
```

Under .NET 10 that value keeps `PrunePackageReference` off by default, keeps the legacy restore resolver with lock files, and keeps a versionless `PackageReference` at NU1603 warning rather than NU1015 error.
Two limits matter. The property covers only the behaviours in the documented table, mostly restore and NuGet diagnostics, so an MSBuild change like custom-culture handling needs its own flag instead.
And a value ages out after three major releases.
([Microsoft Learn: MSBuild properties for .NET SDK projects](https://learn.microsoft.com/dotnet/core/project-sdk/msbuild-props#sdkanalysislevel), [dotnet/designs: SDK Analysis Level Property and Usage](https://github.com/dotnet/designs/blob/main/proposed/sdk-analysis-level.md); the design document is **unvetted**)

Where the incompatibility is in that table, prefer it to a narrower `rollForward` because it's scoped and reviewable.
It names the behaviour you are declining in the project file, where the next reader will find it, and it leaves the SDK free to roll forward for everything else.
Where it isn't, the same preference for the scoped, reviewable option gives the order: the compatibility switch that governs the specific behaviour if one exists, and only then a narrower `rollForward`. Setting `NuGetAuditMode` explicitly, rather than inheriting a default that moved in a patch, is an example of the first.

## The one case where `disable` is the documented answer

Package lock files.
Microsoft Learn is unambiguous: "A package lock file doesn't isolate the dependency graph from SDK changes. The .NET SDK includes NuGet and controls parts of the restore process. To prevent an SDK update from changing the lock file unexpectedly, pin the SDK version and disable roll-forward."
The `rollForward` table carries the same footnote against `disable`.
([Microsoft Learn: Upgrade to a new .NET version](https://learn.microsoft.com/dotnet/core/install/upgrade#package-lock-files), [Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))

This only applies to a repository that commits `packages.lock.json` and restores in locked mode.
Central package management alone doesn't create the coupling, because the SDK's resolver is what changes the graph.

Two .NET 10 fields make a narrow pin survivable, and both are worth pairing with one.
`sdk.paths` lets the repository carry its own SDK and search it before the machine's, which is a stronger reproducibility guarantee than any `rollForward` value.
`errorMessage` replaces the resolver's generic failure with an instruction.

```json
{
  "sdk": {
    "version": "10.0.400",
    "rollForward": "disable",
    "paths": [".dotnet", "$host$"],
    "errorMessage": "This repository pins the SDK to keep packages.lock.json stable. Run ./install-sdk.sh."
  }
}
```

## Decision table

| Situation                                                          | Policy                                                                                            | Floor                                                |
| ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Default, including this repository                                 | `latestFeature`                                                                                   | Lowest band that has what you need                   |
| A band update broke a behaviour `SdkAnalysisLevel` documents       | `latestFeature` + `SdkAnalysisLevel`                                                              | Unchanged                                            |
| A band update broke anything else                                  | `latestFeature` + that behaviour's own switch, or a narrower `rollForward` where no switch exists | Unchanged, unless narrowing                          |
| Build tooling must not move during a release, audit or attestation | `latestPatch`                                                                                     | A band still serviced today: `1xx` or the final band |
| Committed `packages.lock.json` with locked-mode restore            | `disable`, ideally with `sdk.paths`                                                               | Exact patch                                          |
| Reproducing a regression locally                                   | `disable`, temporarily                                                                            | The known-good patch                                 |

The `latestPatch` row is safe only under both halves of its floor condition: the band is still in support, **and** the band has a current patch published for the policy to resolve to.
`latestPatch` is a resolution policy, not a servicing guarantee. On a band that has aged out it resolves to the newest patch that band will ever get, which is exactly the pin to unpatched tooling it looks like it avoids.

## A disagreement worth recording

Current documentation says that when `version` is set and `rollForward` is omitted, the default policy is **`patch`**.
Several secondary write-ups still say the default is `latestPatch`, which was the historical behaviour.
Weigh the docs, and take the practical lesson: state `rollForward` explicitly rather than relying on a default that has moved.
([Microsoft Learn: global.json overview, Matching rules](https://learn.microsoft.com/dotnet/core/tools/global-json#matching-rules))

## What this reveals about the repository

The current pin is sound and needs no change.
`global.json` and `templates/global.json` both sit at `10.0.400` with `latestFeature`, and `10.0.4xx` is the final .NET 10 band, supported to Nov 2028.
Under that floor, `latestFeature` and `latestPatch` resolve identically today, so the choice is about intent rather than behaviour.

Four gaps are worth resolving:

1. **[project-structure.md](../opinions/project-structure.md) states the floor rule but not the exceptions.** It's right that the pin is a floor and that chasing bands is pointless, and it says nothing about band support windows, `SdkAnalysisLevel`, or the lock-file case. A short block would close it.
2. **[ci.md](../opinions/ci.md) claims "`global.json` sets the SDK".** Under `latestFeature` it decides a floor, and `setup-dotnet` decides the rest on the day it runs. The line deserves the precision.
3. **[audit-freshness](../skills/audit-freshness/SKILL.md) and [verify-project](../skills/verify-project/SKILL.md) both treat "a `rollForward` narrower than `latestFeature`" as a finding.** If the opinion gains a legitimate narrow case, both need the carve-out, or they will report a lock-file repository's correct `disable` as a defect.
4. **Aside, outside this topic:** the docs state that `global.json` supports JavaScript and C#-style comments. [AGENTS.md](../AGENTS.md) exempts `templates/global.json` from the metadata header "because JSON has no comment syntax", which isn't true of this particular file. Worth a separate look, not an edit here.

**Recommended next step:** `resolve-research`, folding items 1 to 3 into `opinions/project-structure.md` and `opinions/ci.md`.
No new opinion file is needed.

**Source sweep note:** direct fetching of `learn.microsoft.com`, `andrewlock.net` and other roster domains was blocked by this environment's network policy.
Everything cited above was read from first-party sources reachable over `raw.githubusercontent.com`, which is where the Microsoft Learn pages are authored, so the `ms-learn` citations are the documentation itself.
The community sweep ran through search summaries only, so no roster blog is quoted here.
Andrew Lock's "Exploring the new rollForward and allowPrerelease settings in global.json" is the obvious citable cross-check and should be read before this is promoted.
