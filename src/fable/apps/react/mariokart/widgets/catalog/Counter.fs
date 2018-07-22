[<RequireQualifiedAccess>]
module Utilitarian.Mariokart.Widgets.Counter

module T = 
  type Model = {
    Count : float
    Step : float
    MinValue : float
    MaxValue : float
  }
  with 
    static member Default =  {
      Count=0.
      Step=1.
      MinValue= System.Double.MinValue
      MaxValue=System.Double.MaxValue
    }

  type MsgProtocol = 
    | Increment 
    | Decrement 
    | Reset 
    | SetStep of float
    | ChangeCount of float

[<RequireQualifiedAccess>]
module Logic = 

  open T

  let init (cfg : Model) = 
    cfg, []

  let update msg model =
    let nextModel = 
      match msg with 
      | Increment -> {model with Count=model.Count + model.Step}
      | Decrement -> {model with Count=model.Count - model.Step}
      | Reset -> Model.Default
      | SetStep n -> { model with Step = n }
      | ChangeCount n -> {model with Count=n}
    nextModel, []

module DomainExtensions = 

  open Utilitarian.Widgets
  
  type System.Double with
    static member CounterDarkWidget = Widget.createDark Logic.init Logic.update