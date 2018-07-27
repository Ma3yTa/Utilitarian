namespace Utilitarian.Widgets

open Elmish

[<RequireQualifiedAccess>]
/// ### Composing with Widgets
/// In this module you find composition strategies that work universally on all values of type `IWidget`.
/// You can't mess anything up when you compose with functions from this module, the F# compiler service would always redline you.
module Widget = 
  
  /// ### Description
  /// Creates a widget from 3 loose functions. 
  /// The compiler checks their type signature for cross-compatbility, i.e. whether they agree with the `IWidget` protocol.
  let create init update render  =
    {
      new IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput> with 
        member  __.Initializer = init
        member __.Updater = update
        member __.Renderer = render
    }

  /// ### Description
  /// Creates a dark widget from 2 loose functions (a widget with `unit` as render output). 
  /// The compiler checks their type signature for cross-compatbility, i.e. whether they agree with the `IDarkWidget` protocol.
  /// ### When should I use this?
  /// Whenever you want to declare a widget in shared code without a renderer attached.
  /// Usually the renderers will contain most of platform-specific code by introducing the native UI framework into the widget flow (e.g. React) 
  let createDark init update : IDarkWidget<'WidgetCfg,'MsgProtocol,'Model> = 
    create init update (fun _ _ -> ()) 

  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a different way to create the initial widget state.
  let withInitializer init (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) = 
    create init (sourceWidget.Updater) (sourceWidget.Renderer)

  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a different way to create flowtime updates
  /// A single runtime update consists of a new model + runtime instructions (leads to incoming messages **eventually**).
  let withUpdater updater (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) = 
    create (sourceWidget.Initializer) updater (sourceWidget.Renderer)

  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a different way to turn flow into render output.
  /// That means the new widget will have another way to render the data model and a different way to handle feedback.
  /// ### When should I use this?
  /// Usually when things are getting platform-specific. You'll most likely start from a dark widget (a widget that doesn't render anything) defined in shared code.
  /// You can elevate a dark widget using this function together with a compatible renderer.
  /// You will usually define one per platform and design-system. To do so, use this function to get a widget that renders the flow logic defined in shared code employing the native UI framework engrained in the renderer.
  /// The F# type system does all the compatibility checks for you. It is impossible to elevate a dark widget with an incompatible renderer. 
  let withRenderer renderer (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) = 
    create (sourceWidget.Initializer) (sourceWidget.Updater) renderer

  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a configuration object baked into the widget context.
  /// ### When should I use this?
  /// When you want to store a widget as a value that is fully self-contained and shouldn't be configurable from extern anymore.
  let withEmbeddedConfig cfg (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>)  : IConfiguredWidget<_,_,_> =
    create ((fun () -> cfg)>>sourceWidget.Initializer) (sourceWidget.Updater) (sourceWidget.Renderer)

  let withMappedFlow 
    (msgMap : 'MsgProtocol -> 'MsgProtocolMapped)
    (msgMapRev : 'MsgProtocolMapped -> 'MsgProtocol)
    (modelMap : 'Model -> 'ModelMapped)
    (modelMapRev : 'ModelMapped -> 'Model)
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) =

    let suffixInitUpdate (model,flowAcc) =  
      model |> modelMap, flowAcc |> Cmd.map msgMap
    
    let init cfg = 
      cfg |> (sourceWidget.Initializer >> suffixInitUpdate)

    let update msg model = 
      sourceWidget.Updater (msg |> msgMapRev) (model |> modelMapRev)
      |> suffixInitUpdate

    let render model fbChannel = 
      sourceWidget.Renderer (model |> modelMapRev) (msgMap>>fbChannel)
      
    create init update render

  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with an additional mapping applied to rendered output.
  /// The type signature gives the hint that we can reuse the render logic of the source widget to define the mapping.
  let withMappedRenderOutput 
    (map : 'RenderOutput -> 'Model -> FeedbackChannel<_> -> 'RenderMapping) 
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) =
    
    let renderer model fbChannel = 
      let sourceRendered = sourceWidget.Renderer model fbChannel
      map sourceRendered model fbChannel

    create (sourceWidget.Initializer) (sourceWidget.Updater) renderer

  let private withFlowAccelerated
    (flowScanner : 'MsgProtocol -> 'Model -> 'Model -> FlowAcceleration<'MsgProtocol> option)
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) =

    let injectInstructions msg model model' =
      let maybeInsts = flowScanner  msg model model'
      maybeInsts |> Option.toList 
    let updater msg model =
      let newModel, instructions = sourceWidget.Updater msg model
      newModel, 
      Cmd.batch[
        yield instructions
        yield! injectInstructions msg model newModel
      ]

    create (sourceWidget.Initializer) updater (sourceWidget.Renderer)

  /// ### Description
  /// Creates a new widget with same semantics as the source widget, but will produce additional runtime messages (thus accelerating flow) according to the semantics of the flow scanner to be weaved in.
  /// You can pattern-match on the old model, the updated model and the message that caused the update. With that information you decide whether or not you want to schedule an `async` . 
  /// ### When should I use this?
  /// Use this combinator to model interactions that involve reconciling external data fetched from async workflows with the flow of the widget (e.g. handle a API respose initiated by a user action).
  ///
  /// As long as you express flow interaction behavior with this function you'll always end up with an enhanced widget without breaking the flowgramming happy place.
  ///
  /// The `flowScanner` is used to trigger external behaviour based on patterns of messages and data flowing through the widget. For matchting patterns, an acceleration of flow happens through returning a `Some async`.
  /// The type signature asks you to provide a `msgMapper` that knows how to translate externally fetched data back into the `'MsgProtocol` of the widget. Usually this will be a case constructor of the `'MsgProtocol` union.
  /// The type signature asks you to provide an `exnMapper` that knows how to translate exceptions/cancellations that might occur within the async workflow back into the message protocol of the widget. Usually you will add a case for `exn` or use `Result<_,_>` to report values.
  let withFlowAcceleratedAsync
    (msgMapper : 'T -> 'MsgProtocol)
    (exnMapper : exn -> 'MsgProtocol)
    (flowScannerAsync : 'MsgProtocol -> 'Model -> 'Model -> Async<'T> option)
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) =

    let flowScanner msg model model' = 
      flowScannerAsync msg model model' 
      |> Option.map (fun a ->
          let asyncMapped = async { 
            let! res = a 
            return res |> msgMapper 
          }
          Cmd.ofAsync (fun _ -> asyncMapped) () id exnMapper)
   
    sourceWidget |> withFlowAccelerated flowScanner
  
  /// ### Description
  /// Lifts the widget into the Elmish `Program<_,_,_,_>` context, i.e. assigns the flowtime context to the widget. 
  /// After this point, you will proceed with app configuration using what the `Program` module of Elmish has to offer.
  /// ### When should I use this?
  /// Whenever you find you ended up with a final widget you want to be your UI app.
  let mkProgram (widget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderOutput>) = 
    Program.mkProgram 
      widget.Initializer
      widget.Updater
      widget.Renderer

/// ### Description
/// Why not have alternative naming to convey different mental models that are isomorphic underneath.
/// Instant soundness check for free by the ML type system + inference F# has to offer :-) 
/// This module is using flowgramming lingo more directly, but it is the exact same thing as the `Widget` module and `IWidget<_,_,_,_>`, just using other function and type parameter names.
module FlowgramChart = 
  
  let create init update render 
      : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow> = 
    Widget.create init update render

  let createProof init update 
      : ProofFlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow> = 
    Widget.createDark init update

  let withInitializer init 
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) 
      : FlowgramChart<_,_,_,_> = 
    sourceChart |> Widget.withInitializer init

  let withUpdater updater 
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) 
      : FlowgramChart<_,_,_,_> = 
    sourceChart |> Widget.withUpdater updater

  let withRenderer renderer 
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) 
      : FlowgramChart<_,_,_,_> = 
    sourceChart |> Widget.withRenderer renderer

  let withSourceEmbedded source 
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) 
      : SourcedFlowgramChart<_,_,_> =
    sourceChart |> Widget.withEmbeddedConfig source
  
  let withMappedOutflow mapping 
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) 
      : FlowgramChart<_,_,_,_> =  
    sourceChart |> Widget.withMappedRenderOutput mapping
  
  let withFlowAcceleratedAsync
    msgMapper exnMapper flowScannerAsync
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) 
      : FlowgramChart<_,_,_,_> = 
    sourceChart |> Widget.withFlowAcceleratedAsync msgMapper exnMapper flowScannerAsync
  
  let mkProgram 
    (sourceChart : FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>) =   
    sourceChart |> Widget.mkProgram