---
targets: [net10.0, csharp-14]
last-reviewed: 2026-08-21
last-used: 2026-08-21
sources:
  [
    ms-learn,
    meziantou,
    andrew-lock,
    khalid,
    steve-gordon,
    jon-skeet,
    avalonia-blog,
  ]
---

# Globalization & localization

The mechanism is settled: `.resx` resources behind `IStringLocalizer<T>`, culture selection through request localization middleware, and ICU as the culture data engine on every platform. The decisions that cause trouble are all defaults:

- Culture leaks into machine-facing strings.
- Invariant mode arrives with a template instead of by choice.
- Container images ship without the data the runtime needs.

## Opinions

### Split every format, parse, and compare call by audience

**Use the current culture for text that people read. For text that machines read, pass `CultureInfo.InvariantCulture` explicitly, and compare with `StringComparison.Ordinal`.** This one rule prevents most globalization bugs, and the machine-facing half is the one people skip. The failure is silent until someone runs the service under `de-DE`, and `1.5` round-trips as `15`. ([Microsoft Learn: Best practices for comparing strings](https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings), [Microsoft Learn: Double.Parse](https://learn.microsoft.com/dotnet/api/system.double.parse))

| Audience                                                        | Formatting and parsing                     | Comparison                                     |
| --------------------------------------------------------------- | ------------------------------------------ | ---------------------------------------------- |
| A human reading a screen                                        | `CurrentCulture` (the default)             | `CurrentCulture`, for sorting a displayed list |
| A machine: JSON, logs, URLs, file names, config, DB round-trips | `CultureInfo.InvariantCulture`, explicitly | `StringComparison.Ordinal`                     |

**Never use `StringComparison.InvariantCulture` or `InvariantCultureIgnoreCase`.** They look like the machine-facing choice, but they aren't. Only the culture _data_ is invariant. The collation still follows whichever ICU or NLS version the host has, so the same comparison can return different results on two machines. `InvariantCulture` is right for formatting and parsing, and wrong for comparison. ([Meziantou: StringComparison.InvariantCulture is not always invariant](https://www.meziantou.net/stringcomparison-invariantculture-is-not-always-invariant.htm), [Microsoft Learn: Globalization and ICU](https://learn.microsoft.com/dotnet/core/extensions/globalization-icu))

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

All five warn at the `latest-recommended` level that [templates/Directory.Build.props](../templates/Directory.Build.props) sets. **CA1307 is the exception, and needs an explicit severity.** [templates/.editorconfig](../templates/.editorconfig) sets it:

```ini
dotnet_diagnostic.CA1307.severity = warning
```

With `TreatWarningsAsErrors` already on, a missing `StringComparison` becomes a build failure. Meziantou.Analyzer, described in [csharp.md](csharp.md), covers the same ground from the other direction. A `BannedApiAnalyzers` list for the two invariant comparisons lost, because CA1309 already catches them.

### Treat `InvariantGlobalization` as an explicit decision, never an inherited template line

**Decide on invariant mode on its own merits, and comment the property where you set it.** `<InvariantGlobalization>true</InvariantGlobalization>` makes the runtime skip ICU and use built-in invariant data. That's a real size and startup gain for a service with no localized output, and a trap when it arrives as part of an unrelated decision.

On SDK 10.0.400, `dotnet new webapi` doesn't set it, but `dotnet new webapiaot` does, next to `<PublishAot>true</PublishAot>` in the generated project file. So it usually enters a codebase when someone adopts Native AOT, as described in [runtime-performance.md](runtime-performance.md). It then changes how every `ToString` and `Compare` call in the app behaves.

Turning it on has these costs:

- Only the invariant culture exists. Constructing any other `CultureInfo` throws, unless you also set `PredefinedCulturesOnly=false`.
- `TimeZoneInfo.TryConvertIanaIdToWindowsId` and `TryConvertWindowsIdToIanaId` fail, because both are ICU-dependent.
- Casing and collation stop being linguistic. This **isn't** a substitute for `Ordinal`: the audience split above still applies.

([Microsoft Learn: Globalization config settings](https://learn.microsoft.com/dotnet/core/runtime-config/globalization), [Microsoft Learn: Globalization and ICU](https://learn.microsoft.com/dotnet/core/extensions/globalization-icu))

### Pin ICU only when reproducible comparison beats current comparison

**Use the default, the system ICU, unless you have a stated reason not to.** Globalization has used ICU on every platform since .NET 5, including Windows, which ships `icu.dll`. There are three alternatives, listed from most to least often right:

- **`System.Globalization.AppLocalIcu`** with a `Microsoft.ICU.ICU4C.Runtime` package reference ships a pinned ICU with the app, so collation and CLDR data are byte-identical in every deployment. Use it when a sort order is part of your contract.
- **`DOTNET_ICU_VERSION_OVERRIDE`** pins a system ICU version on Linux. Before .NET 10, it was `CLR_ICU_VERSION_OVERRIDE`. It applies only to Microsoft-built .NET, not to distribution builds. ([Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **`System.Globalization.UseNls`** goes back to Windows NLS. Use it only for bug compatibility with a legacy app, because you lose the APIs that convert between IANA and Windows time zone IDs. Since .NET 9, the environment variable takes precedence over the `runtimeconfig.json` value for this setting. Before .NET 9, it was the other way round.

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

Four things decide whether this works in production:

- **`SupportedCultures` and `SupportedUICultures` are independent.** `CurrentCulture` controls number, date, and currency formatting, and sorting. `CurrentUICulture` controls which `.resx` file the `ResourceManager` resolves. English text with German number formatting is a supported configuration, not a bug. ([Microsoft Learn: Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization?view=aspnetcore-10.0))
- **The providers run in order: query string, cookie, then `Accept-Language`. The first match wins.** In production, use the cookie, and keep the query string for debugging. `Accept-Language` reflects the user's OS, not a choice they made, so a production app needs a way for users to set their own culture. ([Lock: Adding Localisation to an ASP.NET Core application](https://andrewlock.net/adding-localisation-to-an-asp-net-core-application/))
- **Middleware order matters.** Call `UseRequestLocalization` before anything that reads the culture, and _after_ routing if you use `RouteDataRequestCultureProvider`. ([Microsoft Learn: Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization?view=aspnetcore-10.0))
- **Culture fallback works per resource, and ends at the default file:** `Welcome.fr-CA.resx`, then `Welcome.fr.resx`, then `Welcome.resx`. If you want a missing translation to show its key instead of silently rendering English, don't ship a default `.resx` file.

Learn to recognise one failure: **if `RootNamespace` and `AssemblyName` differ,** for example in a project directory named `my-project-name`, resource lookup fails completely. Fix it with `[assembly: RootNamespace]` and `[assembly: ResourceLocation]`, not by renaming resources. ([Microsoft Learn: Globalization and localization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/localization?view=aspnetcore-10.0))

### Name resource keys with a constants class, not magic strings

**Pass keys to `IStringLocalizer` from a `static class ResourceKeys` of `const string` fields.** `IStringLocalizer` accepts the default-language string as the key by design, as in `_localizer["About Title"]`. That saves the up-front `.resx` work, but scatters magic strings through the codebase. A constants class costs one file, keeps each key in one place, and works in attributes such as `[Display(Name = ResourceKeys.AboutTitle)]`. ([Lock: Localising the DisplayAttribute and avoiding magic strings](https://andrewlock.net/localising-the-displayattribute-and-avoiding-magic-strings-in-asp-net-core/))

Strongly typed resource classes from the designer are checked at compile time, but lost on portability. .NET still has no cross-platform generator for strongly typed resources: on net10.0, `<GenerateSource>true</GenerateSource>` on an `EmbeddedResource` builds without errors and generates nothing.

- The designer route needs Visual Studio tooling and a checked-in `.Designer.cs` file.
- The cross-platform route is `Microsoft.CodeAnalysis.ResxSourceGenerator`. dotnet/runtime uses it, but Microsoft still publishes it only as a prerelease. Revisit this when a stable version ships. ([Abuhakmeh: Getting Started With .NET Localization](https://khalidabuhakmeh.com/getting-started-with-net-localization))

### Client-side: ship the globalization data the app can actually reach

- **Blazor WebAssembly loads only the app's own culture data by default.** If users can switch culture at run time, set `<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>`.
  - Time zone data is trimmed separately, with `<InvariantTimezone>true</InvariantTimezone>`. `<BlazorEnableTimeZoneSupport>` is superseded, so delete it.
  - In .NET 10, standalone WebAssembly apps also load globalization data for `CultureInfo.DefaultThreadCurrentUICulture`. .NET 9 and earlier only used `DefaultThreadCurrentCulture`. ([Microsoft Learn: Blazor globalization and localization](https://learn.microsoft.com/aspnet/core/blazor/globalization-localization?view=aspnetcore-10.0))
- **.NET MAUI has no localization abstraction.** It uses plain `.resx` files per culture and `CultureInfo.DefaultThreadCurrentUICulture`, with no equivalent of `RequestLocalization`. To switch culture at run time, you re-resolve bindings yourself. ([Microsoft Learn: .NET MAUI localization](https://learn.microsoft.com/dotnet/maui/fundamentals/localization?view=net-maui-10.0))
- **Avalonia reads `.resx` from XAML with `{x:Static}`,** against a generated resources class. There's no first-party way to switch language without a restart. It needs a custom markup extension, and the community packages that provide one aren't vetted. The source is the project's own documentation, so rely on it for how this works, not for whether to use it. The source caveat in [ui-frameworks.md](ui-frameworks.md) applies. ([Avalonia Docs: Localizing using ResX](https://docs.avaloniaui.net/docs/app-development/localizing))

### Containers: chiseled and Alpine images drop this data

**If the app formats dates, sorts text, or resolves a time zone, use the `-extra` image tag.** [project-structure.md](project-structure.md) recommends `noble-chiseled` or `alpine` images for size. The size-optimised images (Alpine, Ubuntu chiseled, and Azure Linux distroless) are exactly the ones that "don't include globalization dependencies such as ICU or tzdata … only work with apps that are configured for globalization invariant mode". Each has an `-extra` variant, such as `10.0-alpine-extra` or `10.0-noble-chiseled-extra`, that adds ICU, tzdata, and `stdc++` back. ([Microsoft Learn: .NET container images](https://learn.microsoft.com/dotnet/core/docker/container-images))

The two missing pieces fail differently, and a missing ICU fails in different ways depending on what the app does:

- **No ICU: the image has already decided for you.** The base images set `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true` as an environment variable, so the app runs in invariant mode however it was built.
  - An app that names a culture throws `CultureNotFoundException` at startup. `PredefinedCulturesOnly` defaults to true here, so the `RequestLocalizationOptions` above fails before the app serves a request.
  - An app that only uses `CurrentCulture` fails without an exception: it formats and compares as invariant.
  - The setting lives in the environment, not the project file, so `<InvariantGlobalization>false</InvariantGlobalization>` doesn't undo it. If you install `icu-libs` and `icu-data-full` by hand, you must clear the variable too.
  - The `-extra` images work, because they don't set the variable. ([Microsoft Learn: Environment variables take precedence in app runtime configuration settings](https://learn.microsoft.com/dotnet/core/compatibility/deployment/9.0/envvar-precedence), [Microsoft Learn: Globalization config settings](https://learn.microsoft.com/dotnet/core/runtime-config/globalization), [dotnet-docker: runtime-deps 10.0 Dockerfiles](https://github.com/dotnet/dotnet-docker/tree/main/src/runtime-deps/10.0))
- **No tzdata: a visible failure.** `TimeZoneInfo.FindSystemTimeZoneById` throws `TimeZoneNotFoundException`. Invariant mode doesn't affect it, so it stays a real exception. `RUN apk add --no-cache tzdata` fixes this one on its own. ([Gordon: TimeZoneNotFoundException in Alpine Based Docker Images](https://www.stevejgordon.co.uk/timezonenotfoundexception-in-alpine-based-docker-images))

### Localization makes dates look right, not be right

**Store instants as UTC, store the user's IANA time zone ID instead of a fixed offset, and convert at the edge.** For the full storage and type guidance, including when a future local event should _not_ become UTC, see [datetime.md](datetime.md). A local date and time can occur twice, or not at all. Culture-correct formatting can't fix a value that was ambiguous before it was formatted. That's the reasoning behind Noda Time's separate `Instant`, `LocalDateTime`, and `ZonedDateTime` types. ([Skeet: More fun with DateTime](https://codeblog.jonskeet.uk/2012/05/02/more-fun-with-datetime/))

`TimeZoneInfo.TryConvertIanaIdToWindowsId` converts between the two ID families, but only outside invariant and NLS modes. So the storage choice here, and the invariant mode and ICU choices earlier in this file, are really one decision.

## Coming next (preview, not yet the opinion)

`WebAssemblyComponentsOptions.UseCultureFromServer` lets the WebAssembly client of a Blazor Web App choose a culture independently of the server during prerendering. The documentation lists it only under the `aspnetcore-11.0` moniker. It's a .NET 11 preview feature, not a .NET 10 one, although some summaries describe it as .NET 10. ([Microsoft Learn: Blazor globalization and localization](https://learn.microsoft.com/aspnet/core/blazor/globalization-localization?view=aspnetcore-10.0))
