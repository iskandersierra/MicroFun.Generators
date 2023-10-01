namespace MicroFun.Generators.Commands.Config

open System
open System.ComponentModel
open Spectre.Console
open Spectre.Console.Cli
open Spectre.Console.Json
open FsToolkit.ErrorHandling


type ConfigShowSettings() =
    inherit ConfigSettings()


type ConfigShowCommand
    (
        options: ConfigRepositoryOptions,
        configRepository: IConfigRepository
    ) =
    inherit AsyncCommand<ConfigShowSettings>()

    let printFolders (folders: ConfigFolderInfo seq) =
        let table =
            Table()
                .AddColumns("Exists", "Role", "Folder")
                .SimpleBorder()
                .HideHeaders()

        for folder in folders do
            let roleStyle, roleText =
                match folder.configType with
                | ConfigFolderType.Global -> Style(foreground = Color.Aqua), "global"
                | ConfigFolderType.Local -> Style(foreground = Color.Yellow), "local"
                | ConfigFolderType.Intermediate -> Style(foreground = Color.White), "interm."
                | _ -> Style(foreground = Color.Grey), "unk."

            let role = Markup(roleText, roleStyle)

            let folderPath =
                Markup.FromInterpolated($"[underline]{folder.folder}[/]", style = roleStyle)

            let exists =
                if folder.FolderExists then
                    Markup("[lime]✓[/]")
                else
                    Markup("[red]✗[/]")

            table.AddRow(exists, role, folderPath) |> ignore

        AnsiConsole.Console.Write(table);

    override this.ExecuteAsync(context, settings) =
        task {
            //settings.ToAnsiConsole(AnsiConsole.Console)

            //AnsiConsole.MarkupLine("[lime]Remaining arguments RAW[/]")
            //for arg in context.Remaining.Raw do
            //    AnsiConsole.MarkupLine($"  - [white]{arg}[/]")

            //AnsiConsole.MarkupLine("[lime]Remaining arguments PARSED[/]")
            //for group in context.Remaining.Parsed do
            //    AnsiConsole.MarkupLine($"  - [white]{group.Key}[/]")
            //    for item in group do
            //        AnsiConsole.MarkupLine($"    - [grey]{item}[/]")

            settings.UpdateOptions options

            //options.ToAnsiConsole(AnsiConsole.Console)

            let! folders = configRepository.GetConfigFolders()

            printFolders folders

            return 0
        }
