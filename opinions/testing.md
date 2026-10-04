---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-15
last-used: 2026-09-28
sources: [ms-learn, meziantou, andrew-lock, house, dotnet-blog, mark-seemann]
---

# Testing

Treat tests as production code, with the same review bar and the same conventions.

## Framework and platform

- **Use xUnit v3 on Microsoft.Testing.Platform (MTP) for new test projects, and opt in to the MTP runner in `global.json`.** xunit.v3 includes a native MTP runner, so each test project builds to a self-contained executable that `dotnet run` runs directly. This replaces VSTest's slower process model.
  - `dotnet test` uses MTP only with the runner opt-in below. Without it, the SDK still uses VSTest. xunit.v3 4.0.0 (MTP v2) rejects VSTest on .NET 10 with `Testing with VSTest target is no longer supported`.
  - On xunit.v3 3.x, the same mistake is worse than an error: `dotnet test` runs **zero** tests and exits with code 0.

  ```json
  {
    "sdk": { "version": "10.0.400", "rollForward": "latestFeature" },
    "test": { "runner": "Microsoft.Testing.Platform" }
  }
  ```

  Don't use `<TestingPlatformDotnetTestSupport>` instead. That property is the VSTest bridge, and it causes the error above under MTP v2. MSTest and NUnit also run on MTP, but xUnit is the default because it isolates each test with its own constructor call and has the largest ecosystem. TUnit is promising, but too new to have a track record. ([Microsoft Learn: Microsoft.Testing.Platform overview](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro), [Microsoft Learn: What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview))

  ```xml
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>net10.0</TargetFramework>
      <Nullable>enable</Nullable>
      <OutputType>Exe</OutputType>
      <UseMicrosoftTestingPlatformRunner>true</UseMicrosoftTestingPlatformRunner>
    </PropertyGroup>
    <ItemGroup>
      <PackageReference Include="xunit.v3" />
    </ItemGroup>
  </Project>
  ```

  - The reference has no version. Central Package Management is required, as described in [project-structure.md](project-structure.md), so the version is pinned in [templates/Directory.Packages.props](../templates/Directory.Packages.props).
  - There's no `Microsoft.NET.Test.Sdk` reference. That package belongs to VSTest, and `xunit.v3` needs nothing else under MTP.
  - Don't pin `Microsoft.Testing.Platform` yourself. xunit.v3 4.0.0 dropped MTP v1 and brings its own v2. A separate pin overrides it through transitive pinning under Central Package Management, and causes a `TypeLoadException` at run time.

- **Moving an existing suite from VSTest to MTP breaks two things that the build doesn't catch.** **Review by 2027-11-30.** This is migration guidance. Drop it when VSTest workflows are rare, or re-date it.
  - **`--logger` isn't an MTP option.** VSTest's `--logger "console;verbosity=normal"` or `--logger trx` makes the test application exit with code 5 (invalid command-line arguments) without running a test. Every CI line that uses it fails as soon as `global.json` opts in. MTP has no central logger switch, because each reporter registers its own option. Use `--output Detailed` for console verbosity, and `--report-xunit-trx` for TRX, which is built into xunit.v3. The `--report-trx` option in Microsoft's migration guide comes from the `Microsoft.Testing.Extensions.TrxReport` package. A test application without that package rejects the option the same way. ([Microsoft Learn: Migration guide from VSTest to Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/migrating-vstest-microsoft-testing-platform), [Microsoft Learn: Microsoft.Testing.Platform exit codes](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-exit-codes), [xUnit.net: Microsoft Testing Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform))
  - **A multi-targeted test project runs all its target frameworks at the same time.** Each target framework builds its own test executable. MTP runs test modules in parallel by default, up to `Environment.ProcessorCount`, where VSTest ran them one after another. A fixture that uses a fixed temporary directory, port, file, or database name now races the other framework's process: one run's `Dispose` deletes the directory that the other is still writing to. Make that state unique per instance, for example by putting `Guid.NewGuid()` in the name. Don't set `--max-parallel-test-modules 1` instead. It hides the race, and gives up the speed you adopted MTP for. ([Microsoft Learn: dotnet test command with Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/tools/dotnet-test-mtp))

- **Leaving VSTest gains more speed than switching frameworks.** Meziantou benchmarked xUnit v3 4.0.0, NUnit 4.6.1, MSTest 4.4.0, and TUnit 1.65.68 on the .NET 10 SDK, with up to 10,000 tests. Compared with the MTP executable, the legacy VSTest path was 4.9 times slower for xUnit v3, 5.1 times slower for MSTest, and 1.5 times slower for NUnit. He estimates that difference at three to four times what the choice of framework is worth. The cost per test separates the frameworks much less: 15 µs for MSTest, 42 µs for TUnit, 56 µs for xUnit v3, and 85 µs for NUnit. Take those figures as a reason to keep the `global.json` opt-in above, not as a reason to leave xUnit. ([Meziantou: Benchmarking .NET test frameworks: xUnit v3, NUnit, MSTest, and TUnit](https://www.meziantou.net/benchmarking-dotnet-test-frameworks-xunit-v3-nunit-mstest-and-tunit.htm))

- **If the app ships with Native AOT, add a representative native test run beside the managed suite.** The managed run stays the fast feedback loop on every change. The native run publishes one C# test project with `<PublishAot>true</PublishAot>`, and runs the resulting executable. Trimming and reflection-free serialization change behaviour in ways a managed run can't see, so a passing suite can still fail after publishing.
  - `System.Text.Json` usually fails first. It throws `InvalidOperationException`, because reflection-based serialization is disabled. The fix is a `[JsonSerializable]` context in the app, not a change to the test.
  - The native project references `xunit.v3.aot.mtp-v2` instead of `xunit.v3`. The AOT package replaces the reflection-based one, so it can't share a project with the managed suite.
  - F# test projects stay in the managed run. NUnit doesn't support Native AOT at all. ([xUnit.net: Testing with Native AOT](https://xunit.net/docs/getting-started/v3/native-aot), [.NET Blog: Test what you ship: MSTest and Native AOT](https://devblogs.microsoft.com/dotnet/mstest-source-generation/), [Meziantou: Benchmarking .NET test frameworks: xUnit v3, NUnit, MSTest, and TUnit](https://www.meziantou.net/benchmarking-dotnet-test-frameworks-xunit-v3-nunit-mstest-and-tunit.htm))

## Naming and structure

- **House:** Name tests `UnitOfWork_Scenario_ExpectedBehaviour`, and mark the Arrange, Act, and Assert sections of each test with comments. ([HOUSE-OPINIONS.md](../HOUSE-OPINIONS.md))

  ```csharp
  public class BasketTests
  {
      [Fact]
      public void Total_MultipleItemsAdded_SumsAllItemPrices()
      {
          // Arrange
          var basket = new Basket();
          basket.Add(10.00m);
          basket.Add(2.50m);

          // Act
          var total = basket.Total;

          // Assert
          Assert.Equal(12.50m, total);
      }
  }
  ```

## Integration testing

- **Integration-test ASP.NET Core apps with `WebApplicationFactory<TEntryPoint>`, instead of unit-testing controllers or endpoint handlers.** `Microsoft.AspNetCore.Mvc.Testing` starts the real app in memory, with routing, model binding, filters, DI, and middleware all active. A few factory-based tests catch the wiring bugs that controller unit tests can't. Unit-test the domain logic that the endpoints call, not the endpoints themselves. ([Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests), [Andrew Lock: Should you unit-test API/MVC controllers?](https://andrewlock.net/should-you-unit-test-controllers-in-aspnetcore/))
- **Override services for tests with `ConfigureTestServices`, scoped to the test with `WithWebHostBuilder`.** ([Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests))

## Deterministic time

- **Test time-dependent code through an injected `TimeProvider`, with `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`. Never use `Thread.Sleep` or the real clock.** [datetime.md](datetime.md#testing-time) explains how to drive it, the caveat for a large `Advance`, and which awkward instants to test: DST gaps and overlaps, leap days, and ISO week year boundaries. It also lists the analyzer rules that already ban custom clock abstractions in this repository's templates.
- **Give each test one clock. Construct the `FakeTimeProvider` with an explicit start instant, and derive every other date in the test from it.** Fixed dates aren't the problem: a test of an awkward instant has to name that instant. The rule is about where the literal goes: once, as the provider's start, where it's clearly a deliberate choice.
  - Arrange with `time.GetUtcNow()`, and move time with `Advance`.
  - Assert time-derived outputs against `time.Start` plus the elapsed span, so the test shows how the expected result follows from the input.

  Two literal dates that happen to be six minutes apart are correct today, and unclear at the next review. A hand-typed expected value silently repeats the arithmetic that the code under test should be doing. For the two constructor overloads that break this rule, see [datetime.md](datetime.md#testing-time). ([Andrew Lock: Avoiding flaky tests with TimeProvider and ITimer](https://andrewlock.net/exploring-the-dotnet-8-preview-avoiding-flaky-tests-with-timeprovider-and-itimer/) for `Advance` and `GetUtcNow`)

  Before: two unrelated literals, which the reader must compare to work out what the test means.

  ```csharp
  public class TokenValidatorTests
  {
      [Fact]
      public void IsExpired_LifetimeElapsed_ReturnsTrue()
      {
          // Arrange
          var token = new Token(issuedAt: new DateTimeOffset(2026, 10, 25, 0, 57, 0, TimeSpan.Zero), lifetime: TimeSpan.FromMinutes(5));
          var validator = new TokenValidator(new FakeTimeProvider(new DateTimeOffset(2026, 10, 25, 1, 3, 0, TimeSpan.Zero)));

          // Act
          var expired = validator.IsExpired(token);

          // Assert
          Assert.True(expired);
      }
  }
  ```

  After: one clock, one deliberately chosen start, and the elapsed time is what the test checks. The start is six minutes before the EU fall-back, in a zone that observes it. The local clock goes from 02:57 CEST back to 02:03 CET while only six minutes pass. A validator that compared local clock times would say the token hadn't expired, and this test catches that.

  ```csharp
  public class TokenValidatorTests
  {
      [Fact]
      public void IsExpired_LifetimeElapsedAcrossFallBack_ReturnsTrue()
      {
          // Arrange
          var time = new FakeTimeProvider(new DateTimeOffset(2026, 10, 25, 0, 57, 0, TimeSpan.Zero));
          time.SetLocalTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Europe/Paris"));
          var lifetime = TimeSpan.FromMinutes(5);
          var token = new Token(issuedAt: time.GetUtcNow(), lifetime);
          var validator = new TokenValidator(time);

          // Act
          time.Advance(TimeSpan.FromMinutes(6));

          // Assert
          Assert.Equal(time.Start.Add(lifetime), token.ExpiresAt);
          Assert.True(validator.IsExpired(token));
      }
  }
  ```

## Output and scale

- **Use snapshot testing for complex serialized output,** such as generated code, API payloads, and rendered documents, instead of asserting each field. ([Meziantou: Snapshot testing](https://www.meziantou.net/snapshot-testing-in-dotnet-with-meziantou-framework-snapshottesting.htm))

  Snapshot libraries find the `.verified` files from the test's source path, which deterministic builds rewrite to `/_/…`. So a project with `ContinuousIntegrationBuild` or `DeterministicSourcePaths` enabled can't read or write its snapshots. Restore the mapping at startup instead of turning off determinism. An MSBuild target can generate a `[ModuleInitializer]` that maps the `MappedPath` of each `SourceRoot` to the real directory, which keeps both properties. ([Meziantou: Reproducible builds and snapshot testing](https://www.meziantou.net/reproducible-builds-and-snapshot-testing-handling-file-paths-in-dotnet.htm))

- **Shard slow CI test suites deterministically across parallel jobs, but measure first.** Sharding helps CPU-bound suites much more than I/O-bound ones. ([Meziantou: Test sharding](https://www.meziantou.net/split-dotnet-test-projects-into-shards-with-meziantou-shardedtest.htm))

## Coverage

- **Collect line coverage on every CI run, and use it to find untested code, not as a quality score.** Coverage shows which lines ran, not whether a test asserted anything about them, so a high number doesn't prove the tests are good. ([Microsoft Learn: Use code coverage for unit testing](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage), [Meziantou: Is the code coverage a sufficient metric?](https://www.meziantou.net/is-the-code-coverage-a-sufficient-metric.htm))

## Seeing tests fail

- **See every test fail on its assertion before you trust it to pass.** A test that has never failed might have a tautological assertion, or might lock in a bug as expected behaviour. Coverage and a green run can't tell it apart from a working test. Writing the test first isn't enough: a test that failed only because the member didn't exist yet might pass afterward whatever it's given. So after a test passes, break the code under test once for each assertion, and see the test fail on that assertion's line. Then revert the change, and see the test pass. ([Seemann: Tautological assertion](https://blog.ploeh.dk/2019/10/14/tautological-assertion/), [Seemann: Epistemology of software](https://blog.ploeh.dk/2025/10/20/epistemology-of-software/), [Seemann: Empirical Characterization Testing](https://blog.ploeh.dk/2025/11/03/empirical-characterization-testing/))
- **Review a passing suite as the devil's advocate.** Write the simplest wrong implementation that still passes every test, and then add the test case that catches it. Stop when writing such an implementation takes real effort. A round trip is the usual gap: two identity functions pass any test that only compares the output with the input, so assert the intermediate value too. ([Seemann: Devil's advocate](https://blog.ploeh.dk/2019/10/07/devils-advocate/))

  ```csharp
  [Fact]
  public void FahrenheitToCelsius_RoundTrip_ReturnsOriginalReading()
  {
      // Arrange
      var celsius = 37d;

      // Act
      var fahrenheit = Temperature.CelsiusToFahrenheit(celsius);
      var roundTripped = Temperature.FahrenheitToCelsius(fahrenheit);

      // Assert
      Assert.Equal(98.6d, fahrenheit, 1e-9); // fails if both conversions return their input
      Assert.Equal(celsius, roundTripped, 1e-9);
  }
  ```

## F#

- **Use the same xUnit v3 stack for F# test projects.** Put `[<Fact>]` on `let`-bound functions, with no test class. For the reasoning and an example, see [fsharp.md](fsharp.md#testing-f).
