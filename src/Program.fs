module BulkAdd.Program

open Global

open BulkAdd.Projects

open System
open System.IO
open BulkAdd.Graph


type StartInfo = System.Diagnostics.ProcessStartInfo
type Process = System.Diagnostics.Process

let addProject slnPath (projects: #seq<string>) =
    for projPath in projects do

        printfn"-----------------------"
        printfn"Adding %s to %s" projPath slnPath

        let command = sprintf "sln \"%s\" add \"%s\"" slnPath projPath
        if Debug then
            printfn "dotnet %s" command
        if Global.WhatIfMode then
            printfn "Not Executing in WhatIf mode"
        else
            try

                    let startInfo = StartInfo()
                    startInfo.FileName <- "dotnet"
                    startInfo.Arguments <- command

                    let proc = Process.Start(startInfo)
                    proc.WaitForExit()

                    match proc.ExitCode with
                    | 0 -> cprintf Color.Green "OK"
                    | code -> cprintf Color.Yellow "Unusual Return :("
            with
            | _ -> ()
        printfn"-----------------------"


[<EntryPoint>]
let main argv =
    cprintf ConsoleColor.Yellow "Running Now...%A" argv
    //printfn"Running Now...%A" argv

    let argData = CommandLine.Parse argv

    Global.Debug <- argData.Debug
    Global.WhatIfMode <- argData.WhatIf

    //argData.ProjectFile

    let builder = { GetEdges = Projects.GetProjectDependencies }

    let originProject = argData.ProjectFile |> Paths.toFileLocation  |> Projects.ProjectRef

    let originNode = Graph.Build(builder, originProject)

    let inspector = {
        State = ()
        Inspector = fun (proj, ()) -> printfn"%A" proj
    }

    Graph.Visit(originNode, inspector)

    // |> Path.GetFullPath
    // |> AbsPath
    // |> ProjectRef
    // |> Graph.
    // |> addProject argData.SolutionFile

    0