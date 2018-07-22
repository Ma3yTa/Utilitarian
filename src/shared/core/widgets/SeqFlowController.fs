  /// #### Idea
  /// exploit fractality by combining with a undo flow controller to unset active mappings or filters.
[<RequireQualifiedAccess>]
module Utilitarian.Widgets.SeqFlowController

open Elmish
open Utilitarian.Widgets

type CtrlMsg<'TMsgProtocol, 'TModel, 'TKey> = 
  | Dispatch of int * Message: 'TMsgProtocol
  | SortBy of Projection : ('TModel -> 'TKey)
  | SortWith of Comparer : ('TModel -> 'TModel -> int)
  | Filter of Predicate : ('TModel -> bool)
  | ModelMap of Mapping : ('TModel -> 'TModel)
  | ModelMapi of Mapping : (int-> 'TModel -> 'TModel) 

let dispatch i msg  = Dispatch(i,msg)
let filter predicate = Filter predicate
let sortBy projection = SortBy projection
let sortWith comparer = SortWith comparer
let modelMapi fi = ModelMapi fi 
let modelMap f = ModelMap f  

type T<'TMsgProtocol,'TModel,'TRenderElement,'TKey> = 
  IWidget<'TModel seq, CtrlMsg<'TMsgProtocol, 'TModel, 'TKey>,'TModel seq,'TRenderElement seq>

let fromWidgetGenerator
  (widgetGenerator : 'TModel -> IConfiguredWidget<'TMsgProtocol,'TModel,'TRenderElement>)
  : T<'TMsgProtocol,'TModel,'TRenderElement,'TKey> =

  let init dataflow =

    let initialized = 
      dataflow
      |> Seq.mapi (fun i flowValue  -> 
        let initializer = (widgetGenerator flowValue).Initializer
        let m,inst = initializer ()
        m, inst |> Cmd.map (dispatch i))

    initialized |> Seq.map (fun (model,_) -> model),
    initialized  |> Seq.collect (fun (_,inst) -> inst) |> List.ofSeq

  let update controlMsg (dataflow : 'TModel seq) =
    
    match controlMsg with
    
    | Dispatch (i,msg) -> 
      let dispatched = 
        dataflow
        |> Seq.mapi (fun j flowValue -> 
          if i = j then
            let updater = (widgetGenerator flowValue).Updater
            let flowData', insts = updater msg flowValue
            flowData', insts |> Cmd.map (dispatch i)
          else
            flowValue,[])

      dispatched |> Seq.map (fun (model,_) -> model ),
      dispatched |> Seq.collect (fun (_,inst) -> inst) |> List.ofSeq

    | SortWith comparer -> 
        dataflow |> Seq.sortWith comparer,[]

    | SortBy projection ->
        dataflow |> Seq.sortBy projection,[]
    
    | Filter predicate ->
        dataflow |> Seq.filter predicate,[]

    | ModelMapi fi ->
        dataflow |> Seq.mapi fi,[]
    
    | ModelMap f -> 
        dataflow |> Seq.map f,[]

  let render dataflow feedbackChannelT =
    dataflow
    |> Seq.mapi (fun i flowValue ->
      let renderer = (widgetGenerator flowValue).Renderer
      renderer flowValue ((dispatch i) >> feedbackChannelT))

  Widget.create init update render
  