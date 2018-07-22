[<AutoOpen>]
module Utilitarian.Prelude 

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Linq.Expressions
open System.Threading.Tasks 
open System.Diagnostics
open System.Threading

let inline nameof<'T> = typeof<'T>.Name

let inline nullable v = System.Nullable<_>(v)

let inline dispose (x : #IDisposable) = x.Dispose()

let inline multiDispose xs = xs |> Seq.iter dispose

let inline flip f x y = f y x

let inline flip132 f x y z = f x z y

let inline flip213 f x y z = f y x z

let inline flip231 f x y z = f y z x

let inline flip312 f x y z = f z x y

let inline flip321 f x y z = f z y x

let inline curry f x y = f(x,y)

let inline curry2 f x y z = f(x,y,z)

let inline uncurry f (x,y) = f x y

let inline func1 f = Func<_>(f)
let inline func2 f = Func<_,_>(f)
let inline func3 f = Func<_,_,_>(f)
let inline func4 f = Func<_,_,_,_>(f)
let inline func5 f = Func<_,_,_,_,_>(f)

let impossible _ = failwith "Logical Error: This should be impossible"
  
let inline id x = x 

let cachedFunction<'TKey,'TVal> constructr =
  let cache : ConcurrentDictionary<'TKey,'TVal> = ConcurrentDictionary()
  fun key -> cache.GetOrAdd(key, System.Func<_, _>(constructr))

let cachedAsync<'TKey,'TVal> (constructr : 'TKey -> Async<'TVal>) = 
  let cache : ConcurrentDictionary<'TKey,'TVal> = ConcurrentDictionary()
  fun key -> async {
    match cache.TryGetValue(key) with
    | true, value -> return value 
    | false, _ -> 
      let! value = constructr key
      let _ = cache.TryAdd(key,value)
      return value    
  }

let inline seconds n  = n |> float |> TimeSpan.FromSeconds
let inline minutes n  = n |> float |> TimeSpan.FromMinutes

let inline (|NotNull|_|) value =
  if value |> isNull then
    None
  else
    Some value          

let equalsOn f x (yobj:obj) =
  match yobj with
  :? 'T as y -> (f x = f y)
  | _ -> false

let hashOn f x =  hash (f x)

let compareOn f x (yobj: obj) =
  match yobj with
  | :? 'T as y -> compare (f x) (f y)
  | _ -> invalidArg "yobj" "cannot compare values of different types"

let inline enumStr<'Enum> e = System.Enum.GetName(typeof<'Enum>,e)
 
let inline lambdaExpr<'T> = Expression.Lambda<Func<'T>>

let inline propertyExpr (pName : string) objWithP = 
  Expression.Property (
    Expression.Constant objWithP,
    pName
)

let inline propertyAccessExpr<'T> (pName : string) objWithP =
  let pExpr = propertyExpr pName objWithP
  lambdaExpr<'T> (pExpr)

[<RequireQualifiedAccess>]
module Choice =
  let chooseFirst x = 
    match x with 
    | Choice1Of2 value -> Some value
    | Choice2Of2 _ -> None

  let chooseSecond x = 
    match x with 
    | Choice1Of2 _ -> None
    | Choice2Of2 value -> Some value

[<RequireQualifiedAccess>]
module Array =

  let inline tuple2 arr = 
    match arr with 
    | [|first;second|] -> Some (first,second)
    | _ -> None

  let inline tuple3 arr = 
    match arr with 
    | [|first;second;third|] -> Some (first,second,third)
    | _ -> None  

  let inline (|Tuple2|_|) arr = tuple2 arr

  let inline (|Tuple3|_|) arr = tuple3 arr

[<RequireQualifiedAccess>]
module String =

  let [<Literal>] Empty = ""

  let inline newline j = 
    [
      for i = 1 to j do yield Environment.NewLine
    ]
    |> String.concat ""
    
  let inline indentSpaces i = String(' ',i)

  let inline isNullOrEmpty str = 
    match str with
    | Empty | null -> true
    | _ -> false

  let inline isNonEmpty str = str |> isNullOrEmpty |> not

  let inline (|NonEmpty|_|) str =
    if str |> isNonEmpty then 
      Some str
    else
      None

  let inline fromChars (chars : #seq<char>) = 
    System.String(chars |> Array.ofSeq)
    
  let inline lowerCase (x : string) =
    x.ToLowerInvariant()
    
  let (|LowerCase|) = lowerCase 

  let inline upperCase (x : string) =
    x.ToUpperInvariant()
    
  let (|UpperCase|) = upperCase

[<RequireQualifiedAccess>]
module Option =
  
  let inline foldToString folder opt = 
    Option.fold folder (String.Empty) opt

  let flattenList (optionList : list<_> option) =
    match optionList with
    | Some l -> l
    | None -> []

  let flattenBool (optionBool : bool option) = 
    match optionBool with
    | Some b -> b
    | None -> false

  let force (option : Option<_>) =
    option.Value

[<RequireQualifiedAccess>]
module Seq =

  let toIList (seq : #seq<_>) =
    System.Collections.Generic.List(seq) :> IList<_>

    
type OptionBuilder() =
  member this.Return(x) = Some x

  member this.ReturnFrom(m: 'T option) = m

  member this.Bind(m, f) = Option.bind f m

  member this.Zero() = None

  member this.Combine(m, f) = Option.bind f m

  member this.Delay(f: unit -> _) = f

  member this.Run(f) = f()

  member this.TryWith(m, h) =
      try this.ReturnFrom(m)
      with e -> h e

  member this.TryFinally(m, compensation) =
      try this.ReturnFrom(m)
      finally compensation()

  member this.Using(res:#IDisposable, body) =
      this.TryFinally(body res, fun () -> match res with null -> () | disp -> disp.Dispose())

  member this.While(guard, f) =
      if not (guard()) then Some () else
      do f() |> ignore
      this.While(guard, f)

  member this.For(sequence:seq<_>, body) =
      this.Using(sequence.GetEnumerator(),
        fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> body enum.Current)))

let optional : OptionBuilder = OptionBuilder()

[<RequireQualifiedAccess>]
module Async =
      
  let retn x = async { return x }

  let apply af ax = async {
      // We start both async task in Parallel
      let! pf = Async.StartChild af
      let! px = Async.StartChild ax
      // We then wait that both async operations complete
      let! f = pf
      let! x = px
      // Finally we execute (f x)
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

type Microsoft.FSharp.Control.AsyncBuilder with
  /// An extension method that overloads the standard 'Bind' of the 'async' builder. The new overload awaits on
  /// a standard .NET task
  member inline x.Bind(t : Task<'T>, f:'T -> Async<'R>) : Async<'R> =
    async.Bind(Async.AwaitTask t, f)

  /// An extension method that overloads the standard 'Bind' of the 'async' builder. The new overload awaits on
  /// a standard .NET task which does not commpute a value
  member inline x.Bind(t : Task, f : unit -> Async<'R>) : Async<'R> =
    async.Bind(Async.AwaitTask t, f)


/// Type alias for F# mailbox processor type
type Agent<'T> = MailboxProcessor<'T>


/// Wrapper for the standard F# agent (MailboxProcessor) that
/// supports stopping of the agent's body using the IDisposable 
/// interface (the type automatically creates a cancellation token)
type AutoCancelAgent<'T> private (mbox:Agent<'T>, cts:CancellationTokenSource) = 

  /// Start a new disposable agent using the specified body function
  /// (the method creates a new cancellation token for the agent)
  static member Start(f) = 
    let cts = new CancellationTokenSource()
    new AutoCancelAgent<'T>(Agent<'T>.Start(f, cancellationToken = cts.Token), cts)
  
  /// Returns the number of unprocessed messages in the message queue of the agent.
  member x.CurrentQueueLength = mbox.CurrentQueueLength

  /// Occurs when the execution of the agent results in an exception.
  [<CLIEvent>]
  member x.Error = mbox.Error

  /// Waits for a message. This will consume the first message in arrival order.
  member x.Receive(?timeout) = mbox.Receive(?timeout = timeout)

  /// Scans for a message by looking through messages in arrival order until <c>scanner</c> 
  /// returns a Some value. Other messages remain in the queue.
  member x.Scan(scanner, ?timeout) = mbox.Scan(scanner, ?timeout = timeout)

  /// Like PostAndReply, but returns None if no reply within the timeout period.
  member x.TryPostAndReply(buildMessage, ?timeout) = 
    mbox.TryPostAndReply(buildMessage, ?timeout = timeout)

  /// Waits for a message. This will consume the first message in arrival order.
  member x.TryReceive(?timeout) = 
    mbox.TryReceive(?timeout = timeout)

  /// Scans for a message by looking through messages in arrival order until <c>scanner</c> 
  /// returns a Some value. Other messages remain in the queue.
  member x.TryScan(scanner, ?timeout) = 
    mbox.TryScan(scanner, ?timeout = timeout)

  /// Posts a message to the message queue of the MailboxProcessor, asynchronously.
  member x.Post(m) = mbox.Post(m)

  /// Posts a message to an agent and await a reply on the channel, synchronously.
  member x.PostAndReply(buildMessage, ?timeout) = 
    mbox.PostAndReply(buildMessage, ?timeout = timeout)

  /// Like PostAndAsyncReply, but returns None if no reply within the timeout period.
  member x.PostAndTryAsyncReply(buildMessage, ?timeout) = 
    mbox.PostAndTryAsyncReply(buildMessage, ?timeout = timeout)

  /// Posts a message to an agent and await a reply on the channel, asynchronously.
  member x.PostAndAsyncReply(buildMessage, ?timeout) = 
    mbox.PostAndAsyncReply(buildMessage, ?timeout=timeout)

  interface IDisposable with
    member x.Dispose() = 
      (mbox :> IDisposable).Dispose()
      cts.Cancel()

let inline floatWithUnit<[<Measure>] 'U> = 
  LanguagePrimitives.FloatWithMeasure<'U>

type [<Measure>] bytes
type [<Measure>] ``%``

[<DebuggerDisplay("{StructuredFormatDisplay}")>]
[<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
type ProgressM<[<Measure>] 'U> = { 
  Done : float<'U>
  Total : float<'U>  
}
with 
  member x.Ratio =
    let t = x.Total
    if (t |> float = 0.) then
      None
    else
      (x.Done)/(x.Total) |> Some
  
  member x.InPercent = optional {
      let! ratio = x.Ratio
      return (ratio * 100.) |> floatWithUnit<``%``> 
  }

  member x.PercentLeft = optional {
      let! percent = x.InPercent
      return 100.<``%``> - percent
  }

  member x.AsStringInPercent =
    match x.InPercent with
    | Some progressPercent -> sprintf "%i %%" (progressPercent |> int)
    | None -> "undefined"