---
targets: [net11.0, csharp-15]
last-reviewed: 2026-11-14
sources:
  [andrew-lock, dotnet-blog, jetbrains-dotnet, jon-skeet, meziantou, ms-learn]
---

# C#

Target C# 15, which ships with .NET 11. Use a new feature where it replaces an older idiom, not everywhere it compiles.

## Opinions

### Extension members

**Use extension blocks instead of static classes of `this`-parameter methods.** A C# 14 extension block declares the receiver once for every member in it. It also allows extension properties and operators, which the `this`-parameter syntax can't express.

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
- **Convert a method to a property only while you're already changing its callers.** The conversion breaks every call site.

([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/), [Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14), [.NET Blog: C# 14 - Exploring extension members](https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/), [Microsoft Learn: Extension methods design guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/extension-methods), [dotnet/csharplang: Extension members](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/extensions.md))

### The `field` keyword

**When an accessor needs validation, normalisation, or lazy logic, use the `field` keyword instead of a hand-written backing field.** Declare an explicit field only when code outside the accessors uses it. Without a named field, no other member can write the value and skip the accessor.

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

([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/), [dotnet/csharplang: `field` keyword in properties](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/field-keyword.md))

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

([.NET Blog: Introducing C# 14](https://devblogs.microsoft.com/dotnet/introducing-csharp-14/), [dotnet/csharplang: Null-conditional assignment](https://github.com/dotnet/csharplang/blob/main/proposals/csharp-14.0/null-conditional-assignment.md))

### Pattern matching

**Check for null with `is null` and `is not null`, and use a switch expression when each branch produces a value.** An overloaded `==` or `!=` can't change the result of an `is` pattern. A switch expression with property, relational, and type patterns replaces an `if`/`else` chain with one table. The compiler reports CS8509 when "The switch expression does not handle all possible values of its input type". ([Microsoft Learn: Pattern matching errors and warnings](https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-messages/pattern-matching-warnings))

```csharp
public static decimal DiscountFor(Customer customer) => customer switch
{
    { Tier: Tier.Gold, YearsActive: >= 5 } => 0.20m,
    { Tier: Tier.Gold } => 0.10m,
    { Tier: Tier.Silver } => 0.05m,
    _ => 0m,
};
```

Arms match from top to bottom, so put the most specific first. Keep a two-way `if` as an `if`.

([Microsoft Learn: Pattern matching](https://learn.microsoft.com/dotnet/csharp/fundamentals/functional/pattern-matching))

### Unions and closed hierarchies

**Model a fixed set of alternatives as a `union` when the case types are unrelated or aren't yours, and as a `closed` class when you own the base type. Switch over either without a discard arm.** The compiler knows every case, so the switch expression is exhaustive without `_`. When you add a case, CS8509 reports each switch that doesn't handle it. A `_` arm that throws hides that: when a type is added, "all your existing code continues to compile", and the arm throws at run time. IDE0072 asks for that arm, so [templates/.editorconfig](../templates/.editorconfig) silences it.

```csharp
public sealed record Product(int Id, string Name);
public sealed record ProductNotFound(string Sku);
public readonly union ProductLookup(Product, ProductNotFound);

public closed record class PaymentResult;
public sealed record class Paid(string Reference) : PaymentResult;
public sealed record class Declined(string Reason) : PaymentResult;

public static class Describe
{
    public static string Lookup(ProductLookup lookup) => lookup switch
    {
        Product product => $"Found {product.Name}",
        ProductNotFound missing => $"No product with SKU {missing.Sku}",
        null => "No lookup result",
    };

    public static string Payment(PaymentResult result) => result switch
    {
        Paid paid => $"Paid, reference {paid.Reference}",
        Declined declined => $"Declined: {declined.Reason}",
    };
}
```

The `null` arm is for `default(ProductLookup)`. A union is a struct, so its default value holds no case, and "the null pattern checks whether Value is null". Without that arm, the switch compiles without a warning, and a default value throws `SwitchExpressionException`. Write the `null` arm, not `_`, so CS8509 still reports a new case. ([Microsoft Learn: Union types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union))

([.NET Blog: Announcing .NET 11](https://devblogs.microsoft.com/dotnet/announcing-dotnet-11/), [Lock: .NET (OK, C#) finally gets union types](https://andrewlock.net/exploring-the-dotnet-11-preview-2-dotnet-gets-union-types/), [Lock: Closed class hierarchies](https://andrewlock.net/exploring-the-dotnet-11-preview-4-closed-class-hierarchies/))

- **Replace a `private protected` or `internal` constructor with `closed` when the base type is already abstract.** The constructor stops another assembly deriving from the type, but the compiler still reports CS8509 on a switch without `_`. A closed class is implicitly abstract, so on a base type that code instantiates, `closed` breaks the build, and on a public type in a shipped library it's a breaking change. ([Lock: Closed class hierarchies](https://andrewlock.net/exploring-the-dotnet-11-preview-4-closed-class-hierarchies/))
- **Seal each derived type, or mark it `closed` too.** "Derivation isn't transitive: a non-closed descendant of a closed class can still be derived from in other assemblies." Don't add `abstract` or `sealed` to the closed class itself, because the compiler rejects both. ([Microsoft Learn: What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15))
- **If two cases of a union have the same JSON shape, configure a classifier.** System.Text.Json serializes C# unions in .NET 11, and deserialization can't tell such cases apart without one. ASP.NET Core supports unions only in JSON bodies, "not to route, query, header, or form binding". ([.NET Blog: Announcing .NET 11](https://devblogs.microsoft.com/dotnet/announcing-dotnet-11/))

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

The target type must be explicit: `List<int> ids = [...]`, not `var ids = [...]`.

To pass a capacity or a comparer, put a `with(...)` element first, as in `HashSet<string> names = [with(StringComparer.OrdinalIgnoreCase), "Alice", "ALICE"];`. It calls the collection's constructor or factory method with those arguments, so a comparer is no longer a reason to go back to `new`.

([Microsoft Learn: Collection expressions](https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions), [Microsoft Learn: What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15))

### Records

**Keep a record to the data it's constructed from: no derived state, and no collection members.** The generated members behave as the syntax suggests only when every property is a constructor parameter stored unchanged. Catch these two cases in review:

- **A property initialized from a parameter goes stale under `with`.** `with` doesn't re-run the constructor. It lowers to roughly `var copy = original.<Clone>$(); copy.Value = 3;`, and the copy constructor copies every field _before_ the assignments run. A property computed at construction keeps its old value when `with` changes the parameter it came from. ([Skeet: Unexpected inconsistency in records](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/), [Skeet: Records and the `with` operator, redux](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/))

  ```csharp
  // Wrong: Even is computed once, at construction
  public sealed record Number(int Value)
  {
      public bool Even { get; } = (Value & 1) == 0;
  }

  Number n3 = new Number(2) with { Value = 3 };  // Number { Value = 3, Even = True }

  // Right: derived on read, so it always follows Value
  public sealed record Number(int Value)
  {
      public bool Even => (Value & 1) == 0;
  }
  ```

- **A collection member breaks value equality.** The generated `Equals` compares members with `EqualityComparer<T>.Default`. `ImmutableList<T>` doesn't override `Equals`, so it compares by reference, and two records with identical contents aren't equal. Use a collection member only when you want reference equality, such as instances shared within one object graph. Otherwise, keep the collection outside the record, or document that equality means identity. ([Skeet: Records and Collections](https://codeblog.jonskeet.uk/2025/03/27/records-and-collections/))

### Nullable reference types

**Enable nullable reference types in every project, and make nullable warnings errors.** Set `<Nullable>enable</Nullable>` and `<WarningsAsErrors>nullable</WarningsAsErrors>` in `Directory.Build.props`.

- Never use `#nullable disable` in new code.
- Use the null-forgiving operator `!` only where flow analysis can't see the value, such as a property a serializer or test setup fills in. Justify every `!` in review.
- Keep `ArgumentNullException.ThrowIfNull` at public API boundaries. Annotations exist only at compile time, so they don't stop unannotated or reflection-based callers.

([Microsoft Learn: Nullable reference types](https://learn.microsoft.com/dotnet/csharp/nullable-references))

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

Fix each diagnostic, or suppress it with a written justification. Never lower severity across the board.

([Microsoft Learn: Code analysis overview](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/overview), [Meziantou: The Roslyn analyzers I use](https://www.meziantou.net/the-roslyn-analyzers-i-use.htm), [Meziantou: Meziantou.Analyzer](https://github.com/meziantou/Meziantou.Analyzer))

### Smaller opinions

- **Take `Span<T>` and `ReadOnlySpan<T>` parameters in new APIs.** C# 14's implicit span conversions let callers pass arrays without an allocation. In .NET 10, span overloads win overload resolution in more cases, which is a breaking change. ([Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14), [Microsoft Learn: Breaking changes in .NET 10](https://learn.microsoft.com/dotnet/core/compatibility/10))
- **Use `nameof(List<>)` on unbound generics instead of hard-coded type names in diagnostics and exceptions.** ([Microsoft Learn: What's new in C# 14](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14))
- **Leave nested loops with a labelled `break` or `continue`, not a Boolean flag or a `goto`.** Put the label on the loop, as in `outer: for (...)`, and write `break outer;` or `continue outer;`. IDE0410 reports the flag and `goto` patterns that a labelled jump replaces. ([Microsoft Learn: What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15))
- **Use `StringComparison.Ordinal` for every machine-facing comparison, and `CultureInfo.InvariantCulture` for every machine-facing format or parse.** Never use `StringComparison.InvariantCulture`: its collation isn't invariant. For details, see [globalization.md](globalization.md).
- **Clone records with `with`, and never declare an instance `Clone()` method.** It conflicts with the compiler-generated clone. If you want a named method, wrap `with` in an extension method. ([Meziantou: Adding a Clone method to a C# record](https://www.meziantou.net/adding-a-clone-method-to-a-csharp-record.htm))

## Coming next (preview, not yet the opinion)

C# 15 includes a preview of new memory safety rules, planned to ship as a C# 16 feature in .NET 12. ([Lander: Improving C# Memory Safety](https://devblogs.microsoft.com/dotnet/improving-csharp-memory-safety/)) Under them, `unsafe` on a member means its callers must also be in an `unsafe` context, and declaring a pointer type no longer needs one. It takes `<LangVersion>preview</LangVersion>` and the `updated-memory-safety-rules` feature flag, so don't enable it in shipping code. For the plan, see [security.md](security.md#coming-next-preview-not-yet-the-opinion). ([Microsoft Learn: What's new in C# 15](https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15))
