module MicroFun.Generators.Abstractions.Tests.OutputContentTests

open System
open System.IO
open System.Text
open System.Threading
open System.Threading.Tasks
open Xunit
open Swensen.Unquote

open MicroFun.Generators


let relativeUri = Uri("example.txt", UriKind.Relative)


[<Fact>]
let ``OutputContent.tryBuild should fail if relativeUri not specified`` () =
    let builder =
        OutputContent.builder
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok _ -> Assert.Fail "Expected Error, but got Ok"
    | Error _ -> ()

[<Fact>]
let ``OutputContent.tryBuild should use given relativeUri if specified`` () =
    let builder =
        OutputContent.builder
        |> OutputContent.withRelativeUri relativeUri
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok outputContent -> test <@ outputContent.RelativeUri = relativeUri @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``OutputContent.tryBuild should set default contentType if not specified`` () =
    let builder =
        OutputContent.builder
        |> OutputContent.withRelativeUri relativeUri
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok outputContent -> test <@ outputContent.ContentType = OutputContent.DefaultContentType @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``OutputContent.tryBuild should use given contentType if specified`` () =
    let contentType = "text/html"

    let builder =
        OutputContent.builder
        |> OutputContent.withContentType contentType
        |> OutputContent.withRelativeUri relativeUri
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok outputContent -> test <@ outputContent.ContentType = contentType @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``OutputContent.tryBuild should set default encoding if not specified`` () =
    let builder =
        OutputContent.builder
        |> OutputContent.withRelativeUri relativeUri
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok outputContent -> test <@ outputContent.Encoding = OutputContent.DefaultEncoding @>
    | Error error -> Assert.Fail $"Expected Ok, but got Error {error}"

[<Fact>]
let ``OutputContent.tryBuild should use given encoding if specified`` () =
    let encoding = Encoding.ASCII

    let builder =
        OutputContent.builder
        |> OutputContent.withEncoding encoding
        |> OutputContent.withRelativeUri relativeUri
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok outputContent -> test <@ outputContent.Encoding = encoding @>
    | Error _ -> Assert.Fail "Expected Ok"

[<Fact>]
let ``OutputContent.tryBuild should fail if no writer is specified`` () =
    let builder =
        OutputContent.builder
        |> OutputContent.withRelativeUri relativeUri

    let outputContentResult = OutputContent.tryBuild builder

    match outputContentResult with
    | Ok _ -> Assert.Fail "Expected Error"
    | Error _ -> ()

[<Fact>]
let ``OutputContent.build should use writer when Write is called`` () =
    task {
        let text = "Hello world!"

        let builder =
            OutputContent.builder
            |> OutputContent.withRelativeUri relativeUri
            |> OutputContent.withWriter (fun cancel stream ->
                task {
                    use writer = new StreamWriter(stream, leaveOpen = true)
                    do! writer.WriteAsync(text.AsMemory(), cancel)
                    return ()
                } :> Task)

        let outputContent = OutputContent.build builder

        let! result = outputContent |> OutputContent.toText

        test <@ result = text @>
    }

[<Fact>]
let ``OutputContent.build should return if builder is correct`` () =
    let builder =
        OutputContent.builder
        |> OutputContent.withRelativeUri relativeUri
        |> OutputContent.withWriter (fun _ _ -> Task.CompletedTask)

    let outputContent = OutputContent.build builder

    test <@ outputContent.ContentType = OutputContent.DefaultContentType @>

[<Fact>]
let ``OutputContent.build should fail if builder is incorrect`` () =
    let builder = OutputContent.builder

    let exn =
        Assert.Throws<exn>(fun () -> OutputContent.build builder |> ignore)

    ()
