[<AutoOpen>]
module rec Utilitarian.Mariokart.Domain

type Character =
  | BowserJr
  | Bowser
  | DryBowser
  | Wario
  | Waluigi
  | Peach
  | Mario
  | Luigi
  | BabyLuigi

type Tires =
  | Standard

type Glider =
  | Standard

type Vehicle =
  | Standard of VehicleClass
  | PipeFrame
  | Mach8
  | SteelDriver
  | CatCruiser
  | CircuitSpecial
  | TriSpeeder
  | Badwagon

and VehicleClass = Kart | Bike | SportBike | ATV

type RoadType =
  | AntiGravity
  | Water
  | Air
  | Ground
with
  override x.ToString() = x |> RoadType.canonicalString

type Attribute =
  | Speed of RoadType
  | Acceleration
  | Handling of RoadType
  | Traction
  | Weight
  | MiniTurbo
with
  override x.ToString() = x |> Attribute.canonicalString

type ScoredAttribute = (Attribute * float)

type ItemScore =  ScoredAttribute Set

type DriveConfiguration = {
  CharacterScore : Character * ItemScore  
  VehicleScore : Vehicle * ItemScore 
  TiresScore :  Tires * ItemScore
  GliderScore : Glider * ItemScore  
}

[<RequireQualifiedAccess>]
module RoadType =
  
  type T = RoadType

  module L =
    let [<Literal>] AntiGravity = "Anti-Gravity"
    let [<Literal>] Water = "Water"
    let [<Literal>] Air = "Air"
    let [<Literal>] Ground = "Ground"

  let canonicalString = function  
    | AntiGravity -> RoadType.L.AntiGravity
    | Water -> RoadType.L.Water
    | Air -> RoadType.L.Air
    | Ground -> RoadType.L.Ground

[<RequireQualifiedAccess>]
module Attribute =
  
  type T = Attribute
  
  module L =
    let [<Literal>] Acceleration = "Acceleration"
    let [<Literal>] Speed = "Speed"
    let [<Literal>] Traction = "Traction"
    let [<Literal>] Handling = "Handling"
    let [<Literal>] MiniTurbo = "Mini-Turbo"
    let [<Literal>] Weight = "Weight"
    let [<Literal>] RoadTypeSep = "/"

  let canonicalString = function
    | Speed rt ->
        L.Speed +
        L.RoadTypeSep +
        (RoadType.canonicalString rt)
    | Handling rt ->
        L.Handling +
        L.RoadTypeSep +
        (RoadType.canonicalString rt)
    | Acceleration -> L.Acceleration
    | Traction -> L.Traction
    | Weight -> L.Weight
    | MiniTurbo -> L.MiniTurbo  

  let tellAllValues = [
    Speed AntiGravity; Speed Water; Speed Air; Speed Ground
    Handling AntiGravity; Handling Water; Handling Air; Handling Ground
    Acceleration
    Traction
    Weight
    MiniTurbo
  ]

[<RequireQualifiedAccess>]
module Character =

  type T = Character

  module L =
    let [<Literal>] BowserJr= "Bowser Jr."
    let [<Literal>] DryBowser = "Dry Bowser"
    let [<Literal>] Bowser = "Bowser"
    let [<Literal>] Wario = "Wario"
    let [<Literal>] Waluigi = "Waluigi"
    let [<Literal>] Peach = "Peach"
    let [<Literal>] Mario = "Mario"
    let [<Literal>] Luigi = "Luigi"
    let [<Literal>] BabyLuigi = "Baby Luigi"
    
  let canonicalString = function
    | Bowser -> L.Bowser
    | DryBowser -> L.DryBowser
    | BowserJr -> L.BowserJr
    | Wario -> L.Wario
    | Waluigi -> L.Waluigi
    | Peach -> L.Peach
    | Mario -> L.Mario
    | Luigi -> L.Luigi
    | BabyLuigi -> L.BabyLuigi
 
