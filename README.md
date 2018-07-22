# Utilitarian

Utilitarian is an effort to bring the best things of the F# ecosystem together to create stable, high-quality end-user software in record time.

If you like to get some inspiration for your own work, just have a look around the repository. You'll find documentation for things always as close to the thing as possible.

If you like to dive into live-coding and exploring, just open a VS code workspace for one of the [apps](#apps), hit the green button and choose `Develop`. Creating a pre-configured developer machine with Vagrant is current WIP.

If you like to take part in this, please have a look into the [Contribution Guide](CONTRIBUTING.md).

## Apps
    
### Mario Kart 8 Deluxe Tools
The first ever sample app, with a pure typeful design approach. It is a little configuration finder, that allows you to find a driving configuration complying with a set of attribute (speed, traction ..) filters. This is useful when you want to tweak towards your favorite configuration after you played the game for a while.

Furthermore it contains a navigation shell to showcase how navigation between pages would be implemented with Elmish.

I intend to use this as the running sample for the Tale of F#, so I will extend this with a server aspect for my 2nd talk and I am interested in incorporating some interaction with the [hidden Nintendo APIs.](https://github.com/ZekeSnider/NintendoSwitchRESTAPI)

#### Tests
- [ ] Module tests
- [ ] Automated UI tests
- [ ] Performance tests

#### Client platforms
- [ ] React
- [ ] React Native
- [ ] Xamarin Forms

## Current WIP

### The Tale of F#
The Tale of F# is a 5-part series **covering the whole story about software development with F# and the SAFE-Stack**.
And it does not end with programming concepts and technology. It is also about exploring and creating a software development process that works really well together with this new functional, full-stack, multi-platform architecture. And to proof that this isn't yet another round of buzzword bingo, but can hold its promises in reality.

### Elmish Widgets
This is an effort to design the **core unit of modularization for functional UI development** with [Elmish](https://elmish.github.io/elmish/). This is a base requirement for developing the following things: 

* a clean and repeatable approach for code sharing between multiple client platforms and client-server
* leverage powerful functional composition techniques - **Dataflow Controllers**
* develop a new style of libraries with a UI-first approach in mind - **Widget Catalogs**

You can now just use 1 interface and its incorporated mental model to reason about and compose beautiful and complex UI applications with new powerful safety guarantees to support you.
All there is to know about those concepts is attached as a long markdown comment to the [interface itself](./src/shared/core/widgets/interfaces.fs). Recommended reading mode is VS Code tooltip :).

### Apply *Infrastructure is Code* methodology
In 2018, virtualization technology is just too good and powerful not to be leveraged. Something that works great can be captured, redistributed and reused by others, potentially saving them tons of time and work. Especially when it comes to communicating an approach, it is important to be gentle to beginners. They do not have the knowledge and context in their head yet to work around issues they encounter. The credo is: if infrastructural problems can be eliminated, they must be eliminated.

#### [Vagrant](https://www.vagrantup.com)
From app development perspective it is very desirable to have a definition of a standard working machine with vagrant:

- clean failure boundary between application and infrastructure (baseline machine to reproduce bugs)
- eases onboarding of beginners / team members
- great to accompany educational efforts like dojos / katas
