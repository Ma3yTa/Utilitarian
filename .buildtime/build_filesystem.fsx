#r @"../packages/build/FAKE/tools/FakeLib.dll"

open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators

module Paths = 
  
  /// ensure the dir will always exist at the beginning of the build pipeline.
  /// It will also create an empty README.md if doesn't exist.
  /// That way the folder structure can be tracked down by git.
  /// This also enforces the good practice of describing the purpose of the folder.
  let private ensureDir dirPath =
    do
      Fake.IO.Directory.ensure(dirPath)
      Fake.IO.File.create(dirPath </> "README.md")
    dirPath    

  /// always yields the correct absolute path to repo root folder
  let repoRoot = 
    __SOURCE_DIRECTORY__ </> ".." |> ensureDir
  
  /// always yields the correct absolute path to `.buildtime` folder
  let ``/.buildtime`` = 
    __SOURCE_DIRECTORY__ |> ensureDir

  /// always yields the correct absolute path to `.buildtime/dotnet` folder.
  /// the build process will install a version of `dotnet` dedicated to the build process in this folder.
  let ``/.buildtime/dotnet`` = 
    ``/.buildtime`` </> "dotnet" |> ensureDir

  /// always yields the correct absolute path to `src` folder
  let ``/src`` = 
   repoRoot </> "src" |> ensureDir

  /// always yields the correct absolute path to `src/fable` folder
  let ``/src/fable``  = 
    ``/src`` </> "fable" |> ensureDir

  /// always yields the correct absolute path to `src/fable/libs` folder
  let ``/src/fable/libs`` = 
    ``/src/fable`` </> "libs"

  /// always yields the correct absolute path to `src/fable/apps` folder
  let ``/src/fable/apps`` = 
    ``/src/fable`` </> "apps" |> ensureDir

  /// always yields the correct absolute path to `src/fable/apps/react` folder
  let ``/src/fable/apps/react`` = 
    ``/src/fable/apps`` </> "react" |> ensureDir

  /// always yields the correct absolute path to `src/fable/apps/react_native` folder
  let ``/src/fable/apps/react_native`` = 
    ``/src/fable/apps`` </> "react_native" |> ensureDir

  /// always yields the correct absolute path to `src/netstandard` folder
  let ``/src/netstandard`` = 
    ``/src`` </> "netstandard" |> ensureDir


module Globs = 
  open Paths
  /// yields all NET Standard library projects 
  let ``/src/netstandard/**/"*.fsproj"`` =
    !! (``/src/netstandard`` </> "**" </> "*.fsproj")
