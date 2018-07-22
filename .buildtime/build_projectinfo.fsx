#r @"../packages/build/FAKE/tools/FakeLib.dll"
#load @"build_filesystem.fsx"

open Fake.Core
open Fake.IO.FileSystemOperators

module Path = Build_filesystem.Paths

type RepoInfo = {
  Owner : string
  Name : string
}
with 
  member x.GitLabUrl = 
    sprintf @"https://gitlab.com/%s/%s" x.Owner x.Name
  member x.GitHubUrl = 
    sprintf @"https://github.com/%s/%s" x.Owner x.Name

let repo = {Owner="kfrie";Name="Utilitarian"}

type ProjectInfo = {
  VersionPrefix : string
  ReleaseNotes : Fake.Core.ReleaseNotes.ReleaseNotes list
  Description : string
  Tags : string list
  Authors : string list
  Repository : RepoInfo 
}
with 
  member __.Copyright =  
    sprintf "Copyright \169 %i" System.DateTime.Now.Year
  member p.LicenseUrl =
    sprintf "%s/raw/master/LICENSE" (p.Repository.GitLabUrl)
  member p.RepositoryUrl = p.Repository.GitLabUrl  
  member p.LatestRelease = 
    p.ReleaseNotes.Head
  /// https://docs.microsoft.com/en-us/dotnet/core/tools/csproj#packagetags
  member p.NuGetPackageTags = 
    p.Tags |> String.concat ";"
  /// https://docs.microsoft.com/en-us/nuget/reference/package-versioning  
  member p.NugetVersion = p.LatestRelease.NugetVersion
  /// https://docs.microsoft.com/en-us/dotnet/core/tools/csproj#authors
  member p.NugetAuthors = p.Authors |> String.concat ";"
  member p.NugetPackageReleaseNotes = 
    p.LatestRelease.Notes |> String.concat "\n"

let [<Literal>] ReleaseNotesFilename = "CHANGELOG.md"

let repositoryInfo = {
  VersionPrefix = "utilitarian"
  ReleaseNotes = []
  Description = @"The Utilitarian project is an effort to promote holistic development consistency and maximum code reuse with next-gen F#"
  Tags = ["fsharp"; "fable"; "elmish" ;"safe-stack";"react"]
  Authors = ["Kai Friedrich"]
  Repository = repo
}

let [<Literal>] UtilitarianNetstandard = 
  "Utilitarian.Netstandard.fsproj"

let [<Literal>] UtilitarianAppsMariokartReact = 
  "Utilitarian.Apps.Mariokart.React.fsproj"

/// maps .fsproj file paths in solution to ProjectInfo record. 
/// Intended to be used when iterating over globbing pattern
let getProjectInfo projectPath =  
  
  let projectReleaseNotes _ = 
    try
      projectPath 
      |> Fake.IO.Path.getDirectory
      </> ReleaseNotesFilename
      |> System.IO.File.ReadAllLines  
      |> Fake.Core.ReleaseNotes.parseAll
      |> Some
    with _ -> None
    
  match (projectPath |> System.IO.Path.GetFileName) with
  | UtilitarianNetstandard -> 
    Some {
      VersionPrefix = "lib/netstandard"
      ReleaseNotes = defaultArg 
        (projectReleaseNotes ())
        (repositoryInfo.ReleaseNotes)
      Description = @"yet another F# utility library for NETStandard2.0"
      Tags = ["fsharp"; "netstandard"; "utils"; "prelude"; "logging"]
      Authors = repositoryInfo.Authors
      Repository = repo 
    }
  | _ -> None 
    