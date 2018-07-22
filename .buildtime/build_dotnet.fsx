#load "build_filesystem.fsx"

open Fake.IO.FileSystemOperators
open Fake.DotNet

module Path = Build_filesystem.Paths

/// NET Core SDK version 
let sdkVersion = "2.1.105" |> DotNet.Version
  
/// `dotnet` will be installed project-local in `{repoRoot}/.buildtime/dotnet` ) 
let installDir =
  do System.IO.Directory.CreateDirectory Path.``/.buildtime/dotnet`` |> ignore
  Path.``/.buildtime/dotnet``

let generalOptions (opts : DotNet.Options) = 
  {opts with
    DotNetCliPath = (installDir @@ "dotnet")}

let clean projFile = 
    let projDir = Fake.IO.Path.getDirectory (projFile)
    [projDir</>"bin";projDir</>"obj"]
    |> List.iter Fake.IO.Directory.delete

let install _ = DotNet.install (fun opts -> 
  {opts with 
    Version=sdkVersion
    CustomInstallDir = Some installDir})

let build cfg projFile  = projFile |> DotNet.build (fun c -> 
  { c with  
      Configuration = cfg
      Common = generalOptions c.Common
  })

let buildDebug = build DotNet.Debug
let buildRelease = build DotNet.Release

let restore projFile  =
  DotNet.restore (fun opts ->
    { opts with
        Common = generalOptions opts.Common   
    }) projFile  

let run cmd args workingDir =
  DotNet.exec 
    (fun opts ->
      let opts' = generalOptions opts
      { opts' with
          WorkingDirectory = workingDir
      }) 
    cmd args  

  