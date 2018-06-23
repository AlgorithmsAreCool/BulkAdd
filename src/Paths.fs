module BulkAdd.Paths

open System.IO

type AbsPath = AbsPath of string
type RelPath = RelPath of string

type Location =
| Rel of RelPath
| Abs of AbsPath

type FileLocation = FileLocation of Location
type DirLocation = DirLocation of Location

let toAbs (AbsPath anchorPath) (RelPath relativeSegment) =
    let anchorDir = Path.GetDirectoryName(anchorPath)
    Path.Combine(anchorDir, relativeSegment) |> Path.GetFullPath |> AbsPath
let toRel (AbsPath anchorPath) (AbsPath targetPath) =
    Path.GetRelativePath(anchorPath, targetPath) |> RelPath

let normalize anchor location =
    match location with
    | Rel relPath -> toAbs anchor relPath |> Abs
    | Abs absPath -> absPath |> Abs
let toLocation(str : string) =
    match Path.IsPathRooted(str) with
    | true -> str |> AbsPath |> Location.Abs
    | false -> str |> RelPath |> Location.Rel

let toFileLocation =  toLocation >> FileLocation

let toDirLocation = toLocation >> FileLocation