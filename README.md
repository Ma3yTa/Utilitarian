# Utilitarian

  - [Apps](#apps)
    - [Mario Kart 8 Deluxe Tools](#mario-kart-8-deluxe-tools)
  - [Current WIP](#current-wip)
    - [The Tale of F](#the-tale-of-f)
    - [Elmish Widgets (Flowgrams)](#elmish-widgets-flowgrams)
    - [Apply *Infrastructure is Code* methodology](#apply-infrastructure-is-code-methodology)

Utilitarian is an effort to bring the best things of the F# ecosystem together with other promising technology and sane software engineering principles to create stable, high-quality end-user software in record time.

You should have some familiarity with functional modelling techniques and F# algebraic data types. [F# for Fun and Profit](https://fsharpforfunandprofit.com/) is a great resource to get started with those topics. This will really pay off for all the things you can expect to find here. 

If you like to get some inspiration for your own work, just have a look around the repository. Documentation is likely to be next to the thing it describes.

If you like to dive into live-coding and exploring, you can open one of the [VS Code](https://code.visualstudio.com/) app workspaces and start a live code session by activating green button with `Develop` (Windows only atm).

For Windows, there is an install script in the [`.vscode` folder](https://github.com/kfrie/Utilitarian/tree/master/.vscode) with a setup that meets all requirements for live-coding. It's about the extensions, so with minor modifications it can run on Mac or Linux as well. Creating a pre-configured developer machine with Vagrant is current WIP to avoid manual setup for the live-coding experience.

If you like to take part in this, please have a look into the [Contribution Guide](CONTRIBUTING.md).

## Apps

### Mario Kart 8 Deluxe Tools
The first app development model project. It serves as proof-of-concept for a new widget (flowgram) abstraction and a typeful design approach. The MVP is a little configuration finder that allows you to find a driving configuration complying with a set of attribute score filters. This is useful when you want to tweak towards your favorite configuration after you played the game for a while.

Furthermore a navigation shell showcases how navigation between pages can be implemented with widgets. Additional features like overview pages are planned to be added at some point.

This app is intended to be the running sample for the [The Tale of F](#the-tale-of-f), so it will be extended this with a server aspect and if possible some interaction scenario with the [hidden Nintendo API](https://github.com/ZekeSnider/NintendoSwitchRESTAPI).

#### [@kfrie](CONTRIBUTING.md#maintainers)
> I needed a patient zero and I really want such a filter feature myself since I am an active player and a huge fan of this game. 

> The game is a definitive masterpiece. There is nothing I could imagine one could add something to it or to make it any better.

> I have the same feelings with the F# type system combined with powerful pattern-matching. 

> I thought this might be a good omen and starting point to reach out for an epic journey!

#### Tests
- [ ] Module tests
- [ ] Automated UI tests
- [ ] Performance tests

#### Client platforms
- [ ] React
- [ ] React Native
- [ ] Xamarin Forms
- [ ] Trail (Blazor)

## Current WIP

### The Tale of F#
The Tale of F# is a 6-part talk series covering **the whole story about developing software products** with [F#](https://www.microsoft.com/net/learn/languages/fsharp), the [SAFE](https://safe-stack.github.io/) stack and [Xamarin.Forms](https://fsprojects.github.io/Elmish.XamarinForms/guide.html).

It is about more than just programming concepts and technology. It is also about exploring and creating a software development process that works really well together with a new functional, full-stack and multi-platform architecture. And the tale should yield proof along the way, that this is more than yet another round of IT buzzword bingo.

### Elmish Widgets (Flowgrams)

This is an effort to design a **core unit of modularization for functional UI development** dervied from [Elmish](https://elmish.github.io/elmish/). This is being developed with the following goals in mind:

* come to a powerful, clean and repeatable approach for code sharing between multiple client platforms + client-server and frictionless distribution through **UI app as a value**
* leverage powerful functional composition techniques like **flow controllers**
* make the case for a new style of libraries centered around **knowledge** and enhanced with **widget catalogs** to capture domain knowledge that involves **interaction with a human operator** and **graphical output**

This enables you to reuse a single interface and its incorporated mental model to reason about any kind of program (especially with rich user interaction and graphical output) in a very **human-friendly** way.

<img alt="flowgramming screengif" src="https://user-images.githubusercontent.com/3251459/43350137-21f9f4be-9205-11e8-879e-b5e0c4b28878.gif" width="850" height="472"></img>

#### Flowgramming

By using widgets, you start to do **flowgramming** with F#.

It is all about the imagination of a functional pseudo-runtime or **flowtime** around the main concept of **flow**. 

There is a **data flow** (`Model` type) that gets **accelerated** (`update`) by a **message flow** (`Msg` type), starting with an initial datum and initally scheduled list of messages (`init`). 

Widgets are best imagined as **composable static flow charts** that don't introduce any notion of (run-)**time** themselves.

This allows you to express any kind of program and interaction logic only in terms of [F# data types](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/fsharp-types). That makes it easy to reconcile them with any kind of external behaviour (e.g. button click triggers API call) in **deterministic** fashion, with full guard from the F# type system. 

In no way does this prevent potential runtime (flowtime) compositions between widgets in execution, that is just a separate design space to explore (**reactive flowgramming**)

Flowgramming is the principle **form follows function** or [Utilitarianism](https://en.wikipedia.org/wiki/Form_follows_function#Utilitarianism) applied in its purest form to software design and architecture. You could even say taken literally. 😉

The widget abstraction offers a mechanism on top of just the F# type system to do declarative [flow-based programming](https://en.wikipedia.org/wiki/Flow-based_programming), a variant of [dataflow programming](https://en.wikipedia.org/wiki/Dataflow_programming). You are highly encouraged to explore those concepts by diving into a live-coding browser session.

#### Fractal Architecture

There is a **fractal architecture pattern** hidden in widgets (see [`SeqFlowController`](https://github.com/kfrie/Utilitarian/blob/master/src/shared/core/widgets/SeqFlowController.fs) as PoC). 

This pattern requires thinking **bottom-up** instead of top-down. Trees in nature are very unlikely to be observed growing top-down 😉

By thinking in values stored in catalogs, composition with the pipe operator, strict file depdendency order and some peculiar powerful fractal compositions (**flow controllers**) we simply align with this pattern.

This way we **scale as we grow** to handle complexity, while a top-down approach would make things blown up and messy quite quick with this kind of architectural pattern. It has probably something to do with the fact that typed F# values stored in assemblies are better in **bookkeeping of compositional complexity** than human brains are.

Just **grow** your application by composing static charts that prescribe ground truths (sort of like DNA does) on process evolution that will hold in **any conceivable runtime environment**. There is a right time and place for things. You can take care of technical pecularities after all your domain knowledge is already safely engrained within widgets.

Natural growth starts out with some cells and grows into something magnificient by building up layer after layer of complexity. In software, you unfortunately will almost always find the exact opposite of this to be the case.

This can potentially lead to an **unprecedented degree of safe code reuse for UI apps across native platforms** and the status-quo of the F# ecosystem has never been better to achieve this. It allows the F# type system to be your friendly ghost that has your back all the way into app stores, CDNs and clusters.

#### DDD

Widgets provide a mental framework to turn F# data types and values into a **core unit of modularization** for inter-platform code-sharing with **zero framework dependencies**.

The widget abstraction is thus an oasis for **domain-driven and typeful design** and provides a clean boundary between user interaction, technical platform pecularities like events+concurrency and, domain knowledge. 

#### Widgets + Elmish ?
Please note that the classic Elmish top-down style effectively gives you the same kind of guarantees. It will just turn out to be less brain-friendly and cumbersome after a certain composition depth.

It is all just pure F#, there isn't even a factual dependency to Elmish, but Elmish offers a runtime model (flowtime) that can execute widgets and the widget abstraction has been derived from Elmish. 


### Apply *Infrastructure is Code* methodology
In 2018, virtualization technology is just too good and powerful not to be leveraged. Something that works great can be captured, redistributed and reused by others, potentially saving them tons of time and work. Especially when it comes to communicating an approach, it is important to be gentle to beginners. They do not have the knowledge and context in their head yet to work around issues they encounter. The credo is: if infrastructural problems can be eliminated, they must be eliminated.

#### Vagrant
From app development perspective it is desirable to have a definition of a standard working machine with [Vagrant](https://www.vagrantup.com):

- clean failure boundary between application and infrastructure (baseline machine to reproduce bugs)
- eases onboarding of beginners / team members
- great to accompany educational efforts like dojos / katas
