namespace GabeSoft.Common

open System

module Geom =
   /// Converts the given degrees to radians.
   let DegToRad d = (Math.PI * d) / 180.
   /// Converts the given radians to degrees.
   let RadToDeg r = (r * 180.) / Math.PI   

   /// Sine of the given number in degrees.
   let SinDeg d = sin (DegToRad d)
   /// Cosine of the given number in degrees.
   let CosDeg d = cos (RadToDeg d)

   let private dotProductCore<'a> (add : 'a -> 'a -> 'a) (mul : 'a -> 'a -> 'a) (v1 : seq<'a>, v2 : seq<'a>) : 'a = 
      let mutable sum = Unchecked.defaultof<'a>
      use e1 = v1.GetEnumerator()
      use e2 = v2.GetEnumerator()
      while e1.MoveNext() && e2.MoveNext() do
         sum <- add sum (mul e1.Current e2.Current)
      sum

   /// Computes the dot product of the given vectors.
   let dotProduct (v1 : seq<'a>) (v2 : seq<'a>) =
      if typeof<'a> = typeof<int> then 
         dotProductCore<int>     ( + ) ( * ) (downcast (box v1), downcast (box v2)) |> box :?> 'a 
      elif typeof<'a> = typeof<float> then 
         dotProductCore<float>   ( + ) ( * ) (downcast (box v1), downcast (box v2)) |> box :?> 'a 
      elif typeof<'a> = typeof<int64> then
         dotProductCore<int64>   ( + ) ( * ) (downcast (box v1), downcast (box v2)) |> box :?> 'a 
      else 
         failwithf "type %s is not supported" typeof<'a>.Name

   /// Computes the euclidean length of the given vector.
   let vecLength (v : seq<'a>) = 
      let p = dotProduct v v
      sqrt (Core.floatEx p)
      
   