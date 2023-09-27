namespace MicroFun.Generators.Commands.Config

open System
open System.ComponentModel
open Spectre.Console.Cli
open FsToolkit.ErrorHandling


type ConfigShowOutputFormat =
    | Human = 0
    | Json = 1
    | Yaml = 2


type ConfigShowSettings() =
    inherit ConfigSettings()

    [<CommandOption("-o|--output-format")>]
    [<Description( "Output format")>]
    [<DefaultValue(ConfigShowOutputFormat.Human)>]
    member val OutputFormat = ConfigShowOutputFormat.Human with get, set

    member this.ValidateOutputFormat() =
        if Enum.IsDefined<ConfigShowOutputFormat>(this.OutputFormat) |> not then
            Error "Invalid output format"
        else
            Ok this.OutputFormat

    override this.ValidateSettings() =
        base.ValidateSettings()
        |> Result.bind (fun () ->
            result {
                let! _ = this.ValidateOutputFormat()
                return ()
            }
        )


type ConfigShowCommand() =
    inherit AsyncCommand<ConfigShowSettings>()

    override this.ExecuteAsync(context, settings) =
        task {
            return 0
        }
