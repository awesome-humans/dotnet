// awesome-humans/dotnet template | targets: net10.0, fsharp-10 | last-reviewed: 2026-08-23 | sources: ms-learn, scott-wlaschin
// Library surface example: exhaustive matching and explicit seq (opinions/fsharp.md).
module Example.Library.FSharp.Payments

/// Exhaustive match: adding a case is a compile error at every use site until handled.
let describe payment =
    match payment with
    | Cash -> "cash"
    | Card(number, _) -> $"card ending {number.Substring(number.Length - 4)}"
    | DirectDebit iban -> $"direct debit from {iban}"

/// seq { ... } written explicitly: F# 10 deprecates a brace block without it (FS3873).
/// The warning is about the missing seq, not the comprehension, so the body needs no yield.
let evenSquares upTo =
    seq {
        for i in 1..upTo do
            if i % 2 = 0 then
                i * i
    }
