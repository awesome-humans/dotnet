---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources:
  [
    ms-learn,
    aspnet-blog,
    meziantou,
    gerald-versluis,
    james-montemagno,
    avalonia-blog,
  ]
---

# UI frameworks

Use Blazor for web UI, .NET MAUI for mobile and desktop, and Avalonia for cross-platform desktop. Choose per app, and mix render modes in a Blazor app only on purpose.

## Blazor

- **Choose render mode per page, not per app.** ([Microsoft Learn: ASP.NET Core Blazor render modes](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes))
- **Use `[PersistentState]` for state that must survive prerendering and circuit loss,** instead of ad hoc session storage. ([Microsoft Learn: ASP.NET Core Blazor prerendered state persistence](https://learn.microsoft.com/aspnet/core/blazor/state-management/prerendered-state-persistence))
- **Use `NavigationManager.NotFound()` for missing resources, and show the not-found state in the UI too.** It sets a 404 status only under static SSR or during prerendering. Once a component is interactive, it renders the Not Found content without a status code. ([Microsoft Learn: ASP.NET Core Blazor navigation](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/navigation))
- **In interactive server rendering, get the user's time zone from the browser through a scoped `TimeProvider` per circuit.** The process's local zone belongs to the server and means nothing to the user.
  1. Get the zone ID through JavaScript interop with `Intl.DateTimeFormat().resolvedOptions().timeZone`, and validate it with `TimeZoneInfo.TryFindSystemTimeZoneById`.
  2. Register a scoped `TimeProvider` that overrides `LocalTimeZone` with that zone.

  Components then render in the user's local time wherever they inject time instead of reading `DateTimeOffset.Now`. For details, see [datetime.md](datetime.md). Under `InteractiveServer`, the interop only runs in `OnAfterRenderAsync`, so the first render shows the wrong zone. Plan for one corrected re-render, instead of assuming the zone is known at first paint, and handle `JSDisconnectedException`. ([Meziantou: Convert DateTime to the user's time zone with Blazor](https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor-time-provider.htm))

- **Read WebAssembly `HttpClient` responses asynchronously. Streaming is on by default in .NET 10, which is a breaking change.** `response.Content.ReadAsStreamAsync()` now returns a `BrowserHttpReadStream` instead of a `MemoryStream`, and that stream rejects synchronous calls such as `Stream.Read(Span<byte>)`. Fix the call site, not the default, because streaming keeps a large response off the heap. If a dependency you don't own makes the synchronous call, you can opt out:
  - Per request, with `requestMessage.SetBrowserResponseStreamingEnabled(false)`.
  - Per project, with `<WasmEnableStreamingResponse>false</WasmEnableStreamingResponse>`.
  - With an environment variable. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), last updated 2026-08-31)
- **A WebAssembly app loads only the globalization data for its own culture.** If users can switch culture at run time, set `<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>`. Remove the superseded `<BlazorEnableTimeZoneSupport>`. ([globalization.md](globalization.md))

### Component testing

- **Unit-test components with bUnit, and use Playwright only for end-to-end tests.** Microsoft has no official component testing framework, and Microsoft Learn points to bUnit, which renders components in-process without a browser.
  - Assert with `MarkupMatches`, never with string equality on markup. It compares HTML semantically, so whitespace and attribute order don't fail the test.
  - Behaviour that depends on real JavaScript interop or browser DOM changes needs an end-to-end test. Test it with Playwright, instead of faking `IJSRuntime` until the test proves nothing. ([Microsoft Learn: Test components in ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/test))
- **Use bUnit 2.x with xUnit v3,** to match this repository's testing stack in [testing.md](testing.md). In 2.x, the test class inherits `BunitContext` and renders with `Render<T>()`. ([bUnit: Writing tests](https://bunit.dev/docs/getting-started/writing-tests.html))

  <!-- Verified: compiles and passes on net10.0 / bunit 2.9.0 / xunit.v3 4.0.0 (2026-08-18) -->

  ```csharp
  public class CounterTests : BunitContext
  {
      [Fact]
      public void Counter_ClickingButton_IncrementsCount()
      {
          // Arrange
          var cut = Render<Counter>();

          // Act
          cut.Find("button").Click();

          // Assert
          cut.Find("[role=status]").MarkupMatches(
              """<p role="status">Current count: 1</p>""");
      }
  }
  ```

## .NET MAUI

- **Put platform billing and purchases behind a single interface, such as `IBillingService`, with conditional compilation per store. Always validate purchases on the server.** ([Versluis: Implementing Cross-Platform In-App Billing in .NET MAUI Applications](https://devblogs.microsoft.com/dotnet/cross-platform-billing-dotnet-maui/))
- **Enable Material 3 on Android with `<UseMaterial3>true</UseMaterial3>`** (MAUI 10.0.60 and later) for current theming. ([Versluis: Give Your .NET MAUI Android Apps a Material 3 Makeover](https://devblogs.microsoft.com/dotnet/dotnet-maui-material-3/))
- **Keep trimming on, and exclude a problem assembly instead of disabling trimming.** Root the assembly in a `Linker.xml` file, referenced by `<TrimmerRootDescriptor Include="Linker.xml" />`. Turning off `PublishTrimmed` is a diagnostic step to confirm that trimming is the cause, not a fix. ([Versluis: Excluding Assemblies from Trimming in .NET MAUI](https://blog.verslu.is/maui/exclude-assemblies-from-trimming/))
- **Google Play's 16 KB page size requirement is about native library alignment, not trimming.** Targeting .NET 9 or later meets it without any changes. ([Versluis: Preparing Your .NET MAUI Apps for Google Play's 16 KB Page Size Requirement](https://devblogs.microsoft.com/dotnet/maui-google-play-16-kb-page-size-support/))
- **Test preview SDKs with `sdk.paths` in `global.json`,** instead of changing the machine default, and ignore the `.dotnet/` folder it installs into. For setup and caveats, see [project-structure.md](project-structure.md). ([Versluis: Test .NET MAUI Preview SDKs Locally with global.json sdk.paths](https://blog.verslu.is/maui/test-dotnet-maui-preview-sdk-locally/))

### App lifecycle

- **Handle the app lifecycle with the cross-platform `Window` events, not per-platform code.**
  1. Subclass `Window`, and override the events you need: `OnCreated`, `OnActivated`, `OnDeactivated`, `OnStopped`, `OnResumed`, and `OnDestroying`, plus `OnBackgrounding` on iOS and Mac Catalyst.
  2. Return the subclass from `App.CreateWindow`.
  3. In `Stopped`, disconnect long-running work and cancel pending requests. In `Resumed`, subscribe again and refresh the visible content.

  Use `ConfigureLifecycleEvents` in `MauiProgram` only when a platform-specific hook has no cross-platform event. ([Microsoft Learn: .NET MAUI app lifecycle](https://learn.microsoft.com/dotnet/maui/fundamentals/app-lifecycle))

  <!-- Illustrative only: MAUI workloads are not installed on the authoring machine, so this snippet is unverified -->

  ```csharp
  public class MainWindow : Window
  {
      protected override void OnStopped() => _sync.PauseBackgroundSync();

      protected override void OnResumed() => _sync.ResumeAndRefresh();
  }
  ```

### Offline & sync

- **Build mobile apps offline-first. The local store is the source of truth, and the network is how it syncs.** Cache remote data locally with an explicit expiry. Use SQLite-net for data you query, or MonkeyCache's `Barrel.Current.Add(key, data, expireIn, eTag)` for simple payloads. The ETag overloads let you skip downloading a response that hasn't changed. ([Montemagno: Data caching made simple with Monkey Cache](https://montemagno.com/data-caching-made-simple-with-monkey-cache/))
- **Check `Connectivity.NetworkAccess == NetworkAccess.Internet` before remote calls, and react to `ConnectivityChanged`. Never ping to test reachability.** The old `IsReachable` APIs were removed on purpose, so make the real request and handle failure. ([Montemagno: Upgrading to Xamarin.Essentials from Plugins](https://montemagno.com/upgrading-from-plugins-to-xamarin-essentials/))

## Avalonia & reactive UI

- **Keep view models free of UI framework references.** `INotifyPropertyChanged`, Rx, and Dynamic Data types are platform-neutral. A view model with no Avalonia or WPF reference binds unchanged on either framework, and you can unit-test it without a UI. ([Avalonia Docs: The MVVM pattern](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/))
- **For reactive collection state in XAML UIs, use Dynamic Data instead of your own `ObservableCollection` plumbing.** Key entities in a `SourceCache<T, TKey>`, and use `Connect()` to project filtered and sorted views into a bindable collection. Updates, filters, and sorts then compose, instead of being rewritten for each screen. ([Avalonia Docs: Sorting, filtering, and grouping collections](https://docs.avaloniaui.net/docs/data-binding/collection-views))

  <!-- Verified: compiles and runs on net10.0 / DynamicData 9.4.33 (2026-08-12) -->

  ```csharp
  var orders = new SourceCache<Order, int>(o => o.Id);

  ReadOnlyObservableCollection<Order> openOrders;
  using var subscription = orders.Connect()
      .Filter(o => o.Status == OrderStatus.Open)
      .SortBy(o => o.Placed)
      .Bind(out openOrders)   // the view binds to openOrders; only mutate the cache
      .Subscribe();

  orders.AddOrUpdate(new Order(1, OrderStatus.Open, DateTimeOffset.UtcNow));
  ```

### Avalonia 12

- **Target Avalonia 12 for new cross-platform desktop apps.** It was released on 2026-04-07, on .NET 10 and SkiaSharp 3.0, and it rebuilds the foundations:
  - The compositor was reworked.
  - Android has a real `IDispatcherImpl` over `Looper` and `MessageQueue`.
  - Mac Catalyst is enabled in `Avalonia.iOS`.

  Plan for the removals when you migrate: Direct2D1, Tizen support, `Avalonia.Browser.Blazor`, `BinaryFormatter` usage, and netstandard2.0 in almost all projects are gone. ([Avalonia 12: Ready for What's Next](https://avaloniaui.net/blog/avalonia-12))

- **Keep compiled bindings on, as they are by default in 12.** Compiled bindings fail at build time, instead of silently binding to nothing. Use a reflection-based binding only to opt out for a single dynamic case. ([Avalonia 12: Ready for What's Next](https://avaloniaui.net/blog/avalonia-12))
- **Embed web content with the built-in WebView, not a bundled Chromium.** The WebView, previously a commercial Accelerate component, became open source in 12. It renders through each platform's native engine, so it doesn't add to app size. The Accelerate brand was retired in the same release, so check the current cost of a "commercial-only" Avalonia feature before you work around it. ([Avalonia: The Avalonia WebView Is Going Open-Source](https://avaloniaui.net/blog/the-avalonia-webview-is-going-open-source/), [Avalonia: Retiring Accelerate: One Brand, One Clear Path](https://avaloniaui.net/blog/retiring-accelerate))
- **Use the built-in page navigation for mobile-style Avalonia apps.** Avalonia 12 includes `ContentPage`, `DrawerPage`, `CarouselPage`, `TabView`, and `PipsPager`, with gesture and wrap-selection support. Before 12, the workaround was a custom navigation stack over a `ContentControl`. Replace it. ([Avalonia 12: Ready for What's Next](https://avaloniaui.net/blog/avalonia-12))
- **Avalonia's XAML hot reload works as of 2026-09-02, and it's a paid feature.** `AvaloniaUI.DiagnosticsSupport.HotReload` is GA on Windows, macOS, Linux, iOS, and Android.
  - `dotnet watch` drives it, using the same .NET Hot Reload mechanism as MAUI, not one specific to Avalonia.
  - It patches changed elements in place, so view state survives the edit.
  - It needs an `AvaloniaUILicenseKey` and a Plus, Pro, or Enterprise subscription. That's the current form of the commercial component question above. ([Avalonia: Hot Reload for Avalonia](https://avaloniaui.net/blog/hot-reload-ga))
- **Don't build on Impeller yet.** The Impeller backend is a collaboration between Avalonia and Google's Flutter team to bring a GPU-first renderer to .NET, and it's experimental. A maintainer paused it on 2026-03-09 to ship v12. By 2026-09-24, well after the v12 release, no restart had been announced. Stay on the default Skia renderer, and treat Impeller as a "coming next" item. ([Avalonia: Avalonia Partnering with Google's Flutter Team to Bring Impeller Rendering to .NET](https://avaloniaui.net/blog/avalonia-partners-with-google-s-flutter-t-eam-to-bring-impeller-rendering-to-net); [Avalonia: Impeller rendering subsystem, discussion #20838](https://github.com/AvaloniaUI/Avalonia/discussions/20838))

- **Source-redundancy note:** Avalonia guidance has the fewest sources of anything in this repository. It lost one more on 2026-08-23, when `nick-polyak`, the only independent source, moved to the watch list for dormancy, along with his back catalogue.
  - What remains is `avalonia-blog` and the discovery-only `awesome-avalonia`. Both were admitted on 2026-08-14 under the lowered longevity bars, and both come from the project itself.
  - They're authoritative on what shipped and on the documented API, but they focus on adoption when it comes to whether you should use it. Read release claims, especially headline FPS numbers, as vendor benchmarks.
  - Cite the docs freely for how something works, such as a binding, a control, or a collection view, the way [csharp.md](csharp.md) cites `ms-learn`. Treat any judgement about whether to adopt something as uncorroborated until an independent source supports it.
  - No current source can provide that independent view, so it's an open gap for `vet-source`.

<!-- Mobile practice opinions (lifecycle, offline/sync, store compliance) split out into their own file if the MAUI section outgrows this one -->
