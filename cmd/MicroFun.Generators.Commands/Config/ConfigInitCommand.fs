namespace MicroFun.Generators.Commands.Config

open Spectre.Console.Cli


type ConfigInitSettings() =
    inherit ConfigSettings()

    override this.ValidateSettings() =
        base.ValidateSettings()


type ConfigInitCommand() =
    inherit AsyncCommand<ConfigInitSettings>()

    override this.ExecuteAsync(context, settings) =
        task {
            return 0
        }
