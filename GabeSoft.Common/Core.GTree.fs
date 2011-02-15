namespace GabeSoft.Common

open System
open System.Text.RegularExpressions

type GTree = class end

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module CoreGTree =
   let private rev = List.rev
   let private exists = List.exists
   let private it_list = List.fold
   let private list_it = List.foldBack
   let private cons = fun a l -> a::l

   let ofString f =
      Tree.treeOfString (fun has_children info -> GNode(f info, []))
                        (fun info children -> GNode(f info, children))

   let rec stringOfGTree f (GNode(info, children)) =
       match children with
       | []     -> sprintf "%s" (f info)
       | _ as l ->
          let arr = [| for c in List.map (stringOfGTree f) l -> c |]
          sprintf "%s(%s)" (f info) (String.Join(", ", arr))

   let toString t = stringOfGTree (fun a -> a.ToString()) t

   /// tree traversal
   let rec trav h g x (GNode (a, l)) =
       h a (list_it ((trav h g x) >> g) l x)

   /// infix tree traversal right
   let rec foldr f (GNode (a, l)) x =   
      f a (list_it (fun b acc -> foldr f b acc) l x)

   /// infix tree traversal left
   let rec foldl f x (GNode (a, l)) = 
      it_list (fun acc b -> foldl f acc b) (f x a) l 
      
   let size t = trav (fun x l -> 1 + l) (+) 0 t
   let height t = trav (fun x l -> 1 + l) max 0 t
   let flat t = trav (fun x l -> x::l) (@) [] t
   let map f t = trav (fun x l -> GNode(f x, l)) cons [] t
   let mirror t = trav (fun x l -> GNode(x, l)) (fun x l -> l @ [x]) [] t

   let rec iter f (GNode(x, l)) =
       f x
       List.iter (iter f) l

   type GTree with
      static member toString t = toString t
      static member ofString s = ofString s
      /// tree traversal
      static member trav t = trav t
      /// right infix tree traversal
      static member foldr f t x = foldr f t x
      /// left infix tree traversal
      static member foldl f x t = foldl f x t
      /// node count
      static member size t = size t
      /// tree height
      static member height t = height t
      static member flat t = flat t
      static member map t = map t
      static member mirror t = mirror t
      static member iter t = iter t