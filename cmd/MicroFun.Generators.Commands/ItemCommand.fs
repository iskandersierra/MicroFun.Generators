namespace MicroFun.Generators.Commands

open System
open System.ComponentModel
open System.IO
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Spectre.Console
open Spectre.Console.Cli
open FsToolkit.ErrorHandling

open MicroFun.Generators

[<RequireQualifiedAccess>]
type ItemCommandModel =
    | Single of obj
    | Multiple of Map<string, obj>

type ItemCommandRequest =
    { getTemplateSource: CancellationToken -> Task<TextReader * IDisposable>
      getModel: CancellationToken -> Task<ItemCommandModel> }

type ItemCommandService() =
    class
    end

type ItemSettings(http: Lazy<HttpClient>) =
    inherit CommandSettings()

    [<CommandArgument(0, "<template>")>]
    [<Description("Template source file or url. Use schemes 'http/s', 'file' or local file")>]
    member val TemplateSource = Unchecked.defaultof<string> with get, set

    [<CommandOption("-m|--model")>]
    [<Description("Model source file or url")>]
    member val ModelSources = Array.empty<string> with get, set

    [<CommandOption("-n|--model-name")>]
    [<Description("Model name")>]
    member val ModelNames = Array.empty<string> with get, set

    [<CommandOption("-f|--model-format")>]
    [<Description("Model name")>]
    member val ModelFormats = Array.empty<ModelFormatEnum> with get, set

    [<CommandOption("-t|--model-type")>]
    [<Description("Model type")>]
    member val ModelTypes = Array.empty<string> with get, set

    [<CommandOption("-k|--key")>]
    [<Description("Key")>]
    member val Keys = Array.empty<string> with get, set

    [<CommandOption("-v|--value")>]
    [<Description("Value")>]
    member val Values = Array.empty<string> with get, set

    member this.GetTemplateSource() =
        if String.IsNullOrWhiteSpace this.TemplateSource then
            Error("You must specify a template source.")
        else
            let getTemplateSource cancel =
                getSourceReader cancel http this.TemplateSource

            Ok getTemplateSource

    member this.GetModel() =
        let fromKeyValues () =
            match this.Keys with
            | [||] ->
                match this.Values with
                | [||] ->
                    let getModel (_: CancellationToken) = ItemCommandModel.Single(obj ()) |> Task.FromResult
                    Ok getModel

                | _ -> Error $"When no key is given, value most not be provided"

            | keys ->
                match this.Values with
                | values when values.Length = keys.Length ->
                    let duplicates = keys |> Seq.duplicates |> Seq.toArray

                    match duplicates with
                    | [||] ->
                        let getModel (_: CancellationToken) =
                            let model =
                                Seq.zip keys (values |> Seq.cast<obj>) |> dict

                            ItemCommandModel.Single(model) |> Task.FromResult

                        Ok getModel

                    | _ ->
                        let ls = duplicates |> String.concat ", "
                        Error $"Found duplicate keys: {ls}"

                | values -> Error $"Found {keys.Length} keys, but found {values.Length} values"


        match this.ModelSources with
        | [||] ->
            match this.ModelNames, this.ModelFormats, this.ModelTypes with
            | [||], [||], [||] -> fromKeyValues ()

            | modelNames, modelFormats, modelTypes ->
                let ls =
                    seq {
                        if modelNames.Length > 0 then
                            yield "model-name"

                        if modelFormats.Length > 0 then
                            yield "model-format"

                        if modelTypes.Length > 0 then
                            yield "model-type"
                    }
                    |> String.concat ", "

                Error $"When no model is given, {ls} most not be provided"

        | modelSources ->
            if modelSources.Length > 1
               && this.ModelNames.Length <> modelSources.Length then
                Error $"When multiple models are given, model-name most be provided for each model"
            elif this.ModelNames.Length > modelSources.Length then
                Error $"Found {this.ModelNames.Length} model-names, but only {modelSources.Length} models were found"
            elif this.ModelFormats.Length > modelSources.Length then
                Error
                    $"Found {this.ModelFormats.Length} model-formats, but only {modelSources.Length} models were found"
            elif this.ModelTypes.Length > modelSources.Length then
                Error $"Found {this.ModelTypes.Length} model-types, but only {modelSources.Length} models were found"
            else
                let duplicates =
                    this.ModelNames |> Seq.duplicates |> Seq.toArray

                match duplicates with
                | [||] ->
                    let getModel (cancel: CancellationToken) =
                        task {
                            let! models =
                                modelSources
                                |> Seq.mapi (fun index modelSource ->
                                    let modelFormat =
                                        this.ModelFormats
                                        |> Array.tryItem index
                                        |> Option.orElse (Array.tryLast this.ModelFormats)
                                        |> Option.defaultValue ModelFormatEnum.Json

                                    let modelType =
                                        this.ModelTypes
                                        |> Array.tryItem index
                                        |> Option.orElse (Array.tryLast this.ModelTypes)
                                        |> Option.defaultValue ""

                                    readModel cancel http modelSource modelFormat modelType)
                                |> Task.whenAll

                            if this.ModelNames.Length = 0 then
                                return ItemCommandModel.Single models.[0]
                            else
                                let map =
                                    Seq.zip this.ModelNames models |> Map.ofSeq

                                return ItemCommandModel.Multiple map
                        }

                    Ok getModel

                | _ ->
                    let ls = duplicates |> String.concat ", "
                    Error $"Found duplicate model-names: {ls}"


    override this.Validate() =
        result {
            let! _ = this.GetTemplateSource()
            let! _ = this.GetModel()
            return ()
        }
        |> function
            | Ok _ -> ValidationResult.Success()
            | Error message -> ValidationResult.Error(message)


type ItemCommand(http: Lazy<HttpClient>) =
    inherit AsyncCommand<ItemSettings>()

    override this.ExecuteAsync(context, settings) =
        task {
            Console.WriteLine("Validated")
            return 0
        }
