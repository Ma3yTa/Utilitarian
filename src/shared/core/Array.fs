[<RequireQualifiedAccess>]
module Utilitarian.Array

let inline choosePair arr = 
  match arr with 
  | [|first;second|] -> Some (first,second)
  | _ -> None

let inline chooseTriple arr = 
  match arr with 
  | [|first;second;third|] -> Some (first,second,third)
  | _ -> None  

let (|Pair|_|) = choosePair

let (|Triple|_|) = chooseTriple