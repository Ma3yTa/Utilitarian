namespace Utilitarian.Widgets

open Elmish

[<RequireQualifiedAccess>]
/// ### Widget combinators
/// You can't mess anything up when composing with widget combinators, the F# compiler service would always redline you.
/// Now just our programs need to make sense and actually do something useful :)
module Widget = 
  
  /// ### Description
  /// Creates a widget from 3 loose functions. 
  /// The compiler checks their type signature for cross-compatbility, i.e. whether they agree with the `IWidget` protocol.
  let create init update render  =
    {
      new IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement> with 
        member  __.Initializer = init
        member __.Updater = update
        member __.Renderer = render
    }

  /// ### Description
  /// Creates a dark widget from 2 loose functions (a widget with a renderer that evaluates to `unit`). 
  /// The compiler checks their type signature for cross-compatbility, i.e. whether they agree with the `IDarkWidget` protocol.
  /// ### When should I use this?
  /// Whenever you want to declare a widget in shared code without a renderer attached.
  /// Usually the renderers will contain most of platform-specific code by introducing the native UI framework into the program flow (e.g. React) 
  let createDark init update : IDarkWidget<'WidgetCfg,'MsgProtocol,'Model> = 
    create init update (fun _ _ -> ()) 


  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a different way to create the initial program state.
  /// The first program state consists of an initial data model + startup runtime instructions for Elmish
  let withInitializer init (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) = 
    create init (sourceWidget.Updater) (sourceWidget.Renderer)


  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a different way to create runtime updates
  /// A single runtime update consists of a new model + runtime instructions for Elmish.
  let withUpdater updater (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) = 
    create (sourceWidget.Initializer) updater (sourceWidget.Renderer)


  /// ### Description
  /// Creates a new widget with same semantics as source widget, but with a different renderer attached.
  /// That means the new widget will have another way to render the data model and different way to handle user feedback.
  /// ### When should I use this?
  /// Usually when things are getting platform-specific. You'll most likely start from a dark widget (a widget that doesn't render anything) defined in shared code.
  /// You can elevate a dark widget using this function together with a compatible renderer.
  /// You will usually define one per platform and design-system and use this function to yield a widget that renders program flow defined in shared code with the native UI framework engrained in the renderer.
  /// The F# type system does all the compatibility checks for you. It is impossible to elevate a dark widget with an incompatible renderer. 
  let withRenderer renderer (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) = 
    create (sourceWidget.Initializer) (sourceWidget.Updater) renderer


  let embedConfig cfg (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>)  : IConfiguredWidget<_,_,_> =
    create ((fun () -> cfg)>>sourceWidget.Initializer) (sourceWidget.Updater) (sourceWidget.Renderer)


  let mapRenderedElement 
    (renderMapper : 'RenderElement -> 'Model -> FeedbackChannel<_> -> 'RenderMapping) 
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) =
    
    let renderer model fbChannel = 
      let sourceRendered = sourceWidget.Renderer model fbChannel
      renderMapper sourceRendered model fbChannel

    create (sourceWidget.Initializer) (sourceWidget.Updater) renderer


  let augmentProgramFlow
    (flowScanner : 'MsgProtocol -> 'Model -> 'Model -> Instructions<'MsgProtocol> option)
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) =

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
  /// Creates a new widget with same semantics as the source widget, but will produce additional messages according to the semantics of the flow scanner to be weaved in.
  /// The flow scanner looks at every program update and then decides whether or not to schedule an async workflow to yield a message compatible with the `'MsgProtocol` of the widget.
  /// ### When should I use this?
  /// Use this combinator when you want to bring external data fetched from async workflow back into the program flow of the widget, e.g. to trigger API calls based on user feedback.
  /// The type signature hints you to provide a translation of the external data into the `'MsgProtocol` of the widget.
  /// This is very versatile and enables you to reconcile program flow of the widget with any kind of external behaviour in a predictable/deterministic way.
  /// ### An ideal failure boundary towards the UI layer 
  /// The type signature hints you to provide an exception mapper of type `exn -> 'MsgProtocol` that knows how to translate exceptions/cancellations that might occur within the async workflow.
  let augmentProgramFlowAsync
    (exnMapper : exn -> 'MsgProtocol)
    (flowScannerAsync : 'MsgProtocol -> 'Model -> 'Model -> Async<'MsgProtocol> option)
    (sourceWidget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) =

    let flowScanner msg model model' = 
      flowScannerAsync msg model model' 
      |> Option.map (fun asyncGen -> Cmd.ofAsync (fun _ -> asyncGen) () id exnMapper)
    
    sourceWidget |> augmentProgramFlow flowScanner
  

  /// ### Description
  /// Lifts the widget into the `Program` context. After this point, you can configure your app with the `Program` module of Elmish.
  /// ### When should I use this?
  /// Whenever you find you ended up with a final widget you want to be your UI app.
  let inline mkProgram (widget : IWidget<'WidgetCfg,'MsgProtocol,'Model,'RenderElement>) = 
    Program.mkProgram 
      widget.Initializer
      widget.Updater
      widget.Renderer