module BulkAdd.CommandLine

open System.IO

open Argu

let private halt msg =
    let exiter = ProcessExiter() :> IExiter
    exiter.Exit(msg, ErrorCode.HelpText)

type BulkAddArgs =
    | [<AltCommandLine("-s", "-sln")>] Solution of path:string
    | [<Mandatory;AltCommandLine("-p", "-proj")>] Project of path:string
    | WhatIf
    | Debug
    with
        interface IArgParserTemplate with
            member this.Usage =
                match this with
                | Solution _ -> "The solution to modify"
                | Project _ -> "The project to add to the solution"
                | WhatIf -> "Simulates the action without actually performing it"
                | Debug -> "EnablesDebugLogging"

let private validateArgs parsedArgs =

    let testFile allowedExtensions (subject: string) =
        let subjectExt = Path.GetExtension(subject)

        match List.contains subjectExt allowedExtensions with
        | false -> Error (sprintf "File %s (%s) does not match extensions %A" subject subjectExt allowedExtensions)
        | true ->
            match File.Exists subject with
            | false -> Error (sprintf "File '%s' does not exist" subject)
            | true -> subject |> Ok

    let testArg arg =
        match arg with
        | Solution slnFile -> testFile [".sln"] slnFile
        | Project projFile -> testFile [".csproj";".fsproj"] projFile
        | _ -> Ok ""

    let results = List.map testArg parsedArgs
    let errors = List.choose (function Error e -> Some e | _ -> None) results

    if List.isEmpty errors then
        Ok parsedArgs
    else
        Error errors


let private getLocalSlnFile =
    let currentDir = System.Environment.CurrentDirectory
    let localSolutions = System.IO.Directory.GetFiles(currentDir, "*.sln")
    match localSolutions with
    | [||] -> Error (sprintf "No solution files found (%s)" currentDir)
    | [|slnFile|] -> Ok slnFile
    | _ -> Error "Too many solution files found. Not sure which to use"

let private parseRawArgs args =
    let errorColorizer error =
        match error with
        | ErrorCode.HelpText -> None
        | _ -> Some System.ConsoleColor.Red
    let errorHandler = ProcessExiter(colorizer = errorColorizer)
    let parser = ArgumentParser.Create<BulkAddArgs>(errorHandler = errorHandler)

    let results = parser.ParseCommandLine args

    results


type CommandLineOptions = {
    ProjectFile : string
    SolutionFile : string
    WhatIf : bool
    Debug : bool
}

let Parse args =
    let parsedArgs = parseRawArgs args

    let chooseSolution = (function Solution sln -> Some sln | _ -> None)
    let chooseProject = (function Project prj -> Some prj | _ -> None)

    let solutionIsMissing =
        parsedArgs.GetAllResults()
        |> List.choose chooseSolution
        |> List.isEmpty

    let preProcessedArgs =
        match solutionIsMissing with
        | false -> parsedArgs.GetAllResults()
        | true ->
            match getLocalSlnFile with
            | Ok slnFile -> (Solution slnFile)::(parsedArgs).GetAllResults()
            | Error error -> halt error

    match validateArgs preProcessedArgs with
    | Error errors -> halt(String.concat "; " errors)
    | Ok value ->
        {
            ProjectFile = List.pick chooseProject value
            SolutionFile = List.pick chooseSolution value
            WhatIf = parsedArgs.Contains <@ WhatIf @>
            Debug = parsedArgs.Contains <@ Debug @>
        }
