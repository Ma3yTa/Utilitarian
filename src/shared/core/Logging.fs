[<AutoOpen>]
module Utilitarian.Logging 

open System
open System.Diagnostics

[<DebuggerDisplay("{LoggingFormat}")>]
[<StructuredFormatDisplay("{LoggingFormat}")>]
type DateTimeOffset with
  member x.LoggingFormat = x.ToString("fffffff.hh:mm:ss dd.MM.yyyy")

[<DebuggerDisplay("{StructuredFormatDisplay}")>]
[<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
type LogLevel = Debug | Trace | Info | Warning | Error
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

[<DebuggerDisplay("{StructuredFormatDisplay}")>]
[<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
type LogMessageAtomic = {
    Time : DateTimeOffset
    Level: LogLevel
    Message: string
    Properties : List<string*obj>
}
with
  member x.StructuredFormatDisplay' i  =
    sprintf "%s @ %s : %s%s%s%s" 
      (x.Level.StructuredFormatDisplay) 
      (x.Time.LoggingFormat) 
      (x.Message) (String.newline 1) (String.indentSpaces (i*2)) 
      (x.PrettyPrintProperties i)
  
  member x.StructuredFormatDisplay = x.StructuredFormatDisplay' 1
  
  override x.ToString() = x.StructuredFormatDisplay

  member x.PrettyPrintProperties i = 
    x.Properties 
    |> List.map (fun (k,v) -> 
      sprintf "%s%s%s%A" 
        k 
        (String.newline 1) 
        (String.indentSpaces ((i+1)*2)) 
        v
      )
    |> String.concat (String.newline 1 + String.indentSpaces (i*2))


[<DebuggerDisplay("{StructuredFormatDisplay}")>]
[<StructuredFormatDisplay("{StructuredFormatDisplay}")>]
type LogMessage = 
  | Atomic of LogMessageAtomic
  | Batch of DateTimeOffset * LogMessage list
with
  member x.StructuredFormatDisplay' i  =
    match x with
    | Atomic logMsg -> logMsg.StructuredFormatDisplay' i
    | Batch (batchTime,logMsgs) -> 
      let msgsString = 
        let sep = (String.newline 1) + (String.indentSpaces(i*2))
        logMsgs
        |> Seq.map (fun msg -> msg.StructuredFormatDisplay' (i+1))
        |> String.concat sep

      sprintf "BATCHED @ %s : %s%s%s" 
        (batchTime.LoggingFormat) 
        (String.newline 1) (String.indentSpaces(i*2)) 
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
      |> List.map (fun msg -> msg.Level) 
      |> List.max 
        
type ILogger =
  abstract member LogMessage :  LogLevel -> string -> List<string*obj> -> unit

let inline logTrace msg p (logger : #ILogger)=  
  logger.LogMessage LogLevel.Trace msg p

let inline logInfo msg p (logger : #ILogger) =  
  logger.LogMessage LogLevel.Info msg p

let inline logWarning msg p (logger : #ILogger)=  
  logger.LogMessage LogLevel.Warning msg p 

let inline logError msg p (logger : #ILogger)=  
  logger.LogMessage LogLevel.Error msg p 

let inline logDebug msg p (logger : #ILogger)=
#if DEBUG
  logger.LogMessage LogLevel.Debug msg p
#else
  ()
#endif

type ILogStream = IObservable<LogMessage>

type LogSource (id) = 
  
  let eventSource = Event<LogMessage>()

  interface ILogStream with  
    member x.Subscribe observer = 
      eventSource.Publish
      |> Observable.subscribe (observer.OnNext)

  interface ILogger with 
    member x.LogMessage level msg prop=
      eventSource.Trigger(Atomic {Properties=("source",id |> box)::prop;Level = level;Message = msg;Time = DateTimeOffset.UtcNow})

[<CustomEquality;CustomComparison>]
type LogProcessor = {
  Id : string
  Transformator : ILogStream -> ILogStream
  ProcessingAction : LogMessage -> Async<unit>
}
with
  override x.Equals y = equalsOn (fun p -> p.Id ) x y
  override x.GetHashCode() = hashOn (fun p -> p.Id ) x

  interface System.IComparable with
    member x.CompareTo y = compareOn (fun p -> p.Id ) x y

type LogPipeMessage =
  | AddProcessors of LogProcessor seq
  | RemoveProcessors of LogProcessor seq
  | RemoveProcessorById of string
  | Message of LogMessage
  | Cleanup of AsyncReplyChannel<unit>

type LogPipe(processors : LogProcessor seq, source : LogSource) = 
    
  let subscribeProcessors (processors : LogProcessor list ) (a : MailboxProcessor<LogPipeMessage>) =
    processors 
    |> List.map (fun processor -> 
      let stream = source :> ILogStream |> processor.Transformator
      let sub = 
        stream |> Observable.subscribe (fun msg -> 
          msg |> Message |> a.Post)
      processor, sub)

  let agent = AutoCancelAgent.Start(fun mbox -> 
     
    let rec loop (subscribedProcessors : (LogProcessor * IDisposable) list) = async {
      let! msg = mbox.Receive()
      match msg with
      | Message logMsg ->
          subscribedProcessors |> Seq.iter (fun ({ProcessingAction=action},_) -> 
            logMsg |> action |> Async.Start
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
          subscribeProcessors (newProcessors |> List.ofSeq) mbox

        return! loop(subscribedProcessors@(newSubscriptions |> List.ofSeq))
        
      | RemoveProcessors processors ->
        let filter = (fun (proc,_) -> processors |> Seq.contains proc)
        let toDispose = subscribedProcessors |> Seq.filter filter
        let toKeep = subscribedProcessors |> Seq.filter (filter>>not)
        do 
          toDispose 
          |> Seq.iter (fun (_,disposable) -> disposable.Dispose())
        return! loop(toKeep |> List.ofSeq)

      | RemoveProcessorById id -> 
        let withIdRemoved = [
          for (p,disp) in subscribedProcessors do
            if p.Id = id then
              do disp.Dispose()
            else
              yield (p,disp)
        ]
        return! loop(withIdRemoved)

      | Cleanup ch-> 
        subscribedProcessors 
        |> Seq.iter (fun (_,disposable) -> disposable.Dispose())
        ch.Reply()
        return! loop([])        
    }

    loop (subscribeProcessors (processors |> List.ofSeq) mbox)
  )

  member x.AddProcessors (processors) = agent.Post (AddProcessors processors)

  member x.AddProcessor processor = x.AddProcessors([processor])

  member x.RemoveProcessors (processors) = agent.Post (RemoveProcessors processors)

  member x.RemoveProcessorById id = agent.Post (RemoveProcessorById id)

  interface IDisposable with  
    member x.Dispose() = 
      async {
        do! agent.PostAndAsyncReply (Cleanup)
        (agent :> IDisposable).Dispose()
      }
      |> Async.Start

let createLogPipe sourceId initialProcessors= 
  let src = new LogSource(sourceId)
  new LogPipe (initialProcessors,src), (src :> ILogger)

let (stdLogPipeline, stdLogger) = createLogPipe "std" []

let inline stdLogTrace msg p =  
  logTrace msg p stdLogger

let inline stdLogInfo msg p =  
  logInfo msg p stdLogger

let inline stdLogWarning msg p=  
  logWarning msg p stdLogger

let inline stdLogError msg p=  
  logError msg p stdLogger

let inline stdLogDebug msg p=
  logDebug msg p stdLogger

let inline stdLogExn (e : exn) = stdLogError "An Exception occured" ["exn",e |> box]

let stdLogExnF f x =
  try
    x |> f
  with e -> 
    e |> stdLogExn
    raise e

let stdLogExnAsync asyncC =
  let wrapped = async {
    try 
      return! asyncC
    with e -> 
      do e |> stdLogExn
      return e |> raise      
  }
  Async.TryCancelled(wrapped,(fun cancelExn -> cancelExn |> stdLogExn))
