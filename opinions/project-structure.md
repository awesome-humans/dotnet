---
targets: [net11.0, csharp-15, fsharp-11]
last-reviewed: 2026-11-14
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
- **Pin the SDK in `global.json` with `rollForward` set to `latestFeature`,** as in [templates/global.json](../templates/global.json). ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))
- **Treat the pinned version as a minimum, and raise it only for a feature you need.** Under `latestFeature`, `11.0.100` means that version or any later band and patch on the machine, so new bands reach the build without a file change. Pinning a patch such as `11.0.105`, or bumping for every band, only fails contributors one servicing release behind. When a band ships something the repository uses, raise the minimum, and name the feature in the commit. ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))
- **To try a preview SDK, use `sdk.paths`, and ignore its install folder in the same commit.** `./dotnet-install.sh --install-dir .dotnet` (`-InstallDir` in PowerShell) installs the SDK inside the repository. `paths` makes the host prefer it, and `$host$` falls back to the machine install.
  - Paths resolve relative to `global.json`, so this works from any subdirectory.
  - `errorMessage` replaces the bare version error for a contributor who has neither SDK.
  - Only SDK commands, such as `dotnet build` and `dotnet run`, read `paths`. It doesn't affect "running the native apphost launcher (`app.exe`), running with `dotnet app.dll`, or running with `dotnet exec app.dll`". ([Microsoft Learn: global.json overview](https://learn.microsoft.com/dotnet/core/tools/global-json))
  - The folder holds a full SDK of several hundred megabytes, so never commit it.

  ([Versluis: Test .NET MAUI Preview SDKs Locally with global.json sdk.paths](https://blog.verslu.is/maui/test-dotnet-maui-preview-sdk-locally/), [Microsoft Learn: test prerelease SDKs locally](https://learn.microsoft.com/dotnet/core/tools/test-prerelease-sdk-locally))

  ```json
  {
    "sdk": {
      "version": "11.0.100-rc.2.26504.105",
      "paths": [".dotnet", "$host$"],
      "errorMessage": "Run ./dotnet-install.sh --install-dir .dotnet --version 11.0.100-rc.2.26504.105"
    }
  }
  ```

  **Review by 2027-03-31.** The example pins .NET 11 RC 2, a real version of a release that has since shipped, because no .NET 12 preview exists yet. When .NET 12 Preview 1 ships, replace both version strings with its SDK version, and drop this note.

  `dotnet new gitignore` doesn't ignore `.dotnet/`, because it covers build output and a local SDK is a build _prerequisite_. `dotnet/runtime`, `dotnet/aspnetcore`, `dotnet/sdk`, and `dotnet/maui` each add the rule by hand. When you adopt `sdk.paths`, add `.dotnet/` to the repository-specific block of `.gitignore` described below.

- **Use Central Package Management (`Directory.Packages.props`), even in a single-project repository.** It applies to every `PackageReference`-based project. Versions set per project drift apart once a second project arrives. On .NET 10 and later, NU1510 flags direct references that package pruning can remove. Start from [templates/Directory.Packages.props](../templates/Directory.Packages.props), which enables `CentralPackageTransitivePinningEnabled`. ([Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management), [Microsoft Learn: NU1510](https://learn.microsoft.com/nuget/reference/errors-and-warnings/nu1510), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Put shared build settings in `Directory.Build.props`:** the TFM, `LangVersion` set to latest, nullable enabled, analyzers on, and warnings as errors. Copy [templates/Directory.Build.props](../templates/Directory.Build.props). A setting that depends on a property the project file sets, such as `IsPackable`, goes in [templates/Directory.Build.targets](../templates/Directory.Build.targets), which MSBuild imports after the project file. Project files are then nearly empty, as in [templates/projects/](../templates/projects/). ([Microsoft Learn: Customize the build by folder](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory))
- **Set `<IsPackable>true</IsPackable>` in the project file of every library that ships as a NuGet package.** MSBuild's default is `true`, but [templates/Directory.Build.props](../templates/Directory.Build.props) sets it to `false`, so `dotnet pack` skips every project that doesn't opt in. The same property keeps MA0004 on for the library, as [runtime-performance.md: Async](runtime-performance.md#async) describes. ([Microsoft Learn: NuGet pack and restore as MSBuild targets](https://learn.microsoft.com/nuget/reference/msbuild-targets))

/// house | simon-mckenna

**House:** Warnings as errors is always on, not only in CI. You can suppress a warning, but only with an inline comment that says why. [ci.md](ci.md) holds the canonical statement.

///

- **Run one-off tools with `dotnet tool exec` or `dnx` instead of installing them globally.** `dnx` ignores the SDK selection in `global.json`. ([Microsoft Learn: What's new in the .NET 10 SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Use file-based apps (`dotnet run app.cs`) for scripts and samples instead of throwaway console projects.** They support publishing and Native AOT. ([Microsoft Learn: What's new in the .NET 10 SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk))
- **Target the latest release, LTS or STS.** That's this repository's freshness policy. .NET 11 is an STS release, supported until 2028-11-14. .NET 10, the LTS release before it, ends support on the same day. ([Microsoft: .NET Support Policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core))

## Repository layout

- **Put shipping code in `src/`, test projects in `tests/`, and build configuration at the root.** Give each project its own directory, named after the assembly. Mirror the disk layout in the solution with `/src/` and `/tests/` solution folders, as [templates/example.slnx](../templates/example.slnx) does. ([dotnet/samples: migrate-library-csproj](https://github.com/dotnet/samples/tree/main/framework/libraries/migrate-library-csproj))

/// house | simon-mckenna

**House:** Add a `/build/` solution folder that lists `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, `global.json`, and `.editorconfig`. They configure every project, and the folder shows them in Solution Explorer. [templates/example.slnx](../templates/example.slnx) includes it, and [Stannard: project-structure](https://github.com/Aaronontheweb/dotnet-skills/blob/master/skills/project-structure/SKILL.md) uses the same shape.

///

- **Set `<UseArtifactsOutput>true</UseArtifactsOutput>` in the root `Directory.Build.props`.** All build output goes under one `artifacts/<type>/<project>/<pivot>` root, instead of `bin/` and `obj/` in each project.
  - The types are `bin`, `obj`, `publish`, and `package`. `package` has no project folder, so `.nupkg` files go to `artifacts/package/<configuration>`.
  - Tools and CI steps can rely on the layout. The per-project layout "can change drastically via relatively simple MSBuild changes".
  - It has been GA since .NET 8, and it's still opt-in in the .NET 10 SDK. [templates/Directory.Build.props](../templates/Directory.Build.props) opts in.
  - MSBuild reads the property before it evaluates the project, so set it only in `Directory.Build.props` or on the command line. In a project file, it fails the build with `NETSDK1199`. ([dotnet/sdk: Strings.resx](https://github.com/dotnet/sdk/blob/3450faa58bf7e1ed0487f56d4b68b6c64ae60508/src/Tasks/Common/Resources/Strings.resx#L885))
  - For a single-TFM project, the last folder is the lowercase configuration, with no TFM: `artifacts/bin/App/release`. Update hard-coded `bin/<Config>/<tfm>` paths in Dockerfiles, CI steps, and scripts in the same commit.
  - `dotnet new gitignore` already ignores `artifacts/`. ([dotnet/sdk: Gitignore template](https://github.com/dotnet/sdk/blob/main/template_feed/Microsoft.DotNet.Common.ItemTemplates/content/Gitignore/.gitignore))

  ([Microsoft Learn: Artifacts output layout](https://learn.microsoft.com/dotnet/core/sdk/artifacts-output), [.NET Blog: Announcing .NET 8 Preview 3](https://devblogs.microsoft.com/dotnet/announcing-dotnet-8-preview-3/))

- **Keep `global.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`, and `.editorconfig` at the repository root.** MSBuild searches up from each project for the nearest `Directory.Build.props` and `Directory.Build.targets`, and Central Package Management finds `Directory.Packages.props` the same way, so every project inherits the root files. The search stops at the first file it finds. A file of the same name in a subfolder therefore replaces the root one for the projects under it, unless it imports the root one first: `<Import Project="$([MSBuild]::GetPathOfFileAbove('Directory.Build.targets', '$(MSBuildThisFileDirectory)../'))" />`. ([Microsoft Learn: Customize the build by folder](https://learn.microsoft.com/visualstudio/msbuild/customize-by-directory), [Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management))
- **Put test projects beside the code they test, never inside it.** `tests/Example.Library.Tests` mirrors `src/Example.Library`, named `<Project>.Tests`. For what goes in them, see [testing.md](testing.md).
- **Inside a project, group by feature, not by pattern.** For modules and vertical slices, see [architecture.md](architecture.md).
- **Don't add structure before you need it.** A single-project tool can be `src/Tool` and `tests/Tool.Tests`. Add `docs/` and solution filters when the repository needs them. ([Microsoft Learn: Organizing and testing projects with the .NET CLI](https://learn.microsoft.com/dotnet/core/tutorials/testing-with-cli))
- **Generate `.gitignore` with `dotnet new gitignore`, and don't edit the generated part.** The SDK template is the standard .NET ignore list, and it changes with the toolchain, so this repository ships no `.gitignore` template. Regenerate the file after major SDK upgrades. Keep repository-specific additions, such as `.dotnet/`, in a marked block at the end, so you can overwrite everything above it. ([Microsoft Learn: dotnet new gitignore](https://learn.microsoft.com/dotnet/core/tools/dotnet-new-sdk-templates))

## .editorconfig

- **Give every repository a root `.editorconfig`, copied from [templates/.editorconfig](../templates/.editorconfig).** Remove a rule only after you've decided you disagree with it. The file sets file-scoped namespaces, expression-bodied members where they fit on one line, collection expressions, auto-implemented properties (IDE0032), and standard .NET naming, with analyzer diagnostics at `warning` by default.
- **Let the build enforce style.** `.editorconfig` severities fail the build only because [templates/Directory.Build.props](../templates/Directory.Build.props) sets `EnforceCodeStyleInBuild` and `TreatWarningsAsErrors`. Without both files, nothing enforces the style rules.
- **Use one `.editorconfig` at the root, not one per project.** Add a nested file only for a real exception, such as relaxed documentation comment rules under `tests/`. Put only the differences in it: when both files set a key, the deeper file wins. ([Microsoft Learn: Configuration files for code analysis rules](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/configuration-files))

## Containers

- **Build images with `dotnet publish /t:PublishContainer`, not a hand-written Dockerfile.** The SDK builds the image from the project, so there's no Dockerfile to keep in step with it. It pushes to the local container runtime by default. From .NET 11, it uses `wslc` on Windows or `container` on macOS when one is installed, and falls back to Docker or Podman. Set `ContainerRegistry` to push to a registry, or `ContainerArchiveOutputPath` to write a tarball. Since .NET 10, console apps work without `<EnableSdkContainerSupport>`, like ASP.NET Core and Worker apps. ([Microsoft Learn: Containerize a .NET app with dotnet publish](https://learn.microsoft.com/dotnet/core/containers/sdk-publish), [Microsoft Learn: What's new in the .NET 10 SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/sdk), [Microsoft Learn: What's new in the SDK and tooling for .NET 11](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-11/sdk))

  ```xml
  <!-- All container config is MSBuild properties; illustrative values -->
  <PropertyGroup Label="Container image">
    <ContainerRepository>contoso/example-worker</ContainerRepository>
    <ContainerImageTags>1.4.0;latest</ContainerImageTags>
    <ContainerFamily>resolute-chiseled</ContainerFamily>
  </PropertyGroup>
  ```

- **Expect Ubuntu, not Debian, in the base images.** Since .NET 10, version-only tags resolve to Ubuntu, and there are no Debian images. `mcr.microsoft.com/dotnet/aspnet:11.0` is Ubuntu 26.04 "Resolute", where `aspnet:10.0` is Ubuntu 24.04 "Noble". For a smaller image, set `ContainerFamily` to a chiseled variant, such as `resolute-chiseled`, or to `alpine`. ([Microsoft Learn: Default .NET container tags now use Ubuntu](https://learn.microsoft.com/dotnet/core/compatibility/containers/10.0/default-images-use-ubuntu), [Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration#containerfamily), [dotnet/dotnet-docker: .NET Container Tags -- Patterns and Policies](https://github.com/dotnet/dotnet-docker/blob/main/documentation/supported-tags.md))
- **If the app isn't invariant, use the `-extra` tag.** The chiseled and Alpine images include neither ICU nor `tzdata`, and set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true`. An app that names a culture throws `CultureNotFoundException` at startup. An app that only reads `CurrentCulture` formats and compares as invariant without an error. `<InvariantGlobalization>false</InvariantGlobalization>` doesn't override the environment variable. `resolute-chiseled-extra` and `alpine-extra` add back ICU and `tzdata`. For details, see [globalization.md](globalization.md).
- **Keep the rootless default.** For .NET 8 and later on the Microsoft runtime images, container publish sets `ContainerUser` to the non-root `app` user on Linux. `ContainerPort` comes from `ASPNETCORE_URLS`, `ASPNETCORE_HTTP_PORTS`, or `ASPNETCORE_HTTPS_PORTS`. To fix a broken volume mount, fix the mount. Don't set `ContainerUser` to `root` or expose a privileged port. ([Microsoft Learn: Containerize a .NET app reference](https://learn.microsoft.com/dotnet/core/containers/publish-configuration))
