---
targets: [net10.0, fsharp-10]
last-reviewed: 2026-09-14
last-used: 2026-09-14
sources: [dotnet-blog, mark-seemann, ms-learn, scott-wlaschin]
---

# F#

Target F# 10, which ships with .NET 10.

## Domain modelling

- **Make illegal states unrepresentable: model them as discriminated unions, not as combinations of flags and nullable fields.** When a value takes one of several shapes, such as cash, card, or direct debit, give the union one case per shape. The compiler then rejects every invalid combination, so you need no defensive checks or tests for states that can't exist. ([Wlaschin: Designing with types: Making illegal states unrepresentable](https://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/), and the whole [Wlaschin: Designing with types series](https://fsharpforfunandprofit.com/series/designing-with-types/))
- **Wrap domain primitives in single-case unions with a private constructor and a validating `create` function.** `CustomerId of int` can't be confused with `OrderId of int`, and a private `EmailAddress` case means every instance has passed validation. ([Wlaschin: Designing with types: Single case union types](https://fsharpforfunandprofit.com/posts/designing-with-types-single-case-dus/))
- **Return `Result` or `Option` from domain operations instead of throwing.** Model failure as data, and compose operations with `Result.bind` (railway-oriented programming). Throw for infrastructure faults, not for "customer not found". ([Wlaschin: Railway oriented programming](https://fsharpforfunandprofit.com/rop/), [Wlaschin: Domain Modeling Made Functional](https://fsharpforfunandprofit.com/ddd/))

```fsharp
type CustomerId = CustomerId of int

type EmailAddress = private EmailAddress of string

module EmailAddress =
    let create (s: string) =
        if s.Contains "@" then Some(EmailAddress s) else None

    let value (EmailAddress s) = s

type PaymentMethod =
    | Cash
    | Card of cardNumber: string * expiry: string
    | DirectDebit of iban: string

// Exhaustive match: adding a case is a compile error at every use site until handled
let describe payment =
    match payment with
    | Cash -> "cash"
    | Card(number, _) -> $"card ending {number.Substring(number.Length - 4)}"
    | DirectDebit iban -> $"direct debit from {iban}"
```

## Language opinions (F# 10)

- **Use `and!` in `task` expressions to await independent operations concurrently,** instead of sequential `let!` bindings. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

  ```fsharp
  let loadDashboard id =
      task {
          let! user = fetchUser id
          and! orders = fetchOrders id // starts before fetchUser completes
          return user, List.length orders
      }
  ```

- **On hot paths, mark optional parameters `[<Struct>]`,** so they compile to `ValueOption<'T>` and skip the heap allocation of `Option<'T>`. ([.NET Blog: Introducing F# 10](https://devblogs.microsoft.com/dotnet/introducing-fsharp-10/), [Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

  ```fsharp
  type Formatter =
      static member Pad(text: string, [<Struct>] ?width: int) =
          let w =
              match width with
              | ValueSome w -> w
              | ValueNone -> text.Length

          text.PadLeft w
  ```

- **Write `seq { ... }` explicitly.** F# 10 deprecates a bare brace block as a sequence expression and reports `FS3873: This construct is deprecated. Sequence expressions should be of the form 'seq { ... }'`. The warning is about the missing `seq`, not the contents. A comprehension inside `seq { ... }` is fine with or without `yield`. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

  ```fsharp
  // Before: FS3873
  let numbers: seq<int> = { 1..10 }

  // After
  let numbers = seq { 1..10 }

  let evenSquares =
      seq {
          for i in 1..10 do
              if i % 2 = 0 then
                  i * i
      }
  ```

- **Scope a warning suppression with a `#nowarn` and `#warnon` pair around the lines concerned.** Never leave a bare `#nowarn` that runs to the end of the file. ([.NET Blog: Introducing F# 10](https://devblogs.microsoft.com/dotnet/introducing-fsharp-10/))
- **Enable `ParallelCompilation` on multi-project F# solutions once it leaves preview.** It turns on graph-based type checking, parallel IL generation, and parallel optimization. In F# 10 it's still a preview feature, so turn it on now only if compile time is your bottleneck. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

## Mixed C#/F# solutions

- **Put the domain model and core business logic in F# projects. Hosts and framework-heavy edges, such as ASP.NET Core startup and UI shells, can stay in C#.** The domain is where the type-driven modelling above pays for F#. Project references work in both directions, so the C# host uses the F# domain like any other assembly. ([Microsoft Learn: F# component design guidelines](https://learn.microsoft.com/dotnet/fsharp/style-guide/component-design-guidelines))
- **Use F# projects to ban dependency cycles.** F# compiles files in declaration order and rejects circular dependencies between them, so a layering violation fails the build. Seemann calls this the strongest architectural argument for F#: C# relies on convention where F# enforces the rule. He credits C# with Razor integration, better editor tooling, and more mature complexity, coverage, and mutation testing tools. ([Seemann: Worse is better: C# versus F#](https://blog.ploeh.dk/2026/09/08/worse-is-better-c-versus-f/))
- **Keep F#-specific types off public APIs that C# consumes.** Inside F#, use `Option`, F# lists, and curried functions freely. At the boundary, expose namespaces with classes and tupled methods. Use `ValueOption` or nullable types, and `IReadOnlyList<'T>` or `seq`, so `FSharpOption` and `FSharpList` stay out of C# signatures. ([F# component design guidelines: Guidelines for libraries for use from other .NET languages](https://learn.microsoft.com/dotnet/fsharp/style-guide/component-design-guidelines))
- **Share `Directory.Build.props` and `Directory.Packages.props` from `templates/` across both languages.** F# projects then get the same TFM, Central Package Management, and CI setup. Central Package Management covers every project that uses `PackageReference`, including `.fsproj` files. ([Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management))

## Testing F#

**Use xUnit v3 for F# tests, the same stack as the rest of the solution.** Put `[<Fact>]` on a `let`-bound function, with no class, as the Microsoft Learn walkthrough shows. One stack shares fixtures, tooling, and CI configuration across C# and F#. For details, see [testing.md](testing.md). Expecto lost because a second runner and assertion style in one solution costs more than its tests-as-values model gives back. ([Microsoft Learn: Unit testing F# with dotnet test and xUnit](https://learn.microsoft.com/dotnet/core/testing/unit-testing-fsharp-with-dotnet-test))

```fsharp
module EmailAddressTests

open Xunit

[<Fact>]
let ``Create_StringWithoutAtSign_ReturnsNone`` () =
    // Arrange
    let input = "not-an-email"

    // Act
    let result = EmailAddress.create input

    // Assert
    Assert.Equal(None, result)
```

## Source redundancy

Independent F# sources are scarce:

- F# for Fun and Profit hasn't published since its two-part design series in December 2025. Its back catalogue on domain modelling and functional design is still the standard reference. The 2026-09-14 harvest flagged the gap for a `vet-source` pass.
- F# Weekly reports news, not argument.
- `mark-seemann` is independent and unmarked, and this file cites him only on F# versus C# trade-offs.

So the F# opinions lean on official sources more than the C# ones do.
