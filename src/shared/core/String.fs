namespace Utilitarian

[<RequireQualifiedAccess>]
module String = 

  open System

  let [<Literal>] Empty = ""

  let inline newlines n = 
    Array.zeroCreate n
    |> Array.map (fun _ -> Environment.NewLine)
    |> String.concat ""
      
  let inline spaces n = String(' ',n)
  
  let inline tabs n = String('\t',n)

  let inline isNullOrEmpty str = 
    match str with
    | Empty | null -> true
    | _ -> false

  let inline isNonEmpty str = str |> isNullOrEmpty |> not

  let inline chooseNonEmpty str =
    if str |> isNonEmpty then 
      Some str
    else
      None

  let (|NonEmpty|_|) = chooseNonEmpty 

  let inline fromChars (chars : #seq<char>) = 
    System.String(chars |> Array.ofSeq)

  let inline lowerCase (x : string) =
    x.ToLowerInvariant()
    
  let (|LowerCase|) = lowerCase 

  let inline upperCase (x : string) =
    x.ToUpperInvariant()
    
  let (|UpperCase|) = upperCase

[<AutoOpen>]
module OptionExtensions = 

  [<RequireQualifiedAccess>]
  module Option =
    
    let inline stringFold folder opt = 
      Option.fold folder (String.Empty) opt