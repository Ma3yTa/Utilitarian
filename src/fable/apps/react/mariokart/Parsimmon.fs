[<RequireQualifiedAccess>]
module Utilitarian.Fable.Parsimmon

open Fable.Parsimmon

let tryParse (parser : IParser<'t>) str =
  parser |> Parsimmon.parse str

let (|TryParse|_|) = tryParse

let scParser singleCase tagStr=
  Parsimmon.str tagStr |> Parsimmon.map (fun _ -> singleCase)


/// #### Description
/// This is a domain extension module offering platform-dependent (Parsimmon : Fable JS) parsers and parse type augmentations using the Parsimmon parser combinator library. 
/// Platform-specific domain code like Parsimmon parsers need to go into an extra implementation file to guard our domain code from parser combinator implementation details. 
/// No Parsimmon parser or other any other such library specifics should be made visible to the domain layer. 
/// Instead, offer ready to use parse functions for your domain types. Those should always be of shape `string option -> 'T` 
module Mariokart =

  open Utilitarian.Mariokart

  let pCharacter _ = 
    Parsimmon.choose [
      scParser BowserJr Character.L.BowserJr
      scParser Bowser Character.L.Bowser
      scParser DryBowser Character.L.DryBowser
      scParser Wario Character.L.Wario
      scParser Waluigi Character.L.Waluigi
      scParser Peach Character.L.Peach
      scParser Mario Character.L.Mario
      scParser Luigi Character.L.Luigi
      scParser BabyLuigi Character.L.BabyLuigi
  ]

  let pRoadType _ = 
    Parsimmon.choose [
      scParser Ground RoadType.L.Ground
      scParser AntiGravity RoadType.L.AntiGravity
      scParser Air RoadType.L.Air
      scParser Water RoadType.L.Water
    ]

  let pAttribute _ = 

    let pWithRoadType attrLiteral rtCase =
      Parsimmon.str attrLiteral
      |> Parsimmon.chain (Parsimmon.str Attribute.L.RoadTypeSep)
      |> Parsimmon.chain (pRoadType ())
      |> Parsimmon.map rtCase

    Parsimmon.choose [
      pWithRoadType Attribute.L.Speed Speed
      pWithRoadType Attribute.L.Handling Handling
      scParser Acceleration Attribute.L.Acceleration
      scParser Traction Attribute.L.Traction
      scParser Weight Attribute.L.Weight
      scParser MiniTurbo Attribute.L.MiniTurbo
    ]

  [<AutoOpen>]
  module DomainExtensions =
    
    type Character with
      static member FromCanonicalString str = pCharacter() |> Parsimmon.parse str 
    
    type RoadType with 
      static member FromCanonicalString str = pRoadType() |> Parsimmon.parse str 
    
    type Attribute with 
      static member FromCanonicalString str = pAttribute() |> Parsimmon.parse str 