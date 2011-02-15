namespace GabeSoft.Drawing   

open System
open System.Drawing
open GabeSoft.Common
open GabeSoft.Common.CoreGTree
open GabeSoft.Drawing.Draw

module DrawGTree =
   let private hd = List.head
   let private tl = List.tail
   let private rev = List.rev
   let private len = List.length
   let private fold = List.fold

   let private partition test l = 
      let switch elem (l1, l2) = 
         if test elem 
         then (l1, elem::l2) 
         else (elem::l1, l2)
      List.foldBack switch l ([], [])
   let private keep test = (partition test) >> snd

   let rec private find prop = function
      | [] -> None
      | a::l -> if prop a then Some a else find prop l

   let rec private pairsfoldl f e l =
      match l with
      | []           -> e
      | [h]          -> failwith "the list has only one element"
      | h1::h2::[]   -> f e h1 h2
      | h1::h2::t    -> pairsfoldl f (f e h1 h2) (h2::t)
   let private pairsfoldr f l e = pairsfoldl (fun acc a b -> f b a acc) e (rev l)

   /// Merges a picture tree into a single picture.
   let drawPictureTree tsty t =
      let rec draw spread coefs pt = function
      | GNode (pict : picture, [])     -> pict.CenterAround pt 
      | GNode (pict : picture, sons)   -> 
         let spread:float = spread * (hd coefs)
         let points = List.foldBack (fun c (d, acc) -> 
                              (d + spread), 
                              (c, {xc=pt.xc+d; yc=pt.yc-tsty.Vdist})::acc)
                              (rev sons) (0., []) 
                     |> snd
                     |> List.map (fun (t, p:point) -> 
                                 let xamt = - (spread * float (len sons - 1) / 2.)
                                 t, Trans.apply (Trans.translation (xamt, 0.)) p)
         let lines = List.map (fun (t, p) -> 
                                 picture.Draw tsty.Tlstyl 
                                       tsty.Tcolor
                                       (sketch.Make [ Seg [pt; p] ])) points
         let rest = List.map (fun (t, p) -> draw spread (tl coefs) p t) points
         picture.Group (lines @ [ pict.CenterAround pt ] @ rest)
      draw tsty.Hdist tsty.Coefs point.Origin t

   let drawStrNode fontSize nodeContents =
      let fs = fontSize
      let nc = nodeContents
      let text =  picture.Text (font.Make Helvetica fs) Color.Black nc 
                  |> centerPic point.Origin
      let fill =  picture.Fill Alternate Color.White (sketch.Make [ Arc (point.Origin, fs, 0., 360.) ])
      let contour = picture.Draw {  Width= fs * 0.1; 
                                    Cap=Buttcap; 
                                    Join=Miterjoin; 
                                    Dashpat=[] }
                                    Color.Black
                                    (sketch.Make [ Arc (point.Origin, fs, 0., 360.) ])
      picture.Group [ fill; contour; text ]
   
   let drawIntNode fontSize (n:int) = drawStrNode fontSize (string n)

   let computeSize (hcoef, wcoef) t =
      let h, w = GTree.foldl (fun (x, y) (p : picture) ->   
                              max x (p.Height),
                              max y (p.Width))
                             (0., 0.) t
      h * hcoef, w * wcoef                     

   let compl f l =
      let rec fold cont = function
         | [], l2 -> cont l2
         | l1, [] -> cont l1
         | a1::l1, a2::l2 -> fold (fun e -> cont ((f a1 a2)::e)) (l1, l2)
      List.fold (fun acc e -> fold (fun x -> x) (acc, e)) [] l

   let minll l = compl min l
   let maxll l = compl max l

   let recomputeTriples (cl:float list) =
      let rec compute (n, cl) = function
         | [] -> []
         | (l,r,c)::t -> (l * n / c, r * n / c, n)::compute(n * (hd cl), (tl cl)) t
      compute (hd cl, tl cl)      
   
   let computeRootCoef trll =
      let rec compute = function
         | [], _ -> []
         | _, [] -> []
         | (_, r1, c)::t1, (l2, _, _)::t2 -> 
            let d = r1 - l2 + c
            (if d <= 0. then 1. else 1./d)::compute (t1, t2)
      let filtered = keep (fun l -> len l > 0) trll         
      let l = if len filtered > 1 then filtered else trll         
      pairsfoldl (fun acc trl1 trl2 -> List.fold min acc (compute (trl1, trl2))) 1. l

   let attachIndexes l =
      let v = float (len l - 1) / 2.
      let attached = List.fold   (fun (v, acc) a -> v + 1., (v + 1., a)::acc)
                                 (-v, [(-v, hd l)])
                                 (tl l)
      (-v, v), rev (snd attached)

   let combineTriples x trl =
      let (minv, maxv), indexed = attachIndexes trl
      let computed = List.map (fun (v, l) -> 
                              List.map (fun (l, r, c) -> 
                                    v + x * l, v + x * r, x * c) l) indexed

      let cll = List.map (fun l -> List.map (fun (_, _, c) -> c) l) computed
      let rll = List.map (fun l -> List.map (fun (_, r, _) -> r) l) computed
      let lll = List.map (fun l -> List.map (fun (l, _, _) -> l) l) computed

      let rl = maxll rll
      let ll = minll lll
      let cl = match find (fun l -> len l = len rl) cll with
               | Some a -> a
               | None -> failwith "malfunction"

      (minv, maxv, 1.)::(List.zip3 ll rl cl)

   let computeCoefs t =
      let rec comp = function
      | GNode (a, [])    -> [1.], []
      | GNode (a, [c])   -> 
         let cl, trl = comp c
         1.::cl, (0., 0., 1.)::trl
      | GNode (a, sons)  -> 
         let coefs = List.map comp sons
         let cl = minll (List.map fst coefs)
         let trll = List.map snd coefs |> List.map (recomputeTriples cl)
         let x = computeRootCoef trll
         (1.::x::(tl cl), combineTriples x trll)
      fst (comp t)

   /// Draws a generic tree into a picture
   let draw drn (vcoef, hcoef) color t = 
      let tp = GTree.map drn t
      let coefs = computeCoefs t
      let height, width = computeSize (vcoef, hcoef) tp
      let startWidth = width / (List.fold (*) 1. coefs)
      let tsty = {   Vdist = height; 
                     Hdist = startWidth; 
                     Coefs = coefs;
                     Tlstyl = {  Width = height / 50.;
                                 Cap = Buttcap;
                                 Join = Beveljoin;
                                 Dashpat = [] };
                     Tcolor = color }
      drawPictureTree tsty tp

   type GTree with
      static member drawIntNode fontSize n = drawIntNode fontSize n
      static member drawStrNode fontSize nodeText  = drawStrNode fontSize nodeText 
      static member drawPictureTree tsty t = drawPictureTree tsty t
      static member draw drawNode vcoef hcoef color t = draw drawNode (vcoef, hcoef) color t
