namespace Utilitarian

[<RequireQualifiedAccess>]
module Choice2 =
  
  let choose1 x = 
    match x with 
    | Choice1Of2 value -> Some value
    | Choice2Of2 _ -> None

  let choose2 x = 
    match x with 
    | Choice1Of2 _ -> None
    | Choice2Of2 value -> Some value