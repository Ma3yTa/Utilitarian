#load @"setup_fsi.fsx"

open Utilitarian
open Utilitarian.Logging

do 
  // we can extent the std pipe with a processor whenever we need it. It is a MBP implmentation and thus thread-safe
  // This operation is also idempotent, i.e. doing it once or multiple times has the same net effect
  Log.stdPipe.AddProcessor Processors.consolePlain

  Log.stdPipe.RemoveProcessor Processors.consolePlain

do 
  Log.stdInfo "Hello" 
    ["who am I?", box <| "the king of nuts!"
     "current local time", box <| System.DateTime.Now]


