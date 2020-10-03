module Done.Domain
open System
open System.Text.RegularExpressions

type Period =
| Days of float
| Weeks of float

let private startOfDay (date: DateTime) = date.Date
let private daysAgo (date: DateTime) days = date.AddDays(-days) |> startOfDay
let private startOfWeek (date: DateTime) = date.AddDays(-(float)date.DayOfWeek) |> startOfDay
let private weeksAgo (date: DateTime) weeks = date.AddDays(-7.0 * weeks) |> startOfWeek |> startOfDay

module Done =

    [<CLIMutable>] type CompletedItem = {CompletedOn: DateTime; Item: string}

    let create (completedOn: DateTime) (item: string) =
        {CompletedOn = completedOn; Item = item}

    let createDefault item = item |> create DateTime.Now

    let toString item =
        sprintf "[%s] %s" (item.CompletedOn.ToString("s")) item.Item

    let tryParse (s: string) =
        let result =
            try
                s
                |> fun s -> Regex.Match(s, "^\[(?<completedOn>.*)\] (?<item>.*)")
                |> fun m -> if m.Success then Some m.Groups else None
                |> Option.map (
                    fun g -> (
                                (g |> Seq.filter (fun x -> x.Name = "completedOn") |> Seq.exactlyOne).Value,
                                (g |> Seq.filter (fun x -> x.Name = "item") |> Seq.exactlyOne).Value
                            )
                    )
                |> Option.map (fun (date, item) -> (DateTime.TryParse(date), item))
                |> Option.map (fun ((success, date), item) -> if success then Some (create date item) else None)
            with
            | :? ArgumentException -> None
        match result with
        | Some(Some(completedItem)) -> Some completedItem
        | _ -> None

    let completedSince period (item: CompletedItem) =
        let since =
            match period with
            | Days x -> daysAgo DateTime.Now x
            | Weeks x -> weeksAgo DateTime.Now x
        since < item.CompletedOn

type SaveCompletedItem = Done.CompletedItem -> Result<unit,string>
type GetCompletedItems = unit -> Done.CompletedItem seq
