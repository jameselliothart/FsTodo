module Done.Config
open Persistence.File

let save = saveCompletedItem Path
let get = getCompletedItems Path
