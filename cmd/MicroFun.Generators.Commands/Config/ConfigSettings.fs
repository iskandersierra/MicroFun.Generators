namespace MicroFun.Generators.Commands.Config

open System
open System.ComponentModel
open Spectre.Console.Cli
open FsToolkit.ErrorHandling

open MicroFun.Generators.Commands

type ConfigSettings() =
    inherit CommandSettings()

    [<CommandOption("--local-folder")>]
    [<Description("Local configuration folder")>]
    [<DefaultValue(".mfg")>]
    member val LocalFolder = Unchecked.defaultof<string> with get, set

    member this.ValidateLocalFolder() =
        if String.IsNullOrWhiteSpace(this.LocalFolder) then
            Error "Local folder cannot be empty"
        else
            Ok this.LocalFolder

    abstract ValidateSettings : unit -> Result<unit, string>
    default this.ValidateSettings() =
        result {
            let! _ = this.ValidateLocalFolder()
            return ()
        }

    override this.Validate() = this.ValidateSettings() |> ValidationResult.ofResult
