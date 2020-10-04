module Done.Config
open Persistence.File
open Persistence.Sqlite

// let save = saveCompletedItem Path
// let get = getCompletedItems Path

let save =
    Db.Schema.initializeDiskDb DataSource
    Db.saveCompletedItem

let get =
    Db.Schema.initializeDiskDb DataSource
    Db.getCompletedItems
