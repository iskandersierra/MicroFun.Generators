namespace MicroFun.Generators

open System.Threading
open System.Threading.Tasks


type IInputModelSource =
    abstract GetModel : cancel: CancellationToken -> Task<obj>


type ConstantInputModelSource(model: obj) =
    interface IInputModelSource with
        member _.GetModel cancel = Task.FromResult model


type IInputModelLoader =
    abstract LoadModel: input: IInputContent * cancel: CancellationToken -> Task<obj>
