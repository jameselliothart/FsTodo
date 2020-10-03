module Persistence.Sqlite

open Microsoft.Data.Sqlite
open FSharp.Data.Dapper
open System
open Done.Domain

[<Literal>]
let DataSource = "../done.db"

module Connection =
    let private connectionStringInMemory (dataSource : string) =
        sprintf "Data Source = %s; Mode = Memory; Cache = Shared;" dataSource

    let private connectionStringOnDisk (dataSource: string) =
        sprintf "Data Source = %s;" dataSource

    let Memory () = new SqliteConnection (connectionStringInMemory "MEMORY")
    let Disk () = new SqliteConnection (connectionStringOnDisk DataSource)

module Db =
    let private connection () = Connection.SqliteConnection (Connection.Disk())

    let private querySeqAsync<'R>    = querySeqAsync<'R> (connection)
    let private querySingleAsync<'R> = querySingleOptionAsync<'R> (connection)

    module Schema =
        let createTables = querySingleAsync<int> {
            script """
                CREATE TABLE IF NOT EXISTS CompletedItems (
                    Id INTEGER PRIMARY KEY,
                    CompletedOn DATETIME,
                    Item VARCHAR(255)
                )
                """
        }

        let initializeDiskDb () =
            if (IO.File.Exists DataSource) then ()
            else createTables |> Async.RunSynchronously |> ignore

    let saveCompletedItem : SaveCompletedItem =
        fun item ->
            querySingleAsync<int> {
            script "INSERT INTO CompletedItems (CompletedOn, Item) VALUES (@CompletedOn, @Item)"
            parameters (dict ["CompletedOn", box item.CompletedOn; "Item", box item.Item])
            } |> Async.RunSynchronously |> ignore
            Ok ()

    let getCompletedItems : GetCompletedItems =
        fun _ ->
            querySeqAsync<Done.CompletedItem> { script "SELECT CompletedOn, Item FROM CompletedItems" }
            |> Async.RunSynchronously

    let getCompletedItemsByDate (completedOn: DateTime) = querySeqAsync<Done.CompletedItem> {
        script """
            SELECT CompletedOn, Item FROM CompletedItems
            WHERE datetime(CompletedOn) > datetime(@CompletedOn)
            """
        parameters (dict ["CompletedOn", box (completedOn.ToString("s"))])
        }
