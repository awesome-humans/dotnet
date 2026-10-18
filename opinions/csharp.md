---
targets: [net10.0, csharp-14]
last-reviewed: 2026-08-18
sources:
  [andrew-lock, dotnet-blog, jetbrains-dotnet, jon-skeet, meziantou, ms-learn]
---

# C#

Target C# 14, which ships with .NET 10. Use a new feature where it replaces an older idiom, not everywhere it compiles.

## Opinions

### Extension members

**Use extension blocks instead of static classes of `this`-parameter methods.** A C# 14 extension block declares the receiver once and groups its extension methods, properties, and operators. It also allows extension properties, which the `this` parameter syntax can't express.

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

- **Use an extension member for a pure query or transform over a type you don't own.** For a type you own, write an instance method. The exception is a domain entity you keep as plain data, with validation, formatting, and mapping outside it.
- **Use a property when the member states a fact about the instance.** `IsEmpty` and `WordCount` read naturally as properties and awkwardly as calls.
- **Convert a method to a property only while you're already changing its callers.** The conversion breaks every call site. ([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/), [Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14), [.NET Blog: C# 14 - Exploring extension members](https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/), [Microsoft Learn: Extension methods design guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/extension-methods))

### The `field` keyword

**When an accessor needs validation, normalisation, or lazy logic, use the `field` keyword instead of a hand-written backing field.** Declare an explicit field only when code outside the accessors uses it. With no named field, no code can write to it and skip the accessor.

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

**Use null-conditional assignment (`obj?.Property = value`) instead of an `if (obj is not null)` guard.** The right-hand side runs only when the receiver isn't null, so the rewrite behaves exactly like the guard.

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

**Check for null with `is null` and `is not null`, and use a switch expression when each branch produces a value.** An overloaded `==` or `!=` can't change the result of an `is` pattern. A switch expression with property, relational, and type patterns replaces an `if`/`else` chain with one table, and the compiler reports CS8509 when an input isn't handled.

```csharp
public static decimal DiscountFor(Customer customer) => customer switch
{
    { Tier: Tier.Gold, YearsActive: >= 5 } => 0.20m,
    { Tier: Tier.Gold } => 0.10m,
    { Tier: Tier.Silver } => 0.05m,
    _ => 0m,
};
```

Arms match from top to bottom, so put the most specific first. Keep a two-way `if` as an `if`. ([Microsoft Learn: Pattern matching](https://learn.microsoft.com/dotnet/csharp/fundamentals/functional/pattern-matching))

### Collection expressions

**Create and combine collections with collection expressions (`[...]`), not a constructor with an initializer or a `Concat().ToArray()` chain.** One syntax covers arrays, `List<T>`, spans, and immutable collections. The compiler picks the cheapest construction for the target type, and the spread element `..` replaces allocating concatenation.

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

The target type must be explicit: `List<int> ids = [...]`, not `var ids = [...]`. ([Microsoft Learn: Collection expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions))

### Nullable reference types

**Enable nullable reference types in every project, and make nullable warnings errors.** Set `<Nullable>enable</Nullable>` and `<WarningsAsErrors>nullable</WarningsAsErrors>` in `Directory.Build.props`.

- Never use `#nullable disable` in new code.
- Use the null-forgiving operator `!` only where flow analysis can't see the value, such as a property a serializer or test setup fills in. Justify every `!` in review.
- Keep `ArgumentNullException.ThrowIfNull` at public API boundaries. Annotations exist only at compile time, so they don't stop unannotated or reflection-based callers. ([Microsoft Learn: Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references))

### Analyzers

**Set the built-in analyzers to `latest-recommended`, enforce code style in the build, and add Meziantou.Analyzer.** In `Directory.Build.props`:

```xml
<PropertyGroup>
  <AnalysisLevel>latest-recommended</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

- **`latest-recommended`:** Picks up new rules with each SDK, without the noise of `all`.
- **`EnforceCodeStyleInBuild`:** Reports `.editorconfig` style rules (`IDExxxx`) in the build, not only in the IDE, so CI and editors agree.
- **Meziantou.Analyzer:** Adds correctness rules the SDK lacks, such as culture-sensitive string operations, `CancellationToken` forwarding, and async mistakes. Add other analyzers alongside it for rules it doesn't cover. Skip StyleCop if `dotnet format` already enforces your formatting.

Fix each diagnostic, or suppress it with a written justification. Never lower severity across the board. ([Microsoft Learn: Code analysis overview](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview), [Meziantou: The Roslyn analyzers I use](https://www.meziantou.net/the-roslyn-analyzers-i-use.htm), [Meziantou: Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer))

### Records

**Keep a record to the data it's constructed from: no derived state, and no collection members.** The generated members behave as the syntax suggests only when every property is a constructor parameter stored unchanged. Catch these two cases in review:

- **A property initialized from a parameter goes stale under `with`.** `with` doesn't re-run the constructor. It lowers to roughly `var copy = original.<Clone>$(); copy.Value = 3;`, and the copy constructor copies every field _before_ the assignments run. A value computed at construction keeps its old value while its source parameter changes. ([Skeet: Unexpected inconsistency in records](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/), [Skeet: Records and the `with` operator, redux](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/))

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

- **A collection member breaks value equality.** The generated `Equals` compares members with `EqualityComparer<T>.Default`. `ImmutableList<T>` and the other immutable collections don't override `Equals`, so they compare by reference, and two records with identical contents aren't equal. Use a collection member only when you want reference equality, such as instances shared within one object graph. Otherwise, keep the collection outside the record, or document that equality means identity. ([Skeet: Records and Collections](https://codeblog.jonskeet.uk/2025/03/27/records-and-collections/))

### Smaller opinions

- **Take `Span<T>` and `ReadOnlySpan<T>` parameters in new APIs.** C# 14's implicit span conversions let callers pass arrays without an allocation. In .NET 10, span overloads win overload resolution in more cases, which is a breaking change. ([Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Use `nameof(List<>)` on unbound generics instead of hard-coded type names in diagnostics and exceptions.** ([Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14))
- **Use `StringComparison.Ordinal` for every machine-facing comparison, and `CultureInfo.InvariantCulture` for every machine-facing format or parse.** Never use `StringComparison.InvariantCulture`: its collation isn't invariant. For details, see [globalization.md](globalization.md).
- **Clone records with `with`, and never declare an instance `Clone()` method.** It conflicts with the compiler-generated clone. If you want a named method, wrap `with` in an extension method. ([Meziantou: Adding a Clone method to a C# record](https://www.meziantou.net/adding-a-clone-method-to-a-csharp-record.htm))

## Coming next (preview, not yet the opinion)

.NET 11 previews add **union types** and **closed class hierarchies**, with compile-time exhaustiveness checks. Add them to the opinions when .NET 11 reaches GA. ([Lock: .NET (OK, C#) finally gets union types](https://andrewlock.net/exploring-the-dotnet-11-preview-2-dotnet-gets-union-types/), [Lock: Closed class hierarchies](https://andrewlock.net/exploring-the-dotnet-11-preview-4-closed-class-hierarchies/))
