// reference NuGet dependencies with implementations for net461 runtime
// those are usable by F# Interactive (on Windows machine at least)
// create them locally with following command:
// paket generate-load-scripts -g netstandard -f net461
#load @"../.paket/load/net461/netstandard/netstandard.group.fsx"

// include files from github paket references
#I @"../paket-files/netstandard/fsprojects/FSharp.Control.AsyncSeq/src/FSharp.Control.AsyncSeq"
#I @"../paket-files/netstandard/fsprojects/FSharpx.Async/src/FSharpx.Async"
#load @"Agent.fs"
#load @"AutoCancelAgent.fs"
#load @"AsyncSeq.fs"
#load @"Async.fs"

// include shared code
#I @"../src/shared/core"
#load "Prelude.fs"
#load "Logging.fs"

// include project files
#I @"../src/netstandard/Utilitarian"


// this makes relative paths in scripts work as expected
// http://brandewinder.com/2016/02/06/10-fsharp-scripting-tips/
#I __SOURCE_DIRECTORY__

// here we can configure additional stuff in the future, e.g. customizing fsi session