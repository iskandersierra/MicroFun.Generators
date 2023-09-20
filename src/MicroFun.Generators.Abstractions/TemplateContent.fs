namespace MicroFun.Generators

open System
open System.IO

type TemplateContent =
    { baseUri: Uri
      contentType: string
      reader: TextReader }

[<RequireQualifiedAccess>]
module TemplateContent =
    [<Literal>]
    let DefaultContentType = "text/plain"

    [<Literal>]
    let DefaultBaseUri = "urn:"

    let empty() = {
        TemplateContent.baseUri = Uri(DefaultBaseUri)
        contentType = DefaultContentType
        reader = new StringReader("") }

    let create baseUri contentType reader =
        { baseUri = Uri(baseUri)
          contentType = contentType
          reader = reader }

    let createText baseUri contentType text =
        create baseUri contentType (new StringReader(text))
        

    let withBaseUri baseUri this = { this with baseUri = baseUri }

    let withContentType contentType this = { this with contentType = contentType }

    let withReader reader this = { this with reader = reader }

    let withText text = withReader (new StringReader(text))

    let ofReader reader = empty() |> withReader reader

    let ofText text = empty() |> withText text
