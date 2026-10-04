---
targets: [net10.0, csharp-14, fsharp-10]
last-reviewed: 2026-08-21
last-used: 2026-09-25
sources:
  [ms-learn, dotnet-blog, gerald-versluis, steve-gordon, aaron-stannard, house]
---

# Project structure & SDK

Use one solution format and Central Package Management, and pin versions at the repository root. The `templates/` directory encodes these opinions as copy-paste-ready files. Copy them verbatim and trim, instead of writing your own.

## Solution & SDK

- **Use `.slnx` for new solutions.** In the .NET 10 SDK, `dotnet new sln` creates one by default. Migrate `.sln` files when you're already changing them. Use `.slnf` filters for large solutions. Start from [templates/example.slnx](../templates/example.slnx) and [templates/example.slnf](../templates/example.slnf). ([Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Pin the SDK with `global.json`,** with `rollForward` set to `latestFeature`, as in [templates/global.json](../templates/global.json). ([Microsoft Learn: What's new in .NET 10](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview))
- **Treat the pinned version as a minimum, and raise it only when you need a newer feature band.** Under `rollForward: latestFeature`, `10.0.400` means that version, or any later band and patch installed on the machine. So a new band reaches the build without a change to the file. Pinning a patch, such as `10.0.412`, or following each band as it ships, gains nothing: it narrows who can build, and fails contributors who are one servicing release behind. Raise the minimum when a band ships something the repository uses, and name that feature in the commit. ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))
- **Try a preview SDK with `sdk.paths`, and ignore its install folder in the same commit.** `./dotnet-install.sh --install-dir .dotnet` (`-InstallDir` in the PowerShell script) installs the preview SDK inside the repository instead of on the machine. `paths` makes the host prefer it, and `$host$` falls back to the machine install.
  - Paths resolve relative to `global.json`, not the working directory, so this works from any subdirectory.
  - `errorMessage` is what a contributor with neither SDK sees, instead of a bare version error.
  - `paths` applies only to commands that use the SDK, such as `dotnet build` and `dotnet run`. The apphost and `dotnet app.dll` ignore it.
  - The folder holds a full SDK, hundreds of megabytes, so never commit it. ([Versluis: Test .NET MAUI Preview SDKs Locally with global.json sdk.paths](https://blog.verslu.is/maui/test-dotnet-maui-preview-sdk-locally/), [Microsoft Learn: test prerelease SDKs locally](https://learn.microsoft.com/dotnet/core/tools/test-prerelease-sdk-locally))

  ```json
  {
    "sdk": {
      "version": "11.0.100-preview.1.25000.1",
      "paths": [".dotnet", "$host$"],
      "errorMessage": "Run ./dotnet-install.sh --install-dir .dotnet --version 11.0.100-preview.1.25000.1"
    }
  }
  ```

  `dotnet new gitignore` doesn't cover `.dotnet/`. The template ignores build output, and a locally installed SDK is a build _prerequisite_. Every .NET repository that installs one this way adds the rule by hand: `dotnet/runtime`, `dotnet/aspnetcore`, `dotnet/sdk`, and `dotnet/maui` all have it. So when you adopt `sdk.paths`, you must add one line to the repository-specific block of `.gitignore`, described below.

- **Use Central Package Management (`Directory.Packages.props`) in multi-project repositories.** Versions set per project get harder to keep consistent as the number of projects grows. NU1510 flags direct references that can be pruned. Start from [templates/Directory.Packages.props](../templates/Directory.Packages.props), which includes `CentralPackageTransitivePinningEnabled`. ([Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Put shared build settings in `Directory.Build.props`:** the TFM, `LangVersion` set to latest, nullable enabled, analyzers on, and warnings as errors. Copy [templates/Directory.Build.props](../templates/Directory.Build.props). A project file is then nearly empty, as in [templates/projects/](../templates/projects/).
- **House:** The warnings-as-errors setting is always on, never only in CI, and every suppression has a comment that gives the reason. The canonical statement is in [ci.md](ci.md).
- **Run one-off tools with `dotnet tool exec` or `dnx`,** instead of installing them globally. `dnx` ignores the SDK selection in `global.json`. ([What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Use file-based apps (`dotnet run app.cs`) for scripts and samples,** instead of throwaway console projects. They now support publishing and Native AOT. ([What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk))
- **Target the latest release, whether it's LTS or STS.** That's this repository's freshness policy. .NET 10 is supported as LTS through November 2028. ([Microsoft Learn: .NET releases, patches, and support](https://learn.microsoft.com/dotnet/core/releases-and-support))

## Repository layout

- **Put shipping code in `src/`, test projects in `tests/`, and build configuration at the root.** Give each project its own directory, named after the assembly. Make the solution mirror the disk layout, with `/src/` and `/tests/` solution folders. [templates/example.slnx](../templates/example.slnx) has exactly this shape. ([Microsoft Learn: Organize projects for .NET Framework and .NET](https://learn.microsoft.com/dotnet/core/porting/project-structure), [dotnet/samples: migrate-library-csproj](https://github.com/dotnet/samples/tree/main/framework/libraries/migrate-library-csproj))
- **House:** Add a `/build/` solution folder that lists the root build files: `Directory.Build.props`, `Directory.Packages.props`, `global.json`, and `.editorconfig`. They hold every project's settings, and listing them puts them in Solution Explorer next to the projects they configure. [templates/example.slnx](../templates/example.slnx) includes the folder, and [Stannard: project-structure](https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/project-structure/SKILL.md) uses the same shape.
- **Enable the artifacts output layout with `<UseArtifactsOutput>true</UseArtifactsOutput>` in the root `Directory.Build.props`.** All build output from all projects goes under one `artifacts/<type>/<project>/<pivot>` root, instead of a `bin/` and `obj/` pair in each project directory.
  - The types are `bin`, `obj`, `publish`, and `package`. The `package` type has no project folder, so `.nupkg` files go to `artifacts/package/<configuration>`.
  - Tools and CI steps can rely on this layout. The per-project layout "can change drastically via relatively simple MSBuild changes".
  - It's been GA since .NET 8, but is still opt-in in the .NET 10 SDK, so adopting it is a deliberate choice. [templates/Directory.Build.props](../templates/Directory.Build.props) adopts it.
  - MSBuild reads the property before it evaluates the project, so it only works in `Directory.Build.props` or on the command line. Setting it in a project file fails the build with `NETSDK1199`.
  - For a single-TFM project, the last folder is just the lowercase configuration, with no TFM: `artifacts/bin/App/release`. Change every hard-coded `bin/<Config>/<tfm>` path in Dockerfiles, CI copy steps, and scripts in the same commit.
  - `dotnet new gitignore` already ignores `artifacts/`. ([Microsoft Learn: Artifacts output layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output), [.NET Blog: Announcing .NET 8 Preview 3](https://devblogs.microsoft.com/dotnet/announcing-dotnet-8-preview-3/))
- **Keep the shared configuration files at the root.** Put `global.json`, `Directory.Build.props`, `Directory.Packages.props`, and `.editorconfig` at the repository root, so every project inherits them without any setup of its own. MSBuild searches up from each project for the nearest `Directory.Build.props`, and Central Package Management finds `Directory.Packages.props` the same way. ([Microsoft Learn: Customize the build by folder](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory), [Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management))
- **Put test projects beside the code under test, never inside it.** `tests/Example.Library.Tests` mirrors `src/Example.Library` and is named `<Project>.Tests`, as in the same sample. For what goes in them, see [testing.md](testing.md).
- **Inside a project, group by feature, not by pattern.** For how modules and vertical slices fit into this layout, see [architecture.md](architecture.md).
- **Don't add structure you don't need yet.** A single-project tool can be just `src/Tool` and `tests/Tool.Tests`. Add `docs/` and solution filters when the repository needs them. ([Microsoft Learn: Organizing and testing projects with the .NET CLI](https://learn.microsoft.com/dotnet/core/tutorials/testing-with-cli))
- **Generate `.gitignore` with `dotnet new gitignore`, and don't maintain one by hand.** The SDK template is the canonical .NET ignore list, and it changes with the toolchain. A hand-written copy, or one pasted from another repository, drifts. That's why this repository ships no `.gitignore` template. Regenerate the file after major SDK upgrades. Keep repository-specific additions in a clearly marked block at the end, so you can safely overwrite everything above it. `.dotnet/` from an `sdk.paths` install is one such addition. ([Microsoft Learn: dotnet new gitignore](https://learn.microsoft.com/dotnet/core/tools/dotnet-new-sdk-templates))

## .editorconfig

- **Give every repository a root `.editorconfig`, starting from [templates/.editorconfig](../templates/.editorconfig).** Copy it verbatim, and remove the rules you disagree with, but only after you've thought about them. It encodes the house style: file-scoped namespaces, expression-bodied members where they fit on one line, collection expressions, auto-implemented properties (IDE0032), and standard .NET naming, with analyzer diagnostics set to `warning` by default.
- **Let the build enforce style, not reviewers.** `.editorconfig` severities fail the build only because [templates/Directory.Build.props](../templates/Directory.Build.props) sets `EnforceCodeStyleInBuild` and `TreatWarningsAsErrors`. Adopt both files together. Otherwise, the style file is only documentation.
- **Use one `.editorconfig` at the root, not one per project.** Add a nested file only for a real exception, such as relaxing documentation comment rules under `tests/`. A nested file should contain only the differences: when both files set a key, the deeper file wins. ([Microsoft Learn: Configuration files for code analysis rules](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files))

## Containers

- **Build images with the SDK (`dotnet publish /t:PublishContainer`), not a hand-written Dockerfile.** The SDK produces the image directly, so there's no Dockerfile to drift out of sync with the project. By default, it pushes to the local Docker or Podman daemon. Set `ContainerRegistry` to push to a registry, or `ContainerArchiveOutputPath` to write a tarball. In .NET 10, console apps are supported too, without `<EnableSdkContainerSupport>`, the same as ASP.NET Core and Worker apps. ([Microsoft Learn: Containerize a .NET app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish), [Microsoft Learn: What's new in the .NET 10 SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk))

  ```xml
  <!-- All container config is MSBuild properties; illustrative values -->
  <PropertyGroup Label="Container image">
    <ContainerRepository>contoso/example-worker</ContainerRepository>
    <ContainerImageTags>1.4.0;latest</ContainerImageTags>
    <ContainerFamily>noble-chiseled</ContainerFamily>
  </PropertyGroup>
  ```

- **Know that .NET 10 base images use Ubuntu, not Debian.** Version-only tags, such as `mcr.microsoft.com/dotnet/aspnet:10.0`, now resolve to Ubuntu 24.04 "Noble". Microsoft ships no Debian images for .NET 10, so there's no tag to switch back to. If image size matters, set `ContainerFamily` to a chiseled variant, such as `noble-chiseled`, or to `alpine`, instead of building your own image. ([Microsoft Learn: Default .NET container tags now use Ubuntu](https://learn.microsoft.com/dotnet/core/compatibility/containers/10.0/default-images-use-ubuntu))
- **Use the `-extra` tag if the app isn't invariant.** The chiseled and Alpine variants above include neither ICU nor `tzdata`, and set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true` in the image. An app that names a culture throws `CultureNotFoundException` at startup. An app that only reads `CurrentCulture` silently formats and compares as invariant. `<InvariantGlobalization>false</InvariantGlobalization>` in the project file doesn't override the environment variable. `noble-chiseled-extra` and `alpine-extra` restore both ICU and `tzdata`. ([globalization.md](globalization.md))
- **Keep the rootless default.** Since .NET 8, Linux images run as the non-root `app` user, and `ContainerPort` is inferred from `ASPNETCORE_URLS`, `ASPNETCORE_HTTP_PORTS`, or `ASPNETCORE_HTTPS_PORTS`. Don't set `ContainerUser` to `root`, or expose privileged ports, to work around a broken volume mount. Fix the mount instead. ([Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration))
