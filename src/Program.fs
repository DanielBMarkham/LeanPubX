open System
open Markdig

let pipelineBuilder = new MarkdownPipelineBuilder()
let pipeline = (pipelineBuilder.UseAdvancedExtensions()).Build()
let booksFileName = "Book.txt"
let directorySeparator = string System.IO.Path.DirectorySeparatorChar
let sourceDirectory =
  try System.Environment.GetCommandLineArgs().[1] + string directorySeparator with |_->System.Environment.CurrentDirectory
let targetDirectory =
  try System.Environment.GetCommandLineArgs().[2] + string directorySeparator with |_->System.Environment.CurrentDirectory

let inputStuff (argv:string []) =
  let filesToWork =
    try
      System.IO.File.ReadLines(sourceDirectory + string System.IO.Path.DirectorySeparatorChar + booksFileName ) |> Seq.toArray
    with |_-> Array.empty

  let filesToProcess =
    try
      filesToWork
        |> Array.map(fun filename->
          (
            let fileItemName=sourceDirectory + string System.IO.Path.DirectorySeparatorChar + filename
            (
              System.IO.FileInfo(fileItemName)
              ,System.IO.File.ReadAllText(fileItemName)
            )
          )
        )
    with |_->Array.empty
  filesToProcess  

let processStuff dataIn  =
  dataIn |> Array.map(fun (x:(IO.FileInfo*string))->
    let fileName = (fst x).FullName
    let fileText = snd x
    let resourcesDirectory = """file:///""" + sourceDirectory + "resources" + directorySeparator
    let markdownText=Markdown.ToHtml(fileText) 
    markdownText
      .Replace("<img src=\"images/", "<img src=\"" + resourcesDirectory + "images" + directorySeparator)
    )

let outputStuff (processedDatata:string []) =
  printfn ""
  let ret = Markdown.ToHtml("This is some *emphasis* text, dang it", pipeline)
  printfn "%A" ret

  let pagePrefix ="""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <title>Info-Ops2</title>
  <style>
    img {
      width:100%;
      }
  </style>
  </head>
  <body>
  """

  let pageSuffix ="""
    </body>
    </html>
    """

  let totalFile = pagePrefix + (String.concat "\n" (processedDatata |> Array.toSeq)) + pageSuffix
  System.IO.File.WriteAllText(sourceDirectory + string System.IO.Path.DirectorySeparatorChar + "foo.html", totalFile)
  0



[<EntryPoint>]
let main argv =
  try
     argv |> inputStuff |> processStuff |> outputStuff 
  with | e->
    printfn "%A" e
    1
