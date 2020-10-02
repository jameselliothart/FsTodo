module Domain
open Done.Domain

module Todo =
    type Todo = private Todo of index: int * item: string

    type EnumeratedTodos =
    | Todos of Todo array
    | Nothing

    type TodoEvent =
    | AddedTodoEvent of string
    | RemainingTodosEvent of EnumeratedTodos
    | CompletedTodosEvent of EnumeratedTodos
    | PurgedTodosEvent of EnumeratedTodos

    let create (todos: string[]) =
        match todos with
        | [||] -> Nothing
        | todos ->
            todos
            |> Array.indexed
            |> Array.map Todo
            |> Todos

    let value (Todo(_,item)) = item
    let index (Todo(i,_)) = i

    let private partitionTodos index todos =
        match todos with
        | Nothing -> (Nothing, Nothing)
        | Todos todos ->
            todos
            |> Array.partition (fun (Todo(i,_)) -> i = index)
            |> fun (completed, remaining) -> (Todos completed, Todos remaining)

    let toCompletedItems todos =
        match todos with
        | Todos todos ->
            todos
            |> Array.map (value >> Done.createDefault)
            |> Some
        | Nothing -> None

    let complete index todos =
        partitionTodos index todos
        |> fun (completed, remaining) -> [CompletedTodosEvent completed; RemainingTodosEvent remaining]

    let purge index todos =
        partitionTodos index todos
        |> fun (purged, remaining) -> [PurgedTodosEvent purged; RemainingTodosEvent remaining]

type PrintTodo = Todo.EnumeratedTodos -> unit
type GetTodos = unit -> Todo.EnumeratedTodos
type AddTodo = string -> Result<unit,string>
type SaveTodos = Todo.EnumeratedTodos -> Result<unit,string>
