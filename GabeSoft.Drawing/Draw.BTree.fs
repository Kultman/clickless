namespace GabeSoft.Drawing   

open System
open System.Drawing
open GabeSoft.Common
open GabeSoft.Common.CoreBTree
open GabeSoft.Drawing.Draw

module DrawBTree =
   let private hd = List.head
   let private tl = List.tail
   let private fold = List.fold

   /// Merges a picture tree into a single picture
   let drawPictureTree tsty t = 
      let rec drawCore spread coefs ({xc=x; yc=y} : point as pt) = function
      | BLeaf ->  picture.Blank
      | BNode (BLeaf, pict : picture, BLeaf) -> pict.CenterAround pt 
      | BNode (t1, pict, t2) -> 
         let d = spread * hd coefs 
         let pt1 = point.Make (x - d / 2.) (y - tsty.Vdist)
         let pt2 = point.Make (x + d / 2.) (y - tsty.Vdist)
         let line1 = picture.Draw tsty.Tlstyl tsty.Tcolor (sketch.Make [ Seg [ pt; pt1 ] ])
         let line2 = picture.Draw tsty.Tlstyl tsty.Tcolor (sketch.Make [ Seg [ pt; pt2 ] ])
         match t1, t2 with
         | _, BLeaf -> 
            picture.Group [   line1; 
                              pict.CenterAround pt; 
                              drawCore d (tl coefs) pt1 t1 ]
         | BLeaf, _ -> 
            picture.Group [   line2; 
                              pict.CenterAround pt; 
                              drawCore d (tl coefs) pt2 t2 ]
         | _ ->
            picture.Group [   line1; 
                              line2; 
                              pict.CenterAround pt;
                              drawCore d (tl coefs) pt1 t1;
                              drawCore d (tl coefs) pt2 t2 ]
      drawCore tsty.Hdist tsty.Coefs point.Origin t

   let drawStrNode fontSize nodeText =
      let fs = fontSize
      let nc = nodeText
      let text = picture.Text (font.Make Helvetica fs) Color.Black nc |> centerPic point.Origin
      let fill = picture.Fill Alternate Color.White (sketch.Make [ Arc (point.Origin, fs, 0., 360.) ])
      let contour = picture.Draw { Width = fs * 0.1; Cap = Buttcap; Join = Miterjoin; Dashpat = [] }
                                 Color.Black
                                 (sketch.Make [ Arc (point.Origin, fs, 0., 360.) ])
      picture.Group [ fill; contour; text ]
   
   let drawIntNode fontSize (n:int) = drawStrNode fontSize (string n)
   
   let private computeHeightWidth (hcoef, wcoef) t =
      let h, w = BTree.foldr  (fun (x, y) (p : picture) -> (max x (p.Height), max y (p.Width)))
                              (0., 0.) t
      h * hcoef, w * wcoef
   
   let rec private minl = function
      | [], l2 -> l2
      | l1, [] -> l1
      | a1::l1, a2::l2 -> (min a1 a2)::minl(l1, l2)

   let private recomputeTriples (cl:float list) =
      let rec compute (n, cl) = function
         | [] -> []
         | (l,r,c)::t -> (l * n / c, r * n / c, n)::compute(n * (hd cl), (tl cl)) t
      compute (hd cl, tl cl)      
   
   let competHeadCoef (trl1, trl2) =
      let rec compute = function
         | [], _ -> []
         | _, [] -> []
         | (_, r1, c)::t1, (l2, _, _)::t2 -> 
            let d = r1 - l2 + c
            (if d <= 0. then 1. else 1./d)::compute (t1, t2)
      fold min 1.0 (compute (trl1, trl2))   
   
   let private combineTriples x (trl1, trl2) =
      let rec combine = function
         | [], [] -> []
         | (l1, r1, c)::t1, [] -> 
            (-0.5 + x * l1, -0.5 + x * r1, c * x)::combine(t1, [])
         | [], (l2, r2, c)::t2 ->
            (0.5 + x * l2,  0.5 + x * r2, c * x)::combine([], t2)
         | (l1, r1, c)::t1, (l2, r2, _)::t2 ->
            (-0.5 + x * l1,  0.5 + x * r2, c * x)::combine(t1, t2)
      (-0.5, 0.5, 1.)::combine(trl1, trl2)
   
   let private computeCoefs t =
      let rec compute = function
         | BLeaf -> [], []
         | BNode (BLeaf, _, BLeaf) -> [1.], []
         | BNode (t1, _, BLeaf) -> 
            let cl, trl = compute t1
            1.0::cl, 
            (-0.5, -0.5, 1.0)::List.map (fun (l, r, c) -> -0.5 + l, -0.5 + r, c) trl
         | BNode (BLeaf, _, t2) -> 
            let cl, trl = compute t2
            1.0::cl,
            (0.5, 0.5, 1.0)::List.map (fun (l, r, c) -> 0.5 + l, 0.5 + r, c) trl
         | BNode (t1, _, t2) ->
            let cl1, trl1 = compute t1
            let cl2, trl2 = compute t2
            let cl = minl (cl1, cl2)
            let trl1 = recomputeTriples cl trl1
            let trl2 = recomputeTriples cl trl2
            let x = competHeadCoef (trl1, trl2)
            1.0::x::(tl cl), combineTriples x (trl1, trl2)
      fst (compute t)

   /// Draws a binary tree into a picture
   let draw drawNode (vcoef, hcoef) color t =
      let tp = BTree.map drawNode t
      let coefs = computeCoefs t
      let height, width = computeHeightWidth (vcoef, hcoef) tp
      let start_width = width / (fold ( * ) 1. coefs)
      let tsty = { Vdist=height;
                  Hdist=start_width;
                  Coefs=coefs;
                  Tlstyl={ Width=height / 50.; Cap=Buttcap; Join=Beveljoin; Dashpat=[]};
                  Tcolor=color }
      drawPictureTree tsty tp

   type BTree with
      static member drawIntNode fontSize n = drawIntNode fontSize n
      static member drawStrNode fontSize nodeText  = drawStrNode fontSize nodeText 
      static member drawPictureTree tsty t = drawPictureTree tsty t
      static member draw drawNode vcoef hcoef color t = draw drawNode (vcoef, hcoef) color t
