open System
open Domain
open Persistence.File

let helpMessage = "Valid commands are 'a <item>' and 'r <index>' for Add and Remove"

let tryParseIndex (data: string) =
    match Int32.TryParse(data) with
    | (true, i) -> Some i
    | (false, _) -> None

let (|Show|Add|Remove|Purge|Invalid|) (argv: string []) =
    match argv with
    | [||] -> Show
    | [|cmd;data|] ->
        match cmd.ToLowerInvariant() with
        | "a" -> Add data
        | "r" ->
            match tryParseIndex data with
            | Some i -> Remove i
            | None -> Invalid "Specify number index of item to Remove"
        | "p" ->
            match tryParseIndex data with
            | Some i -> Purge i
            | None -> Invalid "Specify number index of item to Purge"
        | _ -> Invalid helpMessage
    | _-> Invalid helpMessage

let printTodo : PrintTodo = function
    | Todo.Nothing -> printfn "No todos yet"
    | Todo.Todos todos -> todos |> Array.iter (fun t -> printfn "%i. %s" (Todo.index t) (Todo.value t))

let showTodos () = get() |> printTodo

let printIfError = function
    | Error e -> printfn "%s" e
    | Ok _ -> ()

let remainingTodosEvent = new Event<Todo.EnumeratedTodos>()
remainingTodosEvent.Publish |> Event.add (save >> printIfError)

let completedItemsEvent = new Event<Todo.EnumeratedTodos>()
completedItemsEvent.Publish |> Event.add (printTodo)
completedItemsEvent.Publish
|> Event.add (Todo.toCompletedItems >> Option.iter (Array.iter (Done.Persistence.File.save >> printIfError)))

let dispatch = function
    | Show ->
        showTodos()
    | Add data ->
        add data |> printIfError
        showTodos()
    | Remove i ->
        let (completed, remaining) =
            get() |> Todo.partitionCompletedTodo i
        remainingTodosEvent.Trigger remaining
        completedItemsEvent.Trigger completed
    | Purge i ->
        get() |> Todo.partitionCompletedTodo i |> (snd >> remainingTodosEvent.Trigger)
    | Invalid msg -> printfn "%s" msg

[<EntryPoint>]
let main argv =
    argv |> dispatch
    0 // return an integer exit code
