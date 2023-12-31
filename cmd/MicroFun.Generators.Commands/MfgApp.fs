namespace MicroFun.Generators.Commands

open System
open System.Net.Http
open Microsoft.Extensions.DependencyInjection
open Spectre.Console
open Spectre.Console.Cli

open MicroFun.Generators
open MicroFun.Generators.Json
open MicroFun.Generators.Scriban

module MfgApp =
    type TypeResolver(provider: IServiceProvider) =
        interface ITypeResolver with
            member _.Resolve service =
                match provider.GetService(service) with
                | null -> ActivatorUtilities.CreateInstance(provider, service)
                | result -> result

        interface IDisposable with
            member _.Dispose() =
                match provider with
                | :? IDisposable as disposable -> disposable.Dispose()
                | _ -> ()

    type TypeRegistrar(services: IServiceCollection) =
        interface ITypeRegistrar with
            member _.Build() =
                let provider = services.BuildServiceProvider()
                new TypeResolver(provider)

            member _.Register(service, implementation) =
                services.AddSingleton(service, implementation)
                |> ignore

            member _.RegisterInstance(service, implementation) =
                services.AddSingleton(service, implementation)
                |> ignore

            member _.RegisterLazy(service, factory) =
                services.AddSingleton(service, (fun _ -> factory.Invoke()))
                |> ignore


    let create (services: IServiceCollection) =
        let registrar = TypeRegistrar(services)
        let app = CommandApp(registrar)

        app.Configure (fun mfg ->
            mfg.SetApplicationName "mfg" |> ignore

            mfg.SetExceptionHandler (fun ex ->
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything)
                -1)
            |> ignore

            mfg
                .AddConfigCommands()
            |> ignore)

        app

    let defaultServices () =
        let services = ServiceCollection()

        services
            .AddSingleton<HttpClient>()

            .AddSingleton<ITemplateFactory>(fun _ ->
                SequentialSelector.builder
                |> ScribanTemplate.Scriban.configSelector
                |> ScribanTemplate.DotLiquid.configSelector
                |> SequentialSelector.build
                |> SelectorTemplateFactory
                :> ITemplateFactory)

            .AddSingleton<IInputContentUriLoader>(fun _ ->
                InputContentFileLoader.Default
                :> IInputContentUriLoader)

            .AddSingleton<IInputModelLoader>(fun _ ->
                InputModelDynamicJsonLoader()
                :> IInputModelLoader)

            .AddSingleton<IOutputContentUploader>(fun _ ->
                OutputContentSpecterConsoleUploader()
                :> IOutputContentUploader)

            .AddConfigServices()
        |> ignore

        services
