---
targets: [net10.0, csharp-14, fsharp-10]
last-reviewed: 2026-08-21
sources:
  [
    aaron-stannard,
    dotnet-blog,
    gerald-versluis,
    house:simon-mckenna,
    ms-learn,
    steve-gordon,
  ]
---

# Project structure & SDK

Use one solution format and Central Package Management, and pin versions at the repository root. The files in `templates/` implement these opinions. Copy them verbatim and trim, instead of writing your own.

## Solution & SDK

- **Use `.slnx` for new solutions.** The .NET 10 SDK's `dotnet new sln` creates one by default. Migrate a `.sln` file when you're already changing it, and use `.slnf` filters for large solutions. Start from [templates/example.slnx](../templates/example.slnx) and [templates/example.slnf](../templates/example.slnf). ([Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Pin the SDK in `global.json` with `rollForward` set to `latestFeature`,** as in [templates/global.json](../templates/global.json). ([Microsoft Learn: What's new in .NET 10](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview))
- **Treat the pinned version as a minimum, and raise it only for a feature you need.** Under `latestFeature`, `10.0.400` means that version or any later band and patch on the machine, so new bands reach the build without a file change. Pinning a patch such as `10.0.412`, or bumping for every band, only fails contributors one servicing release behind. When a band ships something the repository uses, raise the minimum, and name the feature in the commit. ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))
- **To try a preview SDK, use `sdk.paths`, and ignore its install folder in the same commit.** `./dotnet-install.sh --install-dir .dotnet` (`-InstallDir` in PowerShell) installs the SDK inside the repository. `paths` makes the host prefer it, and `$host$` falls back to the machine install.
  - Paths resolve relative to `global.json`, so this works from any subdirectory.
  - `errorMessage` replaces the bare version error for a contributor who has neither SDK.
  - Only SDK commands, such as `dotnet build` and `dotnet run`, read `paths`. The apphost and `dotnet app.dll` ignore it.
  - The folder holds a full SDK of several hundred megabytes, so never commit it. ([Versluis: Test .NET MAUI Preview SDKs Locally with global.json sdk.paths](https://blog.verslu.is/maui/test-dotnet-maui-preview-sdk-locally/), [Microsoft Learn: test prerelease SDKs locally](https://learn.microsoft.com/dotnet/core/tools/test-prerelease-sdk-locally))

  ```json
  {
    "sdk": {
      "version": "11.0.100-preview.1.25000.1",
      "paths": [".dotnet", "$host$"],
      "errorMessage": "Run ./dotnet-install.sh --install-dir .dotnet --version 11.0.100-preview.1.25000.1"
    }
  }
  ```

  `dotnet new gitignore` doesn't ignore `.dotnet/`, because it covers build output and a local SDK is a build _prerequisite_. `dotnet/runtime`, `dotnet/aspnetcore`, `dotnet/sdk`, and `dotnet/maui` each add the rule by hand. When you adopt `sdk.paths`, add `.dotnet/` to the repository-specific block of `.gitignore` described below.

- **Use Central Package Management (`Directory.Packages.props`) in multi-project repositories.** Per-project versions drift apart as projects multiply. NU1510 flags direct references that can be pruned. Start from [templates/Directory.Packages.props](../templates/Directory.Packages.props), which enables `CentralPackageTransitivePinningEnabled`. ([Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Put shared build settings in `Directory.Build.props`:** the TFM, `LangVersion` set to latest, nullable enabled, analyzers on, and warnings as errors. Copy [templates/Directory.Build.props](../templates/Directory.Build.props). Project files are then nearly empty, as in [templates/projects/](../templates/projects/).

/// house | simon-mckenna

**House:** Warnings as errors is always on, not only in CI, and every suppression has a comment that gives the reason. [ci.md](ci.md) holds the canonical statement.

///

- **Run one-off tools with `dotnet tool exec` or `dnx` instead of installing them globally.** `dnx` ignores the SDK selection in `global.json`. ([What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Use file-based apps (`dotnet run app.cs`) for scripts and samples instead of throwaway console projects.** They support publishing and Native AOT. ([What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk))
- **Target the latest release, LTS or STS.** That's this repository's freshness policy. .NET 10 is an LTS release, supported until November 2028. ([Microsoft Learn: .NET releases, patches, and support](https://learn.microsoft.com/dotnet/core/releases-and-support))

## Repository layout

- **Put shipping code in `src/`, test projects in `tests/`, and build configuration at the root.** Give each project its own directory, named after the assembly. Mirror the disk layout in the solution with `/src/` and `/tests/` solution folders, as [templates/example.slnx](../templates/example.slnx) does. ([Microsoft Learn: Organize projects for .NET Framework and .NET](https://learn.microsoft.com/dotnet/core/porting/project-structure), [dotnet/samples: migrate-library-csproj](https://github.com/dotnet/samples/tree/main/framework/libraries/migrate-library-csproj))

/// house | simon-mckenna

**House:** Add a `/build/` solution folder that lists `Directory.Build.props`, `Directory.Packages.props`, `global.json`, and `.editorconfig`. They configure every project, and the folder shows them in Solution Explorer. [templates/example.slnx](../templates/example.slnx) includes it, and [Stannard: project-structure](https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/project-structure/SKILL.md) uses the same shape.

///

- **Set `<UseArtifactsOutput>true</UseArtifactsOutput>` in the root `Directory.Build.props`.** All build output goes under one `artifacts/<type>/<project>/<pivot>` root, instead of `bin/` and `obj/` in each project.
  - The types are `bin`, `obj`, `publish`, and `package`. `package` has no project folder, so `.nupkg` files go to `artifacts/package/<configuration>`.
  - Tools and CI steps can rely on the layout. The per-project layout "can change drastically via relatively simple MSBuild changes".
  - It's been GA since .NET 8, and it's still opt-in in the .NET 10 SDK. [templates/Directory.Build.props](../templates/Directory.Build.props) opts in.
  - MSBuild reads the property before it evaluates the project, so set it only in `Directory.Build.props` or on the command line. In a project file, it fails the build with `NETSDK1199`.
  - For a single-TFM project, the last folder is the lowercase configuration, with no TFM: `artifacts/bin/App/release`. Update hard-coded `bin/<Config>/<tfm>` paths in Dockerfiles, CI steps, and scripts in the same commit.
  - `dotnet new gitignore` already ignores `artifacts/`. ([Microsoft Learn: Artifacts output layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output), [.NET Blog: Announcing .NET 8 Preview 3](https://devblogs.microsoft.com/dotnet/announcing-dotnet-8-preview-3/))
- **Keep `global.json`, `Directory.Build.props`, `Directory.Packages.props`, and `.editorconfig` at the repository root.** MSBuild searches up from each project for the nearest `Directory.Build.props`, and Central Package Management finds `Directory.Packages.props` the same way, so every project inherits them. ([Microsoft Learn: Customize the build by folder](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory), [Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management))
- **Put test projects beside the code they test, never inside it.** `tests/Example.Library.Tests` mirrors `src/Example.Library`, named `<Project>.Tests`. For what goes in them, see [testing.md](testing.md).
- **Inside a project, group by feature, not by pattern.** For modules and vertical slices, see [architecture.md](architecture.md).
- **Don't add structure before you need it.** A single-project tool can be `src/Tool` and `tests/Tool.Tests`. Add `docs/` and solution filters when the repository needs them. ([Microsoft Learn: Organizing and testing projects with the .NET CLI](https://learn.microsoft.com/dotnet/core/tutorials/testing-with-cli))
- **Generate `.gitignore` with `dotnet new gitignore`, and don't edit the generated part.** The SDK template is the standard .NET ignore list, and it changes with the toolchain, so this repository ships no `.gitignore` template. Regenerate the file after major SDK upgrades. Keep repository-specific additions, such as `.dotnet/`, in a marked block at the end, so you can overwrite everything above it. ([Microsoft Learn: dotnet new gitignore](https://learn.microsoft.com/dotnet/core/tools/dotnet-new-sdk-templates))

## .editorconfig

- **Give every repository a root `.editorconfig`, copied from [templates/.editorconfig](../templates/.editorconfig).** Remove a rule only after you've decided you disagree with it. The file sets file-scoped namespaces, expression-bodied members where they fit on one line, collection expressions, auto-implemented properties (IDE0032), and standard .NET naming, with analyzer diagnostics at `warning` by default.
- **Let the build enforce style.** `.editorconfig` severities fail the build only because [templates/Directory.Build.props](../templates/Directory.Build.props) sets `EnforceCodeStyleInBuild` and `TreatWarningsAsErrors`. Without both files, the style file is documentation.
- **Use one `.editorconfig` at the root, not one per project.** Add a nested file only for a real exception, such as relaxed documentation comment rules under `tests/`. Put only the differences in it: when both files set a key, the deeper file wins. ([Microsoft Learn: Configuration files for code analysis rules](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files))

## Containers

- **Build images with `dotnet publish /t:PublishContainer`, not a hand-written Dockerfile.** The SDK builds the image from the project, so there's no Dockerfile to drift. It pushes to the local Docker or Podman daemon by default. Set `ContainerRegistry` to push to a registry, or `ContainerArchiveOutputPath` to write a tarball. In .NET 10, console apps work without `<EnableSdkContainerSupport>`, like ASP.NET Core and Worker apps. ([Microsoft Learn: Containerize a .NET app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish), [Microsoft Learn: What's new in the .NET 10 SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk))

  ```xml
  <!-- All container config is MSBuild properties; illustrative values -->
  <PropertyGroup Label="Container image">
    <ContainerRepository>contoso/example-worker</ContainerRepository>
    <ContainerImageTags>1.4.0;latest</ContainerImageTags>
    <ContainerFamily>noble-chiseled</ContainerFamily>
  </PropertyGroup>
  ```

- **Expect Ubuntu, not Debian, in .NET 10 base images.** Version-only tags, such as `mcr.microsoft.com/dotnet/aspnet:10.0`, resolve to Ubuntu 24.04 "Noble", and there are no Debian images for .NET 10. For a smaller image, set `ContainerFamily` to a chiseled variant, such as `noble-chiseled`, or to `alpine`. ([Microsoft Learn: Default .NET container tags now use Ubuntu](https://learn.microsoft.com/dotnet/core/compatibility/containers/10.0/default-images-use-ubuntu))
- **If the app isn't invariant, use the `-extra` tag.** The chiseled and Alpine images include neither ICU nor `tzdata`, and set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true`. An app that names a culture throws `CultureNotFoundException` at startup. An app that only reads `CurrentCulture` formats and compares as invariant without an error. `<InvariantGlobalization>false</InvariantGlobalization>` doesn't override the environment variable. `noble-chiseled-extra` and `alpine-extra` add back ICU and `tzdata`. For details, see [globalization.md](globalization.md).
- **Keep the rootless default.** Since .NET 8, Linux images run as the non-root `app` user, and `ContainerPort` comes from `ASPNETCORE_URLS`, `ASPNETCORE_HTTP_PORTS`, or `ASPNETCORE_HTTPS_PORTS`. To fix a broken volume mount, fix the mount. Don't set `ContainerUser` to `root` or expose a privileged port. ([Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration))
