// dotnet-awesome-humans template | targets: net10.0, fsharp-10 | last-reviewed: 2026-08-21 | last-used: 2026-08-21 | sources: scott-wlaschin
// Domain model example: make illegal states unrepresentable (opinions/fsharp.md).
namespace Example.Library.FSharp

/// Single-case union: a CustomerId can't be confused with any other int.
type CustomerId = CustomerId of int

/// Private constructor: every EmailAddress in the system has already passed create.
type EmailAddress = private EmailAddress of string

module EmailAddress =
    let create (s: string) =
        if s.Contains "@" then Some(EmailAddress s) else None

    let value (EmailAddress s) = s

/// One case per shape: the compiler rejects every invalid combination.
type PaymentMethod =
    | Cash
    | Card of cardNumber: string * expiry: string
    | DirectDebit of iban: string
