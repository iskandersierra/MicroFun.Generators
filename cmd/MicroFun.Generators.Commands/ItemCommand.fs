namespace MicroFun.Generators.Commands

open System
open System.Net
open System.Net.Http
open System.IO
open System.ComponentModel
open Spectre.Console
open Spectre.Console.Cli


type ItemSettings() =
    inherit CommandSettings()

    [<CommandOption("-f|--file")>]
    [<Description("Template source file")>]
    member val TemplateFile = Unchecked.defaultof<string> with get, set

    [<CommandOption("-u|--url")>]
    [<Description("Template source URL")>]
    member val TemplateUrl = Unchecked.defaultof<string> with get, set

    override this.Validate() =
        let templateInputCount =
            [ this.TemplateFile; this.TemplateUrl ]
            |> List.filter (fun x -> x <> null)
            |> List.length

        if templateInputCount <> 1 then
            ValidationResult.Error("You must specify exactly one template file.")
        else
            ValidationResult.Success()


type ItemCommand(http: Lazy<HttpClient>) =
    inherit AsyncCommand<ItemSettings>()

    override this.ExecuteAsync(context, settings) =
        let loadContent() =
            task {
                if settings.TemplateFile <> null then
                    let filePath = Path.GetFullPath(settings.TemplateFile)
                    let fileInfo = FileInfo(filePath)
                    if not fileInfo.Exists then
                        return failwithf "Template file '%s' does not exist." filePath
                    else
                        return! File.ReadAllTextAsync(filePath)
                else
                    let! response = http.Value.GetAsync(settings.TemplateUrl)
                    response.EnsureSuccessStatusCode() |> ignore
                    let! content = response.Content.ReadAsStringAsync()
                    return content
            }

        task {
            let! content = loadContent()
            Console.WriteLine(content)
            return 0
        }


