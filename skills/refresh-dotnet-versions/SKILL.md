---
name: refresh-dotnet-versions
description: Detect new .NET, C#, and F# releases and update all opinions and templates in this repository to target them, including the central NuGet package pins in templates/Directory.Packages.props. Use when a new .NET SDK/runtime, C# language version, or F# version has shipped, when a pinned package has a newer stable release, or when asked to check whether the repository targets the latest released versions.
license: See repository LICENSE
compatibility: Requires git and internet access
metadata:
  repo: dotnet-awesome-humans
  change-flow: branch-pr
---

# Refresh .NET versions

Bring every resource in this repository up to the **latest released** versions of .NET, C#, and F#, meaning GA and not preview. Also bring the NuGet packages pinned in `templates/Directory.Packages.props` up to their **latest stable** versions.

## Trigger

The .NET release watch GitHub Action, `.github/workflows/dotnet-release-watch.yml`, checks the official releases index daily. When the GA state changes, it opens a **trigger pull request** with the label `dotnet-release` on the branch `automation/dotnet-release-watch`.

- That pull request only updates the snapshot in `.github/state/dotnet-releases.json`.
- Run this skill against the pull request's branch to make the actual update, and merge both together.
- You can also run the skill on its own. The snapshot is also a fast offline answer to "Which GA versions does this repository know about?"

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, delegate the web lookups to worker agents: the version checks in step 1, the nuget.org lookups in step 4b, and the "What's new" reading in step 5. Only the editor decides what changes, and edits `opinions/` and `templates/`.

## Steps

1. **Determine the latest released versions.**
   - .NET: check <https://dotnet.microsoft.com/download/dotnet> and the release announcement on the .NET Blog (<https://devblogs.microsoft.com/dotnet/>).
   - C#: check <https://learn.microsoft.com/dotnet/csharp/whats-new/> for the latest released language version.
   - F#: check <https://learn.microsoft.com/dotnet/fsharp/whats-new/>.
   - Use GA releases only. Previews and release candidates never become the target, but you can mention them in a "coming next" note.
2. **Compare with the repository's current targets.** Search the `targets:` metadata across `opinions/`, the version pins in `templates/`, and the repository's own root `global.json`. The pins in `templates/` are the `global.json` SDK version, `<TargetFramework>` in project files, and `<LangVersion>` if it's pinned.
   - If everything already matches, the framework part of this run has nothing to do. Say so, but **still run step 4b**, because package pins drift on their own schedule. Also check step 4a for a band the repository now needs.
   - Report "up to date" and stop only when those checks also find nothing.
3. **Start a worktree** on the branch `refresh/dotnet-<version>`, following the worktree and branch name rules in [AGENTS.md](../../AGENTS.md).
4. **Update the template files** under `templates/`: the TFMs in the example projects, and anything else pinned to a version. Then update the two pins that drift independently of a .NET release:
   - **4a. The SDK minimum in both `global.json` files.** Leave `sdk.version` alone unless the repository needs something from a newer feature band.
     - The pinned value is a minimum. With `"rollForward": "latestFeature"`, it accepts that version, or any later band and patch on the machine, so a new band arrives without an edit.
     - Raising the minimum only narrows who can build. Pinning a patch, such as `10.0.412` instead of `10.0.400`, fails contributors who are one servicing release behind, for no benefit.
     - When a band ships something the repository uses, raise the minimum and give the reason in the pull request description in step 8. For example, the root `global.json` is at `10.0.400` because that's the first band that supports `#:include`.
     - The two files are independent. The root pin follows what `scripts/` needs, and the template pin follows what the example projects need. Use `latestFeature` roll-forward wherever the opinion says so.
     - `global.json` can't carry a comment header, so it has no `last-reviewed` date. The pinned value **is** the record.
     - Renovate never changes this field: `renovate.json` disables the `dotnet-sdk` dependency type, because a minimum under `latestFeature` doesn't need mechanical bumps. This skill is the only thing that changes it.
   - **4b. The package pins in `templates/Directory.Packages.props`.** Look up every `<PackageVersion>` on nuget.org, with the registration index (`https://api.nuget.org/v3/registration5-gz-semver2/<lowercased-id>/index.json`) or `dotnet package search <id>`.
     - Take the **latest stable** version only. Never take a prerelease, or a version whose TFM support falls below the repository's `targets:`.
     - When a package changes major version, read its release notes before you take it. A major version that changes the idiom an opinion teaches is a content change, not a version bump, so handle it in step 5.
     - Set the `Versions verified against nuget.org … on <date>` comment to the run date **whether or not any version changed**. That comment is the file's only freshness record.

5. **Update each opinion file** under `opinions/`:
   - Read the release's "What's new" pages from the first-party sources in [AWESOME-HUMANS.md](../../AWESOME-HUMANS.md), `dotnet-blog` and `ms-learn`. Read deep dives on individual features from the rest of the citable roster, such as `andrew-lock`'s "Exploring .NET" series when it's available.
   - Add new language and runtime features where they change an existing opinion, for example when a new syntax replaces an old idiom.
   - When a new feature needs a new opinion, create a stub with a `TODO` and a source link. Add its `README.md` index entries too, a linked Scope bullet and a Repository layout entry, because CI enforces them.
   - Update the `targets:` and `last-reviewed:` metadata on every file you change **or verify as unchanged**.
   - **Never remove or weaken `**House:**` content** ([HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works)). If a release makes one obsolete, note that the release supersedes it, and keep the marking.
6. **Verify the code samples** against the new targets if a .NET SDK is available. Assemble a scratch project from `templates/`, and run **`dotnet build` and `dotnet test`**, not `dotnet build` or `dotnet run` alone.
   - `dotnet run` runs the test project's entry point directly and bypasses the SDK's runner selection. It can pass while `dotnet test` fails, and `dotnet test` is what `templates/`, `opinions/ci.md`, and `audit-freshness` all use.
   - Treat "0 tests ran, exit code 0" as a failure. A test project that has lost its runner configuration reports exactly that.
   - If no SDK is available, flag the samples as unverified in the pull request description.
7. **Record the change** in the decision log of `AWESOME-HUMANS.md` only if the source roster changed. Otherwise, summarise it in the pull request.
8. **Open a pull request** to the default branch that describes:
   - The versions before and after: the framework, the SDK band, and each package pin that changed.
   - The opinions that changed, and the ones verified as unchanged.
   - Anything left as a TODO.

   A person reviews it before it becomes the opinion.

## Edge cases

- **Release timing:** A new major .NET version ships every November, along with a new C# version. In the middle of the year, expect only servicing releases. They don't change the framework targets, and usually not step 4a either, because a new feature band alone isn't a reason to raise the minimum. Package pins still change on their own schedule, so a mid-year run that only changes `Directory.Packages.props` is normal.
- **An unmaintained package:** If a package's latest stable release is much older than the repository's current `targets:`, the library is probably unmaintained. Leave the pin alone, and raise it for `vet-source` or opinion review, instead of silently changing the version.
- **Guidance lags the runtime:** If a feature is GA in the runtime, but the vetted sources haven't caught up, update the target version, and keep the old idiom with a note. Don't invent unsourced guidance.
- **STS or LTS:** Target the **latest release**, whatever its support track. Note the end of support in `opinions/project-structure.md`.
