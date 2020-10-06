module Domain
open Done.Domain

module Todo =
    type Todo = private Todo of index: int * item: string

    type TodoList =
    | Todos of Todo array
    | Nothing

    type TodoEvent =
    | TodoAddedEvent of string
    | TodosRemainingEvent of TodoList
    | TodosCompletedEvent of TodoList
    | TodosPurgedEvent of TodoList

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

    let private partitionTodos idx todos =
        match todos with
        | Nothing -> (Nothing, Nothing)
        | Todos todos ->
            todos
            |> Array.partition (fun t -> index t = idx)
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
        |> fun (completed, remaining) -> [TodosCompletedEvent completed; TodosRemainingEvent remaining]

    let purge index todos =
        partitionTodos index todos
        |> fun (purged, remaining) -> [TodosPurgedEvent purged; TodosRemainingEvent remaining]

type PrintTodo = Todo.TodoList -> unit
type GetTodos = unit -> Todo.TodoList
type AddTodo = string -> Result<unit,string>
type SaveTodos = Todo.TodoList -> Result<unit,string>
