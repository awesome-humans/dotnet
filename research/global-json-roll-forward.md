---
targets: [net10.0, csharp-14, fsharp-10]
last-reviewed: 2026-09-14
sources: [ms-learn]
---

# global.json roll-forward: when to narrow past `latestFeature`

Keep `latestFeature` as the default. Use `latestPatch` only when the build must not pick up new SDK tooling mid-release, **and** the floor sits in a feature band that's still serviced.
For .NET 10, that means `10.0.1xx` or `10.0.4xx`, because `10.0.2xx` and `10.0.3xx` are already out of support.
Narrowing to an unsupported band swaps a possible build break for a certain one: no more security patches.

Two consequences follow:

- `latestPatch` costs no security fixes **only if** the floor's band is still serviced and has a current patch for the policy to resolve to. The policy alone promises nothing.
- When a band update breaks a build, use `SdkAnalysisLevel` first, but only where its documentation lists the behaviour that broke. It doesn't make a newer SDK behave like an older band in general. Otherwise, use the compatibility switch for that specific behaviour, and narrow `rollForward` only when no switch exists.

## Why the feature band is where compatibility stops

An SDK version `x.y.znn` is major, minor, feature band (`z`, the hundreds digit), and patch (`nn`).
Bands ship about quarterly, in step with Visual Studio. Patches ship monthly, on the second Tuesday.
([Microsoft Learn: Releases and support for .NET](https://learn.microsoft.com/dotnet/core/releases-and-support), [Microsoft Learn: .NET SDK, MSBuild, and Visual Studio versioning](https://learn.microsoft.com/dotnet/core/porting/versioning-sdk-msbuild-vs))

The docs make different promises for each:

- **Patches:** "Servicing updates maintain compatibility."
- **Bands:** SDK updates "sometimes include new features or new versions of components like MSBuild and NuGet. These new features or components **might be incompatible** with the versions that shipped in previous SDK updates for the same major or minor version."

So the toolchain is allowed to change at a band boundary, and `latestFeature` agrees to cross it unattended.
That's usually the right agreement, but choose it explicitly.

The breaking-change catalogue indexes its "Version introduced" column by band:

- `dotnet sln add` tightened file-name validation in 9.0.2xx.
- MSBuild's custom-culture resource handling arrived in SDK 9.0.200, then became opt-in behind `EnableCustomCulture` in 9.0.300.

One `"version": "9.0.100", "rollForward": "latestFeature"` covered both behaviours, so two machines a quarter apart built the same commit differently.
([Microsoft Learn: .NET 9 breaking changes](https://learn.microsoft.com/dotnet/core/compatibility/9.0), [Microsoft Learn: MSBuild custom culture resource handling](https://learn.microsoft.com/dotnet/core/compatibility/sdk/10.0/msbuild-custom-culture))

Patches are lower risk, not no risk.
`NuGetAuditMode` defaulted to `all` in .NET 9 previews and reverted to `direct` in the **9.0.101 SDK**, a patch-level behaviour change.
Servicing regressions happen too: .NET 10.0.4 crashed the debugger on macOS under VS Code, and an out-of-band 10.0.5 fixed it on 2026-03-12.
([Microsoft Learn: `dotnet restore` audits transitive packages](https://learn.microsoft.com/dotnet/core/compatibility/sdk/10.0/nugetaudit-transitive-packages), [dotnet/core: .NET 10 known issues](https://github.com/dotnet/core/blob/main/release-notes/10.0/known-issues.md), **unvetted**: first-party release notes on GitHub, not a roster source)

## The cost of narrowing is a support cliff, not lost patches

Bands have different support windows:

- The `1xx` band is serviced for the life of the major version.
- The final band is serviced for the life of the matching runtime.
- The bands between are supported only until about when the next band ships.

As of 2026-09-14 ([Microsoft Learn: .NET SDK, MSBuild, and Visual Studio versioning](https://learn.microsoft.com/dotnet/core/porting/versioning-sdk-msbuild-vs)):

| SDK band   | Ship date | Supported until |
| ---------- | --------- | --------------- |
| `10.0.1xx` | Nov 2025  | Nov 2028        |
| `10.0.2xx` | Mar 2026  | May 2026        |
| `10.0.3xx` | May 2026  | Aug 2026        |
| `10.0.4xx` | Aug 2026  | Nov 2028        |

So a `latestPatch` floor of `10.0.200` isn't conservative.
It pins an SDK that stopped receiving security fixes in May 2026.

On a serviced band, the security objection disappears.
The 10.0.11 servicing release on 2026-08-11 shipped **three** SDKs, 10.0.400, 10.0.303, and 10.0.111, with the same ten CVE fixes.
A repository on `latestPatch` at `10.0.100` took 10.0.111, and got every fix without the 4xx band's MSBuild and NuGet changes.
([dotnet/core: .NET 10.0.11 release notes](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.11/10.0.11.md), **unvetted** as above)

The next release shows the other side.
10.0.12, on 2026-09-08, shipped only **two** SDKs: 10.0.401 and 10.0.112.
The 3xx band had aged out, as the table predicts, and got nothing, so a repository on `latestPatch` at `10.0.300` went from fully patched to frozen in one month without a change.
The floor's band matters more than the policy on it.
([dotnet/core: .NET 10.0.12 release notes](https://github.com/dotnet/core/blob/main/release-notes/10.0/10.0.12/10.0.12.md), **unvetted** as above)

This rules out `rollForward: disable` for most repositories. It pins one patch, and a servicing update **removes the patch it supersedes** within the same band.
The pinned SDK disappears from any machine that updates, although different bands install side by side.
Visual Studio keeps one SDK copy and replaces it on upgrade, across bands and major versions, unless the SDK was installed stand-alone.
([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))

## Where the risk lies: CI

`actions/setup-dotnet` installs whatever SDK the `latest*` policy resolves to.
Under `latestFeature`, CI's SDK changes on the day a new band ships, with no commit and no review.
That's how a build passes one day and fails the next, so choose the policy deliberately, even if you keep `latestFeature`.
([actions/setup-dotnet: README](https://github.com/actions/setup-dotnet#using-the-global-json-file-input), **unvetted**: first-party action documentation)

One reported case, unconfirmed: SDK 10.0.300 reportedly doubled `.slnx` build times compared with 10.0.204, and the reporter worked around it with a narrower `global.json` pin.
([dotnet/sdk#54344](https://github.com/dotnet/sdk/issues/54344), **unvetted**: an unresolved issue report, not verified here)

## The better option, where it applies: `SdkAnalysisLevel`

`SdkAnalysisLevel`, new in .NET 9, takes a feature band as its value. It reverts a newer SDK to older behaviour **only for the behaviours its documentation lists**, not the whole band.
Its design document names this exact case: users "are often not in control of the versions of the SDK used to build their code".

```xml
<PropertyGroup>
  <SdkAnalysisLevel>10.0.100</SdkAnalysisLevel>
</PropertyGroup>
```

Under .NET 10, that value keeps `PrunePackageReference` off by default, keeps the legacy restore resolver with lock files, and keeps a versionless `PackageReference` at warning NU1603 instead of error NU1015.
It has two limits:

- It covers only the documented behaviours, mostly restore and NuGet diagnostics. An MSBuild change like custom-culture handling needs its own flag.
- A value expires after three major releases.

([Microsoft Learn: MSBuild properties for .NET SDK projects](https://learn.microsoft.com/dotnet/core/project-sdk/msbuild-props#sdkanalysislevel), [dotnet/designs: SDK Analysis Level Property and Usage](https://github.com/dotnet/designs/blob/main/proposed/sdk-analysis-level.md); the design document is **unvetted**)

For a listed behaviour, prefer it to a narrower `rollForward`. It names the declined behaviour in the project file, where the next reader finds it, and lets the SDK roll forward for everything else.
For anything else, use the switch for that behaviour if one exists, such as setting `NuGetAuditMode` explicitly instead of inheriting a default that changed in a patch. Narrow `rollForward` only as a last resort.

## The one case where `disable` is the documented answer

Package lock files.
Microsoft Learn says: "A package lock file doesn't isolate the dependency graph from SDK changes. The .NET SDK includes NuGet and controls parts of the restore process. To prevent an SDK update from changing the lock file unexpectedly, pin the SDK version and disable roll-forward."
The `rollForward` table has the same footnote against `disable`.
([Microsoft Learn: Upgrade to a new .NET version](https://learn.microsoft.com/dotnet/core/install/upgrade#package-lock-files), [Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))

This applies only to a repository that commits `packages.lock.json` and restores in locked mode.
Central Package Management alone doesn't create the coupling, because the SDK's resolver is what changes the graph.

Pair a narrow pin with two .NET 10 fields:

- `sdk.paths` lets the repository carry its own SDK and search it before the machine's, which reproduces builds better than any `rollForward` value.
- `errorMessage` replaces the resolver's generic failure with an instruction.

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

| Situation                                                           | Policy                                                                                            | Floor                                                |
| ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Default, including this repository                                  | `latestFeature`                                                                                   | Lowest band that has what you need                   |
| A band update broke a behaviour `SdkAnalysisLevel` lists            | `latestFeature` + `SdkAnalysisLevel`                                                              | Unchanged                                            |
| A band update broke anything else                                   | `latestFeature` + that behaviour's own switch, or a narrower `rollForward` where no switch exists | Unchanged, unless narrowing                          |
| Build tooling must not move during a release, audit, or attestation | `latestPatch`                                                                                     | A band still serviced today: `1xx` or the final band |
| Committed `packages.lock.json` with locked-mode restore             | `disable`, ideally with `sdk.paths`                                                               | Exact patch                                          |
| Reproducing a regression locally                                    | `disable`, temporarily                                                                            | The known-good patch                                 |

The `latestPatch` row is safe only when the band is still supported **and** has a current patch for the policy to resolve to.
`latestPatch` is a resolution policy, not a servicing guarantee. On an unsupported band, it resolves to the last patch that band will ever get: the unpatched pin it seems to avoid.

## A disagreement to record

Current documentation says that when `version` is set and `rollForward` is omitted, the default is **`patch`**.
Several secondary write-ups still say `latestPatch`, the historical default.
The docs win. The practical lesson: set `rollForward` explicitly instead of relying on a default that has changed.
([Microsoft Learn: global.json overview, Matching rules](https://learn.microsoft.com/dotnet/core/tools/global-json#matching-rules))

## What this reveals about the repository

The current pin is sound.
`global.json` and `templates/global.json` both sit at `10.0.400` with `latestFeature`, and `10.0.4xx` is the final .NET 10 band, supported to Nov 2028.
At that floor, `latestFeature` and `latestPatch` resolve identically today, so the choice expresses intent, not behaviour.

Four gaps to resolve:

1. **[project-structure.md](../opinions/project-structure.md) states the floor rule but not the exceptions.** It correctly treats the pin as a floor, and says nothing about band support windows, `SdkAnalysisLevel`, or lock files. A short block would cover them.
2. **[ci.md](../opinions/ci.md) says "`global.json` sets the SDK".** Under `latestFeature`, it sets a floor, and `setup-dotnet` picks the rest on the day it runs. The line needs that precision.
3. **[audit-freshness](../skills/audit-freshness/SKILL.md) and [verify-project](../skills/verify-project/SKILL.md) both report "a `rollForward` narrower than `latestFeature`".** If the opinion gains a legitimate narrow case, both need the exception, or they'll report a lock-file repository's correct `disable` as a defect.
4. **Aside, outside this topic:** The docs say `global.json` supports JavaScript and C#-style comments. [AGENTS.md](../AGENTS.md) exempts `templates/global.json` from the metadata header "because JSON has no comment syntax", which isn't true of this file. Look at it separately.

**Recommended next step:** `resolve-research`, to add items 1 to 3 to `opinions/project-structure.md` and `opinions/ci.md`.
No new opinion file is needed.

**Source sweep note:** This environment's network policy blocked direct fetches of `learn.microsoft.com`, `andrewlock.net`, and other roster domains.
Everything cited was read from first-party sources on `raw.githubusercontent.com`, where the Microsoft Learn pages are authored, so the `ms-learn` citations are the documentation itself.
The community sweep ran through search summaries only, so no roster blog is quoted.
Read Andrew Lock's "Exploring the new rollForward and allowPrerelease settings in global.json", the obvious citable cross-check, before promoting this.
