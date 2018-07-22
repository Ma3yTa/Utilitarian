#load "build_projectinfo.fsx"
open Fake.Runtime
#load "build_dotnet.fsx"

open Fake.Core
open Fake.IO
open Fake.IO.FileSystemOperators

module DotNet = Build_dotnet
module Proj = Build_projectinfo  


let platformTool tool winTool =
    let tool = if Environment.isUnix then tool else winTool
    tool
    |> Process.tryFindFileOnPath
    |> function Some t -> t | _ -> failwithf "%s not found" tool

let nodeTool = platformTool "node" "node"
let yarnTool = platformTool "yarn" "yarn"

do if not Environment.isWindows then
    // We have to set the FrameworkPathOverride so that dotnet sdk invocations know
    // where to look for full-framework base class libraries
    let mono = platformTool "mono" "mono"
    let frameworkPath = Path.getDirectory(mono) </> ".." </> "lib" </> "mono" </> "4.5"
    Environment.setEnvironVar "FrameworkPathOverride" frameworkPath

let clean projFile = 
  let appDir = Fake.IO.Path.getDirectory (projFile)
  let buildDir = appDir </> "build" 
  [appDir</>"bin";appDir</>"obj";buildDir]
  |> List.iter Fake.IO.Directory.delete

let build projFile =
    do DotNet.restore projFile
    let workingDir = Fake.IO.Path.getDirectory projFile
    do 
      workingDir 
      |> DotNet.run "fable" "webpack -- -p" 
      |> ignore

let install projFile = 
  let run cmd tool = 
    Process.execSimple
      (fun pInfo -> 
        {pInfo with 
          FileName=tool
          WorkingDirectory=Fake.IO.Path.getDirectory(projFile)
          Arguments=cmd}
      ) (System.TimeSpan.FromMinutes(3.))
    |> ignore    

  do 
    nodeTool |> run "--version"
    printfn "Node version:"
    printfn "Yarn version:"  
    yarnTool |> run "--version"
    yarnTool |> run "install --frozen-lockfile"

let developAsync projFile =
  do DotNet.restore projFile
  let projDir = Fake.IO.Path.getDirectory projFile
  
  let runWebpackDevServer = async { 
     projDir 
     |> DotNet.run "fable" "webpack-dev-server"
     |> ignore
  }
  runWebpackDevServer


