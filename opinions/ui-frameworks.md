---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-14
last-used: 2026-09-25
sources:
  [
    aspnet-blog,
    avalonia-blog,
    gerald-versluis,
    james-montemagno,
    meziantou,
    ms-learn,
  ]
---

# UI frameworks

Use Blazor for web UI, .NET MAUI for mobile and desktop, and Avalonia for cross-platform desktop. Choose per app, and mix render modes in a Blazor app only on purpose.

## Blazor

- **Choose the render mode per page, not per app.** ([Microsoft Learn: ASP.NET Core Blazor render modes](https://learn.microsoft.com/aspnet/core/blazor/components/render-modes))
- **Use `[PersistentState]` for state that must survive prerendering and circuit loss,** instead of ad hoc session storage. ([Microsoft Learn: ASP.NET Core Blazor prerendered state persistence](https://learn.microsoft.com/aspnet/core/blazor/state-management/prerendered-state-persistence))
- **Call `NavigationManager.NotFound()` for a missing resource, and show the not-found state in the UI too.** It sets a 404 only under static SSR or during prerendering. In an interactive component, it renders the Not Found content with no status code. ([Microsoft Learn: ASP.NET Core Blazor navigation](https://learn.microsoft.com/aspnet/core/blazor/fundamentals/navigation))
- **In interactive server rendering, take the user's time zone from the browser through a scoped `TimeProvider` per circuit.** The process's local zone is the server's, not the user's.
  1. Read the zone ID through JavaScript interop with `Intl.DateTimeFormat().resolvedOptions().timeZone`, and validate it with `TimeZoneInfo.TryFindSystemTimeZoneById`.
  2. Register a scoped `TimeProvider` that overrides `LocalTimeZone` with that zone.

  Components that inject the provider, instead of reading `DateTimeOffset.Now`, then render in the user's local time. For details, see [datetime.md](datetime.md). Under `InteractiveServer`, interop runs only in `OnAfterRenderAsync`, so the first render uses the wrong zone. Expect one corrective re-render, and handle `JSDisconnectedException`. ([Meziantou: Convert DateTime to the user's time zone with Blazor](https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor-time-provider.htm))

- **Read WebAssembly `HttpClient` responses asynchronously.** In .NET 10, response streaming is on by default, which is a breaking change. `response.Content.ReadAsStreamAsync()` returns a `BrowserHttpReadStream` instead of a `MemoryStream`, and it rejects synchronous calls such as `Stream.Read(Span<byte>)`. Fix the call site and keep the default, because streaming keeps large responses off the heap. If a dependency you don't own makes the synchronous call, opt out:
  - Per request, with `requestMessage.SetBrowserResponseStreamingEnabled(false)`.
  - Per project, with `<WasmEnableStreamingResponse>false</WasmEnableStreamingResponse>`.
  - With an environment variable. ([Microsoft Learn: What's new in ASP.NET Core 10](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-10.0), last updated 2026-08-31)
- **If users can switch culture at run time, set `<BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>`.** A WebAssembly app otherwise loads only its own culture's data. Remove `<BlazorEnableTimeZoneSupport>`, which is superseded. For details, see [globalization.md](globalization.md).

### Component testing

- **Unit-test components with bUnit, and keep Playwright for end-to-end tests.** Microsoft has no component testing framework of its own, and Microsoft Learn recommends bUnit, which renders components in-process without a browser.
  - Assert with `MarkupMatches`, never string equality on markup. It compares HTML semantically, so whitespace and attribute order don't fail the test.
  - Test behaviour that depends on real JavaScript interop or DOM changes end to end with Playwright. A faked `IJSRuntime` proves nothing about it. ([Microsoft Learn: Test components in ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/test))
- **Use bUnit 2.x with xUnit v3,** the stack in [testing.md](testing.md). In 2.x, the test class inherits `BunitContext` and renders with `Render<T>()`. ([bUnit: Writing tests](https://bunit.dev/docs/getting-started/writing-tests.html))

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

- **Put store billing behind one interface, such as `IBillingService`, with conditional compilation per store. Validate every purchase on the server.** ([Versluis: Implementing Cross-Platform In-App Billing in .NET MAUI Applications](https://devblogs.microsoft.com/dotnet/cross-platform-billing-dotnet-maui/))
- **On Android, set `<UseMaterial3>true</UseMaterial3>`** (MAUI 10.0.60 and later) for current theming. ([Versluis: Give Your .NET MAUI Android Apps a Material 3 Makeover](https://devblogs.microsoft.com/dotnet/dotnet-maui-material-3/))
- **Keep trimming on, and exclude a problem assembly instead.** Root it in a `Linker.xml` file referenced by `<TrimmerRootDescriptor Include="Linker.xml" />`. Turn off `PublishTrimmed` only to confirm that trimming causes the problem. ([Versluis: Excluding Assemblies from Trimming in .NET MAUI](https://blog.verslu.is/maui/exclude-assemblies-from-trimming/))
- **Target .NET 9 or later to meet Google Play's 16 KB page size requirement.** It concerns native library alignment, not trimming, and needs no other change. ([Versluis: Preparing Your .NET MAUI Apps for Google Play's 16 KB Page Size Requirement](https://devblogs.microsoft.com/dotnet/maui-google-play-16-kb-page-size-support/))
- **Test preview SDKs with `sdk.paths` in `global.json`** instead of changing the machine default, and ignore the `.dotnet/` folder it installs. For setup, see [project-structure.md](project-structure.md). ([Versluis: Test .NET MAUI Preview SDKs Locally with global.json sdk.paths](https://blog.verslu.is/maui/test-dotnet-maui-preview-sdk-locally/))

### App lifecycle

- **Handle the app lifecycle with the cross-platform `Window` events, not per-platform code.**
  1. Subclass `Window`, and override the events you need: `OnCreated`, `OnActivated`, `OnDeactivated`, `OnStopped`, `OnResumed`, and `OnDestroying`, plus `OnBackgrounding` on iOS and Mac Catalyst.
  2. Return the subclass from `App.CreateWindow`.
  3. In `Stopped`, disconnect long-running work and cancel pending requests. In `Resumed`, subscribe again and refresh the visible content.

  Use `ConfigureLifecycleEvents` in `MauiProgram` only for a platform hook with no cross-platform event. ([Microsoft Learn: .NET MAUI app lifecycle](https://learn.microsoft.com/dotnet/maui/fundamentals/app-lifecycle))

  <!-- Illustrative only: MAUI workloads are not installed on the authoring machine, so this snippet is unverified -->

  ```csharp
  public class MainWindow : Window
  {
      protected override void OnStopped() => _sync.PauseBackgroundSync();

      protected override void OnResumed() => _sync.ResumeAndRefresh();
  }
  ```

### Offline & sync

- **Build mobile apps offline-first: the local store is the source of truth, and the network syncs it.** Cache remote data locally with an explicit expiry. Use SQLite-net for data you query, and MonkeyCache's `Barrel.Current.Add(key, data, expireIn, eTag)` for simple payloads. The ETag overloads skip downloading unchanged responses. ([Montemagno: Data caching made simple with Monkey Cache](https://montemagno.com/data-caching-made-simple-with-monkey-cache/))
- **Check `Connectivity.NetworkAccess == NetworkAccess.Internet` before remote calls, and handle `ConnectivityChanged`. Never ping to test reachability.** The `IsReachable` APIs were removed deliberately: make the real request and handle its failure. ([Montemagno: Upgrading to Xamarin.Essentials from Plugins](https://montemagno.com/upgrading-from-plugins-to-xamarin-essentials/))

## Avalonia & reactive UI

- **Keep UI framework references out of view models.** `INotifyPropertyChanged`, Rx, and Dynamic Data are platform-neutral. A view model with no Avalonia or WPF reference binds on either framework, and you can unit-test it without a UI. ([Avalonia Docs: The MVVM pattern](https://docs.avaloniaui.net/docs/concepts/the-mvvm-pattern/))
- **Use Dynamic Data for collection state in XAML UIs instead of hand-written `ObservableCollection` code.** Key entities in a `SourceCache<T, TKey>`, and project filtered and sorted views into a bindable collection with `Connect()`. Filters and sorts then compose, instead of being rewritten per screen. ([Avalonia Docs: Sorting, filtering, and grouping collections](https://docs.avaloniaui.net/docs/data-binding/collection-views))

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

- **Target Avalonia 12 for new cross-platform desktop apps.** It shipped on 2026-04-07, on .NET 10 and SkiaSharp 3.0, with:
  - A reworked compositor.
  - A real Android `IDispatcherImpl` over `Looper` and `MessageQueue`.
  - Mac Catalyst support in `Avalonia.iOS`.

  When you migrate, plan for the removals: Direct2D1, Tizen, `Avalonia.Browser.Blazor`, `BinaryFormatter` usage, and netstandard2.0 in almost every project. ([Avalonia 12: Ready for What's Next](https://avaloniaui.net/blog/avalonia-12))

- **Keep compiled bindings on, the default in 12.** A broken compiled binding fails the build instead of binding to nothing. Use a reflection binding only for a single dynamic case. ([Avalonia 12: Ready for What's Next](https://avaloniaui.net/blog/avalonia-12))
- **Embed web content with the built-in WebView, not a bundled Chromium.** The WebView, formerly a commercial Accelerate component, is open source from 12. It uses each platform's native engine, so it adds nothing to app size. Avalonia retired the Accelerate brand in the same release, so before you work around a "commercial-only" feature, check what it costs now. ([Avalonia: The Avalonia WebView Is Going Open-Source](https://avaloniaui.net/blog/the-avalonia-webview-is-going-open-source/), [Avalonia: Retiring Accelerate: One Brand, One Clear Path](https://avaloniaui.net/blog/retiring-accelerate))
- **Use the built-in page navigation for mobile-style apps.** Avalonia 12 includes `ContentPage`, `DrawerPage`, `CarouselPage`, `TabView`, and `PipsPager`, with gestures and wrap selection. Replace a custom navigation stack over a `ContentControl`. ([Avalonia 12: Ready for What's Next](https://avaloniaui.net/blog/avalonia-12))
- **Avalonia's XAML hot reload is GA since 2026-09-02, and it's paid.** `AvaloniaUI.DiagnosticsSupport.HotReload` runs on Windows, macOS, Linux, iOS, and Android.
  - `dotnet watch` drives it through the same .NET Hot Reload mechanism as MAUI.
  - It patches changed elements in place, so view state survives the edit.
  - It needs an `AvaloniaUILicenseKey` and a Plus, Pro, or Enterprise subscription. ([Avalonia: Hot Reload for Avalonia](https://avaloniaui.net/blog/hot-reload-ga))
- **Don't build on Impeller yet.** Avalonia and Google's Flutter team are building an experimental GPU-first Impeller renderer for .NET. A maintainer paused the work on 2026-03-09 to ship v12, and no restart had been announced by 2026-09-24. Stay on the default Skia renderer. ([Avalonia: Avalonia Partnering with Google's Flutter Team to Bring Impeller Rendering to .NET](https://avaloniaui.net/blog/avalonia-partners-with-google-s-flutter-t-eam-to-bring-impeller-rendering-to-net); [Avalonia: Impeller rendering subsystem, discussion #20838](https://github.com/AvaloniaUI/Avalonia/discussions/20838))

## Source redundancy

Avalonia has the fewest sources of any topic here. Its only independent source, `nick-polyak`, moved to the watch list for dormancy on 2026-08-23, back catalogue included. What remains is `avalonia-blog` and the discovery-only `awesome-avalonia`, both from the Avalonia project and its community.

- They're authoritative on what shipped and how the API works, but they argue for adoption. Read release claims, especially headline FPS numbers, as vendor benchmarks.
- Cite the docs for how something works, such as a binding, a control, or a collection view. Treat any judgement about whether to adopt something as uncorroborated until an independent source supports it. No current source does, so it's an open gap for `vet-source`.

<!-- Mobile practice opinions (lifecycle, offline/sync, store compliance) split out into their own file if the MAUI section outgrows this one -->
