module Persistence.File
open Domain
open System.IO

[<LiteralAttribute>]
let path = "todo.txt"

let writeAllLines path (s: string []) =
    File.WriteAllLines(path, s)

let addTodo path : AddTodo =
    fun todo ->
        if not (File.Exists path) then
            File.Create(path).Dispose()
            printfn "Created %s" (Path.GetFullPath path)
        File.ReadAllLines path
        |> Array.append [|todo|]
        |> writeAllLines path
        Ok ()

let saveTodos path : SaveTodos =
    fun todos ->
        let write = writeAllLines path
        match todos with
        | Todo.Nothing -> write [||]
        | Todo.Todos todos ->
            todos
            |> Array.map Todo.value
            |> write
        Ok ()

let getTodos path : GetTodos =
    fun () ->
        if (File.Exists path) then File.ReadAllLines path else [||]
        |> Todo.create

let get = getTodos path
let add = addTodo path
let save = saveTodos path
