module Done.Persistence.File
open Done.Domain
open System.IO

[<LiteralAttribute>]
let FilePath = "todo.done.txt"

let saveCompletedItem path : SaveCompletedItem =
    fun item ->
        use writer = File.AppendText path
        item |> Done.toString |> writer.WriteLine
        Ok ()

let getCompletedItems path : GetCompletedItems =
    fun _ ->
        if (File.Exists path) then File.ReadAllLines path else [||]
        |> Seq.map Done.tryParse
        |> Seq.filter Option.isSome
        |> Seq.map (fun i -> i.Value)
