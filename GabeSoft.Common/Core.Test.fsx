#r "System.Xml.Linq.dll"
open System
open System.Linq

#load "Types.fs"
open GabeSoft.Common
#load "Core.fs"
open GabeSoft.Common
#load "Core.BTree.fs"
open GabeSoft.Common
#load "Core.GTree.fs"
open GabeSoft.Common

open GabeSoft.Common.CoreBTree
open GabeSoft.Common.CoreGTree

let nodeStr = fun x -> x.ToString().Trim()
let treeOps t =
   printfn "TREE: %s" (GTree.toString t)
   printfn "SIZE: %d" (GTree.size t)
   printfn "FLAT: %A" (GTree.flat t)
   printfn "HEIGHT: %d" (GTree.height t)
   printfn "MIRROR: %s" (t |> GTree.mirror |> GTree.toString)
   printfn "MAP: %A" (t |> GTree.map (fun (x:string) -> int x.[0]) |> GTree.toString)

(*
BTree.ofString int "2 (3,4)"
BTree.ofString int "2 (3,)"
BTree.ofString int "2 (,4)"
BTree.ofString int "2 ()"
BTree.ofString int "2"
BTree.ofString int ""
*)

(*
BTree.toString (BTree.ofString int "4(3,5(9,6(7,)))")
BTree.toString (BTree.ofString string "a(b(c,d),e(f,))")
BTree.toString (BTree.ofString int "2 (3,4)")
BTree.toString (BTree.ofString int "2 (3,)")
BTree.toString (BTree.ofString int "2 (,4)")
BTree.toString (BTree.ofString int "2 ()")
BTree.toString (BTree.ofString int "2")
BTree.toString (BTree.ofString int "")
*)
   
(*
BTree.ofString string "a(b(c,d),e(f,))"
|> BTree.map (char >> int)
|> BTree.toString

BTree.ofString string "a(b(c,d),e(f,))"
|> BTree.mirror
|> BTree.toString
*)

(*
BTree.ofString string "a(b(c,d),e(f,))" |> BTree.flatl
BTree.ofString string "a(b(c,d),e(f,))" |> BTree.flatr
*)

(*
BTree.ofString string "a(b(c,d),e(f,))" 
|> BTree.iter (fun a -> printfn "%s" (string a))
*)

(*
GTree.ofString nodeStr "a (b, c(d,e),f())"
GTree.ofString nodeStr "a (b, c(d,e(g, h, k(l, m))),f(), p(q, r, s(y)))"
GTree.ofString nodeStr "a"
GTree.ofString nodeStr ""
GTree.ofString nodeStr "a ()"
GTree.ofString nodeStr "a ( )"
GTree.ofString nodeStr "a (b)"
GTree.ofString nodeStr "a( b )"
GTree.ofString nodeStr "f(x1, y1, z1)"
GTree.ofString nodeStr "u2(a, b)"
GTree.ofString int "2(3,4)"
GTree.ofString int "2(3)"
*)

(*
GTree.toString (GTree.ofString nodeStr "a (b, c(d,e),f())")
GTree.toString (GTree.ofString nodeStr "a (b, c(d,e(g, h, k(l, m))),f(), p(q, r, s(y)))")
GTree.toString (GTree.ofString nodeStr "a")
GTree.toString (GTree.ofString nodeStr "a ()")
GTree.toString (GTree.ofString nodeStr "a ( )")
GTree.toString (GTree.ofString nodeStr "a (b)")
GTree.toString (GTree.ofString nodeStr "a( b )")
*)

(*
"a(b, c(d,e(g, h, k(l, m))),f(), p(q, r, s(y)))"
|> GTree.ofString nodeStr 
|> fun t -> GTree.foldr (fun a acc -> a + acc) t "$"

GTree.ofString nodeStr "a(b, c(d,e(g, h, k(l, m))),f(), p(q, r, s(y)))"
|> GTree.foldl (fun acc a -> a + acc) "$"

GTree.ofString int "1(2,3,4(5,6,7(9,11,12),13(14,15)),10(20(21(22(23)))))"
|> (fun t -> printfn "%d" (GTree.foldr (fun a acc -> a + acc) t 0); t)
|> (fun t -> printfn "%d" (GTree.foldl (fun acc a -> a + acc) 0 t); t)
|> GTree.flat
|> List.fold (fun acc a -> acc + a) 0 
*)
  
(*
"a (b, c(d, e), f)" |> GTree.ofString nodeStr |> treeOps
"a (b, c(d,e(g, h, k(l, m))),f(), p(q, r, s(y)))" |> GTree.ofString nodeStr |> treeOps
"a (b, c(d, e), f, a, b(c, d))" |> GTree.ofString nodeStr |> treeOps
*)
