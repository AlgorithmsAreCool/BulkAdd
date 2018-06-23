module BulkAdd.Global

let mutable WhatIfMode = false
let mutable Debug = false


type Console = System.Console
type Color = System.ConsoleColor

let cprintf color =
    let writeColor (value:string) =
        let old = Console.ForegroundColor
        try
            Console.ForegroundColor <- color
            Console.WriteLine(value)
        finally
            Console.ForegroundColor <- old

    Printf.kprintf writeColor

