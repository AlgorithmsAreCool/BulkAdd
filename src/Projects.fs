﻿module BulkAdd.Projects

open System.Xml.Linq
open BulkAdd.Paths
open System.IO

type ProjectRef = ProjectRef of Location
let private getXml(location) =
    match location with
    | Abs (AbsPath path) -> XElement.Load path
    | Rel (RelPath path) -> XElement.Load path

let private (!!) str = XName.Get str
let private getAttribute name (node:XElement) = name |> node.Attribute |> Option.ofObj
let private getProjectDependencies(projectPath : AbsPath) =

    let (AbsPath projectFilePath) = projectPath
    let projectXml = XElement.Load(projectFilePath)

    let makeReference = Paths.normalize projectPath >> ProjectRef

    projectXml.Descendants !!"ProjectReference"
    |> Seq.choose (getAttribute !!"Include")
    |> Seq.map (fun attr -> attr.Value |> Paths.toLocation |> makeReference)
    |> Seq.toList

let GetProjectDependencies(ProjectRef  location) =
    let pwd = System.Environment.CurrentDirectory |> AbsPath
    match location with
    | Rel relPath -> relPath |> Paths.toAbs pwd |> getProjectDependencies
    | Abs absPath -> absPath |> getProjectDependencies

