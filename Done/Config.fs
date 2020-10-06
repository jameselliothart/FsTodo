module Done.Config
open Persistence.File
open Persistence.Sqlite

// let save = saveCompletedItem FilePath
// let get = getCompletedItems FilePath

let save =
    Db.Schema.initializeDiskDb ()
    Db.saveCompletedItem

let get =
    Db.Schema.initializeDiskDb ()
    Db.getCompletedItems
