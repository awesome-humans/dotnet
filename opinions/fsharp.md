---
targets: [net10.0, fsharp-10]
last-reviewed: 2026-09-14
last-used: 2026-09-14
sources: [dotnet-blog, ms-learn, scott-wlaschin, mark-seemann]
---

# F#

Target F# 10, which ships with .NET 10. This repository treats F# as a first-class language, with opinions that stand alongside the C# ones.

## Domain modelling

- **Make illegal states unrepresentable: model states as discriminated unions, not as combinations of flags and nullable fields.** When a value can take one of several shapes, such as cash, card, or direct debit, or a verified or unverified email, give the union one case per shape. The compiler then rejects every invalid combination, and you don't need defensive checks or unit tests for states that can't exist. ([Wlaschin: Designing with types: Making illegal states unrepresentable](https://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/), and the whole [Wlaschin: Designing with types series](https://fsharpforfunandprofit.com/series/designing-with-types/))
- **Wrap domain primitives in single-case unions, with a private constructor and a `create` function that validates.** `CustomerId of int` can't be confused with `OrderId of int`, and a private `EmailAddress` case guarantees that every instance in the system has passed validation. ([Wlaschin: Designing with types: Single case union types](https://fsharpforfunandprofit.com/posts/designing-with-types-single-case-dus/))
- **Return `Result` or `Option` from domain operations instead of throwing.** Model the failure path as data, and compose operations with `Result.bind`, an approach called railway-oriented programming. Use exceptions for exceptional cases, such as infrastructure faults, not for "customer not found". ([Wlaschin: Railway oriented programming](https://fsharpforfunandprofit.com/rop/), [Wlaschin: Domain Modeling Made Functional](https://fsharpforfunandprofit.com/ddd/))

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

- **Use `and!` in `task` expressions to await independent operations concurrently,** instead of a chain of sequential `let!` bindings. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

  ```fsharp
  let loadDashboard id =
      task {
          let! user = fetchUser id
          and! orders = fetchOrders id // starts before fetchUser completes
          return user, List.length orders
      }
  ```

- **Mark optional parameters `[<Struct>]` on hot paths,** so they compile to `ValueOption<'T>` and avoid the heap allocation of `Option<'T>`. ([.NET Blog: Introducing F# 10](https://devblogs.microsoft.com/dotnet/introducing-fsharp-10/), [Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

  ```fsharp
  type Formatter =
      static member Pad(text: string, [<Struct>] ?width: int) =
          let w =
              match width with
              | ValueSome w -> w
              | ValueNone -> text.Length

          text.PadLeft w
  ```

- **Write `seq { ... }` explicitly.** A brace block on its own is a deprecated sequence expression, and F# 10 reports `FS3873: This construct is deprecated. Sequence expressions should be of the form 'seq { ... }'`. The warning is about the braces, not their contents. A comprehension already inside `seq { ... }` is correct, with or without an explicit `yield`. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

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

- **Scope warning suppression with a `#nowarn` and `#warnon` pair** around the exact lines concerned. Never leave a bare `#nowarn` that suppresses to the end of the file. ([.NET Blog: Introducing F# 10](https://devblogs.microsoft.com/dotnet/introducing-fsharp-10/))
- **Enable `ParallelCompilation` on multi-project F# solutions** for graph-based type checking, parallel IL generation, and parallel optimization. It's in preview in F# 10, so adopt it when it leaves preview, or now if build time is a problem. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

## Mixed C#/F# solutions

- **Put the domain model and core business logic in F# projects. Hosts and framework-heavy edges, such as ASP.NET Core startup and UI shells, can stay in C#.** The type-driven modelling above is where F# repays its cost. Project references work in both directions, so the C# host uses the F# domain like any other assembly. ([Microsoft Learn: F# component design guidelines](https://learn.microsoft.com/dotnet/fsharp/style-guide/component-design-guidelines))
- **Use the F# projects to ban dependency cycles.** F# compiles files in declaration order and doesn't allow circular dependencies between them. A layering violation fails the build, instead of waiting for a reviewer to notice it. Seemann rates this as the strongest architectural argument for the language: C# relies on convention for what F# enforces by design. He also weighs the cost, and credits C# with the Razor integration, the editor tooling, and more mature complexity, coverage, and mutation testing tools. ([Seemann: Worse is better: C# versus F#](https://blog.ploeh.dk/2026/09/08/worse-is-better-c-versus-f/))
- **Keep F#-specific types off public APIs that C# consumes.** Inside F# projects, use `Option`, F# lists, and curried functions freely. At the boundary, expose namespaces with classes and tupled methods. Use `ValueOption` or nullable types, and `IReadOnlyList<'T>` or `seq`, so `FSharpOption` and `FSharpList` don't leak into C# signatures. ([F# component design guidelines: Guidelines for libraries for use from other .NET languages](https://learn.microsoft.com/dotnet/fsharp/style-guide/component-design-guidelines))
- **Share the standard `Directory.Build.props` and `Directory.Packages.props` from `templates/` across both languages.** F# projects get the same TFM, Central Package Management, and CI setup, with no separate build conventions. Central Package Management applies to every project that uses `PackageReference`, including `.fsproj` files. ([Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management))

## Testing F#

**Use xUnit v3 for F# test projects, the same stack as the rest of the solution.** xUnit fully supports F#: you put `[<Fact>]` on a `let`-bound function, with no class required, as the F# walkthrough on Microsoft Learn shows. One test stack means shared fixtures, tooling, and CI configuration across a mixed C# and F# solution. For details, see [testing.md](testing.md). Expecto's tests-as-values model is elegant, but a second runner and assertion style in the same solution costs more than it returns. ([Microsoft Learn: Unit testing F# with dotnet test and xUnit](https://learn.microsoft.com/dotnet/core/testing/unit-testing-fsharp-with-dotnet-test))

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

## Source-redundancy note

There are few independent F# sources:

- F# for Fun and Profit hasn't published since its two-part design series in December 2025. Its back catalogue on domain modelling and functional design is still the canonical reference. The roster records the gap against `scott-wlaschin` for the next `vet-source` pass.
- F# Weekly reports ecosystem news, not argument.
- `mark-seemann` is the one addition here: independent and unmarked, though he writes more about the language than in it.

So the F# opinions rely more on official sources than the C# opinions do.
