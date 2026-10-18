---
name: refresh-dotnet-versions
description: Detect new .NET, C#, and F# releases and update all opinions and templates in this repository to target them, including the central NuGet package pins in templates/Directory.Packages.props. Use when a new .NET SDK/runtime, C# language version, or F# version has shipped, when a pinned package has a newer stable release, or when asked to check whether the repository targets the latest released versions.
license: See repository LICENSE
compatibility: Requires git and internet access
metadata:
  repo: awesome-humans/dotnet
  change-flow: branch-pr
---

# Refresh .NET versions

Move every resource to the **latest released** (GA) versions of .NET, C#, and F#, and every package pin in `templates/Directory.Packages.props` to its **latest stable** version.

## Trigger

The `.github/workflows/dotnet-release-watch.yml` workflow checks the official releases index daily. When the GA state changes, it opens a **trigger pull request** labelled `dotnet-release` on the branch `automation/dotnet-release-watch`.

- The trigger pull request only updates the snapshot in `.github/state/dotnet-releases.json`.
- Run this skill on that branch to make the update, and merge both together.
- You can also run the skill by itself. The snapshot answers "Which GA versions does this repository know about?" offline.

## Orchestration

As [AGENTS.md: Orchestration and model economy](../../AGENTS.md#orchestration-and-model-economy) describes, delegate the web lookups to worker agents: the version checks in step 1, the nuget.org lookups in step 4b, and the "What's new" reading in step 5. Only the editor decides what changes, and edits `opinions/` and `templates/`.

## Steps

1. **Find the latest released versions.**
   - .NET: <https://dotnet.microsoft.com/download/dotnet> and the release announcement on the [.NET Blog](https://devblogs.microsoft.com/dotnet/).
   - C#: <https://learn.microsoft.com/dotnet/csharp/whats-new/>.
   - F#: <https://learn.microsoft.com/dotnet/fsharp/whats-new/>.
   - Use GA releases only. A preview or release candidate can appear in a "coming next" note, never as the target.
2. **Compare with the current targets:** the `targets:` metadata in `opinions/`, the version pins in `templates/`, and the root `global.json`. The template pins are the `global.json` SDK version, `<TargetFramework>` in project files, and `<LangVersion>` if it's pinned.
   - If everything matches, the framework part has nothing to do. Say so, and **still run step 4b**, because package pins drift on their own schedule. Check step 4a for a band the repository now needs.
   - Report "up to date" and stop only when those checks find nothing too.
3. **Start a worktree** on the branch `refresh/dotnet-<version>`, following the rules in [AGENTS.md](../../AGENTS.md).
4. **Update the template files** under `templates/`: the TFMs in the example projects, and anything else pinned to a version. Then update the two pins that drift independently of a .NET release:
   - **4a. The SDK minimum in both `global.json` files.** Leave `sdk.version` alone unless the repository needs something from a newer feature band.
     - The value is a minimum. With `"rollForward": "latestFeature"`, it accepts that version or any later band and patch on the machine, so new bands arrive without an edit.
     - Raising the minimum only narrows who can build. Pinning a patch, such as `10.0.412` instead of `10.0.400`, fails contributors one servicing release behind.
     - When a band ships something the repository uses, raise the minimum, and give the reason in the pull request. For example, the root `global.json` is at `10.0.400` because that's the first band that supports `#:include`.
     - The two files are independent. The root pin follows what `scripts/` needs, and the template pin follows what the example projects need. Use `latestFeature` wherever the opinion says so.
     - `global.json` can't carry a comment header, so the pinned value **is** the record.
     - Renovate never changes this field: `renovate.json` disables the `dotnet-sdk` dependency type, because a minimum under `latestFeature` doesn't need mechanical bumps. Only this skill changes it.
   - **4b. The package pins in `templates/Directory.Packages.props`.** Look up every `<PackageVersion>` on nuget.org, with the registration index (`https://api.nuget.org/v3/registration5-gz-semver2/<lowercased-id>/index.json`) or `dotnet package search <id>`.
     - Take the **latest stable** version. Never take a prerelease, or a version whose TFM support falls below `targets:`.
     - Before you take a new major version, read its release notes. A major version that changes an idiom an opinion teaches is a content change: handle it in step 5.
     - Set the `Versions verified against nuget.org … on <date>` comment to the run date **whether or not any version changed**. It's the file's only freshness record.

5. **Update each opinion** under `opinions/`:
   - Read the release's "What's new" pages from the first-party sources, `dotnet-blog` and `ms-learn`. Read feature deep dives from the rest of the citable roster, such as `andrew-lock`'s "Exploring .NET" series.
   - Add a new language or runtime feature where it changes an opinion, for example a new syntax that replaces an old idiom.
   - When a feature needs a new opinion, create a stub with a `TODO` and a source link, and add its `README.md` Scope bullet and Repository layout entry, which CI enforces.
   - Update `targets:` and `last-reviewed:` on every file you change **or verify as unchanged**.
   - **Never remove or weaken a house block or region** ([HOUSE-OPINIONS.md: How this works](../../HOUSE-OPINIONS.md#how-this-works)). If a release makes one obsolete, note inside the block that the release supersedes it, and keep the marking. A template must still build with its house regions removed.
6. **If a .NET SDK is available, verify the code samples** against the new targets. Assemble a scratch project from `templates/`, and run **`dotnet build` and `dotnet test`**.
   - `dotnet run` calls the test project's entry point directly and skips the SDK's runner selection. It can pass where `dotnet test` fails, and `dotnet test` is what `templates/`, `opinions/ci.md`, and `audit-freshness` use.
   - Treat "0 tests ran, exit code 0" as a failure. It's what a test project reports after losing its runner configuration.
   - Without an SDK, flag the samples as unverified in the pull request.
7. **If the source roster changed, record it** in the `AWESOME-HUMANS.md` decision log. Otherwise, summarise the run in the pull request.
8. **Open a pull request** to the default branch, filling in the [`version-refresh.md`](https://github.com/awesome-humans/.github/blob/main/PULL_REQUEST_TEMPLATE/version-refresh.md) template, as [AGENTS.md](../../AGENTS.md) describes. It lists:
   - The versions before and after: the framework, the SDK band, and each package pin that changed.
   - The opinions that changed, and the ones verified as unchanged.
   - Whether the samples built and tested.
   - Anything left as a TODO.

   A person reviews it before it changes any opinion.

## Edge cases

- **Release timing:** A new major .NET version ships every November, with a new C# version. Mid-year releases are servicing releases. They don't change the framework targets, and rarely step 4a, because a new band alone isn't a reason to raise the minimum. A mid-year run that only changes `Directory.Packages.props` is normal.
- **An unmaintained package:** If a package's latest stable release is much older than the current `targets:`, the library is probably unmaintained. Leave the pin, and raise it for `vet-source` or opinion review.
- **Guidance lags the runtime:** If a feature is GA but the vetted sources haven't covered it, update the target, and keep the old idiom with a note. Don't write unsourced guidance.
- **STS or LTS:** Target the **latest release** on either track. Note its end of support in `opinions/project-structure.md`.
