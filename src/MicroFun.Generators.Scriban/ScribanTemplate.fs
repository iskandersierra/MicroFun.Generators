namespace MicroFun.Generators.Scriban

open System
open System.Collections.Generic
open System.IO
open System.Text
open System.Threading.Tasks
open Scriban
open Scriban.Runtime
open MicroFun.Generators


type internal ScribanTemplate(template: Template, baseUri: Uri) =
    let relativeUri = Uri("output.txt", UriKind.Relative)
    let contentType = "text/plain"
    let encoding = Encoding.UTF8

    let streamSink (model: obj) cancel (stream: Stream) =
        task {
            use writer =
                new StreamWriter(stream, encoding, leaveOpen = true)

            let scriptObject = ScriptObject()

            match model with
            | :? IDictionary<string, obj> as model ->
                for kvp in model do
                    scriptObject.Add(kvp.Key, kvp.Value)

            | model -> scriptObject.Import(model)

            let templateContext = TemplateContext()
            templateContext.PushGlobal(scriptObject)

            let! result = template.RenderAsync(templateContext)
            do! writer.WriteAsync(result.AsMemory(), cancel)
        }
        :> Task

    interface ITemplate with
        member this.BaseUri = baseUri

        member this.RenderAsync(context, cancel) =
            task {
                let output =
                    OutputContent.builder
                    |> OutputContent.withRelativeUri relativeUri
                    |> OutputContent.withContentType contentType
                    |> OutputContent.withEncoding encoding
                    |> OutputContent.withWriter (streamSink context.model)
                    |> OutputContent.build

                return output
            }

        member _.Dispose() = ()


module internal ScribanTemplateUtils =
    let checkTemplate (template: Scriban.Template) =
        if template.HasErrors then
            let errors =
                template.Messages
                |> Seq.map (fun message ->
                    $"{message.Type}: {message.Message} at {message.Span.Start.Line}:{message.Span.Start.Column}")
                |> String.concat Environment.NewLine

            invalidArg "content" errors


open ScribanTemplateUtils

type ScribanTemplateFactory() =
    static member val Default: ITemplateFactory = ScribanTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                let reader =
                    new StreamReader(content.Stream, content.Encoding, leaveOpen = true)

                let! text = reader.ReadToEndAsync(cancel)

                let template =
                    Template.Parse(text, content.BaseUri.ToString())

                checkTemplate template

                return new ScribanTemplate(template, content.BaseUri) :> ITemplate
            }


type DotLiquidTemplateFactory() =
    static member val Default: ITemplateFactory = DotLiquidTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                let reader =
                    new StreamReader(content.Stream, content.Encoding, leaveOpen = true)

                let! text = reader.ReadToEndAsync(cancel)

                let template =
                    Template.ParseLiquid(text, content.BaseUri.ToString())

                checkTemplate template

                return new ScribanTemplate(template, content.BaseUri) :> ITemplate
            }


[<RequireQualifiedAccessAttribute>]
module ScribanTemplate =
    open System.Text.RegularExpressions

    [<RequireQualifiedAccessAttribute>]
    module Scriban =
        let extensionRegex =
            Regex(@".+(\.(scriban|sbn))((-|\.|)\w+)?$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

        let contentTypeRegex =
            Regex(@"^(text|application)/(\w+\+)?(scriban|sbn)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

        let configSelector =
            SequentialSelector.withFactoryRegexes ScribanTemplateFactory.Default [ extensionRegex; contentTypeRegex ]

    [<RequireQualifiedAccessAttribute>]
    module DotLiquid =
        let extensionRegex =
            Regex(@".+(\.(dot(-)?)?(liquid))((-|\.|)\w+)?$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

        let contentTypeRegex =
            Regex(@"^(text|application)/(\w+\+)?(dot(-)?)?(liquid)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

        let configSelector =
            SequentialSelector.withFactoryRegexes DotLiquidTemplateFactory.Default [ extensionRegex; contentTypeRegex ]
