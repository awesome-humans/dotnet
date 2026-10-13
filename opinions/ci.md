---
targets: [net10.0]
last-reviewed: 2026-10-09
last-used: 2026-09-26
sources: [meziantou, ms-learn, dotnet-blog, house]
---

# CI & automation

Treat supply-chain security as a requirement. Agents now write and run much of the code in a pipeline, so nobody reviews each step by hand.

## Opinions

- **Pin GitHub Actions to commit SHAs, not mutable tags.** A tag can move without warning, and a SHA can't. Automate the sweep across repositories. ([Meziantou: SHA pinning](https://www.meziantou.net/enable-sha-pinning-for-github-actions-across-personal-repositories.htm))
- **Never interpolate user input into a workflow script.** Pass it in an environment variable, and parse it in the script. Script injection is the most common GitHub Actions vulnerability. ([Meziantou: Safely passing extra arguments](https://www.meziantou.net/safely-passing-extra-arguments-in-github-actions-workflows-using-powershell.htm))
- **Pin the SDK and every package in CI.** `global.json` sets the SDK, and lock files or Central Package Management set the packages. A CI run must never pick up a version the repository didn't choose. ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json), [Meziantou: Faster and Safer NuGet restore using Source Mapping and Lock files](https://www.meziantou.net/faster-and-safer-nuget-restore-using-source-mapping-and-lock-files.htm))
- **Restrict what a package can run, not only which version restores.** A `PackageReference` can bring MSBuild props and targets, analyzers, and source generators. All of them run during restore, design-time build, and compile. A version pin decides which code you get, not whether it runs. List only the asset types a package needs:
  - Use `IncludeAssets="compile;runtime"` for a library you only call.
  - Use `PrivateAssets="all"` on build-only tooling, so it never flows to your consumers.

  Packages that are mostly build logic need these loosened. Re-test after you tighten them. ([Meziantou: Limit what NuGet packages can do in your project](https://www.meziantou.net/limit-what-nuget-packages-can-do-in-your-project.htm))

- **House:** Treat warnings as errors on every machine, not only in CI. A warning that fails CI but not your local build reaches a pull request before anyone sees it. Every suppression, whether `#pragma warning disable`, `[SuppressMessage]`, or a `<NoWarn>` entry, needs an inline comment that says why. Revert a suppression that has none.

## Pipeline shape

**Run four stages in order: restore, build, test, and publish. Each stage skips the work of the one before it.** Pass `--no-restore` to build, and `--no-build` to test and publish. Each stage then uses the previous stage's output instead of rebuilding with different flags.

Pass the same `--configuration` to every stage. `dotnet publish` defaults to `Release` for current target frameworks, but `dotnet build` and `dotnet test` default to `Debug`. A `--no-build` stage with a different configuration looks for output that doesn't exist. The example uses GitHub Actions, but the stages work on any CI platform. ([Microsoft Learn: dotnet publish](https://learn.microsoft.com/dotnet/core/tools/dotnet-publish), [Microsoft Learn: dotnet test](https://learn.microsoft.com/dotnet/core/tools/dotnet-test))

1. **Restore.** If the repository commits `packages.lock.json` (`RestorePackagesWithLockFile=true`), pass `--locked-mode`, so a stale lock file fails the build. Without a lock file, the flag fails every restore with `NU1004`. [templates/Directory.Build.props](../templates/Directory.Build.props) pins packages with Central Package Management and no lock files, so the example restores without it.
2. **Build.** Build once, in `Release`, with `-warnaserror`. Keep the switch even though the template sets `TreatWarningsAsErrors`. The property covers compiler and analyzer diagnostics. The switch also fails on MSBuild warnings, such as MSB3277 assembly version conflicts.
   - **House:** [templates/Directory.Build.props](../templates/Directory.Build.props) sets `TreatWarningsAsErrors` unconditionally, so warnings fail the build locally too. This replaces earlier advice to enforce warnings only in CI for faster local builds.
   - Enforce style the same way: run analyzers in the build, and add a `dotnet format --verify-no-changes` step. ([Meziantou: Enforce .NET code style in CI](https://www.meziantou.net/enforce-dotnet-code-style-in-ci-with-dotnet-format.htm), [Meziantou: The Roslyn analyzers I use](https://www.meziantou.net/the-roslyn-analyzers-i-use.htm))
3. **Test.** Run `dotnet test` on Microsoft.Testing.Platform (MTP). MTP needs the `test.runner` opt-in in `global.json`, as [testing.md](testing.md) describes. Without it, the SDK uses VSTest, which fails on .NET 10 before `--no-build` takes effect.
   - Upload TRX and coverage files as build artifacts, so you can diagnose a failure without a rerun.
   - Under xunit.v3, write TRX with `--report-xunit-trx` and `--results-directory`, as in the example. Upload the results even when the step fails, because the failed run is the one you need them for.
   - MTP rejects VSTest's `--logger trx`. For details, see [testing.md](testing.md). ([Microsoft Learn: What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview))
4. **Publish or pack.** Pass `--no-build`, and upload the output as the one artifact that deploy and release consume. Never rebuild for deployment.
   - Don't pass `--output`. With the artifacts layout from [templates/Directory.Build.props](../templates/Directory.Build.props), a single-TFM publish without a RID goes to `artifacts/publish/<Project>/release`. Otherwise, the last folder gets a `_<tfm>` or `_<rid>` suffix. Pack output goes to `artifacts/package/<configuration>`, with no project folder. For details, see [project-structure.md](project-structure.md).
   - Keep test projects out of the deploy artifact with `<IsPublishable>false</IsPublishable>`, as [templates/projects/Example.Library.Tests.csproj](../templates/projects/Example.Library.Tests.csproj) does. Without it, a solution-level publish writes the test host, and another copy of every referenced library, into `artifacts/publish`. ([Microsoft Learn: Artifacts output layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output))

```yaml
# Least privilege: unless the repository says otherwise, the default token can
# write to contents, and a build job doesn't need to.
permissions:
  contents: read

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7
      - uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68 # v6
        with:
          global-json-file: global.json
      - run: dotnet restore
      - run: dotnet build --no-restore --configuration Release -warnaserror
      - run: dotnet test --no-build --configuration Release --report-xunit-trx --results-directory artifacts/test-results
      - uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        if: ${{ !cancelled() }}
        with:
          name: test-results
          path: artifacts/test-results
      - run: dotnet publish --no-build --configuration Release
      - uses: actions/upload-artifact@ea165f8d65b6e75b540449e92b4886f43607fa02 # v4.6.2
        with:
          name: app
          path: artifacts/publish
```

This repository runs the `checkout` and `setup-dotnet` pins. The `upload-artifact` pin is only an example. When you copy the workflow, resolve every SHA against the current release, and check the major version. A pinned SHA never reports that it's out of date, so pair SHA pinning with a dependency bot.

## Artifact signing

**Spend your signing effort on provenance and credentials, not on author signing.** For NuGet packages:

- Ship deterministic builds, with Source Link, symbols, and package validation enabled.
- Publish with a short-lived, least-privilege credential, such as trusted publishing or a scoped API key stored as a repository secret. Don't use a long-lived, organisation-wide key. ([Meziantou: Publishing a NuGet package using GitHub Actions](https://www.meziantou.net/publishing-a-nuget-package-following-best-practices-using-github.htm))
- Author-sign only if your organisation already runs certificate infrastructure. nuget.org repository-signs every package it serves, and most consumers never verify an author signature. ([Microsoft Learn: Sign a NuGet package](https://learn.microsoft.com/nuget/create-packages/sign-a-package))

To protect consumers, pin: locked restore, a pinned SDK, and SHA-pinned actions.

Don't add a signer allow list (`signatureValidationMode` set to `require`, with `trustedSigners` in `nuget.config`) unless you can maintain it. Each certificate rotation fails installs with `NU3034` until you add the new SHA-256 fingerprint beside the old ones. Microsoft rotated its author-signing certificate from 2026-09-23. ([.NET Blog: Microsoft is updating its author-signing certificate starting September 23, 2026](https://devblogs.microsoft.com/dotnet/microsoft-author-signing-certificate-update-2026/))

## Dependency updates

**Use Renovate, with one shared configuration preset across every repository.** It updates everything a .NET repository pins: NuGet packages (grouped, with lock file maintenance), the SDK in `global.json`, `.nuspec` dependencies, and action SHAs. ([Meziantou: Sharing the Renovate configuration across multiple projects](https://www.meziantou.net/sharing-the-renovate-configuration-across-multiple-projects.htm), [Meziantou: Update dependencies in nuspec with Renovate](https://www.meziantou.net/update-dependencies-in-nuspec-file-using-renovate.htm)) Dependabot lost because it can't share configuration across repositories, so each repository's policy drifts.

Merge updates only through the pipeline above. An update that skips locked restore and tests is a supply-chain risk.

**House:** Leave the bot's dependency dashboard issue open. Renovate manages it, and closing it doesn't opt out: the next run re-creates it under a new number and notifies everyone again.

- **It's the only list of pending updates.** Updates held back by `prConcurrentLimit`, or blocked by a failing branch, appear nowhere else. Its checkboxes trigger an off-schedule run without a configuration change.
- **On a large solution, it releases the work.** If you gate updates behind `dependencyDashboardApproval`, you approve them from the dashboard.
- **`dependencyDashboardAutoclose` lost.** It closes the issue whenever nothing is pending. A new issue every cycle costs more attention than one that stays open, and the closed issue hides a growing backlog.
- If a stale-issue bot flags the dashboard, exempt it by label.

Two rules keep the dashboard useful as a repository grows:

- **Don't use its list of detected dependencies as an audit record.** GitHub caps an issue body at 65,536 characters, so Renovate trims the list in a large repository. Check what the bot manages in a configuration test or a scheduled dry run, where truncation fails visibly.
- **Keep the pending list short, and give it an owner.** People stop reading a dashboard with hundreds of open entries. Fix the cause in the grouping and scheduling policy: group related updates, widen the schedule until the queue clears, and name the team that works through it.
