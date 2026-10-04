---
targets: [net10.0, csharp-14]
last-reviewed: 2026-08-18
last-used: 2026-09-25
sources:
  [ms-learn, dotnet-blog, jetbrains-dotnet, andrew-lock, meziantou, jon-skeet]
---

# C#

Target C# 14, which ships with .NET 10. Use its features where they replace an older idiom, not everywhere they compile.

## Opinions

### Extension members

**Use extension members (extension blocks) instead of scattered static extension method classes.** A C# 14 extension block groups the extension methods, properties, and operators for a receiver type in one place, and declares the receiver once. It also allows extension properties, which the old `this` parameter syntax can't express.

Before (C# 13 and earlier):

```csharp
public static class OrderExtensions
{
    public static bool IsOpen(this Order order) => order.ClosedAt is null;

    public static decimal Total(this Order order) => order.Lines.Sum(l => l.Price);
}
```

After (C# 14):

```csharp
public static class OrderExtensions
{
    extension(Order order)
    {
        public bool IsOpen => order.ClosedAt is null;

        public decimal Total => order.Lines.Sum(l => l.Price);
    }
}
```

- **Use an extension member for a pure query or transform over a type you don't own.** If you own the type, default to an instance method. The exception is a domain entity that you keep as a plain data holder, with validation, formatting, and mapping outside it.
- **Use the property form when the member reads as a fact about the instance, not an action.** `IsEmpty` and `WordCount` are awkward as calls and read naturally as properties.
- **Convert a method to a property while you're already changing its callers.** The change breaks every call site. ([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/), [Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14), [.NET Blog: C# 14 - Exploring extension members](https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/), [Microsoft Learn: Extension methods design guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/extension-methods))

### The `field` keyword

**Use the `field` keyword instead of a hand-written backing field** when a property needs simple validation, normalisation, or lazy logic in an accessor. Declare an explicit backing field only when code outside the accessors uses it. The keyword removes the field and property naming convention, and the risk that code bypasses the accessor by writing to the field directly.

Before:

```csharp
public class Sensor
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => _name = value.Trim();
    }
}
```

After:

```csharp
public class Sensor
{
    public string Name
    {
        get;
        set => field = value.Trim();
    } = "";
}
```

([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/))

### Null-conditional assignment

**Use null-conditional assignment (`obj?.Property = value`) instead of a nested `if (obj is not null)` guard.** The right-hand side is evaluated only when the receiver isn't null, so it's an exact, flatter rewrite of the guard.

Before:

```csharp
if (customer is not null)
{
    customer.LastSeen = DateTimeOffset.UtcNow;
}
```

After:

```csharp
customer?.LastSeen = DateTimeOffset.UtcNow;
```

([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/))

### Pattern matching

**Use `is null` and `is not null` for all null checks, and a switch expression when each branch produces a value.** An overloaded `==` or `!=` operator can't change the result of an `is` pattern. A switch expression with property, relational, and type patterns turns a chain of `if` and `else` into one table, and the compiler checks it for exhaustiveness. It reports CS8509 when an input isn't handled.

```csharp
public static decimal DiscountFor(Customer customer) => customer switch
{
    { Tier: Tier.Gold, YearsActive: >= 5 } => 0.20m,
    { Tier: Tier.Gold } => 0.10m,
    { Tier: Tier.Silver } => 0.05m,
    _ => 0m,
};
```

Put the most specific arms first, because they're matched from top to bottom. Don't force a pattern where a plain Boolean expression reads better: a two-way `if` doesn't improve by becoming a switch. ([Microsoft Learn: Pattern matching](https://learn.microsoft.com/dotnet/csharp/fundamentals/functional/pattern-matching))

### Collection expressions

**Create and combine collections with collection expressions (`[...]`), not a constructor with an initializer or a LINQ `Concat` and `ToArray` chain.** One syntax works for arrays, `List<T>`, spans, and immutable collections. The compiler picks the most efficient construction for the target type, and the spread element `..` replaces concatenation pipelines that allocate.

Before:

```csharp
var ids = new List<int> { 1, 2, 3 };
int[] merged = first.Concat(second).ToArray();
```

After:

```csharp
List<int> ids = [1, 2, 3];
int[] merged = [.. first, .. second];
```

The one cost is that the target type must be explicit: `List<int> ids = [...]`, not `var ids = [...]`. Accept it, because the type documents the code. ([Microsoft Learn: Collection expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions))

### Nullable reference types

**Enable nullable reference types in every project, and promote nullable warnings to errors.** Set `<Nullable>enable</Nullable>` and `<WarningsAsErrors>nullable</WarningsAsErrors>` in `Directory.Build.props`, so the compiler enforces the annotations.

- Never start a new project without it, and never use `#nullable disable` in new code.
- Use the null-forgiving operator `!` only where flow analysis can't see the value, such as a value that a serializer or test setup fills in. Justify every `!` in review.
- Keep `ArgumentNullException.ThrowIfNull` at public API boundaries, even in annotated code. Annotations exist only at compile time, so they don't protect against unannotated or reflection-based callers. ([Microsoft Learn: Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references))

### Analyzers

**Set the built-in .NET analyzers to `latest-recommended`, enforce code style in the build, and add Meziantou.Analyzer.** In `Directory.Build.props`:

```xml
<PropertyGroup>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

- **`latest-recommended`:** The built-in analyzers ship with the SDK. This level keeps the rules current across SDK updates, without the noise of the `all` level.
- **`EnforceCodeStyleInBuild`:** Turns `.editorconfig` style rules (IDExxxx) into build diagnostics instead of IDE-only suggestions, so CI and editors agree.
- **Meziantou.Analyzer:** Adds correctness rules that the SDK set misses, such as culture-sensitive string operations, `CancellationToken` forwarding, and async pitfalls. Its author runs it alongside other analyzers, so add others where they cover rules you want. Skip StyleCop when `dotnet format` already covers the formatting you care about.

Fix each diagnostic, or suppress it explicitly with a justification. Never lower severity across the board. ([Microsoft Learn: Code analysis overview](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview), [Meziantou: The Roslyn analyzers I use](https://www.meziantou.net/the-roslyn-analyzers-i-use.htm), [Meziantou: Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer))

### Records

**Limit a record to the data it's constructed from, with no derived state and no collection members.** A record's generated members behave the way the syntax suggests only when every property is a constructor parameter, stored unchanged. Review should catch two traps:

- **A property initialized from a constructor parameter goes stale under `with`.** `with` doesn't re-run the constructor. It lowers to roughly `var copy = original.<Clone>$(); copy.Value = 3;`, and the copy constructor copies every field _before_ the property assignments run. So a value computed at construction keeps its old value, while the parameter it came from changes. ([Skeet: Unexpected inconsistency in records](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/), [Skeet: Records and the `with` operator, redux](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/))

  ```csharp
  // Wrong: Even is computed once, at construction
  public sealed record Number(int Value)
  {
      public bool Even { get; } = (Value & 1) == 0;
  }

  var n3 = new Number(2) with { Value = 3 };  // Number { Value = 3, Even = True }

  // Right: derive on read, so `with` cannot desynchronize it
  public sealed record Number(int Value)
  {
      public bool Even => (Value & 1) == 0;
  }
  ```

- **A collection member breaks value equality.** The generated `Equals` compares each member with `EqualityComparer<T>.Default`, and the immutable collections don't override `Equals` or `GetHashCode`. `ImmutableList<T>` and the others compare by reference, so two records with identical contents aren't equal. Put a collection in a record only when you want reference equality, such as shared instances within one object graph. Otherwise, keep the collection outside the record, or accept that equality means identity and document it. ([Skeet: Records and Collections](https://codeblog.jonskeet.uk/2025/03/27/records-and-collections/))

### Smaller opinions

- **Prefer `Span<T>` and `ReadOnlySpan<T>` parameters in new APIs.** The implicit span conversions in C# 14 make them as easy to call as arrays, without the allocation. Note the .NET 10 breaking change: span overloads now win overload resolution in more cases. ([Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Use `nameof(List<>)` on unbound generics** instead of hard-coded strings in diagnostics and exceptions. ([Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14))
- **Use `StringComparison.Ordinal` for every machine-facing comparison, and `CultureInfo.InvariantCulture` for every machine-facing format or parse.** Never use `StringComparison.InvariantCulture`, because its collation isn't actually invariant. ([globalization.md](globalization.md))
- **Clone records with `with` expressions, and never declare an instance `Clone()` method.** It conflicts with the compiler-generated cloning. If you want a named method, wrap `with` in an extension method. ([Meziantou: Adding a Clone method to a C# record](https://www.meziantou.net/adding-a-clone-method-to-a-csharp-record.htm))

## Coming next (preview, not yet the opinion)

.NET 11 previews add **union types** and **closed class hierarchies**, with exhaustiveness checks at compile time. Follow them in Andrew Lock's series, and add them to the opinions when .NET 11 reaches GA. ([Lock: .NET (OK, C#) finally gets union types](https://andrewlock.net/exploring-the-dotnet-11-preview-2-dotnet-gets-union-types/), [Lock: Closed class hierarchies](https://andrewlock.net/exploring-the-dotnet-11-preview-4-closed-class-hierarchies/))
