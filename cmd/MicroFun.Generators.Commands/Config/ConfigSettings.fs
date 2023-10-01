namespace MicroFun.Generators.Commands.Config

open System
open System.IO
open System.ComponentModel
open Spectre.Console
open Spectre.Console.Cli
open FsToolkit.ErrorHandling

open MicroFun.Generators.Commands

type ConfigSettings() =
    inherit CommandSettings()

    let isValidFileName (fileName: string) =
        let invalidChars = Path.GetInvalidFileNameChars()
        fileName.IndexOfAny(invalidChars) < 0


    [<CommandOption("--cwd")>]
    [<Description("Current working directory")>]
    member val CurrentWorkingDirectory = null with get, set

    member this.ValidateCurrentWorkingDirectory() =
        let cwd = 
            if String.IsNullOrWhiteSpace this.CurrentWorkingDirectory ||
               this.CurrentWorkingDirectory = "." then
                Environment.CurrentDirectory
            else
                Path.GetFullPath this.CurrentWorkingDirectory

        if Directory.Exists cwd then
            Ok cwd
        else
            Error "Current working directory does not exist"


    [<CommandOption("--global-config")>]
    [<Description("Global configuration folder. Default is user profile, where .mfg sub-folder must be located")>]
    member val GlobalConfigDirectory = null with get, set

    member this.ValidateGlobalConfigDirectory() =
        let cwd = 
            if String.IsNullOrWhiteSpace this.GlobalConfigDirectory then
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
            else
                Path.GetFullPath this.GlobalConfigDirectory

        Ok cwd


    [<CommandOption("--config-folder-name")>]
    [<Description("Configuration folder name")>]
    [<DefaultValue(".mfg")>]
    member val ConfigFolderName = null with get, set

    member this.ValidateConfigFolderName() =
        if String.IsNullOrWhiteSpace this.ConfigFolderName ||
           isValidFileName(this.ConfigFolderName.Trim()) then
            Ok (this.ConfigFolderName.Trim())
        else
            Error "Invalid configuration folder name"


    abstract ValidateSettings : unit -> Result<unit, string>
    default this.ValidateSettings() =
        result {
            let! _ = this.ValidateCurrentWorkingDirectory()
            let! _ = this.ValidateGlobalConfigDirectory()
            let! _ = this.ValidateConfigFolderName()
            // let! _ = this.ValidateLocalFolder()
            return ()
        }

    override this.Validate() = this.ValidateSettings() |> ValidationResult.ofResult


    abstract TryUpdateOptions: options: ConfigRepositoryOptions -> Result<unit, string>
    default this.TryUpdateOptions(options: ConfigRepositoryOptions) =
        result {
            let! currentWorkingDirectory = this.ValidateCurrentWorkingDirectory()
            let! globalConfigDirectory = this.ValidateGlobalConfigDirectory()
            let! configFolderName = this.ValidateConfigFolderName()

            options.CurrentWorkingDirectory <- currentWorkingDirectory
            options.GlobalConfigDirectory <- globalConfigDirectory
            options.ConfigFolderName <- configFolderName

            return ()
        }


    member this.UpdateOptions(options: ConfigRepositoryOptions) =
        this.TryUpdateOptions(options)
        |> Result.either id failwith


    member this.PropertyToAnsiConsole(console: IAnsiConsole, name: string, value: string) =
        console.MarkupLineInterpolated($"  - [gray]{name}: [/][white]{value}[/]")

    abstract ToAnsiConsole: console: IAnsiConsole -> unit
    default this.ToAnsiConsole(console: IAnsiConsole) =
        console.MarkupLine("[lime]Settings[/]")
        this.PropertyToAnsiConsole(console, "CurrentWorkingDirectory", this.CurrentWorkingDirectory)
        this.PropertyToAnsiConsole(console, "GlobalConfigDirectory", this.GlobalConfigDirectory)
        this.PropertyToAnsiConsole(console, "ConfigFolderName", this.ConfigFolderName)
