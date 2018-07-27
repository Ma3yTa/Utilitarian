#r @"../packages/build/FAKE/tools/FakeLib.dll"
#load @"build_filesystem.fsx"
#load @"build_projectinfo.fsx"
#load @"build_dotnet.fsx"
#load @"build_fable.fsx"

open System

open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO.FileSystemOperators

module DotNet = Build_dotnet
module Fable = Build_fable

module Path = Build_filesystem.Paths
module Glob = Build_filesystem.Globs
module Proj =  Build_projectinfo

// set working dir to repo root
do 
  Environment.CurrentDirectory <- Path.repoRoot

//#region DOTNET TARGETS 

let [<Literal>] InstallDotNetCore = "InstallDotNetCore"
Target.create InstallDotNetCore (DotNet.install >> ignore)

let [<Literal>] CleanDotNet = "CleanDotNet"
Target.create CleanDotNet (fun _ ->
  for p in Glob.``/src/netstandard/**/"*.fsproj"`` do
     p |> DotNet.clean)

let [<Literal>] BuildDotNet = "BuildDotNet"
Target.create BuildDotNet (fun _ -> 
  for p in Glob.``/src/netstandard/**/"*.fsproj"`` do
     p |> DotNet.buildRelease) 

InstallDotNetCore
==> CleanDotNet
==> BuildDotNet

//#endregion

//#region FABLE TARGETS

let [<Literal>] DevelopMk8React = "DevelopMk8React"
Target.create DevelopMk8React (fun _ -> 
  
  let projFile = 
    Path.``/src/fable/apps/react``
    </> "mariokart"
    </> Proj.UtilitarianAppsMariokartReact
  
  do Fable.clean projFile
  do Fable.install projFile

  Fable.developAsync projFile 
  |> Async.Ignore
  |> Async.RunSynchronously
)

let [<Literal>] BuildMk8React = "BuildMk8React"
Target.create BuildMk8React (fun _ -> 
  
  let projFile = 
    Path.``/src/fable/apps/react``
    </> "mariokart"
    </> Proj.UtilitarianAppsMariokartReact
  
  Fable.clean projFile
  Fable.install projFile
  Fable.build projFile 
)

InstallDotNetCore
==> DevelopMk8React

InstallDotNetCore
==> BuildMk8React

//#endregion 

Target.runOrDefault BuildDotNet