namespace Utilitarian.Logging

open Utilitarian
open System
open System.Diagnostics

[<AutoOpen>]
module T =
  [<DebuggerDisplay("{StructuredFormatDisplay}")>]
  [<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
  type Level = Debug | Trace | Info | Warning | Error
  with    
    member x.StructuredFormatDisplay =
      match x with 
      | Debug -> "Debug"
      | Trace -> "Trace" 
      | Info -> "Info"
      | Warning -> "Warning"
      | Error -> "Error"
      |> String.upperCase

    override x.ToString () = x.StructuredFormatDisplay
    
    static member FromInt i = 
      match i with 
      | 0 -> Debug
      | 1 -> Trace
      | 2 -> Info
      | 3 -> Warning
      | 4 -> Error
      | _ -> Info
    
    member x.ToInt =
      match x with
      | Debug -> 0
      | Trace -> 1
      | Info -> 2
      | Warning -> 3
      | Error -> 4


  type DateTimeOffset with
    member dt.LoggingFormat = dt.ToString("fffffff.hh:mm:ss dd.MM.yyyy") 
    
  [<DebuggerDisplay("{StructuredFormatDisplay}")>]
  [<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
  type MsgAtomic = {
      Time : DateTimeOffset
      Level: Level
      Message: string
      Properties : (string*obj) seq
  }
  with
    member x.StructuredFormatDisplay' i  =
      
      let dtFormatter (dt : DateTimeOffset) = dt.ToString("fffffff.hh:mm:ss dd.MM.yyyy")
      sprintf "%s @ %s : %s%s%s%s" 
        (x.Level.StructuredFormatDisplay) 
        (x.Time.LoggingFormat) 
        (x.Message) (String.newlines 1) (String.spaces (i*2)) 
        (x.PrettyPrintProperties i)
    
    member x.StructuredFormatDisplay = x.StructuredFormatDisplay' 1
    
    override x.ToString() = x.StructuredFormatDisplay

    member x.PrettyPrintProperties i = 

      x.Properties 
      |> Seq.map (fun (k,v) -> 
        sprintf "%s%s%s%A" 
          k 
          (String.newlines 1) 
          (String.spaces ((i+1)*2)) 
          v
        )
      |> String.concat (String.newlines 1 + String.spaces (i*2))


  [<DebuggerDisplay("{StructuredFormatDisplay}")>]
  [<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
  type Msg = 
    | Atomic of MsgAtomic
    | Batch of DateTimeOffset * Msg seq
  with
    member x.StructuredFormatDisplay' i  =
      
      match x with
      | Atomic logMsg -> logMsg.StructuredFormatDisplay' i
      
      | Batch (batchTime,logMsgs) -> 
        
        let msgsString = 
          let sep = (String.newlines 1) + (String.spaces(i*2))
          logMsgs
          |> Seq.map (fun msg -> msg.StructuredFormatDisplay' (i+1))
          |> String.concat sep

        sprintf "BATCHED @ %s : %s%s%s" 
          (batchTime.LoggingFormat) 
          (String.newlines 1) (String.spaces(i*2)) 
          msgsString 

    member x.StructuredFormatDisplay = x.StructuredFormatDisplay' 1

    override x.ToString() = x.StructuredFormatDisplay

    member x.Time =
      match x with
      | Atomic msg -> msg.Time
      | Batch (timeProduced,_) -> timeProduced

    member x.Level =
      match x with
      | Atomic a -> a.Level
      | Batch (_,msgs)-> 
        msgs 
        |> Seq.map (fun msg -> msg.Level) 
        |> Seq.max 
      
  type ILogger =
    abstract member LogMessage :  Level -> string -> (string*obj) seq -> unit

  type ILogStream = IObservable<Msg>

  type EventSource () = 
    
    let sourceEvent = Event<Msg>()

    interface ILogStream with  
      member x.Subscribe observer = 
        sourceEvent.Publish
        |> Observable.subscribe (observer.OnNext)

    interface ILogger with 
      member x.LogMessage level msg prop =

        sourceEvent.Trigger(Atomic {Properties=prop;Level = level;Message = msg;Time = DateTimeOffset.UtcNow})

  [<CustomEquality;CustomComparison>]
  type Processor = {
    Id : string
    Transformator : ILogStream -> ILogStream
    ProcessingAction : Msg -> Async<unit>
  }
  with
    override x.Equals y = equalsOn (fun p -> p.Id ) x y
    override x.GetHashCode() = hashOn (fun p -> p.Id ) x

    interface System.IComparable with
      member x.CompareTo y = compareOn (fun p -> p.Id ) x y

  type PipeMsg =
    | AddProcessors of Processor seq
    | RemoveProcessors of Processor seq
    | Message of Msg
    | Cleanup of AsyncReplyChannel<unit>

  type Pipe(processors : Processor seq, ?source, ?scheduler) = 

    let source = defaultArg source (EventSource())
    let scheduler = defaultArg scheduler Async.Start

    let subscribeProcessors (processors : Processor seq ) (a : Agent<PipeMsg>) =
      processors 
      |> Seq.map (fun processor -> 
        let stream = source :> ILogStream |> processor.Transformator
        let processorSub = 
          stream |> Observable.subscribe (Message >> a.Post)
        processor, processorSub)
      |> List.ofSeq      

    let agent = Agent.Start(fun mbox -> 
       
      let rec loop (subscribedProcessors : (Processor * IDisposable) list) = async {
        
        let! msg = mbox.Receive()
        
        match msg with
        
        | Message logMsg ->
            
            subscribedProcessors |> Seq.iter (fun ({ProcessingAction=action},_) -> 
              logMsg |> action |> scheduler
            )
            return! loop(subscribedProcessors)     
          
        | AddProcessors processors -> 
          
          let newProcessors = 
            processors 
            |> Seq.filter (fun newP -> 
              subscribedProcessors 
              |> Seq.map (fun (p,_) -> p) 
              |> Seq.contains newP |> not)
          
          let newSubscriptions = 
            subscribeProcessors newProcessors mbox

          return! loop(subscribedProcessors@newSubscriptions)
          
        | RemoveProcessors processors ->
          
          let filter = (fun (proc,_) -> processors |> Seq.contains proc)
          let toDispose = subscribedProcessors |> Seq.filter filter
          let toKeep = subscribedProcessors |> Seq.filter (filter>>not)
          
          do 
            toDispose 
            |> Seq.iter (fun (_,disposable) -> disposable.Dispose())
          
          return! loop (toKeep |> List.ofSeq)

        | Cleanup ch-> 
          
          do
            subscribedProcessors 
            |> Seq.iter (fun (d,disp) ->disp.Dispose())
            
            ch.Reply()
          
          return! loop([])
          
      }

      loop (subscribeProcessors processors mbox)
    )

    member x.AddProcessors (processors) = agent.Post (AddProcessors processors)

    member x.AddProcessor processor = x.AddProcessors([processor])

    member x.RemoveProcessors (processors) = agent.Post (RemoveProcessors processors)

    member x.RemoveProcessor processor = x.RemoveProcessors([processor])

    interface IDisposable with  
      member x.Dispose() = 
        async {
          do! agent.PostAndAsyncReply (Cleanup)
        }
        |> Async.Start


[<RequireQualifiedAccess>]
module Log = 

  let inline trace p msg (logger : #ILogger) =
  #if DEBUG || TRACE
    logger.LogMessage Level.Trace p msg
  #else
    ()
  #endif

  let inline info p msg (logger : #ILogger) = 
    logger.LogMessage Level.Info p msg

  let inline warning p msg (logger : #ILogger) =
    logger.LogMessage Level.Warning p msg 

  let inline error p msg (logger : #ILogger) =
    logger.LogMessage Level.Error p msg 

  let inline debug p msg (logger : #ILogger) =
  #if DEBUG
    logger.LogMessage Level.Debug p msg
  #else
    ()
  #endif

  let eventSourcePipe initialProcessors= 
    let src = new EventSource()
    new Pipe (initialProcessors,src), (src :> ILogger)

  let (stdPipe, std) = eventSourcePipe []

  let inline stdTrace p msg = trace p msg std

  let inline stdInfo p msg = info p msg std

  let inline stdWarning p msg= warning p msg std

  let inline stdError p msg= error p msg std

  let inline stdDebug p msg= debug p msg std

  let inline stdExn (e : exn) = stdError "An exception occured" ["exn",e |> box]

  let withStdExn f x =
    try
      x |> f
    with e -> 
      e |> stdExn
      raise e

  let withStdExnAsync asyncC =
    let wrapped = async {
      try 
        return! asyncC
      with e -> 
        do e |> stdExn
        return e |> raise      
    }
    Async.TryCancelled(wrapped,(fun cancelExn -> cancelExn |> stdExn))


[<RequireQualifiedAccess>]
module Processors = 

  let [<Literal>] ConsolePlain = "console plain"

  let consolePlain = {
    Id=ConsolePlain
    Transformator = id
    ProcessingAction = fun msg -> async { do printfn "%A" msg }
  }