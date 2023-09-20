namespace MicroFun.Generators.Scriban

open System
open System.Threading.Tasks
open Scriban
open Scriban.Runtime
open MicroFun.Generators

type internal ScribanTemplate(template: Template, baseUri: Uri) =
    interface ITemplate with
        member this.BaseUri = baseUri

        member this.RenderAsync(context, cancel): Task = 
            task {
                let scriptObject = ScriptObject.From(context.model)
                let templateContext = TemplateContext(scriptObject)
                let! result = template.RenderAsync(templateContext)
                do! context.writer.WriteAsync(result)
            }

[<AutoOpen>]
module internal ScribanTemplateUtils =
    let checkTemplate (template: Scriban.Template) =
        if template.HasErrors then
            let errors =
                template.Messages
                |> Seq.map (fun message -> $"{message.Type}: {message.Message} at {message.Span.Start.Line}:{message.Span.Start.Column}")
                |> String.concat Environment.NewLine
            invalidArg "content" errors


type ScribanTemplateFactory() =
    static member val Default: ITemplateFactory = ScribanTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                let! text = content.reader.ReadToEndAsync(cancel)
                let template = Template.Parse(text, content.baseUri.ToString())
                checkTemplate template
                return ScribanTemplate(template, content.baseUri) :> ITemplate
            }

type DotLiquidTemplateFactory() =
    static member val Default: ITemplateFactory = DotLiquidTemplateFactory()

    interface ITemplateFactory with
        member this.CreateTemplateAsync(content, cancel) =
            task {
                let! text = content.reader.ReadToEndAsync(cancel)
                let template = Template.ParseLiquid(text, content.baseUri.ToString())
                checkTemplate template
                return ScribanTemplate(template, content.baseUri) :> ITemplate
            }

[<RequireQualifiedAccessAttribute>]
module ScribanTemplate =
    open System.Text.RegularExpressions

    [<RequireQualifiedAccessAttribute>]
    module Scriban =
        let extensionRegex = Regex(@".+(\.(scriban|sbn))((-|\.|)\w+)?$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
        let contentTypeRegex = Regex(@"^(text|application)/(\w+\+)?(scriban|sbn)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)

    [<RequireQualifiedAccessAttribute>]
    module DotLiquid =
        let extensionRegex = Regex(@".+(\.(dot(-)?)?(liquid))((-|\.|)\w+)?$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
        let contentTypeRegex = Regex(@"^(text|application)/(\w+\+)?(dot(-)?)?(liquid)$", RegexOptions.Compiled ||| RegexOptions.IgnoreCase)
