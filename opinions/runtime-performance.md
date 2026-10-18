---
targets: [net10.0, csharp-14]
last-reviewed: 2026-10-09
sources:
  [
    andrew-lock,
    ardalis,
    dotnet-blog,
    jetbrains-dotnet,
    jon-skeet,
    ms-learn,
    stephen-toub,
    steve-gordon,
  ]
---

# Runtime & performance

The .NET 10 JIT optimizes idiomatic code best. Optimize what you've measured, not what you've heard is slow.

## Opinions

- **Write idiomatic C#.** In .NET 10, the JIT applies escape analysis, stack allocation of small objects, doubled inlining budgets, and bounds-check elimination to plain code. `unsafe` code and unusual patterns written to help it usually block those optimizations. ([Toub: Performance Improvements in .NET 10](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/))
- **Profile first, then benchmark the inner loop.** Find the bottleneck with a profiler, such as dotTrace or PerfView, then iterate on it with BenchmarkDotNet. The two report different allocation numbers because of GC padding and alignment. Trust benchmarks for inner-loop changes, and profilers for where allocations come from. ([Gordon: The Grand Mystery of the Missing 18 Bytes](https://www.stevejgordon.co.uk/the-grand-mystery-of-the-missing-18-bytes), [JetBrains: Your AI Agent Keeps Missing The Real Bottleneck. JetBrains Rider Can Fix It Now.](https://blog.jetbrains.com/dotnet/2026/06/25/performance-profiling-agent-skill-in-rider/))
- **Use `Span<T>` on parsing and formatting hot paths, and `ArrayPool<T>` for large temporary buffers.** For examples, see [`Span<T>` and `ArrayPool<T>`](#spant-and-arraypoolt). ([Gordon: Encrypting Properties with System.Text.Json and a TypeInfoResolver Modifier (Part 2)](https://www.stevejgordon.co.uk/encrypting-properties-with-system-text-json-and-a-typeinforesolver-modifier-part-2))
- **At each GA release, read Toub's "Performance Improvements in .NET X" post.** It lists what the runtime now optimizes by itself, which tells you which manual optimizations to delete. ([Toub: Performance Improvements in .NET 10](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/))

## Immutable collections

**Choose an immutable collection by how you build it.** `ImmutableList<T>` and `ImmutableDictionary<K,V>` are trees built for incremental change: `Add` returns a new collection that shares most of the old one, but every read walks the tree. Most application state is built once, at startup or on refresh, then read constantly and replaced whole. For that, use `ImmutableArray<T>` and `FrozenDictionary<K,V>`, which cost more to build and are flat and fast to read. Skeet's validation pass over election data dropped from 5.5 ms to 0.826 ms after the switch, "due to it performing lots of read accesses". ([Skeet: Changing Immutable Collections](https://codeblog.jonskeet.uk/2025/12/31/changing-immutable-collections/))

Keep the `Immutable*` trees and builders only for incremental change, such as a growing snapshot that you hand to concurrent readers between edits. When you migrate, check for `default`: `ImmutableArray<T>` is a struct, so its default is a valid but unusable value where `ImmutableList<T>` was `null`. Unwrap a nullable `ImmutableArray<T>?` field with `.Value` or an `is` pattern before you use it.

## Async

- **Return `Task`, not `void`, and await instead of blocking.** Use `async void` only for event handlers. `.Result` or `.Wait()` on async work risks deadlocks and thread-pool starvation, because the state machine expects to resume through a continuation, not on a blocked thread. ([Toub: How Async/Await Really Works in C#](https://devblogs.microsoft.com/dotnet/how-async-await-really-works/))
- **Use `ConfigureAwait(false)` in library code, and leave it out of application code.** A library doesn't know its caller's context, so it shouldn't capture it. Skipping the capture avoids deadlocks with callers that block, and saves a context switch. ASP.NET Core has no `SynchronizationContext`, so `ConfigureAwait(false)` there does nothing, and UI code usually _needs_ the context. Apply one rule per layer. ([Toub: ConfigureAwait FAQ](https://devblogs.microsoft.com/dotnet/configureawait-faq/))
- **Return `Task<T>`. Use `ValueTask<T>` only on hot APIs that usually complete synchronously,** such as a buffered read, and only after profiling shows the `Task` allocations matter. Await a `ValueTask` exactly once, and never concurrently. ([Toub: Understanding the Whys, Whats, and Whens of ValueTask](https://devblogs.microsoft.com/dotnet/understanding-the-whys-whats-and-whens-of-valuetask/))

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

After, with no allocations, because `Split` over a span returns `Range` values:

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

For a large temporary buffer, rent from the shared pool, and return the buffer in a `finally` block:

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

Never use a buffer after you return it. `Rent` can return a larger buffer than you asked for, so slice it to the length you use.

## GC modes

**Keep the defaults.** ASP.NET Core uses server GC with DATAS, on by default since .NET 9, which adapts the heap count to load and shrinks the heap when the app is idle. Everything else uses workstation GC. Override the default in only two cases, and measure memory and latency before and after:

- **Many .NET services on one small node,** such as dense containers or sidecars: set `<ServerGarbageCollection>false</ServerGarbageCollection>`. Server GC keeps a heap per core in every process. DATAS reduces that, but doesn't remove it.
- **A CPU-bound batch or worker app outside ASP.NET Core:** opt _in_ to server GC for throughput.

([Microsoft Learn: Workstation and server GC](https://learn.microsoft.com/dotnet/standard/garbage-collection/workstation-server-gc), [Microsoft Learn: Dynamic adaptation to application sizes (DATAS)](https://learn.microsoft.com/dotnet/standard/garbage-collection/datas), [Toub: Performance Improvements in .NET 10](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-10/))

**Set GC options in `runtimeconfig.json` or MSBuild, not environment variables.** The runtime parses numeric GC environment variables as hexadecimal. `DOTNET_GCHeapHardLimitPercent=60` reads as 0x60, which caps the heap at 96%, not 60%. As `System.GC.HeapHardLimitPercent` in `runtimeconfig.json`, the value is decimal. This applies to every numeric GC setting, including the heap count, the LOH threshold, and the high-memory percentage.

Usually you don't need a limit. Under a container memory limit, the GC treats the limit as total physical memory and sets the hard limit to 75% of it. Set your own only to go lower. ([Microsoft Learn: Garbage collector config settings](https://learn.microsoft.com/dotnet/core/runtime-config/garbage-collector), reviewed 2026-02-09; [Smith: Top 10 ways to reduce .NET memory usage in Kubernetes](https://ardalis.com/top-10-ways-to-reduce-net-memory-usage-in-kubernetes/))

**Use `Environment.ProcessorCount` to size work, not to count the machine's cores.** Since .NET 6, it honours process affinity and container CPU limits, so under a cgroup quota it reports the quota. Use it for a thread pool or a `Parallel` loop, not for host capacity or a licence count.

The BCL doesn't expose the host total, so call the platform:

| OS      | Call                                   |
| ------- | -------------------------------------- |
| Windows | `GetActiveProcessorCount`              |
| macOS   | `sysctlbyname("hw.logicalcpu")`        |
| Linux   | Parse `/sys/devices/system/cpu/online` |

The value can't change while the process runs, so cache it in a singleton. Declare the import with `[LibraryImport]`, so the marshalling is source-generated and survives trimming. ([Lock: Finding the total number of processors on a machine with .NET](https://andrewlock.net/finding-the-total-number-of-processors-on-a-machine-with-dotnet/))

## Native AOT

**Use Native AOT for short-lived and size-sensitive workloads, such as CLI tools, serverless functions, and sidecars. Keep the JIT for long-running services.**

- AOT starts in milliseconds with no JIT warm-up, and uses less disk and memory.
- The JIT has higher steady-state throughput from tiered compilation and dynamic PGO, and it runs reflection-heavy libraries that AOT trimming breaks.

AOT needs the whole dependency graph to be trim-safe and AOT-safe: source-generated JSON, and no code generation at run time. Review the `IsAotCompatible` warnings before you commit to it. Run the test suite as an AOT build too, because reflection failures show up nowhere else. For details, see [testing.md](testing.md#framework-and-platform). For a CLI tool, a .NET 10 file-based app needs nothing extra: `dotnet publish app.cs` produces a Native AOT binary by default. ([Microsoft Learn: Native AOT deployment](https://learn.microsoft.com/dotnet/core/deploying/native-aot/), [Microsoft Learn: File-based apps](https://learn.microsoft.com/dotnet/core/sdk/file-based-apps))

**When a library pulls in more than you call, trim the dead branch with an ILLink substitution.** A property the library checks at run time keeps everything behind it. Stub the getter to a constant, and the trimmer removes the whole chain. Andrew Lock stubbed `SymbolServer.get_IsSymweb` in `Microsoft.Diagnostics.Runtime` to `false`. That cut a Native AOT binary from 3.956 MB to 2.949 MB on win-x64 (25.4%), and by 22.2% to 22.3% on the four Linux targets.

For `PublishAot`, pass the file to ILC directly. The `Microsoft.DotNet.ILCompiler` targets don't read `ILLinkSubstitutionsXmls`, and they ignore an `EmbeddedResource` named `ILLink.Substitutions.xml`. A stub changes the library's behaviour, so stub only a branch you've confirmed your app never takes. ([Lock: Stripping extra dependencies in .NET NativeAOT apps using linker substitutions](https://andrewlock.net/stripping-extra-dependencies-in-.net-nativeaot-apps-using-linker-substitutions/))

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

**Decide on invariant globalization separately from AOT.** `dotnet new webapiaot` sets `<InvariantGlobalization>true</InvariantGlobalization>` next to `<PublishAot>true</PublishAot>`, so the property arrives with an unrelated decision. It changes what every `ToString` and `Compare` call does. Keep it only if you chose it. For details, see [globalization.md](globalization.md).

## Coming next (preview, not yet the opinion)

.NET 11 (RC1, GA expected November 2026) makes runtime async an opt-in without preview flags: `<Features>$(Features);runtime-async=on</Features>` on a `net11.0` project, with neither `LangVersion=preview` nor `EnablePreviewFeatures`. Most of the shared framework already ships compiled that way. Toub measures a synchronously completing two-layer chain at 21.221 ns and 144 B before, and 6.151 ns and 0 B after. Exception propagation through 30 frames drops from 51.3 µs and 84.2 KB to 10.7 µs and 5.7 KB. The [Async](#async) opinions stand until .NET 11 ships and the switch is rechecked against the GA docs. ([Toub: Performance Improvements in .NET 11](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-11/))
