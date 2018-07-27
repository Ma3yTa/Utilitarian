namespace Utilitarian

[<RequireQualifiedAccess>]
module Async =
      
  let retn x = async { return x }

  let apply af ax = async {
      let! pf = Async.StartChild af
      let! px = Async.StartChild ax
      let! f = pf
      let! x = px
      return f x
  }

  let (<*>)   = apply

  let map f x = retn f <*> x

  let traverseList f list =
    let folder x xs = retn (fun x xs -> x :: xs) <*> f x <*> xs
    List.foldBack folder list (retn [])

  let traverseOption f opt = async {
    match opt with
    | Some value -> 
      let! result = value |> f
      return result |> Some
    | None -> return None
  } 

  let inline sequenceList list = traverseList id list

  let inline sequenceOption list = traverseOption id list

[<AutoOpen>]
module AsyncExtensions =

/// Type alias for F# mailbox processor type
  type Agent<'T> = MailboxProcessor<'T>