module Persistence.File
open Domain
open System.IO

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
