namespace Utilitarian.Widgets

// A feedback channel is a way to report feedback from the rendering process back to the flowtime.
// Those are passed into renderers to be wired together with the native event mechanism of your rendering framework (e.g. React)
// Equivalent to Elmish.Dispatch<_>
type FeedbackChannel<'TMsgProtocol> = 'TMsgProtocol -> unit

// An accelerator knows how to process feedback from UI (or from scheduled computations in general). 
// The flowtime keeps receiving and executing bacthes of those in a loop. This will drive the flow of the widget when it has been given its notion of (run-)time.
// Equivalent to Elmish.Sub<_>
type Accelerator<'TMsgProtocol> = FeedbackChannel<'TMsgProtocol> -> unit

// the flow acceleratiion are many accelerations to be scheduled together within one update cycle.  
// Equivalent to Elmish.Cmd<_>
type FlowAcceleration<'TMsgProtocol> = Accelerator<'TMsgProtocol> list

type Initializer<'TWidgetCfg,'TMsgProtocol,'TModel> = 'TWidgetCfg -> 'TModel * FlowAcceleration<'TMsgProtocol>

type Updater<'TMsgProtocol,'TModel> = 'TMsgProtocol -> 'TModel -> 'TModel * FlowAcceleration<'TMsgProtocol>

type Renderer<'TMsgProtocol,'TModel,'TRenderOutput> = 'TModel -> FeedbackChannel<'TMsgProtocol> -> 'TRenderOutput

/// ### Widgets (FlowgramCharts)
/// This widget abstraction is the essential building block and unit of modularization for making software 
/// inspired by [Elmish](https://github.com/elmish/elmish) and the [Elm Architecture](https://guide.elm-lang.org/architecture/).
/// No matter how big and complex your programs become, you'll always end up with a widget retaining the same outer type shape (very much like Matryoshka dolls or onions). 
/// The beauty is that the small is guaranteed not to break when reused within the large by the F# type system through the type `IWidget`.
/// If something doesn't work as you intend, then you can always track it down to the outmost composition step.
///
/// ### When and Why?
/// Widgets are an oasis for **domain-driven and typeful design** and provide a clean boundary for UI between technical pecularities like events/concurrency and domain knowledge. 
/// 
/// Widgets provide a mental model to turn F# data types and values into a core unit of modularization and code-sharing with **zero framework dependencies**. 
/// 
/// Widgets also bring a **fractal** aspect into the type system 
/// (see [`SeqFlowController`](https://github.com/kfrie/Utilitarian/blob/master/src/shared/core/widgets/SeqFlowController.fs)). 
/// This can potentially yield an unprecedented degree of safe code reuse for GUI apps across native platforms 
/// and the status-quo of the F# ecosystem has never been better to achieve this. 
/// 
/// It allows the F# type system to be your friendly ghost that has your back all the way into app stores, CDNs and clusters.
///
/// ### Flowgramming
/// It is all about the imagination of a functional pseudo-runtime (**flowtime**) around the main concept of **flow**. 
/// 
/// There is a **data flow** (`'Model` type) that is driven by a **message flow** (`'Msg` type) (`update`) starting from an 
/// initial datum and message (`init`). 
/// 
/// Widgets are best imagined as **composable static flow charts** that don't introduce any notion of (run-)**time** themselves.
/// This allows to express any kind of program and interaction logic in terms of F# data types. 
/// 
/// That makes it easy to reconcile with any kind of external behaviour (e.g. button click triggers API call) 
/// in **deterministic** fashion, with full guard from the F# type system. 
/// 
/// It is all just pure F#, there isn't even a factual dependency to Elmish, but Elmish offers a flowtime implementation that can execute widgets 
/// and the widget abstraction has been derived from Elmish.
///
/// Flowgramming is the principle "Form follows function" or [Utilitarianism](https://en.wikipedia.org/wiki/Form_follows_function#Utilitarianism) 
/// in its purest form for software design and architecture. It is going so far to even take it literally :)
/// 
/// The widget abstraction offers a mechanism on top of just F# types and functions 
/// to do type-safe declarative [flow-based programming](https://en.wikipedia.org/wiki/Flow-based_programming), 
/// a variant of [dataflow programming](https://en.wikipedia.org/wiki/Dataflow_programming).
///
/// Important to note that classic [Elmish](https://github.com/elmish/elmish) **top-down** style effectively gives you the same kind of guarantees. 
/// It might just turn out to be less brain-friendly and cumbersome after a certain composition depth.
/// 
/// ### Type parameters explained
/// A widget is actually just a triple of pure F# functions.
/// This interface validates type compatibility across those 3 functions so that any 3 functions 
/// that can be packed into an `IWidget` represent a widget as described above.
///
/// #### `'TMsgProtocol`
/// Type of message flow. 
/// Represents the message protocol of the widget, i.e. the kind of messages the flowtime is allowed to dispatch / route into the widget. 
/// 
/// The message protocol thus can be enforced by the compiler.
/// 
/// It should always be an immutable [discrimated union](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions).
///
/// #### `'TModel`
/// Type of the data flow. 
/// Will be comprised of types from your functional domain model that take part in the user interaction you want to capture with the widget.
/// It should always be an immutable [record](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/records) or [discrimated union](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions).
///
/// #### `'TWidgetCfg`
/// Gives the flowtime an opportunity to influence the creation of the widget's initial state with an externally passed in configuration object of type `'TWidgetCfg` before the widget is started as a program. 
/// If a widget got its configuration object baked in, this becomes `unit`, Then it must be run without parameter by the flowtime.
///
/// #### `'TRenderOutput`
/// Central abstraction of the rendering framework you like to control with the widget (e.g is `ReactElement` when using React).
/// This is not constricted to visual rendering and even more abstract it is just an arbitrary output derived from the data flow the widget keeps track of.
/// You could also use a sound-based rendering framework, with audio playback as output and users' speech as input.
///
/// ### Composition strategies
/// 
/// There are combinators to be found in the `Widget` or with alternative naming in the `Flowgram` module of the namespace `Utilitarian.Widgets.`.
/// All compositions offer an explanation on what they do and how and when to use them. 
/// 
/// #### Flow controllers
/// Flow controllers (supervisors) are a bit mind-bending and a quite powerful concept to introduce to UI programming.
/// They lift a source data flow into the widget context and are widgets themselves (there's the **fractal** power).
/// 
/// Special semantics of the data flow can be conveyed into the rendering process with a flow controller. 
/// The flow controller exposes it's own message protocol to reifies the valid operations of the data flow semantics as messages.
/// 
/// For instance, the seq controller allows you to dispatch seq operations at runtime like `SortBy`, `Filter`, `Take`  etc. 
/// to reshape e.g. visuals indirectly by rerouting rendering of underlying data models before they actually get rendered and are given their final position in a visual tree.
/// 
/// You can lift special semantics conveyed by types (`Seq<'T>`, `Result<'TSuccess,'TError>`, `Undo`, `Navigation` ...) 
/// into any rendering process by writing a flow controller per such type. 
/// 
/// The gain is, that you can reuse flow controllers with any conceivable UI subprogram. Those are just values of type `IWidget` now :) 
/// 
/// Don't worry, that can possibly only be understood by playing around and see the concept in action. 
/// And you can safely reuse existing flow controllers without fully understanding how they work interally. 
type IWidget<'TWidgetCfg, 'TMsgProtocol,'TModel,'TRenderOutput> =
/// ### Description
/// Knows how to create the initial data and message flow based on an externally passed in configuration object.
  abstract Initializer : Initializer<'TWidgetCfg,'TMsgProtocol,'TModel>
  /// ### Description
  /// Knows how to rehydrate data and message flow based on an incoming message and the current data flow state.
  abstract Updater : Updater<'TMsgProtocol,'TModel>
  /// ### Description
  /// Knows how to up the window of interaction between the data flow of the widget and a human operator.
  /// Usually the renderer will contain most of platform-specific code by introducing the native UI framework into the widget's flow (e.g. React)
  abstract Renderer : Renderer<'TMsgProtocol,'TModel,'TRenderOutput>

/// ### Description
/// A widget with specification of flow, but without rendering defined and thus can't show / display something  (conveyed with type `unit`).
/// The message flow of the widget still lacks the acceleration coming from user feedback (is received by feedback channels passed into renderers)
/// ### When should I use this
/// Use this signature to define widgets partially in shared code by using `Widget.createDark`
type IDarkWidget<'TWidgetCfg, 'TMsgProtocol,'TModel> = IWidget<'TWidgetCfg, 'TMsgProtocol,'TModel,unit>

type IConfiguredWidget<'TMsgProtocol,'TModel, 'TRenderOutput> = IWidget<unit, 'TMsgProtocol,'TModel,'TRenderOutput>

type NavigationContext<'TMsgProtocol,'TPages,'TFormat> = 
  NavigationContext of PageDefinition : 'TPages * RouteParser : (Printf.TextWriterFormat<'TFormat> -> 'TPages option)

/// ### Description
/// A widget that additionally provides a navigation context for application-level routers to process
/// ### What is it good for?
/// Allows you to define navigation behaviour in shared code.
/// When assembling application-level routers from platform-specific projects, you can reuse the route parts from values of `IPaget` during the process.
/// Should you want to navigate to a specific page, you can query an `IPaget` value for its association with all your app's available pages (`'TAppPages`).
/// It makes only sense to elevate an `IWidget` to an `IPaget` after you are done composing and the widget represents a meaningful application-level page you'd like to present as such to a human operator.
type IPaget<'TWidgetCfg, 'TMsgProtocol,'TModel, 'TRenderOutput,'TAppPages,'TFormat> = 
  inherit IWidget<'TWidgetCfg, 'TMsgProtocol,'TModel,'TRenderOutput>
  abstract NavigationContext : NavigationContext<'TMsgProtocol,'TAppPages,'TFormat>

/// ### Description
/// Type alias for `IWidget<'TWidgetCfg, 'TMsgProtocol,'TModel,'TRenderOutput>` that addresses the dataflow programming model more explicitly
type FlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow> = IWidget<'TFlowSource, 'TMsgflow,'TDataflow,'TOutFlow>

/// ### Description
/// Type alias for `IConfiguredWidget<'TMsgProtocol,'TModel,'TRenderOutput>` that addresses the dataflow programming model more explicitly
type SourcedFlowgramChart<'TMsgflow,'TDataflow,'TOutFlow> = FlowgramChart<unit, 'TMsgflow,'TDataflow,'TOutFlow>

/// ### Description
/// Type alias for `IDarkWidget<'TWidgetCfg,'TMsgProtocol,'TModel>` that addresses the dataflow programming model more explicitly. 
/// Called (leak)proof, because isn't any outflow production to be observed.
type ProofFlowgramChart<'TFlowSource, 'TMsgflow,'TDataflow> = FlowgramChart<'TFlowSource,'TMsgflow,'TDataflow,unit>
