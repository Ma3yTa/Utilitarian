#load "build_projectinfo.fsx"

open Fake
open Fake.Tools
open Build_projectinfo

let gitRelease (p : ProjectInfo) _ =
  
  let repo = p.Repository
  let version = p.LatestRelease.NugetVersion
  let tag = 
    if p.VersionPrefix <> "" then sprintf "%s/%s" p.VersionPrefix version
    else version

  let remote =
    Git.CommandHelper.getGitResult "" "remote -v"
    |> Seq.filter (fun (s: string) -> s.EndsWith("(push)"))
    |> Seq.tryFind (fun (s: string) -> s.Contains(repo.Owner + "/" + repo.Name))
    |> function None -> repo.Owner + "/" + repo.Name | Some (s: string) -> s.Split().[0]

  Git.Staging.StageAll ""
  Git.Commit.Commit "" (sprintf "Bump version to %s" tag)
  Git.Branches.pushBranch "" remote (Git.Information.getBranchName "")

  Git.Branches.tag "" tag
  Git.Branches.pushTag "" remote tag