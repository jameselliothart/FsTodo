﻿open System
open Done.Domain
open Done.Persistence.File

let helpMessage = "Usage: `done d <number>` or `done w <number>` to get items done <number> of days/weeks ago"

type Command =
| Add of string
| Query of Period

let tryParsePeriod (period: string) amount =
    match period.ToLowerInvariant() with
    | "d" -> Some (Query (Days amount))
    | "w" -> Some (Query (Weeks amount))
    | _ -> None

let tryParseArgs (argv: string []) =
    match argv with
    | [|command;param|] ->
        if command = "a" then Some (Add param) else
            match Double.TryParse(param) with
            | (true, x) -> tryParsePeriod command x
            | (false, _) -> None
    | [|period|] -> tryParsePeriod period 0.0
    | _ -> None

let printIfError = function
    | Error e -> printfn "%s" e
    | Ok _ -> ()

let dispatch argv =
    let cmd = tryParseArgs argv
    match cmd with
    | Some c ->
        match c with
        | Query p ->
            get()
            |> Array.filter (Done.completedSince p)
            |> Array.iter (Done.toString >> printfn "%s")
        | Add s -> s |> Done.createDefault |> save |> printIfError
    | None -> printfn "%s" helpMessage

[<EntryPoint>]
let main argv =
    argv |> dispatch
    0 // return an integer exit code
