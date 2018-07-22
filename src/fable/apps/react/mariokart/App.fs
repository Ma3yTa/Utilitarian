module Utilitarian.Apps.Mariokart.React

open Fable.Core.JsInterop

open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

do importAll "./node_modules/antd/dist/antd.less"
open Utilitarian.Mariokart.Widgets.AntD

open Utilitarian.Mariokart.Widgets
open Utilitarian.Widgets


type CounterModel = Counter.T.Model
type ScoreFilterModel = AttributeScoreFilter.T.Model

let demoCounter = 
  Catalog.counter 
  |> Widget.embedConfig CounterModel.Default

let demoScoreFilter = 
  Catalog.attributeFilter 
  |> Widget.embedConfig ScoreFilterModel.Default

let demoCounterController = 
  let dataflow = seq {
    for _ in 0..5 do
    yield CounterModel.Default
  }
  let widgetGenerator model = Catalog.counter |> Widget.embedConfig model
  let sortByCount (data : CounterModel) = data.Count
  let filterHighCounts (data : CounterModel) = data.Count > 3.
  
  Catalog.seqDataflowController widgetGenerator sortByCount filterHighCounts
  |> Widget.embedConfig dataflow

let demoScoreFilterController = 
  
  let dataflow = seq {
    for _ in 0..3 do
    yield ScoreFilterModel.Default
  }
  
  let widgetGenerator model = Catalog.attributeFilter |> Widget.embedConfig model
  let sortByCount (data : ScoreFilterModel) = data.ThresholdCounter.Count
  let filterHighCounts (data : ScoreFilterModel) = data.ThresholdCounter.Count > 1.
  
  Catalog.seqDataflowController widgetGenerator sortByCount filterHighCounts
  |> Widget.embedConfig dataflow


demoScoreFilterController
|> Widget.mkProgram
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
