[<AutoOpen>]
module Utilitarian.Units

type [<Measure>] bytes
type [<Measure>] percent

type IllegalOutcome = DividedByZero

let inline measuredDecimal<[<Measure>] 'U> = 
  LanguagePrimitives.DecimalWithMeasure<'U>

let inline measuredFloat<[<Measure>] 'U> = 
  LanguagePrimitives.FloatWithMeasure<'U>

let inline measuredInt<[<Measure>] 'U> = 
  LanguagePrimitives.Int32WithMeasure<'U>

type ProgressInt<[<Measure>] 'TMeasure> = { 
  Done : int<'TMeasure>
  Total : int<'TMeasure>  
}
with 
  member x.Ratio : Partial<_,_> =
    if (x.Total |> int = 0) then
      DividedByZero |> Error
    else
      (x.Done)/(x.Total) |> Ok
 
type ProgressDecimal<[<Measure>] 'TMeasure> = { 
  Done : decimal<'TMeasure>
  Total : decimal<'TMeasure>  
}
with 
  member x.Ratio : Partial<_,_> =
    if (x.Total |> decimal = 0M) then
      DividedByZero |> Error
    else
      (x.Done)/(x.Total) |> Ok
 

