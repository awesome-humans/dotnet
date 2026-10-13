---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-15
last-used: 2026-09-28
sources: [ms-learn, meziantou, andrew-lock, house, dotnet-blog, mark-seemann]
---

# Testing

Hold tests to the same review bar and conventions as production code.

## Framework and platform

- **Use xUnit v3 on Microsoft.Testing.Platform (MTP) for new test projects, and opt in to the MTP runner in `global.json`.** xunit.v3 includes a native MTP runner, so each test project builds to an executable that `dotnet run` runs directly, without VSTest's slower process model.
  - `dotnet test` uses MTP only with the runner opt-in below. Without it, the SDK uses VSTest, and xunit.v3 4.0.0 (MTP v2) fails on .NET 10 with `Testing with VSTest target is no longer supported`.
  - On xunit.v3 3.x, the same mistake runs **zero** tests and exits with code 0.

  ```json
  {
    "sdk": { "version": "10.0.400", "rollForward": "latestFeature" },
    "test": { "runner": "Microsoft.Testing.Platform" }
  }
  ```

  Don't use `<TestingPlatformDotnetTestSupport>` instead. It's the VSTest bridge, and it causes the error above under MTP v2. MSTest and NUnit also run on MTP. xUnit is the default because it constructs a new instance for each test and has the largest ecosystem. TUnit is too new to have a track record. ([Microsoft Learn: Microsoft.Testing.Platform overview](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro), [Microsoft Learn: What's new in .NET 10: SDK](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-10/overview))

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

  - The reference has no version, because Central Package Management pins it in [templates/Directory.Packages.props](../templates/Directory.Packages.props). For details, see [project-structure.md](project-structure.md).
  - There's no `Microsoft.NET.Test.Sdk` reference. That package is for VSTest, and `xunit.v3` needs nothing else under MTP.
  - Don't pin `Microsoft.Testing.Platform`. xunit.v3 4.0.0 brings MTP v2. Under Central Package Management, your pin overrides it through transitive pinning and throws `TypeLoadException` at run time.

- **Moving a suite from VSTest to MTP breaks two things the build doesn't catch.** **Review by 2027-11-30.** This is migration guidance. Drop it when VSTest workflows are rare, or re-date it.
  - **`--logger` isn't an MTP option.** VSTest's `--logger "console;verbosity=normal"` or `--logger trx` makes the test application exit with code 5 (invalid command-line arguments) before any test runs. Every CI line that passes it fails as soon as `global.json` opts in. Each MTP reporter registers its own option. Use `--output Detailed` for console verbosity, and `--report-xunit-trx`, which is built into xunit.v3, for TRX. `--report-trx`, from Microsoft's migration guide, needs the `Microsoft.Testing.Extensions.TrxReport` package and fails the same way without it. ([Microsoft Learn: Migration guide from VSTest to Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/testing/migrating-vstest-microsoft-testing-platform), [Microsoft Learn: Microsoft.Testing.Platform exit codes](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-exit-codes), [xUnit.net: Microsoft Testing Platform](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform))
  - **A multi-targeted test project runs its target frameworks concurrently.** Each target framework builds its own executable, and MTP runs test modules in parallel, up to `Environment.ProcessorCount`. VSTest ran them in sequence. A fixture with a fixed temporary directory, port, file, or database name now races the other framework's process, and one run's `Dispose` deletes a directory the other is writing to. Make that state unique per instance, for example with `Guid.NewGuid()` in the name. `--max-parallel-test-modules 1` hides the race and gives up MTP's speed. ([Microsoft Learn: dotnet test command with Microsoft.Testing.Platform](https://learn.microsoft.com/dotnet/core/tools/dotnet-test-mtp))

- **Leaving VSTest gains more speed than changing frameworks.** Meziantou benchmarked xUnit v3 4.0.0, NUnit 4.6.1, MSTest 4.4.0, and TUnit 1.65.68 on the .NET 10 SDK with up to 10,000 tests. VSTest was 4.9 times slower than the MTP executable for xUnit v3, 5.1 times for MSTest, and 1.5 times for NUnit. He puts that at three to four times the difference between frameworks. Per test, they range from 15 µs (MSTest) through 42 µs (TUnit) and 56 µs (xUnit v3) to 85 µs (NUnit). Read the numbers as a reason to keep the `global.json` opt-in, not to leave xUnit. ([Meziantou: Benchmarking .NET test frameworks: xUnit v3, NUnit, MSTest, and TUnit](https://www.meziantou.net/benchmarking-dotnet-test-frameworks-xunit-v3-nunit-mstest-and-tunit.htm))

- **If the app ships with Native AOT, add a native test run beside the managed suite.** Run the managed suite on every change for fast feedback. For the native run, publish one representative C# test project with `<PublishAot>true</PublishAot>`, and run the executable. Trimming and reflection-free serialization change behaviour that a managed run can't see.
  - `System.Text.Json` usually fails first, with `InvalidOperationException`, because reflection-based serialization is off. Fix it with a `[JsonSerializable]` context in the app, not in the test.
  - The native project references `xunit.v3.aot.mtp-v2` instead of `xunit.v3`. The AOT package replaces the reflection-based one, so it can't share a project with the managed suite.
  - Keep F# test projects in the managed run. NUnit doesn't support Native AOT. ([xUnit.net: Testing with Native AOT](https://xunit.net/docs/getting-started/v3/native-aot), [.NET Blog: Test what you ship: MSTest and Native AOT](https://devblogs.microsoft.com/dotnet/mstest-source-generation/), [Meziantou: Benchmarking .NET test frameworks: xUnit v3, NUnit, MSTest, and TUnit](https://www.meziantou.net/benchmarking-dotnet-test-frameworks-xunit-v3-nunit-mstest-and-tunit.htm))

## Naming and structure

- **House:** Name tests `UnitOfWork_Scenario_ExpectedBehaviour`, and mark each test's Arrange, Act, and Assert sections with comments. ([HOUSE-OPINIONS.md](../HOUSE-OPINIONS.md))

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

- **Integration-test ASP.NET Core apps with `WebApplicationFactory<TEntryPoint>` instead of unit-testing controllers or handlers.** `Microsoft.AspNetCore.Mvc.Testing` runs the real app in memory, with routing, model binding, filters, DI, and middleware. A few factory tests catch wiring bugs that controller unit tests miss. Unit-test the domain logic the endpoints call, not the endpoints. ([Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests), [Lock: Should you unit-test API/MVC controllers?](https://andrewlock.net/should-you-unit-test-controllers-in-aspnetcore/))
- **Replace services for a test with `ConfigureTestServices`, scoped by `WithWebHostBuilder`.** ([Microsoft Learn: Integration tests in ASP.NET Core](https://learn.microsoft.com/aspnet/core/test/integration-tests))

## Deterministic time

- **Test time-dependent code through an injected `TimeProvider`, using `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`. Never use `Thread.Sleep` or the real clock.** [datetime.md](datetime.md#testing-time) covers how to drive it, the large-`Advance` caveat, the instants to test (DST gaps and overlaps, leap days, and ISO week year boundaries), and the analyzer rules that ban custom clocks.
- **Give each test one clock: construct `FakeTimeProvider` with an explicit start, and derive every other date from it.** A test of an awkward instant has to name that instant. Name it once, as the provider's start.
  - Arrange with `time.GetUtcNow()`, and move time with `Advance`.
  - Assert time-derived results against `time.Start` plus the elapsed time, so the expected value visibly follows from the input.

  Two literals six minutes apart are correct today, and a puzzle at the next review. A hand-typed expected value repeats the arithmetic the code under test should do. For the two constructor overloads that break this rule, see [datetime.md](datetime.md#testing-time). ([Lock: Avoiding flaky tests with TimeProvider and ITimer](https://andrewlock.net/exploring-the-dotnet-8-preview-avoiding-flaky-tests-with-timeprovider-and-itimer/) for `Advance` and `GetUtcNow`)

  Before: two unrelated literals, which the reader has to compare.

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

  After: one clock, one chosen start, and the test checks elapsed time. The start is six minutes before the EU fall-back, in a zone that observes it, so the local clock goes from 02:57 CEST back to 02:03 CET while six minutes pass. A validator that compared local clock times would call the token unexpired, and this test catches it.

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

  Snapshot libraries locate `.verified` files from the test's source path, which deterministic builds rewrite to `/_/…`. With `ContinuousIntegrationBuild` or `DeterministicSourcePaths` on, the project can't find its snapshots. Keep determinism, and restore the mapping at startup: an MSBuild target can generate a `[ModuleInitializer]` that maps each `SourceRoot`'s `MappedPath` back to the real directory. ([Meziantou: Reproducible builds and snapshot testing](https://www.meziantou.net/reproducible-builds-and-snapshot-testing-handling-file-paths-in-dotnet.htm))

- **Measure a slow CI suite, then shard it deterministically across parallel jobs.** Sharding helps CPU-bound suites far more than I/O-bound ones. ([Meziantou: Test sharding](https://www.meziantou.net/split-dotnet-test-projects-into-shards-with-meziantou-shardedtest.htm))

## Coverage

- **Collect line coverage on every CI run, and use it to find untested code, not to score quality.** Coverage shows which lines ran, not whether a test checked them. ([Microsoft Learn: Use code coverage for unit testing](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage), [Meziantou: Is the code coverage a sufficient metric?](https://www.meziantou.net/is-the-code-coverage-a-sufficient-metric.htm))

## Seeing tests fail

- **See every test fail on its assertion before you trust it to pass.** A test that has never failed might assert a tautology or lock in a bug, and coverage and a green run can't tell it from a working test. Writing the test first isn't enough: a test that failed only because the member didn't exist might pass whatever the member does. After a test passes, break the code under test once per assertion, and see the test fail on that assertion's line. Then revert, and see it pass. ([Seemann: Tautological assertion](https://blog.ploeh.dk/2019/10/14/tautological-assertion/), [Seemann: Epistemology of software](https://blog.ploeh.dk/2025/10/20/epistemology-of-software/), [Seemann: Empirical Characterization Testing](https://blog.ploeh.dk/2025/11/03/empirical-characterization-testing/))
- **Review a passing suite as the devil's advocate.** Write the simplest wrong implementation that passes every test, then add a test case that catches it. Stop when a wrong implementation takes real effort. Round trips are the usual gap: two identity functions pass any test that only compares output with input, so assert the intermediate value too. ([Seemann: Devil's advocate](https://blog.ploeh.dk/2019/10/07/devils-advocate/))

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

- **Use the same xUnit v3 stack for F# tests.** Put `[<Fact>]` on `let`-bound functions, with no test class. For the reasoning and an example, see [fsharp.md](fsharp.md#testing-f).
