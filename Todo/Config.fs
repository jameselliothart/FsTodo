module Config
open System
open Domain
open Persistence.File

[<LiteralAttribute>]
let Path = "todo.txt"

let get = getTodos Path
let add = addTodo Path
let save = saveTodos Path

let printIfError = function
    | Error e -> printfn "%s" e
    | Ok _ -> ()

let printTodo : PrintTodo = function
    | Todo.Nothing -> printfn "No todos in %s" (IO.Path.GetFullPath Path)
    | Todo.Todos todos ->
        todos
        |> Array.iter (fun t -> printfn "%i. %s" (Todo.index t) (Todo.value t))

let remainingTodosEvent = new Event<Todo.EnumeratedTodos>()
remainingTodosEvent.Publish |> Event.add (save >> printIfError)

let completedTodosEvent = new Event<Todo.EnumeratedTodos>()
completedTodosEvent.Publish |> Event.add (printTodo)
completedTodosEvent.Publish
|> Event.add (
    Todo.toCompletedItems >>
        Option.iter (Array.iter (Done.Config.save >> printIfError))
    )

let purgedTodosEvent = new Event<Todo.EnumeratedTodos>()
purgedTodosEvent.Publish |> Event.add (printTodo)

let handle = function
    | Todo.RemainingTodosEvent todos -> remainingTodosEvent.Trigger todos
    | Todo.CompletedTodosEvent todos -> completedTodosEvent.Trigger todos
    | Todo.PurgedTodosEvent todos -> purgedTodosEvent.Trigger todos
