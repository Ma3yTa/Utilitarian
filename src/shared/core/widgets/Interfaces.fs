namespace Utilitarian.Widgets

// A feedback channel is a way to report feedback from the UI rendering framework to the Elmish runtime.
// Those are passed into renderers to be wired together with the native event mechanism of your UI rendering framework (e.g. React)
// Equivalent to Elmish.Dispatch<_>
type FeedbackChannel<'TMsgProtocol> = 'TMsgProtocol -> unit

// An instruction knows how to process feedback from UI (or from scheduled computations in general). 
// The Elmish runtime keeps receiving and executing those in a loop. This makes the program flow.
// Equivalent to Elmish.Sub<_>
type Instruction<'TMsgProtocol> = FeedbackChannel<'TMsgProtocol> -> unit

// Just multiple instructions expressed with list type.
// Equivalent to Elmish.Cmd<_>
type Instructions<'TMsgProtocol> = Instruction<'TMsgProtocol> list

type Initializer<'TWidgetCfg,'TMsgProtocol,'TModel> = 'TWidgetCfg -> 'TModel * Instructions<'TMsgProtocol>

type Updater<'TMsgProtocol,'TModel> = 'TMsgProtocol -> 'TModel -> 'TModel * Instructions<'TMsgProtocol>

type Renderer<'TMsgProtocol,'TModel,'RenderElement> = 'TModel -> FeedbackChannel<'TMsgProtocol> -> 'RenderElement

/// ### Elmish Widgets
/// Values of type `IWidget` are information flow charts that bear knowledge about program flow, how to display program flow to the user and how to alter program flow based on user feedback.
/// Any value of type `IWidget` can be executed as a program by the Elmish runtime. 
/// This widget abstraction is the essential building block and unit of modularization for making software that follows the fractal Elm Architecture.
/// You build more complex widgets through composition of simpler ones (nesting of model and message protocol type).
/// No matter how big and complex your UI becomes, you'll always end up with a `IWidget` retaining the same outer type shape (very much like Matryoshka dolls or onions).
/// At some point you have to decide on a main widget you want to start as a program / deploy as app.
/// Any `IWidget` effectively makes up a deployable UI app you can push to CDN and app stores with a once defined build process without further friction.
/// The beauty of the Elmish approach is the guarantee, that if it does not break in the small, it does not break in the large. 
/// And the [simplicity](https://pbs.twimg.com/media/DYImH4mW4AA0SHe.jpg") of the Elm Architecture, which makes it very easy to onboard new people to the development process.
/// ### Type parameters explained
/// From the perspective of the Elmish runtime, a program is just a set of 3 pure F# functions. 
/// This interface validates type compatibility across those 3 functions so that any set of 3 functions that can be packed into an `IWidget` is a valid program chart for the Elmish runtime. 
/// #### `'TMsgProtocol`
/// Represents the message protocol of the widget, i.e. the kind of messages the Elmish runtime is allowed to dispatch to the widget. The message protocol if effectively enforced by the compiler that way.
/// It should always be an immutable [discrimated union](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions).
/// #### `'TModel`
/// Will be comprised of types from your functional domain model that take part in the user interaction you want to capture with the widget.
/// It should always be an immutable [record](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/records) or [discrimated union](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/discriminated-unions).
/// #### `'TWidgetCfg`
/// Gives an opportunity to the Elmish runtime to influence the creation of the initial application model with an externally passed in configuration object of type `'TWidgetCfg` before the widget is started as a program. 
/// If this isn't supported by the widget, this evaluates to `unit` 
/// #### `'TRenderOutput`
/// Central abstraction of the rendering framework you like to control with Elmish (e.g is `ReactElement` when using React).
/// This is not constricted to visual rendering. 
/// You could use a sound-based rendering framework, with audio playback as output and users' speech as input.
/// ### Composition strategies
/// #### Dataflow controllers
/// Dataflow controllers are mind-bending and are a very powerful concept to introduce to UI programming.
/// They turn your data into interactive and tangible "flowgrams".
/// It's like the semantics of data flow within your program are getting married before they can possibly know other.
/// They converge against each other naturally, enforced by the F# type system through a peculiar type shape.
/// They are a mean of reshaping render output based on the semantics of operations that happen on the underlying data (model) flow without incurring additional runtime costs for the rendering costs.
/// You can lift special semantics conveyed with types (`Seq<'T>`, `Result<'Success,'Error>`, ...) into any rednering process by defining a dataflow controller per such type. 
/// That can possibly only be really understood by playing around and see the concept in action.
/// The most used controller probably will be the `Seq<'T>` controller. It is a universal decorator handling the aspect **"many"** for every conceivable type of widget.
/// That makes it compatible with every type of widget you are going to create. Also note that dataflow controllers are of type `IWidget` themselves. Fractal is what you get :) 
type IWidget<'TWidgetCfg, 'TMsgProtocol,'TModel,'TRenderOutput> =
/// ### Description
/// Knows how to create the initial model and runtime instructions based on an externally passed in configuration object.
  abstract Initializer : Initializer<'TWidgetCfg,'TMsgProtocol,'TModel>
  /// ### Description
  /// Knows how to evolve the model based on an incoming message and a current model state.
  /// Addionally, it can create a set of instructions to be executed by the Elmish runtime. 
  abstract Updater : Updater<'TMsgProtocol,'TModel>
  /// ### Description
  /// Knows how to up the window of interaction between the program flow of the widget and a human operator.
  /// Usually the renderer will contain most of platform-specific code by introducing the native UI framework into the program flow (e.g. React)
  abstract Renderer : Renderer<'TMsgProtocol,'TModel,'TRenderOutput>

/// ### Description
/// A widget with specification of program flow, but without rendering. 
/// It literally renders to nothing (`unit`) and thus shows nothing and program flow can't incorporate any user feedback (is received by feedback channels passed into renderers)
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