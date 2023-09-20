[<AutoOpen>]
module internal MicroFun.Generators.Preamble

open System

let defer dispose =
    { new IDisposable with member this.Dispose() = dispose() }
