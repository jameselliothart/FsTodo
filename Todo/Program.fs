open System
open Domain
open Config

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

let showTodos () = get() |> printTodo

let dispatch = function
    | Show ->
        showTodos()
    | Add data ->
        Todo.TodoAddedEvent data |> handle
        showTodos()
    | Remove i ->
        get()
        |> Todo.complete i
        |> List.iter handle
    | Purge i ->
        get()
        |> Todo.purge i
        |> List.iter handle
    | Invalid msg -> printfn "%s" msg

[<EntryPoint>]
let main argv =
    argv |> dispatch
    0 // return an integer exit code
