# Utilitarian

  - [Apps](#apps)
    - [Mario Kart 8 Deluxe Tools](#mario-kart-8-deluxe-tools)
  - [Current WIP](#current-wip)
    - [The Tale of F](#the-tale-of-f)
    - [Elmish Widgets (Flowgrams)](#elmish-widgets-flowgrams)
    - [Apply *Infrastructure is Code* methodology](#apply-infrastructure-is-code-methodology)

Utilitarian is an effort to bring the best things of the F# ecosystem together to create stable, high-quality end-user software in record time.

If you like to get some inspiration for your own work, just have a look around the repository. Documentation is likely to be found as close to the things as possible.

If you like to dive into live-coding and exploring, you can open one of the [VS Code](https://code.visualstudio.com/) app workspaces and start a live code session by activating green button with `Develop` (Windows only atm).
For Windows, there is an install script in the [`.vscode` folder](https://github.com/kfrie/Utilitarian/tree/master/.vscode) with a setup that meets all requirements for live-coding. It's about the extensions, so with minor modifications it can run on Mac or Linux as well. Creating a pre-configured developer machine with Vagrant is current WIP to avoid manual setup for the live-coding experience.

If you like to take part in this, please have a look into the [Contribution Guide](CONTRIBUTING.md).

## Apps

### Mario Kart 8 Deluxe Tools
The first app development model project. It serves as proof-of-concept for a new widget (flowgram) abstraction and a typeful design approach. MVP is a little configuration finder, that allows you to find a driving configuration complying with a set of attribute filters. This is useful when you want to tweak towards your favorite configuration after you played the game for a while.

Furthermore a navigation shell showcases how navigation between pages can be implemented with widgets. Additional features like overview pages are planned to be added at some point.

I intend to use this as the running sample for the Tale of F#, so I will extend this with a server aspect and look for possibilities to introduce some interaction scenario with the [hidden Nintendo API](https://github.com/ZekeSnider/NintendoSwitchRESTAPI).

**@kfrie**
I needed a patient zero and I really want such a filter feature myself since I am an active player and huge fan of this game. The game is a definitive masterpiece, there is nothing I could imagine you could add to it or to make it better. I have the same feelings with the F# type system combined with powerful pattern-matching. So I thought this might be a good omen and starting point to reach out for an epic journey.

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
The Tale of F# is a 6-part talk series covering **the whole story about developing software products** with [F#](https://www.microsoft.com/net/learn/languages/fsharp), the [SAFE](https://safe-stack.github.io/) web stack and [Xamarin.Forms](https://fsprojects.github.io/Elmish.XamarinForms/guide.html)
It isn't just about programming concepts and technology. It continues with exploring and creating a software development process that works really well together with a new functional, full-stack and multi-platform architecture. And it should yield proof along the way that this is more than yet another round of IT buzzword bingo.

### Elmish Widgets (Flowgrams)
This is an effort to design a **core unit of modularization for functional UI development** based on [Elmish](https://elmish.github.io/elmish/). This is being developed with the following goals in mind:

* a powerful, clean and repeatable approach for code sharing between multiple client platforms and client-server and frictionless distribution through **UI app as a value**
* leverage powerful functional composition techniques like **flow controllers**
* make the case for a new style of libraries centered around **knowledge** and enhanced with **widget catalogs** to capture domain knowledge that involves **interaction with a human operator**

This enables you to reuse a single interface and its incorporated mental model to reason about any kind of program (especially with rich user interaction and graphical output) in a very human-friendly way.

It is all about the imagination of a functional pseudo-runtime or **flowtime** around the main concept of **flow**. There is a **data flow** (`'Model` type) that is driven by a **message flow** (`'Msg` type) (`update`) starting from an initial datum and message (`init`). Widgets are best imagined as **composable static flow charts** that don't introduce any notion of (run-)**time** themselves.

This allows to express any kind of program and interaction logic in terms of F# data types. That makes it easy to reconcile them with any kind of external behaviour (e.g. button click triggers API call) in deterministic fashion, with full guard from the F# type system. It is all just pure F#, there isn't even a factual dependency to Elmish, but Elmish offers a runtime model (flowtime) that can execute widgets and the widget abstraction has been derived from Elmish. 

On a higher level it is all about thinking in data and programming flow behavior. By using widgets, you can start to do **flowgramming** with F#.

Widgets thus provide a mental model to turn F# data types and values into a core unit of modularization and code-sharing with **zero framework dependencies**. Widgets also bring a **fractal** aspect into the type system . This can potentially yield an unprecedented degree of safe code reuse for UI apps across native platforms and the status-quo of the F# ecosystem has never been better to achieve this. It allows the F# type system to be your friendly ghost that has your back all the way into app stores, CDNs and clusters.

There is a **fractal architecture pattern** hidden in widgets (see [`SeqFlowController`](https://github.com/kfrie/Utilitarian/blob/master/src/shared/core/widgets/SeqFlowController.fs)), that requires thinking **bottom-up** instead of top-down (if anyone has ever heard of a tree that **grows** top-down please correct this).

By thinking in values stored in catalogs, composition with the pipe operator, strict file depdendency order and some peculiar powerful fractal compositions (**flow controllers**) we simply follow along the nature of the architecture. This way the architecture can scale as we grow while a top-down approach would make things blown up and messy quite quick with this kind of architecture.

The widget abstraction is an oasis for **domain-driven and typeful design** and provides a clean boundary between user interaction and technical pecularities like events/concurrency and domain knowledge. In no way does this prevent potential runtime (flowtime) compositions between widgets in execution, that is just a separate design space to explore (**Reactive Flowgramming**)

Flowgramming is the principle **form follows function** or [Utilitarianism](https://en.wikipedia.org/wiki/Form_follows_function#Utilitarianism) applied in its purest form to software design and architecture. It is going so far to even take it literally :)

The widget abstraction offers a mechanism on top of just F# types and functions to do type-safe declarative [flow-based programming](https://en.wikipedia.org/wiki/Flow-based_programming), a variant of [dataflow programming](https://en.wikipedia.org/wiki/Dataflow_programming).

Just follow your types and the project file dependency order all the way to the target platform and into your product. Then everything will just gonna be alright. This poses no restrictions, it is just meant to be a friendly guide towards **freedom and independence** for software developers by increasing the **efficiency level of the F# type system** to averse business risks by largely increasing **predictability towards product**. Let's F# ALL the things :)

Important to note that classic [Elmish](https://github.com/elmish/elmish) **top-down** style effectively gives you the same kind of guarantees. It might just turn out to be less brain-friendly and cumbersome after a certain composition depth.

But why wouldn't you just go with the flow of your enviroment and grow accordingly to a static chart (DNA), like all of nature does. Like the birds and the trees and the flowers and the bees :). They all just start with a bunch of cells and grow into something magnificient. With software, you unfortunately see the exact opposite of that idea very often.

More details with explanation of type parameters can be found next to [the widget interface](https://github.com/kfrie/Utilitarian/blob/master/src/shared/core/widgets/Interfaces.fs) as in-code markdown. Recommended reading mode is VS Code tooltip.

If you have any specific question or want to get in touch about this, please have a look into the [Contribution Guide](CONTRIBUTING.md).

<iframe src="https://drive.google.com/file/d/12Pq0A3OK-Ie1p1utmz_sP61XcbLTexVo/preview" width="850" height="472"></iframe>

### Apply *Infrastructure is Code* methodology
In 2018, virtualization technology is just too good and powerful not to be leveraged. Something that works great can be captured, redistributed and reused by others, potentially saving them tons of time and work. Especially when it comes to communicating an approach, it is important to be gentle to beginners. They do not have the knowledge and context in their head yet to work around issues they encounter. The credo is: if infrastructural problems can be eliminated, they must be eliminated.

#### Vagrant
From app development perspective it is desirable to have a definition of a standard working machine with [Vagrant](https://www.vagrantup.com):

- clean failure boundary between application and infrastructure (baseline machine to reproduce bugs)
- eases onboarding of beginners / team members
- great to accompany educational efforts like dojos / katas
