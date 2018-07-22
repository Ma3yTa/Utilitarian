[<RequireQualifiedAccess>]
module Utilitarian.Mariokart.Widgets.AttributeScoreFilter

open Utilitarian.Mariokart
open Utilitarian.Widgets

module T =
  /// Auxilliary type to display and select a filter condition for set of attributes 
  type FilterCondition =
    | Smaller
    | Greater
  with
    override x.ToString() = 
      match x with
      | Smaller -> "<"
      | Greater -> ">"

  type Model = {
    /// here we do parent-child composition with the model type of Counter. 
    ThresholdCounter : Counter.T.Model
    Attributes : Attribute list
    Condition : FilterCondition
  }
  with
    static member Default = {
      ThresholdCounter = {Count=0.75;Step=0.25;MinValue=0.75;MaxValue=5.25}
      Attributes = []
      Condition = Greater
  }

  type MsgProtocol =
    | ChangeAttributes of Attribute list
    | ChangeCondition of FilterCondition
      /// here we do parent-child composition with the message protocol type of Counter. 
    | ThresholdCounterMsg of Counter.T.MsgProtocol

[<RequireQualifiedAccess>]
module Logic = 

  open T
  open Elmish

  let private parentChildComposition (cModel,cInsts) pModel =        
    {pModel with ThresholdCounter = cModel}, cInsts |> Cmd.map ThresholdCounterMsg
   
  let init childInit (cfg : Model) = 
    cfg |>parentChildComposition (cfg.ThresholdCounter |> childInit) 

  let update childUpdater msg model =
    match msg with
    | ChangeAttributes attrs -> {model with Attributes=attrs}, []
    | ChangeCondition newCond -> {model with Condition=newCond}, []
    | ThresholdCounterMsg counterMsg -> 
        model |> parentChildComposition (childUpdater counterMsg model.ThresholdCounter)

/// ### Reconcilation with domain types
/// Now we link back the widget back to domain types that are used in the UI interaction scenario captured by the widget.
/// To do so, we use [F# type extensions](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/type-extensions). 
/// Whenever we open the domain extension module of a widget, our types get enriched with what the widget has to offer in terms of our domain / business logic. 
/// From DDD perspective this is great, since we can just dot onto domain types to know about associated widgets we can reuse for our UI needs.
module DomainExtensions =

  open T

  type Model with
    member x.GetScoredAttributes  : ScoredAttribute list= 
      x.Attributes
      |> List.map (fun (attr : Attribute)-> attr,x.ThresholdCounter.Count)

  type Attribute with
    static member ScoreFilterDarkWidget = 
       
      Widget.createDark (Logic.init Counter.Logic.init) (Logic.update Counter.Logic.update)