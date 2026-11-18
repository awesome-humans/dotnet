// awesome-humans/dotnet template | targets: net11.0, fsharp-11 | last-reviewed: 2026-11-14 | sources: ms-learn, scott-wlaschin
// Domain model example: make illegal states unrepresentable (opinions/fsharp.md).
namespace Example.Library.FSharp

/// Single-case union: a CustomerId can't be confused with any other int.
type CustomerId = CustomerId of int

/// A module of their own for the type and its functions: `private` stops code outside this
/// module, so every EmailAddress in the system has already passed create. Declared straight
/// in the namespace, the case would be open to every module of the namespace.
module Email =
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
