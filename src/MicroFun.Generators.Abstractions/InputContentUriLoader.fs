namespace MicroFun.Generators

open System
open System.Threading
open System.Threading.Tasks


type IInputContentUriLoader =
    abstract LoadContent : uri: Uri * cancel: CancellationToken -> Task<IInputContent>
