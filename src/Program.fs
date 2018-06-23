module BulkAdd.Program

open Global

open BulkAdd.Projects

open System
open System.IO
open BulkAdd.Graph
open BulkAdd.Paths


type StartInfo = System.Diagnostics.ProcessStartInfo
type Process = System.Diagnostics.Process


let addProjectToSln slnPath (ProjectRef projectLocation) =

    let projectPath =
        match projectLocation with
        | Abs (AbsPath abs) -> abs
        | Rel (RelPath rel) -> rel

    printfn"-----------------------"
    printfn"Adding %A to %s" projectPath slnPath

    let command = sprintf "sln \"%s\" add \"%s\"" slnPath projectPath
    if Debug then
        printfn "dotnet %s" command
    if Global.WhatIfMode then
        printfn "WhatIf mode, No Action Taken"
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
    //printfn"-----------------------"


[<EntryPoint>]
let main argv =
    cprintf ConsoleColor.Yellow "Running Now...%A" argv
    //printfn"Running Now...%A" argv

    let argData = CommandLine.Parse argv

    Global.Debug <- argData.Debug
    Global.WhatIfMode <- argData.WhatIf

    //argData.ProjectFile

    let builder = { GetEdges = Projects.GetProjectDependencies }

    let originProject = argData.ProjectFile |> Paths.toLocation  |> Projects.ProjectRef

    let originNode = Graph.Build(builder, originProject)

    let inspector = {
        State = ()
        Inspector = fun (projectRef, ()) -> addProjectToSln argData.SolutionFile projectRef
    }

    Graph.Visit(originNode, inspector)

    0