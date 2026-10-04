---
targets: [net10.0, csharp-14]
last-reviewed: 2026-10-09
last-used: 2026-09-25
sources:
  [
    stephen-toub,
    dotnet-blog,
    ms-learn,
    steve-gordon,
    jetbrains-dotnet,
    jon-skeet,
    andrew-lock,
    ardalis,
  ]
---

# Runtime & performance

The .NET 10 JIT rewards idiomatic code. Optimize what you've measured, not what folklore says is slow.

## Opinions

- **Write idiomatic C#, and let the JIT do its work.** In .NET 10, escape analysis, stack allocation of small objects, doubled inlining budgets, and bounds-check elimination apply automatically to clean code. Don't use `unsafe` or unusual patterns to "help" the compiler. ([Toub: Performance Improvements in .NET 10](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/))
- **Profile before you optimize, and then benchmark the inner loop.** Use a profiler, such as dotTrace or PerfView, to find the real bottleneck. Then use BenchmarkDotNet to iterate on it. Allocation numbers from benchmarks and profilers differ because of GC padding and alignment. Trust benchmarks for changes in an inner loop, and profilers for finding where allocations come from. ([Gordon: The Grand Mystery of the Missing 18 Bytes](https://www.stevejgordon.co.uk/the-grand-mystery-of-the-missing-18-bytes), [JetBrains: Your AI Agent Keeps Missing The Real Bottleneck. JetBrains Rider Can Fix It Now.](https://blog.jetbrains.com/dotnet/2026/06/25/performance-profiling-agent-skill-in-rider/))
- **Use `Span<T>` for parsing and formatting hot paths, and `ArrayPool<T>` for large temporary buffers.** For an example, see [`Span<T>` and `ArrayPool<T>`](#spant-and-arraypoolt). ([Gordon: Encrypting Properties with System.Text.Json and a TypeInfoResolver Modifier (Part 2)](https://www.stevejgordon.co.uk/encrypting-properties-with-system-text-json-and-a-typeinforesolver-modifier-part-2))
- **Read Toub's annual "Performance Improvements in .NET X" post at each GA release.** It's the canonical record of what the runtime now does automatically, which tells you which manual optimizations to delete. ([Toub: Performance Improvements in .NET 10](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/))

## Immutable collections

**Choose an immutable collection by how it's built, not by the word "immutable".** `ImmutableList<T>` and `ImmutableDictionary<K,V>` are designed for incremental change. Their tree structure lets `Add` return a new collection that shares most of the old one, but every read walks the tree. Most application state isn't built that way. It's assembled once, at startup or on each refresh, then read constantly and replaced as a whole. For that pattern, use `ImmutableArray<T>` and `FrozenDictionary<K,V>`, which cost more to build but are flat and fast to read. Skeet measured a validation pass over election data drop from 5.5 ms to 0.826 ms after the switch, "due to it performing lots of read accesses". ([Skeet: Changing Immutable Collections](https://codeblog.jonskeet.uk/2025/12/31/changing-immutable-collections/))

Keep the `Immutable*` builders only where you need incremental change, such as a growing snapshot that you hand to concurrent readers between edits. Expect one migration cost: `ImmutableArray<T>` is a struct, so `default` is a valid but unusable value, where `ImmutableList<T>` would have been `null`. Unwrap a nullable `ImmutableArray<T>?` field with `.Value` or an `is` pattern before you use it.

## Async

- **Return `Task`, not `void`, and await instead of blocking.** Use `async void` only for event handlers. Calling `.Result` or `.Wait()` on async work risks deadlocks and thread starvation, because the compiler-generated state machine expects to resume through continuations, not on blocked threads. ([Toub: How Async/Await Really Works in C#](https://devblogs.microsoft.com/dotnet/how-async-await-really-works/))
- **Use `ConfigureAwait(false)` in library code, and leave it out of application code.** A library can't know its caller's context, so it shouldn't capture it. Not capturing the context avoids deadlocks with callers that block on it, and skips an unnecessary context switch. ASP.NET Core has no `SynchronizationContext` to capture, so `ConfigureAwait(false)` there adds nothing. UI code usually _wants_ the context. Apply one rule per layer, instead of deciding case by case. ([Toub: ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/))
- **Default to `Task<T>`. Use `ValueTask<T>` only for hot APIs that usually complete synchronously.** `ValueTask` helps only when profiling shows that `Task` allocations matter and the synchronous path dominates, as in a buffered read. Its contract is stricter: await it exactly once, and never concurrently. When in doubt, use `Task`, which is safe and composes well. ([Toub: Understanding the Whys, Whats, and Whens of ValueTask](https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/))

## `Span<T>` and `ArrayPool<T>`

Before, with one string allocation per field:

```csharp
static int SumCsvAllocating(string line)
{
    var sum = 0;
    foreach (var field in line.Split(','))
    {
        sum += int.Parse(field);
    }
    return sum;
}
```

After, with no allocations, because `Split` over a span returns `Range` values instead of strings:

```csharp
static int SumCsv(ReadOnlySpan<char> line)
{
    var sum = 0;
    foreach (var range in line.Split(','))
    {
        sum += int.Parse(line[range]);
    }
    return sum;
}
```

For a large temporary buffer, rent from the shared pool instead of allocating on each call, and always return the buffer in a `finally` block:

```csharp
using System.Buffers;

static async Task CopyAsync(Stream source, Stream destination, CancellationToken cancellationToken = default)
{
    var buffer = ArrayPool<byte>.Shared.Rent(81920);
    try
    {
        int read;
        while ((read = await source.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
        }
    }
    finally
    {
        ArrayPool<byte>.Shared.Return(buffer);
    }
}
```

Never use a rented buffer after you return it. Never assume that `Rent` returns exactly the size you asked for: slice the buffer to what you used.

## GC modes

**Keep the defaults.** ASP.NET Core defaults to server GC with DATAS, which adapts the heap count dynamically and has been on by default since .NET 9. It gives throughput under load and shrinks the heap when the app is idle. Everything else defaults to workstation GC. Override the default in only two cases, and verify the change with memory and latency measurements:

- **Many .NET services on one small node,** such as dense containers or sidecars: force workstation GC with `<ServerGarbageCollection>false</ServerGarbageCollection>`. Server GC has a heap per core, which multiplies across processes. DATAS reduces that footprint, but doesn't remove it.
- **A CPU-bound batch or worker app that isn't ASP.NET Core:** opt _in_ to server GC for throughput.

([Microsoft Learn: Workstation and server GC](https://learn.microsoft.com/dotnet/standard/garbage-collection/workstation-server-gc), [Microsoft Learn: Dynamic adaptation to application sizes (DATAS)](https://learn.microsoft.com/dotnet/standard/garbage-collection/datas), [Toub: Performance Improvements in .NET 10](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/))

**Set GC options in `runtimeconfig.json` or MSBuild, not in environment variables.** The environment variable form is parsed as hexadecimal, and gets the value silently wrong. `DOTNET_GCHeapHardLimitPercent=60` doesn't cap the heap at 60%: it reads as 0x60, which is 96%. As `System.GC.HeapHardLimitPercent` in `runtimeconfig.json`, the same number is decimal and means what it says. This applies to all numeric GC settings, including the heap count, the LOH threshold, and the high-memory percentage.

Usually, you don't need to set a limit at all. Under a container memory limit, the GC already treats that limit as the total physical memory, and sets the hard limit to 75% of it by default. A value of your own tightens that default. ([Microsoft Learn: Garbage collector config settings](https://learn.microsoft.com/dotnet/core/runtime-config/garbage-collector), reviewed 2026-02-09; [Smith: Top 10 ways to reduce .NET memory usage in Kubernetes](https://ardalis.com/top-10-ways-to-reduce-net-memory-usage-in-kubernetes/))

**`Environment.ProcessorCount` tells you what the process can use, not what the machine has.** Since .NET 6, it honours process affinity and container CPU limits, so under a cgroup quota it reports the quota. Use it to size a thread pool or a `Parallel` loop. Don't use it to report host capacity or to count cores for a licence.

The BCL doesn't expose the host total, so you need a platform call:

| OS      | Call                                   |
| ------- | -------------------------------------- |
| Windows | `GetActiveProcessorCount`              |
| macOS   | `sysctlbyname("hw.logicalcpu")`        |
| Linux   | Parse `/sys/devices/system/cpu/online` |

Cache the result in a singleton, because it can't change while the process runs. Declare the import with `[LibraryImport]`, so the marshalling is source-generated and survives trimming. ([Lock: Finding the total number of processors on a machine with .NET](https://andrewlock.net/finding-the-total-number-of-processors-on-a-machine-with-dotnet/))

## Native AOT

**Use Native AOT for short-lived and size-sensitive workloads, such as CLI tools, serverless functions, and sidecars. Keep the JIT for long-running services.**

- AOT wins on startup, with millisecond starts and no JIT warm-up, and on disk and memory footprint.
- The JIT wins on steady-state throughput through tiered compilation and dynamic PGO. It also handles reflection-heavy libraries that AOT trimming breaks.

With AOT, the whole dependency graph must be safe for trimming and AOT, with source-generated JSON and no code generation at run time. Review the `IsAotCompatible` warnings before you commit to it. Run the test suite as an AOT build too, because the reflection failures it causes appear nowhere else. For details, see [testing.md](testing.md#framework-and-platform). .NET 10 file-based apps make the CLI tool case simple: `dotnet publish app.cs` produces a Native AOT binary by default. ([Microsoft Learn: Native AOT deployment](https://learn.microsoft.com/dotnet/core/deploying/native-aot/), [Microsoft Learn: File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps))

**Trim a dependency's dead branch with an ILLink substitution when a library pulls in more than you call.** A property the library checks at run time keeps everything behind it rooted. Stub its getter to a constant, and the trimmer removes that whole chain. Andrew Lock stubbed `SymbolServer.get_IsSymweb` in `Microsoft.Diagnostics.Runtime` to `false`. That cut a Native AOT binary from 3.956 MB to 2.949 MB on win-x64, or 25.4%, and by 22.2–22.3% on the four Linux targets.

For `PublishAot`, pass the file to ILC directly. The `Microsoft.DotNet.ILCompiler` targets don't wire up `ILLinkSubstitutionsXmls`, and they ignore an `EmbeddedResource` named `ILLink.Substitutions.xml`. A stub changes the library's behaviour, so stub only a branch you've confirmed your app never takes. ([Lock: Stripping extra dependencies in .NET NativeAOT apps using linker substitutions](https://andrewlock.net/stripping-extra-dependencies-in-.net-nativeaot-apps-using-linker-substitutions/))

```xml
<!-- ILLink.Substitutions.xml -->
<linker>
  <assembly fullname="Microsoft.Diagnostics.Runtime">
    <type fullname="Microsoft.Diagnostics.Runtime.Implementation.SymbolServer">
      <method signature="System.Boolean get_IsSymweb()" body="stub" value="false" />
    </type>
  </assembly>
</linker>
```

```xml
<!-- in the .csproj -->
<ItemGroup>
  <IlcArg Include="--substitution:$(MSBuildThisFileDirectory)ILLink.Substitutions.xml" />
</ItemGroup>
```

**Decide on invariant globalization separately from AOT.** `dotnet new webapiaot` sets `<InvariantGlobalization>true</InvariantGlobalization>` next to `<PublishAot>true</PublishAot>`. So the property tends to enter a codebase as part of an unrelated decision, and it changes what every `ToString` and `Compare` call in the app does. Keep it only if you chose it on purpose. For details, see [globalization.md](globalization.md).

## Coming next (preview, not yet the opinion)

.NET 11 (RC1, GA expected November 2026) makes runtime async opt-in without preview flags: `<Features>$(Features);runtime-async=on</Features>` on a `net11.0` project, with neither `LangVersion=preview` nor `EnablePreviewFeatures`. Most of the shared framework already ships compiled that way. Toub measures a synchronously completing two-layer chain at 21.221 ns and 144 B before, 6.151 ns and 0 B after. Exception propagation through 30 frames drops from 51.3 µs and 84.2 KB to 10.7 µs and 5.7 KB. The [Async](#async) opinions above stay as written until .NET 11 ships and the switch is re-checked against the GA docs. ([Toub: Performance Improvements in .NET 11](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-11/))
