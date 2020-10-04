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

let addedTodoEvent = new Event<string>()
addedTodoEvent.Publish |> Event.add (add >> printIfError)

let remainingTodosEvent = new Event<Todo.TodoList>()
remainingTodosEvent.Publish |> Event.add (save >> printIfError)

let completedTodosEvent = new Event<Todo.TodoList>()
completedTodosEvent.Publish |> Event.add (printTodo)
completedTodosEvent.Publish
|> Event.add (
    Todo.toCompletedItems >>
        Option.iter (Array.iter (Done.Config.save >> printIfError))
    )

let purgedTodosEvent = new Event<Todo.TodoList>()
purgedTodosEvent.Publish |> Event.add (printTodo)

let handle = function
    | Todo.TodoAddedEvent todo -> addedTodoEvent.Trigger todo
    | Todo.TodosRemainingEvent todos -> remainingTodosEvent.Trigger todos
    | Todo.TodosCompletedEvent todos -> completedTodosEvent.Trigger todos
    | Todo.TodosPurgedEvent todos -> purgedTodosEvent.Trigger todos
