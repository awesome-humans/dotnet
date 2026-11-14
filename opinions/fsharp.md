---
targets: [net11.0, fsharp-11]
last-reviewed: 2026-11-14
sources: [dotnet-blog, mark-seemann, ms-learn, scott-wlaschin]
---

# F#

Target F# 11, which ships with .NET 11.

## Domain modelling

- **Make illegal states unrepresentable: model them as discriminated unions, not as combinations of flags and nullable fields.** When a value takes one of several shapes, such as cash, card, or direct debit, give the union one case per shape. The compiler then rejects every invalid combination, so you need no defensive checks or tests for states that can't exist. ([Wlaschin: Designing with types: Making illegal states unrepresentable](https://fsharpforfunandprofit.com/posts/designing-with-types-making-illegal-states-unrepresentable/), and the whole [Wlaschin: Designing with types series](https://fsharpforfunandprofit.com/series/designing-with-types/))
- **Wrap domain primitives in single-case unions with a private constructor and a validating `create` function.** `CustomerId of int` can't be confused with `OrderId of int`. Wlaschin's post keeps the constructor private only by convention, "Of course the type is not really private in this case", and says "If you need to enforce encapsulation, use signature files". The `private` case in the sample below gets the compiler to enforce it without one, but only as far as the enclosing module: `private` means "the entity can be accessed only from the enclosing type or module". So give the type and its `EmailAddress` module a module of their own. Code outside it can't construct an `EmailAddress`, so every instance it sees has passed `create`. ([Wlaschin: Designing with types: Single case union types](https://fsharpforfunandprofit.com/posts/designing-with-types-single-case-dus/), [Microsoft Learn: Access control](https://learn.microsoft.com/dotnet/fsharp/language-reference/access-control))
- **Return `Result` or `Option` from domain operations instead of throwing.** Model failure as data, and compose operations with `Result.bind` (railway-oriented programming). ([Wlaschin: Railway oriented programming](https://fsharpforfunandprofit.com/posts/recipe-part2/)) Throw for infrastructure faults, not for "customer not found": `Result` is "only for expected control-flow, not for unexpected situations". When "you can't continue, don't return a Result and keep going. Fail fast instead with an exception". ([Wlaschin: Against Railway-Oriented Programming](https://fsharpforfunandprofit.com/posts/against-railway-oriented-programming/))

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
    | Card(number, _) -> $"card ending {number.Substring(max 0 (number.Length - 4))}"
    | DirectDebit iban -> $"direct debit from {iban}"
```

## Language opinions (F# 11)

- **Use `and!` in `task` expressions to await independent operations concurrently.** A sequence of `let!` bindings starts each operation only after the one before it completes. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10))

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

- **Write `seq { ... }` explicitly.** F# 10 deprecates a bare brace block as a sequence expression and reports `FS3873: This construct is deprecated. Sequence expressions should be of the form 'seq { ... }'`. The warning is about the missing `seq`, not the contents. ([Microsoft Learn: What's new in F# 10](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-10), [dotnet/fsharp: FSComp.txt](https://github.com/dotnet/fsharp/blob/d12022d6d0ec9c3355fbb3ae293f67c6babd2c84/src/Compiler/FSComp.txt#L1723), [fsharp/fslang-design: FS-1033 Deprecate places where seq can be omitted](https://github.com/fsharp/fslang-design/blob/main/FSharp-10.0/FS-1033-Deprecate-places-where-seq-can-be-omitted.md))

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
- **Remove `<ParallelCompilation>` from F# project files.** It was a preview opt-in in F# 10. In F# 11, "Parallel compilation is now enabled by default, with deterministic output and source-order rules preserved." ([Microsoft Learn: What's new in F# 11](https://learn.microsoft.com/dotnet/fsharp/whats-new/fsharp-11))

## Mixed C#/F# solutions

- **Put the domain model and core business logic in F# projects. Hosts and framework-heavy edges, such as ASP.NET Core startup and UI shells, can stay in C#.** The domain is where the type-driven modelling under [Domain modelling](#domain-modelling) applies. At the edges, Seemann credits C# with Razor integration and better editor tooling. ([Seemann: Worse is better: C# versus F#](https://blog.ploeh.dk/2026/09/08/worse-is-better-c-versus-f/))
- **Use F# projects to ban dependency cycles.** F# compiles files in declaration order and rejects circular dependencies between them, so a layering violation fails the build. Seemann calls this the strongest architectural argument for F#: C# relies on convention where F# enforces the rule. He also credits C# with more mature complexity, coverage, and mutation testing tools. ([Seemann: Worse is better: C# versus F#](https://blog.ploeh.dk/2026/09/08/worse-is-better-c-versus-f/))
- **Keep F#-specific types off public APIs that C# consumes.** Inside F#, use `Option`, F# lists, and curried functions freely. At the boundary, write what the guidelines call a vanilla .NET library, "minimizing the use of F#-specific constructs in the public API": expose namespaces with classes and tupled methods, and keep `FSharpOption` and `FSharpList` out of C# signatures. ([Microsoft Learn: F# component design guidelines](https://learn.microsoft.com/dotnet/fsharp/style-guide/component-design-guidelines))
- **Share `Directory.Build.props` and `Directory.Packages.props` from `templates/` across both languages.** F# projects then get the same TFM, Central Package Management, and CI setup. Central Package Management covers every project that uses `PackageReference`, including `.fsproj` files. ([Microsoft Learn: Central Package Management](https://learn.microsoft.com/nuget/consume-packages/central-package-management))

## Testing F#

**Use xUnit v3 for F# tests, the same stack as the rest of the solution.** Put `[<Fact>]` on a `let`-bound function, with no class. The Microsoft Learn walkthrough shows that form on xUnit v2, and it's the same on v3. One stack shares fixtures, tooling, and CI configuration across C# and F#. For details, see [testing.md](testing.md). Expecto lost because it puts a second runner and a second assertion style in the solution, and its tests-as-values model isn't worth maintaining both. ([Microsoft Learn: Unit testing F# with dotnet test and xUnit](https://learn.microsoft.com/dotnet/core/testing/unit-testing-fsharp-with-dotnet-test))

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
