module Done.Persistence.File
open Done.Domain
open System.IO

[<LiteralAttribute>]
let path = "todo.done.txt"

let saveCompletedItem path : SaveCompletedItem =
    fun item ->
        use writer = File.AppendText path
        item |> Done.toString |> writer.WriteLine
        Ok ()

let getCompletedItems path : GetCompletedItems =
    fun _ ->
        let allItems = File.ReadAllLines path
        allItems
        |> Array.map Done.tryParse
        |> Array.filter Option.isSome
        |> Array.map (fun i -> i.Value)

let save = saveCompletedItem path
let get = getCompletedItems path
