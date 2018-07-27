[<AutoOpen>]
module Utilitarian.Prelude 

open System

type Partial<'TValue,'TErrorCompensation> = Result<'TValue,'TErrorCompensation>

let inline nameof<'T> = typeof<'T>.Name
let inline enumStr<'Enum> e = Enum.GetName(typeof<'Enum>,e)

let inline nullable v = Nullable<_>(v)

let inline dispose (x : #IDisposable) = x.Dispose()
let inline multiDispose xs = xs |> Seq.iter dispose

/// ### Description
/// Flips return a function derived from another function with altered parameter order.
/// Use them to create alias functions with meaningful names and use flip to correct parameter order.
/// The suffix reflects the permutation of arguments in the resulting function.
let inline flip21 f x y = f y x
let inline flip132 f x y z = f x z y
let inline flip213 f x y z = f y x z
let inline flip231 f x y z = f y z x
let inline flip312 f x y z = f z x y
let inline flip321 f x y z = f z y x

/// ### Description
/// Currying is an operation to turn functions with multiple arguments into a function with just a single tuple argument.
let inline curry f x y = f(x,y)
let inline curry2 f x y z = f(x,y,z)

/// ### Description
/// Uncurrying is the inverse operation of currying. It turns a function with a single tupled argument into a function with multiple arguments.
let inline uncurry f (x,y) = f x y

/// ### Description
/// The `func{n}` functions are convenient constructors for `System.Func`. `{n}` stands for the number of type arguments.
let inline func1 f = Func<_>(f)
let inline func2 f = Func<_,_>(f)
let inline func3 f = Func<_,_,_>(f)
let inline func4 f = Func<_,_,_,_>(f)
let inline func5 f = Func<_,_,_,_,_>(f)

let impossible _ = failwith "Reality is crooked!!! Existence is poisoned!!!"

let inline seconds n  = n |> float |> TimeSpan.FromSeconds
let inline minutes n  = n |> float |> TimeSpan.FromMinutes

let inline chooseNotNull value =
  if value |> isNull then
    None
  else
    Some value          

let (|NotNull|_|) = chooseNotNull

let equalsOn f x (yobj:obj) =
  match yobj with
  :? 'T as y -> (f x = f y)
  | _ -> false

let hashOn f x =  hash (f x)

let compareOn f x (yobj: obj) =
  match yobj with
  | :? 'T as y -> compare (f x) (f y)
  | _ -> invalidArg "yobj" "cannot compare values of different types"