namespace Utilitarian

[<RequireQualifiedAccess>]
module Option =
  
  let inline flattenList (optionList : list<_> option) =
    match optionList with
    | Some l -> l
    | None -> []

  let inline applyPredicate (f : 'T -> bool) x = 
    match x with
    | Some value -> value |> f
    | None -> false

  let inline flattenBool x = x |> applyPredicate id

  let inline force (option : Option<_>) =
    option.Value


[<AutoOpen>]
module OptionBuilderExtension =
  
  open System

  type OptionBuilder() =
    member __.Return(x) = Some x

    member __.ReturnFrom(m: 'T option) = m

    member __.Bind(m, f) = Option.bind f m

    member __.Zero() = None

    member __.Combine(m, f) = Option.bind f m

    member __.Delay(f: unit -> _) = f

    member __.Run(f) = f()

    member this.TryWith(m, h) =
        try this.ReturnFrom(m)
        with e -> h e

    member this.TryFinally(m, compensation) =
        try this.ReturnFrom(m)
        finally compensation()

    member this.Using(res:#IDisposable, body) =
        this.TryFinally(body res, fun () -> 
          match res with 
          | NotNull resource -> resource |> dispose
          | _ -> ()
        )

    member this.While(guard, f) =
        if not (guard()) then Some () else
        do f() |> ignore
        this.While(guard, f)

    member this.For(sequence:seq<_>, body) =
        this.Using(sequence.GetEnumerator(),
          fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> body enum.Current)))

  let optional : OptionBuilder = OptionBuilder()