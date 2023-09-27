namespace MicroFun.Generators.Commands

open Spectre.Console

open MicroFun.Generators

type OutputContentSpecterConsoleUploader() =
    interface IOutputContentUploader with
        member this.UploadContents(outputs, cancel) =
            task {
                for output in outputs do
                    AnsiConsole.MarkupLine $"[white]---[/]"
                    AnsiConsole.MarkupLine $"[lime]Path[/]: [white]{output.RelativeUri}[/]"
                    AnsiConsole.MarkupLine $"[lime]Content-Type[/]: [white]{output.ContentType}; {output.Encoding.WebName}[/]"
                    AnsiConsole.MarkupLine $"[white]---[/]"

                    let! text = output |> OutputContent.toText

                    AnsiConsole.WriteLine text
            }
