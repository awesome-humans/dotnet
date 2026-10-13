---
targets: [net10.0, csharp-14]
last-reviewed: 2026-09-02
last-used: 2026-09-25
sources: [ms-learn, dotnet-blog, jon-skeet, meziantou, andrew-lock]
---

# Dates, times, and time zones

Four rules cover most cases:

- Use `DateTimeOffset` by default, not `DateTime`.
- Store timestamps in UTC, except future local events.
- Inject `TimeProvider` into services. Never call `DateTime.UtcNow`.
- Identify a time zone by its IANA ID (`Europe/Amsterdam`), never by an abbreviation or an offset.

## Choosing the type

**Default to `DateTimeOffset`. Use a narrower type only when the data has no time, no date, or no clock.** Microsoft's guidance: "consider `DateTimeOffset` as the default date and time type for application development". ([Microsoft Learn: Compare types related to date and time](https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime))

| Need                                      | Type             | Why                                                                         |
| ----------------------------------------- | ---------------- | --------------------------------------------------------------------------- |
| An unambiguous point in time              | `DateTimeOffset` | Carries the UTC offset, so it identifies one instant on any machine         |
| A whole date, no time (birthday, invoice) | `DateOnly`       | A time zone can't shift it by a day; serializes smaller; matches SQL `date` |
| A time of day (opening hours, alarm)      | `TimeOnly`       | Wraps at 24 hours instead of overflowing like `TimeSpan` or `DateTime`      |
| A duration or elapsed time                | `TimeSpan`       | An interval, not a clock reading                                            |
| Zone rules and conversions                | `TimeZoneInfo`   | The only type that knows DST transitions                                    |
| `DateTime`                                | Rarely           | Only for abstract dates, or UTC-only code with `Kind` set to `Utc`          |

The documentation adds two caveats:

- **`DateTimeOffset` doesn't know its time zone.** It records the offset at one moment, and many zones share an offset. It "can't reflect a time zone's transition to and from daylight saving time". For arithmetic across a DST transition, convert to UTC, calculate there, and convert back with `TimeZoneInfo`. ([Microsoft Learn: Compare types related to date and time](https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime), [Microsoft Learn: How to use time zones in date and time arithmetic](https://learn.microsoft.com/dotnet/standard/datetime/use-time-zones-in-arithmetic))
- **A `DateTime` with `Kind = Unspecified` is ambiguous, even on the machine that created it.** A `DateTime` that crosses a boundary must be UTC with `Kind = Utc`. ([Microsoft Learn: Compare types related to date and time](https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime))

Don't assume:

- That offsets are whole hours. Newfoundland is UTC−3:30.
- That DST shifts by an hour. Lord Howe Island shifts by 30 minutes.
- That one country agrees on DST. Its zones can differ.
- That an abbreviation names one zone. "BST" matches three.

Anything less than an IANA ID loses information. ([Meziantou: 39 misconceptions about date and time](https://www.meziantou.net/misconceptions-about-date-and-time.htm))

## Persistence: split by kind of data, not by layer

**Store machine-generated timestamps as UTC instants. Store a future or recurring event that a person enters as its local time and IANA zone ID, and derive UTC from them.** The event isn't an instant until zone rules are applied, and those rules can change before it happens. ([Skeet: Storing UTC is not a silver bullet](https://codeblog.jonskeet.uk/2019/03/27/storing-utc-is-not-a-silver-bullet/))

Converting to UTC on input fails here. A conference set for 9 AM Amsterdam time on 2022-07-10 converts to `07:00Z` under that day's tzdb rules. If the Netherlands drops summer time, the correct instant becomes `08:00Z`. A row that stores only UTC is now an hour wrong, and the organiser's intent is gone. Store what the user entered, and cache the conversion:

```text
LocalStart:    2022-07-10T09:00:00     ← what the user said; never mutated
TimeZoneId:    Europe/Amsterdam        ← IANA id, not an offset
UtcStart:      2022-07-10T07:00:00Z    ← derived; recomputed when tzdb updates
TimeZoneRules: 2019a                   ← optional, for resumable re-derivation
```

The same applies to recurring events: "10 AM in New York every week" is a rule, not a series of instants. ([Meziantou: 39 misconceptions](https://www.meziantou.net/misconceptions-about-date-and-time.htm))

- **Re-derive the UTC column when you update the base image.** IANA publishes several tzdb releases a year, sometimes days before they take effect.
- **Store IANA IDs, not Windows IDs.** .NET converts between them, but Windows IDs tie your data to one platform.

Provider behaviour ([Microsoft Learn: EF Core](https://learn.microsoft.com/ef/core/)):

- EF Core 8 and later map `DateOnly` to SQL Server `date` and `TimeOnly` to `time`, and scaffolding generates them instead of `DateTime` and `TimeSpan`. ([.NET Blog: EF Core 8 Preview 1: Raw, lazy, and on-time](https://devblogs.microsoft.com/dotnet/announcing-ef8-preview-1/))
- SQL Server has a `datetimeoffset` column type, and PostgreSQL doesn't. A `DateTimeOffset` property isn't portable across providers.
- `Microsoft.Data.Sqlite` 10.0 converts `DateTimeOffset` to UTC before writing it to a REAL column. If you target SQLite, check this behaviour change. ([Microsoft Learn: EF Core 10 breaking changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-10.0/breaking-changes))
- `System.Text.Json` round-trips `DateOnly` and `TimeOnly` since .NET 7, and writes `DateTimeOffset` as ISO 8601 with the offset. Minimal API bodies need no custom converters. A wire format without the offset hands the ambiguity to the client. ([Microsoft Learn: DateTime and DateTimeOffset support in System.Text.Json](https://learn.microsoft.com/dotnet/standard/datetime/system-text-json-support))

### When the BCL types aren't enough

**Use [NodaTime](https://nodatime.org/) when the domain schedules future or recurring local-time events across zones. Otherwise, use the BCL types.** NodaTime turns the schema above into types the compiler checks:

- `Instant` for machine timestamps.
- `LocalDateTime` and `DateTimeZone` for what a person enters.
- `ZonedDateTime` for the derived conversion.

The cost is an adapter at every boundary the BCL types cross natively: `NodaTime.Serialization.SystemTextJson`, a provider plugin such as `Npgsql.NodaTime`, and model binding. An ordinary service gets everything it needs from `DateTimeOffset`, `DateOnly`, `TimeOnly`, and `TimeProvider`. Conflict of interest: the cited source created NodaTime. ([Skeet: More fun with DateTime](https://codeblog.jonskeet.uk/2012/05/02/more-fun-with-datetime/))

## Services: inject `TimeProvider`, stop writing your own clock

**Take `TimeProvider` as a dependency, and register `TimeProvider.System` once. Never call `DateTime.UtcNow` or `DateTimeOffset.Now` in a service, and never write an `IClock` abstraction.** `TimeProvider` is in the BCL from .NET 8, and `Microsoft.Bcl.TimeProvider` brings it to netstandard2.0. It provides:

- `GetUtcNow()` and `GetLocalNow()`, which return `DateTimeOffset`.
- `GetTimestamp()` and `GetElapsedTime()` for measurement.
- `CreateTimer(...)`.
- `LocalTimeZone`, so one process can serve users in different zones. ([Microsoft Learn: What is TimeProvider?](https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview))

```csharp
// Register once; TimeProvider.System is the production implementation.
builder.Services.AddSingleton(TimeProvider.System);

public sealed class OrderService(TimeProvider time)
{
    public Order Place(Basket basket) => new(basket, PlacedAt: time.GetUtcNow());
}
```

Pass it on. `Task.Delay`, `Task.WaitAsync`, and `CancellationTokenSource` have `TimeProvider` overloads. A `PeriodicTimer` or `BackgroundService` that skips them can't be tested.

Two template files enforce this together. [templates/Directory.Build.props](../templates/Directory.Build.props) adds Meziantou.Analyzer and sets `TreatWarningsAsErrors`, but the rules below default to info severity or off, so they never fail the build alone. `dotnet_analyzer_diagnostic.severity = warning` in [templates/.editorconfig](../templates/.editorconfig) raises them to warnings, which `TreatWarningsAsErrors` makes errors. Without both files, a hand-written `IClock` builds cleanly. ([Meziantou: Meziantou.Analyzer rules](https://github.com/meziantou/Meziantou.Analyzer/blob/main/docs/README.md))

| Rule     | Title                                                          | Default                                                                    |
| -------- | -------------------------------------------------------------- | -------------------------------------------------------------------------- |
| `MA0188` | Use `System.TimeProvider` instead of a custom time abstraction | Enabled, info severity                                                     |
| `MA0166` | Forward the `TimeProvider` to methods that take one            | Enabled, info severity                                                     |
| `MA0167` | Use an overload with a `TimeProvider` argument                 | Disabled; [templates/.editorconfig](../templates/.editorconfig) enables it |

## Testing time

**Use `FakeTimeProvider` from `Microsoft.Extensions.TimeProvider.Testing`.** Set a start instant, and move time with `Advance(TimeSpan)`. Its timers fire as time moves, so you can test retry, backoff, and scheduling without `Thread.Sleep`. A single large `Advance` fires every due callback at once, at the end. If a test checks how callbacks interleave, advance in small steps. ([Lock: Avoiding flaky tests with TimeProvider and ITimer](https://andrewlock.net/exploring-the-dotnet-8-preview-avoiding-flaky-tests-with-timeprovider-and-itimer/))

Don't construct it either of these ways:

- **The parameterless constructor** starts at midnight on 2000-01-01 UTC, a date the test never states. Pass the start explicitly, even when the test passes at any instant.
- **`DateTimeOffset.UtcNow` as the start** brings back the real clock. The test's dates change on every run, and whether it crosses a DST or month boundary is luck. ([Microsoft Learn: FakeTimeProvider constructors](https://learn.microsoft.com/dotnet/api/microsoft.extensions.time.testing.faketimeprovider.-ctor)) For the one-clock rule, see [testing.md](testing.md#deterministic-time).

For date-sensitive logic, test these instants:

- A DST spring-forward gap: a local time that doesn't exist.
- A fall-back overlap: a local time that happens twice.
- A leap day.
- A year boundary where the calendar year and the ISO week year differ: 2022-01-02 is in week 52 of 2021.

([Meziantou: 39 misconceptions](https://www.meziantou.net/misconceptions-about-date-and-time.htm)) For the test framework stack, see [testing.md](testing.md).

## UI and hosts

- **Get the user's zone from the browser, not the server.** In interactive server Blazor, register a scoped `TimeProvider` per circuit, filled through JavaScript interop. For the pattern, see [ui-frameworks.md](ui-frameworks.md#blazor).
- **Run hosts in UTC, and convert at the edges.** Give each scheduled job an explicit zone. Local time repeats an hour when DST ends and skips one when it starts, so a job set to server-local time in that hour runs twice or not at all. ([Microsoft Learn: How to resolve ambiguous times](https://learn.microsoft.com/dotnet/standard/datetime/resolve-ambiguous-times), [Microsoft Learn: TimeZoneInfo.IsInvalidTime](https://learn.microsoft.com/dotnet/api/system.timezoneinfo.isinvalidtime))
- **Include tzdata and ICU in container images.** Without them, no zone conversion works. For image tags, invariant mode, failure modes, and display formatting, see [globalization.md](globalization.md#containers-chiseled-and-alpine-images-drop-this-data).

## Smaller traps

- **`TimeSpan.From*` has integer overloads since .NET 9,** because the `double` overloads lose precision: `TimeSpan.FromSeconds(101.832)` isn't exactly 101.832 seconds. In F#, the new overloads make `TimeSpan.FromMinutes(20)` ambiguous, so it needs a type annotation. ([Microsoft Learn: Breaking change: New TimeSpan.From\*() overloads that take integers](https://learn.microsoft.com/dotnet/core/compatibility/core-libraries/9.0/timespan-from-overloads))
- **`TimeZoneInfo.FindSystemTimeZoneById` takes the platform's native IDs** and needs only tzdata, so it works in invariant globalization mode. `TryConvertIanaIdToWindowsId` and `TryConvertWindowsIdToIanaId` need ICU, so they fail in invariant or NLS mode. For details, see [globalization.md](globalization.md).
