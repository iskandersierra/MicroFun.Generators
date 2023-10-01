[<AutoOpen>]
module MicroFun.Generators.Commands.ConfigExtensions

open Microsoft.Extensions.DependencyInjection
open Spectre.Console.Cli

open MicroFun.Generators.Commands.Config


type IConfigurator with
    member this.AddConfigCommands () =
        this
            .AddCommand<ConfigShowCommand>("config")
            .WithDescription("Shows the current configuration")
        |> ignore

        this
            .AddCommand<ConfigInitCommand>("init")
            .WithDescription("Initializes the local configuration")
        |> ignore

        this


type IServiceCollection with
    member this.AddConfigServices () =
        this
            .AddSingleton(ConfigRepositoryOptions())
            .AddSingleton<IConfigRepository, ConfigRepository>()
