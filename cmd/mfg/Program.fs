open System
open System.Linq
open MicroFun.Generators.Commands

let services = MfgApp.defaultServices()

let app = MfgApp.create(services)

let args = Environment.GetCommandLineArgs().Skip(1)

Environment.Exit(app.Run args)
