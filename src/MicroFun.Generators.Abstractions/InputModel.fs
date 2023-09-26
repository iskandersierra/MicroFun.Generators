namespace MicroFun.Generators

open System
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks

open FsToolkit.ErrorHandling


type IInputModel =
    abstract Load : CancellationToken -> Task<obj>

[<RequireQualifiedAccess>]
module InputModel =
    let ofObj (model: obj) =
        { new IInputModel with
            member _.Load cancel = Task.FromResult model }


type InputModelSource = CancellationToken -> IInputContent -> Task<obj>
