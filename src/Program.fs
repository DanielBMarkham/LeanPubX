open System
open Markdig

// Calculated init data where I don't care if it fails. If so, explode
let pipelineBuilder = new MarkdownPipelineBuilder()
let pipeline = (pipelineBuilder.UseAdvancedExtensions()).Build()
let booksFileName = "Book.txt"
let directorySeparator = string System.IO.Path.DirectorySeparatorChar
let defaultDirectory=System.Environment.CurrentDirectory
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
// Calculated init data where I might care one day, but currently do not. Provide defaults if fails
let sourceDirectory =
  try System.Environment.GetCommandLineArgs().[1] + directorySeparator with |_->defaultDirectory
let targetDirectory =
  try System.Environment.GetCommandLineArgs().[2] + directorySeparator with |_->defaultDirectory
let filesToWork =
  try
    System.IO.File.ReadLines(sourceDirectory + directorySeparator + booksFileName ) |> Seq.toArray
  with |_-> Array.empty

// Now I'm performing the _business_ process of getting the data
// If something unusual were to happen here, I have a new biz condition to manage
let inputStuff (argv:string []) =
  // be "nice" to user. Tell them we're working on it
  printfn "input dir %A" sourceDirectory
  printfn "output dir %A" targetDirectory
  printfn "number of files to work  %A" filesToWork.Length
  // I'll make a tuple with the name in case something breaks later
  let filesToProcess =
    try
      filesToWork
        |> Array.map(fun filename->
          (
            let fileItemName=sourceDirectory + directorySeparator + filename
            (
              System.IO.FileInfo(fileItemName)
              ,System.IO.File.ReadAllText(fileItemName)
            )
          )
        )
    with |_->Array.empty // currently fail with empty result on any file read error
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
  let totalFile = pagePrefix + (String.concat "\n" (processedDatata |> Array.toSeq)) + pageSuffix
  System.IO.File.WriteAllText(targetDirectory + directorySeparator + "foo.html", totalFile)
  printfn ""
  printfn "Total lines of html created %A" totalFile.Length
  0

[<EntryPoint>]
let main argv =
  try
     argv |> inputStuff |> processStuff |> outputStuff 
  with | e->
    printfn "%A" e
    1 // catch-all. if we reach here, I've made a mistake in categorization or processing of inputs
