---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-02
last-used: 2026-09-25
sources: [ms-learn, dotnet-blog, jon-skeet, meziantou, andrew-lock]
---

# Dates, times, and time zones

Four rules cover most cases:

- Use `DateTimeOffset` as the default type, not `DateTime`.
- Store timestamps in UTC, but not future local events.
- Inject `TimeProvider` into services. Never call `DateTime.UtcNow`.
- Identify a time zone by its IANA ID (`Europe/Amsterdam`), never by an abbreviation or an offset.

## Choosing the type

**Default to `DateTimeOffset`. Choose a narrower type only when the data has no time, no date, or no clock.** Microsoft says so directly: "consider `DateTimeOffset` as the default date and time type for application development". ([Microsoft Learn: Compare types related to date and time](https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime))

| Need                                      | Type             | Why                                                                           |
| ----------------------------------------- | ---------------- | ----------------------------------------------------------------------------- |
| An unambiguous point in time              | `DateTimeOffset` | Carries the UTC offset, so it identifies one instant on any machine           |
| A whole date, no time (birthday, invoice) | `DateOnly`       | A time zone can't shift it by a day; smaller to serialize; matches SQL `date` |
| A time of day (opening hours, alarm)      | `TimeOnly`       | Wraps within 24 hours instead of overflowing like `TimeSpan` or `DateTime`    |
| A duration or elapsed time                | `TimeSpan`       | It's an interval, not a clock reading                                         |
| Zone rules and conversions                | `TimeZoneInfo`   | The only type that knows about DST transitions                                |
| `DateTime`                                | Rarely           | Only for abstract dates, or UTC-only code where `Kind` is set to `Utc`        |

The documentation states two caveats:

- **`DateTimeOffset` doesn't know its time zone.** It records the offset at one moment, and many zones share the same offset. It "can't reflect a time zone's transition to and from daylight saving time". For arithmetic across a DST transition, convert to UTC, do the arithmetic there, and then convert the result back to the zone with `TimeZoneInfo`. ([Microsoft Learn: Compare types related to date and time](https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime), [Microsoft Learn: How to use time zones in date and time arithmetic](https://learn.microsoft.com/dotnet/standard/datetime/use-time-zones-in-arithmetic))
- **A `DateTime` with `Kind = Unspecified` is ambiguous, even on the machine that created it.** If a `DateTime` must cross a boundary, it must be UTC with `Kind = Utc`. Anything else is a bug. ([Microsoft Learn: Compare types related to date and time](https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime))

Stop making these assumptions:

- Offsets aren't always whole hours. Newfoundland is UTC−3:30.
- DST shifts aren't always an hour. Lord Howe Island shifts by 30 minutes.
- Zones in one country can differ on whether they observe DST at all.
- Abbreviations are ambiguous. "BST" matches three real zones.

Anything less than an IANA ID loses information. ([Meziantou: 39 misconceptions about date and time](https://www.meziantou.net/misconceptions-about-date-and-time.htm))

## Persistence: split by kind of data, not by layer

**Store machine-generated timestamps as UTC, because they're instants. Store a future or recurring event that a person enters as its local time and IANA zone ID, and treat UTC as derived.** Such an event isn't an instant until zone rules are applied, and those rules can still change. ([Skeet: Storing UTC is not a silver bullet](https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/))

Here's how the common advice to "convert to UTC on the way in" fails. A conference registered for 9 AM Amsterdam time on 2022-07-10 converts to `07:00Z` under that day's tzdb rules. If the Netherlands later drops summer time, the correct instant becomes `08:00Z`. A row that stores only UTC is now silently an hour wrong, and the organiser's intent is lost. A schema that survives rule changes stores what the user entered and caches the conversion:

```text
LocalStart:    2022-07-10T09:00:00     ← what the user said; never mutated
TimeZoneId:    Europe/Amsterdam        ← IANA id, not an offset
UtcStart:      2022-07-10T07:00:00Z    ← derived; recomputed when tzdb updates
TimeZoneRules: 2019a                   ← optional, for resumable re-derivation
```

The same applies to recurring events: "10 AM in New York every week" is a rule, not a series of instants. ([Meziantou: 39 misconceptions](https://www.meziantou.net/misconceptions-about-date-and-time.htm)) Two operational rules follow:

- **Re-derive the UTC column when you update the base image.** IANA publishes several tzdb releases a year, sometimes only days before they take effect.
- **Store IANA IDs, not Windows IDs.** .NET converts between the two, but Windows IDs tie your data to a platform.

Provider mechanics ([Microsoft Learn: EF Core](https://learn.microsoft.com/ef/core/)):

- EF Core 8 and later map `DateOnly` to SQL Server `date` and `TimeOnly` to `time`, and scaffolding generates those types instead of `DateTime` and `TimeSpan`. A `DateTime` that holds only a date is a legacy pattern. ([.NET Blog: EF Core 8 Preview 1: Raw, lazy, and on-time](https://devblogs.microsoft.com/dotnet/announcing-ef8-preview-1/))
- SQL Server has a real `datetimeoffset` column type, and PostgreSQL doesn't. Don't assume that a `DateTimeOffset` property is portable across providers.
- `Microsoft.Data.Sqlite` 10.0 converts `DateTimeOffset` to UTC before it writes to REAL columns. Check this behaviour change if you target SQLite. ([Microsoft Learn: EF Core 10 breaking changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/breaking-changes))
- On the wire, `System.Text.Json` has round-tripped `DateOnly` and `TimeOnly` natively since .NET 7, and serializes `DateTimeOffset` as ISO 8601 with the offset. So minimal API bodies need no custom converters. A wire format that leaves out the offset pushes the ambiguity onto the client. ([Microsoft Learn: DateTime and DateTimeOffset support in System.Text.Json](https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support))

### When the BCL types aren't enough

**Use [NodaTime](https://nodatime.org/) when the domain models future or recurring local-time events across zones, as in the schema above. Otherwise, stay with the BCL types.** NodaTime turns this file's conventions into types that the compiler checks:

- `Instant` for machine timestamps.
- `LocalDateTime` and `DateTimeZone` for the columns a person enters.
- `ZonedDateTime` for the derived conversion.

The library was designed around the idea that a local date and time isn't yet an instant. The cost is an adapter at every boundary that the BCL types cross natively: `NodaTime.Serialization.SystemTextJson`, a provider-specific EF Core plugin such as `Npgsql.NodaTime`, and model binding. That's why it's the exception and not the default: `DateTimeOffset`, `DateOnly`, `TimeOnly`, and `TimeProvider` cover an ordinary service with nothing to add. Conflict of interest: the source cited here created NodaTime. ([Skeet: More fun with DateTime](https://codeblog.jonskeet.uk/2012/05/02/more-fun-with-datetime/))

## Services: inject `TimeProvider`, stop writing your own clock

**Take `TimeProvider` as a dependency, and register `TimeProvider.System` once. Never call `DateTime.UtcNow` or `DateTimeOffset.Now` in a service, and never write your own `IClock` abstraction.** `TimeProvider` is in the BCL from .NET 8, and `Microsoft.Bcl.TimeProvider` brings it back to netstandard2.0. It provides:

- `GetUtcNow()` and `GetLocalNow()`, which return `DateTimeOffset`.
- `GetTimestamp()` and `GetElapsedTime()` for measurement.
- `CreateTimer(...)`.
- `LocalTimeZone`, which lets one process serve users in different zones. ([Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview))

```csharp
// Register once; TimeProvider.System is the production implementation.
builder.Services.AddSingleton(TimeProvider.System);

public sealed class OrderService(TimeProvider time)
{
    public Order Place(Basket basket) => new(basket, PlacedAt: time.GetUtcNow());
}
```

Pass it on instead of dropping it. `Task.Delay`, `Task.WaitAsync`, and `CancellationTokenSource` all have `TimeProvider` overloads. A `PeriodicTimer` or `BackgroundService` built on the overloads without it can't be tested, and gains nothing.

Two files work together to enforce this. [templates/Directory.Build.props](../templates/Directory.Build.props) adds Meziantou.Analyzer and sets `TreatWarningsAsErrors`. But the date and time rules below default to **info** severity or are disabled, so on their own they never fail the build. `dotnet_analyzer_diagnostic.severity = warning` in [templates/.editorconfig](../templates/.editorconfig) raises them to warnings, and `TreatWarningsAsErrors` then turns those into build failures. Adopt both files. Otherwise, a hand-written `IClock` builds without errors. ([Meziantou: Meziantou.Analyzer rules](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/README.md))

| Rule     | Title                                                          | Default                                                                        |
| -------- | -------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| `MA0188` | Use `System.TimeProvider` instead of a custom time abstraction | Enabled, info severity                                                         |
| `MA0166` | Forward the `TimeProvider` to methods that take one            | Enabled, info severity                                                         |
| `MA0167` | Use an overload with a `TimeProvider` argument                 | Disabled; [templates/.editorconfig](../templates/.editorconfig) enables it too |

## Testing time

**Use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`.** Set a start instant, and move time forward with `Advance(TimeSpan)`. Timers created from it fire as time moves, so you can test retry, backoff, and scheduling logic without `Thread.Sleep`. One caveat: a single large `Advance` fires every elapsed callback at once, at the end. If a test checks how callbacks interleave, advance time in small steps. ([Lock: Avoiding flaky tests with TimeProvider and ITimer](https://andrewlock.net/exploring-the-dotnet-8-preview-avoiding-flaky-tests-with-timeprovider-and-itimer/))

Avoid two ways of constructing it:

- **The parameterless constructor** starts at midnight on 2000-01-01 UTC, a hard-coded date that the test never states. It's acceptable only for a test that passes at any instant, and even then, naming a start takes one line and makes that explicit.
- **Passing `DateTimeOffset.UtcNow` as the start** brings back the real clock. The test's dates change on every run, and whether it crosses a DST or month boundary is down to luck. ([Microsoft Learn: FakeTimeProvider constructors](https://learn.microsoft.com/dotnet/api/microsoft.extensions.time.testing.faketimeprovider.-ctor)) For the one-clock rule, see [testing.md](testing.md#deterministic-time).

For date-sensitive logic, test the awkward instants on purpose:

- A DST spring-forward gap: a local time that doesn't exist.
- A fall-back overlap: a local time that happens twice.
- A leap day.
- A year boundary where the calendar year and the ISO week year differ: 2022-01-02 is in week 52 of 2021.

([Meziantou: 39 misconceptions](https://www.meziantou.net/misconceptions-about-date-and-time.htm)) For the test framework stack, see [testing.md](testing.md).

## UI and hosts

- **Get the user's zone from the browser, not the server.** In interactive server Blazor, register a scoped `TimeProvider` per circuit, filled from JavaScript interop. For the pattern, see [ui-frameworks.md](ui-frameworks.md#blazor).
- **Run hosts in UTC, and convert at the edges.** Nothing should depend on the server's own local zone, so give each scheduled job's schedule an explicit zone. Local time repeats an hour when daylight saving time ends, and skips one when it starts. A job set to server-local time in that hour runs twice, or not at all. ([Microsoft Learn: How to resolve ambiguous times](https://learn.microsoft.com/dotnet/standard/datetime/resolve-ambiguous-times), [Microsoft Learn: TimeZoneInfo.IsInvalidTime](https://learn.microsoft.com/dotnet/api/system.timezoneinfo.isinvalidtime))
- **Make sure container images include tzdata and ICU,** or none of the zone conversion above works. For image tags, invariant mode, and the failure modes, see [globalization.md](globalization.md#containers-chiseled-and-alpine-images-drop-this-data). It also has the display rule: format for people with their culture, and for machines with the invariant culture.

## Smaller traps

- **`TimeSpan.From*` gained integer overloads in .NET 9,** because the `double` overloads lose precision: `TimeSpan.FromSeconds(101.832)` isn't exactly 101.832 seconds. In F#, this broke overload resolution, so `TimeSpan.FromMinutes(20)` now needs an explicit type annotation. ([Microsoft Learn: Breaking change: New TimeSpan.From\*() overloads that take integers](https://learn.microsoft.com/dotnet/core/compatibility/core-libraries/9.0/timespan-from-overloads))
- **`TimeZoneInfo.FindSystemTimeZoneById` accepts the platform's native IDs.** The lookup needs only tzdata, so it works even in invariant globalization mode. Converting between IANA and Windows IDs with `TryConvertIanaIdToWindowsId` and `TryConvertWindowsIdToIanaId` depends on ICU, so it isn't available in invariant or NLS mode. ([globalization.md](globalization.md))
