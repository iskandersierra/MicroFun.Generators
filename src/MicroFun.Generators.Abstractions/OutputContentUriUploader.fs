namespace MicroFun.Generators

open System
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks


type IOutputContentUploader =
    abstract UploadContents : outputs: IReadOnlyCollection<IOutputContent> * cancel: CancellationToken -> Task
