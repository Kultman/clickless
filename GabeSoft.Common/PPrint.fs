namespace GabeSoft.Common

open System

/// Pretty print document.
type PDoc = 
   /// text ""
   | Nil                            
   /// text "" $$ x
   | NilAbove of PDoc               
   /// text s <> x
   | TextBeside of (string * PDoc)  
   /// nest k x
   | Nest of (int * PDoc)           
   /// x U y
   | Union of (PDoc * PDoc)         
   /// {}
   | Empty                          

/// Pretty print library.
module PPrint =
   /// Creates a document from a string.
   let text s = TextBeside (s, Nil)
   /// Nests a document the specified number of spaces.
   let nest k x = Nest (k, x)
   
   /// Horizontal composition: x <> y
   let rec beside x y = 
      match x, y with
      | Nil, Nest (k, x)      -> beside Nil x
      | Nil, x                -> x
      | NilAbove x, y         -> NilAbove (beside x y)
      | TextBeside (s, x), y  -> TextBeside (s, beside x y)
      | Nest (k, x), y        -> Nest (k, beside x y)
      | Empty, y              -> Empty
      | Union (x, y), z       -> Union (beside x z, beside y z)

   /// Vertical composition: x $$ y
   let above x y = 
      let rec aboveNest k d1 d2 = 
         match d1 with
         | Nil                -> NilAbove (nest k d2)
         | NilAbove x         -> NilAbove (aboveNest k x d2)
         | TextBeside (s, x)  -> TextBeside (s, aboveNest (k - s.Length) (beside Nil x) d2)
         | Nest (k2, x)       -> Nest (k2, aboveNest (k - k2) x d2)
         | Union (x, y)       -> Union (aboveNest k x d2, aboveNest k y d2)
         | Empty              -> Empty
      aboveNest 0 x y

   /// Restricts the given document to layouts that fit on one line.
   let rec fit = function
   | Nil                      -> Nil
   | NilAbove x               -> Empty
   | TextBeside (s, x)        -> TextBeside (s, fit x)
   | Nest (k, x)              -> Nest (k, fit x)
   | Union (x, y)             -> fit x
   | Empty                    -> Empty

   let horizontal x y = beside (beside x (text " ")) y
   let vertical k x xs = above x (nest k (List.foldBack1 above xs))

   /// Combines a list of documents horizontally or vertically depending on the context.
   let sep docs = 
      let rec core k doc docs = 
         match doc with
         | Nil                -> Union (docs |> List.fold horizontal Nil |> fit, vertical k Nil docs)
         | NilAbove x         -> vertical k (NilAbove x) docs
         | TextBeside (s, x)  -> TextBeside (s, core (k - s.Length) (beside Nil x) docs)
         | Nest (k2, x)       -> Nest (k2, core (k - k2) x docs)
         | Union (x, y)       -> Union (core k x docs, vertical k y docs)
         | Empty              -> Empty

      match docs with
      | [x]    -> x
      | x::xs  -> core 0 x xs
      | _      -> failwith "at least one document expected"

   let rec fits n k x = 
      if n < k then false else
      match x with
      | Nil                   -> true
      | NilAbove _            -> true
      | TextBeside (t, y)     -> fits n (k + t.Length) y
      | Empty                 -> false
      | _                     -> false

   let nicest w r (s:string) x y = 
      if fits (min w r) (s.Length) x then x else y

   /// Finds the best layout for the specified document according to the 
   /// given page width (w) and ribbon width (r). The ribbon width is the 
   /// width of the ribbon of text on the page.
   let rec best w r doc = 
      let rec core w r (s:string) = function
      | Nil                   -> Nil
      | NilAbove x            -> NilAbove (best (w - s.Length) r x)
      | TextBeside (t, x)     -> TextBeside (t, core w r (s + t) x)
      | Nest (k, x)           -> core w r s x
      | Union (x, y)          -> nicest w r s (core w r s x) (core w r s y)
      | Empty                 -> Empty

      match doc with
      | Nil                   -> Nil
      | NilAbove x            -> NilAbove (best w r x)
      | TextBeside (s, x)     -> TextBeside (s, core w r s x)
      | Nest (k, x)           -> Nest (k, best (w - k) r x)
      | Union (x, y)          -> nicest w r String.Empty (best w r x) (best w r y)
      | Empty                 -> Empty

   /// Maps a document layout to an appropriate string.
   let rec layout k doc = 
      let spaces k = 
         [ for i in 1 .. k -> " " ] |> String.concat String.Empty
      
      let rec core k = function
      | Nil                      -> "\n"
      | NilAbove x               -> "\n" + layout k x
      | TextBeside (s, x)        -> s + core (k + s.Length) x
      | x                        -> failwithf "invalid document encountered %A" x
      
      match doc with
      | Nest (k2, x)             -> layout (k + k2) x
      | x                        -> spaces k + core k x

   /// Pretty prints the specified document according to the page width (w) and ribbon width (r).
   let pretty w r d = layout 0 (best w r d)

module PPrintUse = 
   let (<|>) = PPrint.above
   let (<->) = PPrint.beside
