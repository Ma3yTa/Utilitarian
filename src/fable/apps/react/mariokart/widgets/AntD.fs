module Utilitarian.Mariokart.Widgets.AntD

module R = Fable.Helpers.React
module P = Fable.Helpers.React.Props
module B = Fable.AntD.Button
module C = Fable.AntD.Card
module F = Fable.AntD.Form
module L = Fable.AntD.List
module Grid = Fable.AntD.Grid
module InpN = Fable.AntD.InputNumber
module S = Fable.AntD.Select

let private c1 = Fable.Core.Case1

// https://ant.design/components/grid/#components-grid-demo-gutter
let xs = Grid.Xs (c1 8)
let sm = Grid.Sm (c1 16)
let md = Grid.Md (c1 24)
let lg = Grid.Lg (c1 32)

module private Renderers =

  open Fable.Core
  open Utilitarian.Mariokart
  open Utilitarian.Mariokart.Widgets
  open Utilitarian.Fable.Parsimmon.Mariokart
  
  open Utilitarian.Mariokart.Widgets.Counter.T
  let counter (model : Model) feedbackChannel =

    let reportInputChanges value = 
      match value with
      | U2.Case1 f -> f
      | U2.Case2 n -> n |> float
      |> ChangeCount
      |> feedbackChannel

    InpN.inputNumber 
      [
        InpN.OnChange reportInputChanges
        InpN.Min model.MinValue
        InpN.Max model.MaxValue
        InpN.Step (U2.Case1 model.Step)
        InpN.Value model.Count
        InpN.Precision 2.
      ] []

  open Utilitarian.Mariokart.Widgets.AttributeScoreFilter.T
  let attributeScoreFilter (model : Model) feedbackChannel =
        
    let selectFilterAttributes =

      let reportAttributesChange values = 
        
        values
        |> List.choose (fun value ->  
          match value with
          | U4.Case1 str -> str |> Attribute.FromCanonicalString
          | _ -> None)
        |> ChangeAttributes
        |> feedbackChannel
       
      let style = 
        [ 
          if model.Attributes = List.Empty then
            yield P.CSSProp.Width (box 175)
        ]

      S.select 
        [
          P.Style style 
          S.Mode S.Tag 
          S.OnChange reportAttributesChange
        ] 

        [
          for attr in Attribute.tellAllValues do
            let canonicalStr = attr |> Attribute.canonicalString 
            let key = S.Value (c1 canonicalStr)
            let value = R.str canonicalStr
            yield S.option [key] [value]
        ]

    let btnToggleCondition =
    
      let style = 
        [
          P.CSSProp.MarginRight (box 10)
        ]

      let reportConditionChange _  =
 
        match model.Condition with
          | Greater -> Smaller
          | Smaller ->  Greater 
          |> ChangeCondition 
          |> feedbackChannel

      B.button 
        [
          B.Type B.Ghost
          P.OnClick reportConditionChange
          P.Style style
        ] 

        [
          R.str (string model.Condition)
        ]

    /// Here we do parent child composition with the renderer of the counter widget we declared above.
    /// we need to adapt the UI feedback channel with function composition to fit the child message protocol.
    let thresholdCounter =  
      counter 
        model.ThresholdCounter 
        (ThresholdCounterMsg>>feedbackChannel)

    F.form [F.Layout F.Inline] [

      
      F.formItem [F.Label <| c1 "Points"; F.Colon false] 
        [btnToggleCondition; thresholdCounter]

      F.formItem [F.Label <| c1 "Attributes"; F.Colon false]
        [selectFilterAttributes]

    ]


[<AutoOpen>]
module Catalog = 

  open Utilitarian.Mariokart

  open Utilitarian.Widgets
  open Utilitarian.Mariokart.Widgets.Counter.DomainExtensions
  open Utilitarian.Mariokart.Widgets.AttributeScoreFilter.DomainExtensions

  let counter = 
    System.Double.CounterDarkWidget
    |> Widget.withRenderer Renderers.counter

  let attributeFilter = 
    Attribute.ScoreFilterDarkWidget
    |> Widget.withRenderer Renderers.attributeScoreFilter

  let seqDataflowController widgetGenerator sortProjection filterPredicate=
    
    SeqFlowController.fromWidgetGenerator widgetGenerator 
    |> Widget.mapRenderedElement (fun flowPermutation _ fbChannel ->
        
      let reportSortRequest _ =
        SeqFlowController.sortBy sortProjection
        |> fbChannel

      let reportFilterRequest _ =
        SeqFlowController.filter filterPredicate
        |> fbChannel

      let btnSort = B.button [P.OnClick reportSortRequest] [R.str "Sort"]
      let btnFilter = B.button [P.OnClick reportFilterRequest] [R.str "Filter"]

      R.div [P.Class "elmish-flow-controller-seq"] 
        [
          btnSort
          btnFilter
          
          flowPermutation
          |> Seq.map (fun elt -> L.item [] [elt])
          |> List.ofSeq
          |> L.list []
        ]
    )