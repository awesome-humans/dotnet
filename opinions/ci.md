---
targets: [net10.0]
last-reviewed: 2026-10-09
last-used: 2026-09-26
sources: [meziantou, ms-learn, dotnet-blog, house]
---

# CI & automation

Treat supply-chain security as a requirement. Agents now write and run much of the code in a pipeline, so you can't leave it to chance.

## Opinions

- **Pin GitHub Actions to commit SHAs, not mutable tags.** A tag can move silently, and a SHA can't. Automate the sweep across repositories. ([Meziantou: SHA pinning](https://www.meziantou.net/enable-sha-pinning-for-github-actions-across-personal-repositories.htm))
- **Never interpolate user-provided input into a workflow script.** Pass it in an environment variable and parse it explicitly. Script injection is the most common GitHub Actions vulnerability. ([Meziantou: Safely passing extra arguments](https://www.meziantou.net/safely-passing-extra-arguments-in-github-actions-workflows-using-powershell.htm))
- **Make CI builds pinned and reproducible.** `global.json` sets the SDK, and lock files or Central Package Management set the packages. A CI run must not float to a version the repository didn't choose. ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json), [Meziantou: Faster and Safer NuGet restore using Source Mapping and Lock files](https://www.meziantou.net/faster-and-safer-nuget-restore-using-source-mapping-and-lock-files.htm))
- **Restrict what a package can run, not only which version it restores.** A `PackageReference` can bring MSBuild props and targets, analyzers, and source generators, and all of them run during restore, design-time build, and compile. Pinning the version controls which code you get, not whether it runs. List only the asset types a package needs:
  - Use `IncludeAssets="compile;runtime"` for a library you only call.
  - Use `PrivateAssets="all"` on build-only tooling, so it never reaches your consumers.

  Some packages are mostly build logic, so expect to loosen this for individual packages, and re-test when you tighten it. ([Meziantou: Limit what NuGet packages can do in your project](https://www.meziantou.net/limit-what-nuget-packages-can-do-in-your-project.htm))

- **House:** Treat warnings as errors everywhere, not only in CI. A warning that shows up on the build machine but not on your workstation has already shipped a day late. Every suppression, whether `#pragma warning disable`, `[SuppressMessage]`, or a `<NoWarn>` entry, needs an inline comment that says why. Revert any suppression without one: a reviewer should never have to reconstruct the reason.

## Pipeline shape

**Use one pipeline with four explicit stages: restore, build, test, and publish. Each stage skips the work of the stage before it.** Pass `--no-restore` to build, and `--no-build` to test and publish. Every stage then runs against exactly the output of the previous one, instead of silently rebuilding with different flags.

Pass the same `--configuration` to every stage. For current target frameworks, `dotnet publish` defaults to `Release`, but `dotnet build` and `dotnet test` default to `Debug`. A `--no-build` stage with a different configuration looks for output that was never built. The shape works on any CI platform, and GitHub Actions is only the example here. ([Microsoft Learn: dotnet publish](https://learn.microsoft.com/dotnet/core/tools/dotnet-publish), [Microsoft Learn: dotnet test](https://learn.microsoft.com/dotnet/core/tools/dotnet-test))

1. **Restore.** Use `--locked-mode`, so lock file drift fails the build instead of floating a version. Only use it if the repository commits `packages.lock.json` (`RestorePackagesWithLockFile=true`). [templates/Directory.Build.props](../templates/Directory.Build.props) pins the dependency graph with Central Package Management and no lock files, so the example below restores without the flag. With the flag and no lock file, every restore fails with `NU1004`.
2. **Build.** Build once, in `Release`, with `-warnaserror`. Keep the switch even though the template sets `TreatWarningsAsErrors`. The property covers compiler and analyzer diagnostics, and the switch also promotes MSBuild engine warnings, such as MSB3277 assembly version conflicts.
   - **House:** [templates/Directory.Build.props](../templates/Directory.Build.props) sets `TreatWarningsAsErrors` unconditionally, so warnings fail the build on every workstation, not only in CI. This file used to recommend CI-only enforcement to keep local iteration fast. The house rule replaces that advice, because a build that's only red in CI means a warning has already reached a pull request.
   - Enforce style the same way: run analyzers in the build, and run `dotnet format --verify-no-changes` as a step. ([Meziantou: Enforce .NET code style in CI](https://www.meziantou.net/enforce-dotnet-code-style-in-ci-with-dotnet-format.htm), [Meziantou: The Roslyn analyzers I use](https://www.meziantou.net/the-roslyn-analyzers-i-use.htm))
3. **Test.** Run `dotnet test` on Microsoft.Testing.Platform (MTP). MTP requires the `test.runner` opt-in in `global.json`, as described in [testing.md](testing.md). Without it, the SDK uses VSTest, which fails on .NET 10 before the build, so `--no-build` can't help.
   - Publish TRX and coverage files as build artifacts, so you can diagnose a failure without a rerun.
   - Under xunit.v3, the TRX switch is `--report-xunit-trx` with `--results-directory`, as in the example below. Upload the results even when the step fails, because that's the run they diagnose.
   - MTP rejects VSTest's `--logger trx`. For details, see [testing.md](testing.md). ([Microsoft Learn: What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview))
4. **Publish or pack.** Use `--no-build`, and upload the output as the single artifact that later stages, such as deploy and release, consume. Never rebuild for deployment.
   - Don't pass `--output`. With the artifacts layout from [templates/Directory.Build.props](../templates/Directory.Build.props), a single-TFM publish without a RID goes to `artifacts/publish/<Project>/release`. Otherwise, the last folder gains `_<tfm>` or `_<rid>` suffixes. Pack output goes to `artifacts/package/<configuration>`, with no project folder. For details, see [project-structure.md](project-structure.md).
   - Keep the deploy artifact for deployment only. Test projects set `<IsPublishable>false</IsPublishable>`, as [templates/projects/Example.Library.Tests.csproj](../templates/projects/Example.Library.Tests.csproj) does. Otherwise, a solution-level publish writes the test host, and a second copy of every referenced library, into `artifacts/publish`. ([Microsoft Learn: Artifacts output layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output))

```yaml
# Least privilege at the top: the default token is read/write on contents unless
# the repository says otherwise, and a build job needs neither.
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

The `checkout` and `setup-dotnet` pins are the ones this repository runs, and the `upload-artifact` pin is an example. When you copy the workflow, resolve every SHA against the current release, and check the major version while you're there. A pinned SHA never tells you it's out of date, so SHA pinning and a dependency bot work as one policy.

## Artifact signing

**Spend your signing effort on provenance and credential hygiene, not on author-signing.** For NuGet packages:

- Ship deterministic builds, with Source Link, symbols, and package validation enabled.
- Publish with a short-lived, least-privilege credential, such as trusted publishing or a scoped API key stored as a repository secret. Don't use a long-lived, organisation-wide key. ([Meziantou: Publishing a NuGet package using GitHub Actions](https://www.meziantou.net/publishing-a-nuget-package-following-best-practices-using-github.htm))
- Author-sign packages only if your organisation already runs certificate infrastructure. nuget.org repository-signs every package it serves, so for most publishers author signing adds cost, and no consumer verifies it. ([Microsoft Learn: Sign a NuGet package](https://learn.microsoft.com/nuget/create-packages/sign-a-package))

On the consuming side, pinning is the protection that pays off: locked restore, a pinned SDK, and SHA-pinned actions.

A signer allow list, set with `signatureValidationMode` `require` and `trustedSigners` in `nuget.config`, is the exception, and it costs upkeep. Each certificate rotation fails installs with `NU3034` until you add the new SHA-256 fingerprint beside the old ones. Microsoft rotated its author-signing certificate from 2026-09-23. ([.NET Blog: Microsoft is updating its author-signing certificate starting September 23, 2026](https://devblogs.microsoft.com/dotnet/microsoft-author-signing-certificate-update-2026/))

## Dependency updates

**Use Renovate.** Run one bot with one shared configuration preset across every repository. It updates everything a .NET repository pins: NuGet packages (grouped, with lock file maintenance), the SDK version in `global.json`, `.nuspec` dependencies, and the action SHAs from the pinning opinion above. ([Meziantou: Sharing the Renovate configuration across multiple projects](https://www.meziantou.net/sharing-the-renovate-configuration-across-multiple-projects.htm), [Meziantou: Update dependencies in nuspec with Renovate](https://www.meziantou.net/update-dependencies-in-nuspec-file-using-renovate.htm)) Dependabot lost because it has no shared configuration across repositories, so each repository's policy drifts on its own.

Whatever bot you use, merge updates only through the pipeline above. An update pull request that skips `--locked-mode` restore and tests is a supply-chain exposure.

**House:** Leave the bot's dependency dashboard issue open permanently. It's state that the bot manages, not a task, and closing it doesn't opt out. Renovate re-creates it on the next run, which uses a new issue number and sends another round of notifications.

- **It's the only view of pending updates.** Updates held back by `prConcurrentLimit`, or waiting on a failing update branch, appear there and nowhere else. Its checkboxes let you force an off-schedule run without a configuration change.
- **On a large solution, it controls the work.** If you gate updates behind `dependencyDashboardApproval`, the dashboard is how you release them, so closing it breaks the workflow.
- **`dependencyDashboardAutoclose` lost.** It closes the issue whenever nothing is pending. Opening and closing an issue every cycle costs more attention than one issue that stays open. It also hides a growing backlog, exactly when the backlog is the signal.
- If a stale-issue bot flags the dashboard, exempt the dashboard by its label instead of closing it.

Two conditions keep the dashboard useful as a repository grows:

- **Don't treat its list of detected dependencies as an audit record.** GitHub limits an issue body to 65,536 characters, so in a large repository Renovate has already trimmed the list. Test what the bot does and doesn't manage in a configuration test or a scheduled dry run, where truncation can't pass silently.
- **Keep the pending list short, with an owner.** A dashboard with hundreds of unactioned entries is one that everybody learns to skip. That's a problem with the grouping and scheduling policy, not with the issue. Fix it there: group related updates, widen the schedule until the queue drains, and name the team that works through it.
