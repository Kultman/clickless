namespace GabeSoft.Common

open System
open System.Text.RegularExpressions

type BTree = class end

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module CoreBTree =
   let ofString f =
      let full_node root = function
         | [l; r] -> BNode(l, f root, r)
         | _      -> raise (BTreeExc "invalid children count")
      let leaf_node _ = function
         | ""        -> BLeaf
         | _ as root -> BNode(BLeaf, f root, BLeaf)
      Tree.treeOfString leaf_node full_node

   let rec private stringOfBTree f = function
      | BLeaf                    -> ""
      | BNode(BLeaf, a, BLeaf)   -> f a
      | BNode(l, a, r)           -> sprintf "%s(%s, %s)"
                                          (f a)
                                          (stringOfBTree f l)
                                          (stringOfBTree f r)     

   let toString t = stringOfBTree (fun a -> a.ToString()) t

   let rec private btreeHom f v = function
      | BNode (l, a, r) -> f (btreeHom f v l, a, btreeHom f v r)
      | BLeaf -> v
 
   /// tree height
   let height t =
      btreeHom (fun (l, _, r) -> 1 + max l r) 0 t
 
   /// node count
   let size t =
      btreeHom (fun (l, _, r) -> 1 + l + r) 0 t
 
   let map f t =
      btreeHom (fun (l, a, r) -> BNode(l, f a, r)) BLeaf t

   let mirror t =
      btreeHom (fun (l, a, r) -> BNode(r, a, l)) BLeaf t

   /// left binary tree infix traversal
   let rec foldl f t x =
      match t with
      | BLeaf -> x
      | BNode (l, a, r) -> foldl f l (f a (foldl f r x))
  
   /// right binary tree infix traversal
   let rec foldr f x t =
      match t with
      | BLeaf -> x
      | BNode (l, a, r) -> foldr f (f (foldr f x l) a) r
 
   let flatl t = foldl (fun x l -> x :: l) t []
   let flatr t = foldr (fun x l -> l :: x) [] t
   let flat = flatl

   let rec iter f = function
      | BLeaf -> ()
      | BNode (l, a, r) ->
         iter f l
         f a
         iter f r

   type BTree with
      static member toString t = toString t
      static member ofString s = ofString s
      /// tree height
      static member height t = height t
      /// node count
      static member size t = size t
      static member map f t = map f t
      static member mirror t = mirror t
      /// left binary tree infix traversal
      static member foldl f t x = foldl f t x
      /// right binary tree infix traversal
      static member foldr f x t = foldr f x t
      static member flatl t = flatl t
      static member flatr t = flatr t
      static member iter = iter