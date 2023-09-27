[<AutoOpen>]
module MicroFun.Generators.Commands.Preamble

open Spectre.Console

[<RequireQualifiedAccess>]
module ValidationResult =
    let ofResult result =
        match result with
        | Ok _ -> ValidationResult.Success()
        | Error e -> ValidationResult.Error(e)
