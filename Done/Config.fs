module Done.Config
open Persistence.File

[<LiteralAttribute>]
let Path = "todo.done.txt"

let save = saveCompletedItem Path
let get = getCompletedItems Path
