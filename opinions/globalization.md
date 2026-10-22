---
targets: [net10.0, csharp-14]
last-reviewed: 2026-08-21
sources:
  [
    andrew-lock,
    avalonia-blog,
    jon-skeet,
    khalid,
    meziantou,
    ms-learn,
    steve-gordon,
  ]
---

# Globalization & localization

The mechanism is settled: `.resx` resources behind `IStringLocalizer<T>`, culture selection through request localization middleware, and ICU for culture data on every platform. The trouble comes from three defaults:

- Culture leaks into machine-facing strings.
- Invariant mode arrives with a template, not by choice.
- Container images ship without the data the runtime needs.

## Opinions

### Split every format, parse, and compare call by audience

**Use the current culture for text people read. For text machines read, pass `CultureInfo.InvariantCulture` explicitly, and compare with `StringComparison.Ordinal`.** This rule prevents most globalization bugs, and the machine-facing half is the one people skip. Nothing fails until the service runs under `de-DE` and `1.5` round-trips as `15`. ([Microsoft Learn: Best practices for comparing strings](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings), [Microsoft Learn: Double.Parse](https://learn.microsoft.com/dotnet/api/system.double.parse))

| Audience                                                        | Formatting and parsing                     | Comparison                                     |
| --------------------------------------------------------------- | ------------------------------------------ | ---------------------------------------------- |
| A person reading a screen                                       | `CurrentCulture` (the default)             | `CurrentCulture`, for sorting a displayed list |
| A machine: JSON, logs, URLs, file names, config, DB round-trips | `CultureInfo.InvariantCulture`, explicitly | `StringComparison.Ordinal`                     |

**Never use `StringComparison.InvariantCulture` or `InvariantCultureIgnoreCase`.** Only the culture _data_ is invariant. The collation follows the host's ICU or NLS version, so the same comparison can return different results on two machines. Use `InvariantCulture` for formatting and parsing, never for comparison. ([Meziantou: StringComparison.InvariantCulture is not always invariant](https://www.meziantou.net/stringcomparison-invariantculture-is-not-always-invariant.htm), [Microsoft Learn: Globalization and ICU](https://learn.microsoft.com/dotnet/core/extensions/globalization-icu))

```csharp
// Wrong: the host's collation decides, and hosts disagree
if (string.Equals(header, "application/json", StringComparison.InvariantCultureIgnoreCase))

// Right: a protocol token is machine-facing
if (string.Equals(header, "application/json", StringComparison.OrdinalIgnoreCase))
```

The analyzers find the call sites:

| Rule           | Finds                                  |
| -------------- | -------------------------------------- |
| CA1304, CA1305 | Formatting without a culture           |
| CA1309         | `InvariantCulture` comparison          |
| CA1310, CA1311 | `IndexOf` and casing without a culture |

All five warn at the `latest-recommended` level that [templates/Directory.Build.props](../templates/Directory.Build.props) sets. **CA1307 needs an explicit severity**, which [templates/.editorconfig](../templates/.editorconfig) sets:

```ini
dotnet_diagnostic.CA1307.severity = warning
```

With `TreatWarningsAsErrors` on, a missing `StringComparison` fails the build. Meziantou.Analyzer, described in [csharp.md](csharp.md), covers the same cases. A `BannedApiAnalyzers` list for the two invariant comparisons lost, because CA1309 already catches them.

### Treat `InvariantGlobalization` as an explicit decision, never an inherited template line

**Decide on invariant mode by itself, and comment the property where you set it.** `<InvariantGlobalization>true</InvariantGlobalization>` makes the runtime skip ICU and use built-in invariant data. That saves size and startup time for a service with no localized output. It's a mistake when it arrives with an unrelated decision.

On SDK 10.0.400, `dotnet new webapi` doesn't set it, but `dotnet new webapiaot` does, next to `<PublishAot>true</PublishAot>`. So it usually arrives when someone adopts Native AOT, as described in [runtime-performance.md](runtime-performance.md). It then changes every `ToString` and `Compare` call in the app.

Turning it on costs you:

- Every culture except the invariant one. Constructing another `CultureInfo` throws, unless you also set `PredefinedCulturesOnly=false`.
- `TimeZoneInfo.TryConvertIanaIdToWindowsId` and `TryConvertWindowsIdToIanaId`, which need ICU.
- Linguistic casing and collation. Invariant mode **isn't** a substitute for `Ordinal`: the audience split still applies.

([Microsoft Learn: Globalization config settings](https://learn.microsoft.com/dotnet/core/runtime-config/globalization), [Microsoft Learn: Globalization and ICU](https://learn.microsoft.com/dotnet/core/extensions/globalization-icu))

### Pin ICU only when reproducible comparison beats current comparison

**Use the system ICU unless you have a stated reason not to.** .NET has used ICU on every platform since .NET 5, including Windows, which ships `icu.dll`. The alternatives, from most to least often right:

- **`System.Globalization.AppLocalIcu`**, with a `Microsoft.ICU.ICU4C.Runtime` package reference, ships a pinned ICU with the app. Collation and CLDR data are then identical in every deployment. Use it when a sort order is part of your contract.
- **`DOTNET_ICU_VERSION_OVERRIDE`** pins a system ICU version on Linux. Before .NET 10, it was `CLR_ICU_VERSION_OVERRIDE`. It works only on Microsoft-built .NET, not distribution builds. ([Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **`System.Globalization.UseNls`** reverts to Windows NLS. Use it only for bug compatibility with a legacy app: you lose the IANA and Windows time zone ID conversions. Since .NET 9, the environment variable overrides the `runtimeconfig.json` value. Before .NET 9, it was the other way round.

([Microsoft Learn: Globalization config settings](https://learn.microsoft.com/dotnet/core/runtime-config/globalization))

### ASP.NET Core: register localization, then drive culture from a cookie

**Call `AddLocalization` with a `ResourcesPath`, and select the culture with `UseRequestLocalization` early in the pipeline.**

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "en-GB", "fr", "de" };
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures));
```

In production, four details decide whether it works:

- **`SupportedCultures` and `SupportedUICultures` are independent.** `CurrentCulture` controls number, date, and currency formatting, and sorting. `CurrentUICulture` controls which `.resx` file `ResourceManager` resolves. English text with German number formatting is a valid configuration. ([Microsoft Learn: Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization?view=aspnetcore-10.0))
- **Providers run in order: query string, cookie, then `Accept-Language`. The first match wins.** Use the cookie in production, and the query string for debugging. `Accept-Language` reflects the user's OS, not their choice, so give users a way to set their culture. ([Lock: Adding Localisation to an ASP.NET Core application](https://andrewlock.net/adding-localisation-to-an-asp-net-core-application/))
- **Order the middleware.** Call `UseRequestLocalization` before anything that reads the culture. If you use `RouteDataRequestCultureProvider`, call it _after_ routing. ([Microsoft Learn: Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization?view=aspnetcore-10.0))
- **Fallback works per resource and ends at the default file:** `Welcome.fr-CA.resx`, then `Welcome.fr.resx`, then `Welcome.resx`. To show the key for a missing translation instead of English, don't ship a default `.resx` file.

**If `RootNamespace` and `AssemblyName` differ, resource lookup fails completely.** A project directory named `my-project-name` causes this. Fix it with `[assembly: RootNamespace]` and `[assembly: ResourceLocation]`, not by renaming resources. ([Microsoft Learn: Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization?view=aspnetcore-10.0))

### Name resource keys with a constants class, not magic strings

**Pass keys to `IStringLocalizer` from a `static class ResourceKeys` of `const string` fields.** `IStringLocalizer` accepts the default-language text as the key, as in `_localizer["About Title"]`. That skips the up-front `.resx` work but scatters magic strings through the code. A constants class keeps each key in one place, and works in attributes such as `[Display(Name = ResourceKeys.AboutTitle)]`. ([Lock: Localising the DisplayAttribute and avoiding magic strings](https://andrewlock.net/localising-the-displayattribute-and-avoiding-magic-strings-in-asp-net-core/))

Strongly typed resource classes lost on portability. .NET has no cross-platform generator for them: on net10.0, `<GenerateSource>true</GenerateSource>` on an `EmbeddedResource` builds cleanly and generates nothing.

- The designer needs Visual Studio and a checked-in `.Designer.cs` file.
- `Microsoft.CodeAnalysis.ResxSourceGenerator` works cross-platform, and dotnet/runtime uses it, but it's still prerelease only. Revisit this when a stable version ships. ([Abuhakmeh: Getting Started With .NET Localization](https://khalidabuhakmeh.com/getting-started-with-net-localization))

### Client-side: ship the globalization data the app can actually reach

- **Blazor WebAssembly loads only the app's own culture data by default.** If users can switch culture at run time, set `<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>`.
  - Time zone data is trimmed separately, with `<InvariantTimezone>true</InvariantTimezone>`. Delete `<BlazorEnableTimeZoneSupport>`, which it replaces.
  - In .NET 10, standalone WebAssembly apps also load data for `CultureInfo.DefaultThreadCurrentUICulture`. .NET 9 and earlier used only `DefaultThreadCurrentCulture`. ([Microsoft Learn: Blazor globalization and localization](https://learn.microsoft.com/aspnet/core/blazor/globalization-localization?view=aspnetcore-10.0))
- **.NET MAUI has no localization abstraction.** It uses `.resx` files per culture and `CultureInfo.DefaultThreadCurrentUICulture`, with nothing like `RequestLocalization`. To switch culture at run time, re-resolve bindings yourself. ([Microsoft Learn: .NET MAUI localization](https://learn.microsoft.com/dotnet/maui/fundamentals/localization?view=net-maui-10.0))
- **Avalonia reads `.resx` in XAML with `{x:Static}` against a generated resources class.** Switching language without a restart needs a custom markup extension, and the community packages that provide one aren't vetted. The cited source is Avalonia's own documentation, so trust it for how this works, not for whether to use it. The caveat in [ui-frameworks.md](ui-frameworks.md) applies. ([Avalonia Docs: Localizing using ResX](https://docs.avaloniaui.net/docs/app-development/localizing))

### Containers: chiseled and Alpine images drop this data

**If the app formats dates, sorts text, or resolves a time zone, use the `-extra` image tag.** [project-structure.md](project-structure.md) recommends `noble-chiseled` or `alpine` images for size. Those size-optimised images (Alpine, Ubuntu chiseled, and Azure Linux distroless) "don't include globalization dependencies such as ICU or tzdata … only work with apps that are configured for globalization invariant mode". Each has an `-extra` variant, such as `10.0-alpine-extra` or `10.0-noble-chiseled-extra`, that adds back ICU, tzdata, and `stdc++`. ([Microsoft Learn: .NET container images](https://learn.microsoft.com/dotnet/core/docker/container-images), [dotnet-docker: .NET Image Variants](https://github.com/dotnet/dotnet-docker/blob/main/documentation/image-variants.md))

The two missing pieces fail differently:

- **Without ICU, the image forces invariant mode.** The base images set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true`, so the app runs invariant however it was built.
  - An app that names a culture throws `CultureNotFoundException` at startup. `PredefinedCulturesOnly` defaults to true here, so the `RequestLocalizationOptions` above fail before the first request.
  - An app that only uses `CurrentCulture` raises no error: it formats and compares as invariant.
  - The setting is an environment variable, so `<InvariantGlobalization>false</InvariantGlobalization>` doesn't undo it. If you install `icu-libs` and `icu-data-full` yourself, clear the variable too.
  - The `-extra` images don't set the variable. ([Microsoft Learn: Environment variables take precedence in app runtime configuration settings](https://learn.microsoft.com/dotnet/core/compatibility/deployment/9.0/envvar-precedence), [Microsoft Learn: Globalization config settings](https://learn.microsoft.com/dotnet/core/runtime-config/globalization), [dotnet-docker: runtime-deps 10.0 Dockerfiles](https://github.com/dotnet/dotnet-docker/tree/main/src/runtime-deps/10.0))
- **Without tzdata, `TimeZoneInfo.FindSystemTimeZoneById` throws `TimeZoneNotFoundException`.** Invariant mode doesn't change that. `RUN apk add --no-cache tzdata` fixes it by itself. ([Gordon: TimeZoneNotFoundException in Alpine Based Docker Images](https://www.stevejgordon.co.uk/timezonenotfoundexception-in-alpine-based-docker-images))

### Localization makes dates look right, not be right

**Store instants as UTC, store the user's IANA time zone ID instead of a fixed offset, and convert at the edge.** For storage and type guidance, including when a future local event should _not_ become UTC, see [datetime.md](datetime.md). A local date and time can occur twice or not at all, and correct formatting can't fix a value that was ambiguous before it was formatted. Noda Time separates `Instant`, `LocalDateTime`, and `ZonedDateTime` for this reason. ([Skeet: More fun with DateTime](https://codeblog.jonskeet.uk/2012/05/02/more-fun-with-datetime/))

`TimeZoneInfo.TryConvertIanaIdToWindowsId` converts between the two ID families only outside invariant and NLS modes. So this storage choice, invariant mode, and the ICU choice are one decision.

## Coming next (preview, not yet the opinion)

`WebAssemblyComponentsOptions.UseCultureFromServer` lets the WebAssembly client of a Blazor Web App choose its culture independently of the server during prerendering. The documentation lists it only under the `aspnetcore-11.0` moniker, so it's a .NET 11 preview feature, although some summaries call it .NET 10. ([Microsoft Learn: Blazor globalization and localization](https://learn.microsoft.com/aspnet/core/blazor/globalization-localization?view=aspnetcore-10.0))
