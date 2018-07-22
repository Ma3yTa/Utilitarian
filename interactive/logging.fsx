#load @"setup_fsi.fsx"

open Utilitarian

let consoleProcessor _ = {
  Id="out/console"
  Transformator = id
  ProcessingAction = fun msg -> Async.retn (printfn "%A" msg)
}

do stdLogTrace "Hello" []
do stdLogPipeline.AddProcessor (consoleProcessor())
do stdLogTrace "Hello" []
do stdLogPipeline.RemoveProcessors [consoleProcessor()]

