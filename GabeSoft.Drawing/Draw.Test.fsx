#r "bin\Debug\GabeSoft.Common.dll"
open GabeSoft.Common

#load "Types.fs"
open GabeSoft.Drawing
#load "Trans.fs"
open GabeSoft.Drawing
#load "Draw.fs"
open GabeSoft.Drawing
#load "Draw.BTree.fs"
open GabeSoft.Drawing
#load "Draw.GTree.fs"
open GabeSoft.Drawing

open System
open System.Drawing
open GabeSoft.Common.CoreBTree
open GabeSoft.Common.CoreGTree
open GabeSoft.Drawing.Draw
open GabeSoft.Drawing.DrawBTree
open GabeSoft.Drawing.DrawGTree


let styl1 = { Width=1.; Cap=Buttcap; Join=Beveljoin; Dashpat=[] }
let styl2 = Alternate
let sketch1 = sketch.Make [ Seg [   point.Make  20. 20.; 
                                    point.Make 20. 100.; 
                                    point.Make 100. 20.; 
                                    point.Make 20. 20. ] ]
let sketch2 = sketch.Make [   Arc (point.Make 100. 100., 10., 0., 360.);
                              Arc (point.Make 200. 200., 10., 0., 360.);
                              Arc (point.Make 300. 300., 10., 0., 360.);
                              Arc (point.Make 400. 400., 10., 0., 360.) ]
                
let pic1 = picture.Draw styl1 Color.Black sketch1
let pic2 = picture.Fill Alternate Color.Navy sketch1
let pic3 = picture.Text { Style=Helvetica; Size=20. } Color.Blue "I'm HOME"
let pic5 = picture.Draw styl1 Color.Red sketch2
let pic4 = picture.Group [ pic2
                           pic1.CenterAround (point.Make 200. 200.)
                           pic3.CenterAround (point.Make 300. 300.) 
                           pic5 ]
let pic6 =  pic4
            |> transformPic (picRotation (point.Make 0. 0.) 20.)
            |> transformPic (picScaling (2., 2.))          
let pic7 =  pic4 
            |> transformPic (picRotation (point.Make 0. 0.) 60.)
            |> transformPic (picScaling (0.5, 0.5))          
let pic8 = picture.Fill Alternate Color.Gray sketch2
let pic9 = picture.Group [ 
                           centerPic   (point.Make 100. 100.)
                                       (picture.Text { Style=Arial; Size=12. } Color.Navy "a");
                           centerPic   (point.Make 200. 200.)
                                       (picture.Text { Style=Arial; Size=12. } Color.Navy "b");
                           centerPic   (point.Make 300. 300.)
                                       (picture.Text { Style=Arial; Size=12. } Color.Navy "c");
                           centerPic   (point.Make 400. 400.)
                                       (picture.Text { Style=Arial; Size=12. } Color.Navy "d") ]
let pic10 = picture.Group [ pic5; pic9 ]
let pic11 = picture.Group [ pic8; pic9 ]
let pic12 = pic10.Transform (Draw.picScaling (4., 4.))
let pic13 = pic11.Transform (Draw.picScaling (4., 4.))


let sk3 = sketch.Make [ Seg [ point.Make 40. 20.;
                              point.Make 0. 20.; 
                              point.Make 0. -20.;
                              point.Make 40. -20. ] ]
let pic14 = picture.Draw styl1 Color.Black sk3
let picBlogA = picture.Text { Style=Arial; Size=12. } Color.Black "blog a"
let picBlogB = picture.Text { Style=Arial; Size=12. } Color.Black "blog foo"
let pic15 = picture.Group  [  pic14; 
                              picBlogA
                              |> centerPic (point.Make (41. + picBlogA.Width / 2.)  20.);
                              picBlogB
                              |> centerPic (point.Make (41. + picBlogB.Width / 2.) -20.); ]
            |> transformPic (picScaling (2., 2.))


(*
   showPicture false pic3
   showPicture true (pic3.Transform (picScaling (14., 14.)))
   savePicture @"C:\Temp\Pics\test3.jpg" true pic3 
   showPicture true pic4
   savePicture @"C:\Temp\Pics\test4.jpg" true pic4 
   showPicture true pic9
   savePicture @"C:\Temp\Pics\test9.jpg" true pic9 
   savePicture @"C:\Temp\Pics\test11.jpg" true pic11 
   savePicture @"C:\Temp\Pics\test12.jpg" true pic12 
   savePicture @"C:\Temp\Pics\test15.jpg" true pic15 
*)

(*
   let t1 = BTree.ofString int "1(2,3(4(6,7),5))"
   let lsty1 = { Width=1.; Cap=Buttcap; Join=Beveljoin; Dashpat=[] }
   let cl1 = [1.;1.;1.]
   let tstyl1 = { Vdist=50.; Hdist=50.; Coefs=cl1; Tlstyl=lsty1; Tcolor=Color.Black }
   let p1 = BTree.drawPictureTree tstyl1 (BTree.map (fun x -> picture.Blank) t1)
   Draw.showPicture true p1 

   let t2 = BTree.ofString int "1(2(4(8,9),5(10,11)),3(6(12,13),7(14,15)))"
   let lsty2 = { Width=1.; Cap=Buttcap; Join=Beveljoin; Dashpat=[] }
   let cl2 = [ 1.;0.5;0.5 ]
   let tstyl2 = { Vdist=50.; Hdist=100.; Coefs=cl2; Tlstyl=lsty2; Tcolor=Color.Black }
   let p2 = BTree.drawPictureTree tstyl2 (BTree.map (fun x -> picture.Blank) t2)
   Draw.showPicture true p2 
*)

(*
   let t1 = BTree.ofString int "1(2,3(4(6,7),58907890))"
   let lsty1 = { Width=1.; Cap=Buttcap; Join=Beveljoin; Dashpat=[] }
   let cl1 = [1.;1.;1.]
   let tstyl1 = { Vdist=50.; Hdist=50.; Coefs=cl1; Tlstyl=lsty1; Tcolor=Color.Black }
   let p1 = BTree.drawPictureTree tstyl1 (BTree.map (BTree.drawIntNode 10.) t1)
   Draw.showPicture true p1 
   Draw.showPicture true (p1 |> Draw.transformPic (Draw.picScaling (2., 2.))) 

   let t2 = BTree.ofString int "1(2(4(8,9),5(10,11)),3(6(12,13),7(14,15))))"
   let lsty2 = { Width=1.; Cap=Buttcap; Join=Beveljoin; Dashpat=[] }
   let cl2 = [ 1.;0.5;0.5 ]
   let tstyl2 = { Vdist=50.; Hdist=100.; Coefs=cl2; Tlstyl=lsty2; Tcolor=Color.Black }
   let p2 = BTree.drawPictureTree tstyl2 (BTree.map (BTree.drawIntNode 10.) t2)
   Draw.showPicture true p2 
*)

(*
   let t1 = BTree.ofString int "2(1,20(10(6(4(3,5),8(7,9)),15(12,17)),21))"
   let p1 = BTree.draw (BTree.drawIntNode 10.) 3. 2. Color.Black t1
   Draw.showPicture true p1 

   let t2 = BTree.ofString int (
                  "15(6(4(2(1,3),5),10(8(7,9),12(11,14(13,()))))," +
                  "23(19(17(16,18),21(20,22)),25(24,26((),27))))")
   let p2 = BTree.draw (BTree.drawIntNode 10.) (3., 2. ) Color.Black t2
   Draw.showPicture true p2 


   let t3 = BTree.ofString int "1(2((),3((),5)),4(2(6,()),()))"
   let p3 = BTree.draw (BTree.drawIntNode 10.) (3., 2.) Color.Black t3

   let t4 = BTree.ofString int "1(2(3(4,()),()),2(3(4,()),()))"
   let p4 = BTree.draw (BTree.drawIntNode 10.) (3., 2.) Color.Black t4

   let p5 = picture.Group [ p3 |> Draw.transformPic (Draw.picTranslation (-100., 0.));
                           p4 |> Draw.transformPic (Draw.picTranslation ( 100., 0.)) ]

   Draw.showPicture true p3 
   Draw.showPicture true p4 
   Draw.showPicture true p5 
*)

(*
   let t1 = GTree.ofString int "1(2,14(15,3(6,7,8,9)),15(16,17,4(10,11,12,13)),5)"
   let p1 = GTree.draw (GTree.drawIntNode 12.0) 3.0 2.0 Color.Black t1
   Draw.showPicture true p1

   let t2 = GTree.ofString int "1(2(60,61),14(40,41,42),15(16,17,4(10,11,12,13(22,23(30,31,32,33,34,35,36),24))),50(70(71,72)),5(18(19(20(25(26))))))"
   let p2 = GTree.draw (GTree.drawIntNode 12.0) 3.0 1.5 Color.BlueViolet t2
   Draw.showPicture true p2
*)

let drawNode fontSize n = 
   let text = picture.Text (font.Make Helvetica fontSize) Color.Black (string n)
   let height = text.Height + 8.
   let width = text.Width + 8.
   let rect = Seg [  point.Origin; 
                     point.Make 0.0 height; 
                     point.Make width height; 
                     point.Make width 0.0;
                     point.Origin ]
   let fill = picture.Fill Alternate Color.White (sketch.Make [ rect ])
   let cont = picture.Draw linestyle.Default Color.Blue (sketch.Make [ rect ])
   let center = point.Origin
   picture.Group [   fill.CenterAround center 
                     cont.CenterAround center 
                     text.CenterAround center ]

(*
   let t6 = BTree.ofString int "2(1,20(10(6(86778967(3,5),8(7,9)),1(12,17)),8))"
   let p6 = BTree.draw (drawNode 10.) 3.  2. Color.Black t6
   Draw.showPicture true p6
*)

// graph
let yaxis = Seg [ point.Make 0. 300.; point.Make 0. -300. ]
let xaxis = Seg [ point.Make -300. 0.; point.Make 300. 0. ]
let left = Seg [ point.Make -300. 300.; point.Make -300. -300. ]
let bottom = Seg [ point.Make -300. -300.; point.Make 300. -300. ]
let ytriangle = Seg [ point.Make 0. 300.; point.Make 3. 294.; point.Make -3. 294.; point.Make 0. 300. ]
let xtriangle = Seg [ point.Make 300. 0.; point.Make 294. -3.; point.Make 294. 3.; point.Make 300. 0. ]
let ycap = picture.Fill Alternate Color.Red (sketch.Make [ ytriangle ])
let ycapAxis = ycap.CenterAround (point.Make 0. (300. + ycap.Height / 2.))
let xcap = picture.Fill Alternate Color.Red (sketch.Make [ xtriangle ])
let xcapAxis = xcap.CenterAround (point.Make (300. + xcap.Width / 2.) 0.)
let axes = picture.Draw linestyle.Default Color.Silver (sketch.Make [ xaxis; yaxis; ])
let legend = picture.Draw linestyle.Default Color.Silver (sketch.Make [ left; bottom ])

let drawXCoord x y (value : float) = 
   let line = Seg [ point.Make x (y - 5.); point.Make x (y + 5.) ]
   let mark = picture.Draw linestyle.Default Color.Red (sketch.Make [ line ])
   let text = picture.Text { Style=Arial; Size=10. } Color.Black (string value)
   let center = point.Make x (y - 5. - text.Height / 2. - 2.)
   picture.Group [ mark; text.CenterAround center ]

let drawYCoord x y (value : float) =
   let line = Seg [ point.Make (x - 5.) y; point.Make (x + 5.) y ]
   let mark = picture.Draw linestyle.Default Color.Red (sketch.Make [ line ])
   let text = picture.Text { Style=Arial; Size=10. } Color.Black (string value)
   let center = point.Make (x - 5. - text.Width / 2. - 2.) y
   picture.Group [ mark; text.CenterAround center ]

let drawDot pt = 
   let dot = Seg [ point.Make 0. 0.; point.Make 0. 0.1; point.Make 0.1 0.1; point.Make 0.1 0.; point.Make 0. 0. ]
   let dotpic = picture.Fill Alternate Color.Black (sketch.Make [ dot ]) 
   dotpic.CenterAround pt

let xlegend = [ for i in -10 .. 2 .. 10 -> i ] |> List.map float
let ylegend = [ for i in -1.0 .. 0.5 .. 1.0 -> i ]

let xwidth = (600. / (float (Seq.length xlegend) - 1.))
let xpoints = xlegend |> List.mapi (fun i e -> (-300. + float i * xwidth), e)

let ywidth = (600. / (float (Seq.length ylegend) - 1.))
let ypoints = ylegend |> List.mapi (fun i e -> (-300. + float i * ywidth), e)

let xmarks = xpoints |> List.map (fun (x, e) -> drawXCoord x -300. e) |> picture.Group
let ymarks = ypoints |> List.map (fun (y, e) -> drawYCoord -300. y e) |> picture.Group

// plot for sin
let fplot = tanh 
//let fplot = atan
//let fplot = asin
//let fplot = (fun x -> pown (sin x) 2 + pown (cos x) 2)
//let fplot = fun x -> 1. / sqrt x
//let fplot = fun x -> x * x * x
//let fplot = fun x -> x * x
//let fplot = tan
//let fplot = sin
let xplot = [ for i in -10. .. 0.004 .. 10. -> i ]
let yplot = xplot |> List.map fplot

let trans = Trans.scaling (30.0, 300.0)
let plots = xplot 
            |> List.map (fun x -> Trans.apply trans (point.Make x (fplot x))) 
            |> List.filter (fun pt -> pt.yc <= 300. && pt.yc >= -300.)
   
let dots = plots |> List.map drawDot |> picture.Group
let xintercepts = plots |> List.filter (fun pt -> pt.yc < 0.3 && pt.yc > -0.3)

// Draw.showPicture true (picture.Group [ axes; legend; ycapAxis; xcapAxis; xmarks; ymarks; dots ])

#load "Graph.fs"
open GabeSoft.Drawing

let gb1 = Graph.plot {  minX = -5.; maxX = 15.; 
                        minY = -100.; maxY = 30.;
                        stepX = 2.5; stepY = 5.;
                        funs = [ { color = Color.Black; lineWidth = 0.1; step = 0.006; f = fun x -> x * x * x };
                                 { color = Color.Navy; lineWidth = 0.1; step = 0.006; f = fun x -> Math.Pow (Math.E, -x) } ] } 

let gb2 = Graph.plot {  minX = 0.; maxX = 45.; 
                        minY = 0.; maxY = 600.; 
                        stepX = 7.5; stepY = 55.;
                        funs = [ { color = Color.Black; lineWidth = 0.5; step = 0.005; f = fun x -> x * x / Math.Pow(Math.E, (sin x)) } ] } 

let gb3 = Graph.plot {  minX = -5.; maxX = 15.; 
                        minY = -2.; maxY = 30.; 
                        stepX = 2.5; stepY = 5.;
                        funs = [ { color = Color.Goldenrod; lineWidth = 0.1; step = 0.005; f = fun x -> 1. / x }
                                 { color = Color.Chartreuse; lineWidth = 0.1; step = 0.005; f = sqrt }
                                 { color = Color.Navy; lineWidth = 0.1; step = 0.009; f = sin }
                                 { color = Color.Black; lineWidth = 0.1; step = 0.005; f = fun x -> Math.Pow (Math.E, 1. / x) }
                                 { color = Color.DarkRed; lineWidth = 0.1; step = 0.007; f = fun x -> x ** sqrt x + (1. / cos x * x) } ] } 

let rand = new Random()
let arr = [| for i in 1 .. 100 -> rand.Next (-50, 70) |]
let arrf = fun (x:float) -> float arr.[int x]

let gb4 = Graph.plot {  minX = 0.0; maxX = 99.0;
                        minY = -60.0; maxY = 90.0;
                        stepX = 10.0; stepY = 10.0;
                        funs = [ { color = Color.Black; lineWidth = 0.8; step = 1.0; f = arrf } ] }

let gb5 = Graph.plot {  minX = 0.0; maxX = 10.0;
                        minY = -1.0; maxY = 1.0;
                        stepX = 2.0; stepY = 0.5;
                        funs = [ { color = Color.DarkRed; lineWidth = 0.1; step = 0.003; f = sin } ] }

let gb6 = Graph.plot { minX = -10.0; maxX = 50.0;
                       minY = -0.5; maxY = 1.0;
                       stepX = 5.0; stepY = 0.2;
                       funs = [ { color = Color.Black; lineWidth = 0.1; step = 0.005; f = fun x -> (1.0 / x) * sin x } ] }

let gb7 = Graph.plot { minX = -50.0; maxX = 50.0;
                       minY = -40.0; maxY = 40.0;
                       stepX = 5.0; stepY = 5.0;
                       funs = [ { color = Color.Black; lineWidth = 0.1; step = 0.009; f = fun x -> (1.0 / sin x) * x } 
                                { color = Color.DarkGoldenrod; lineWidth = 0.1; step = 0.009; f = sin } 
                                { color = Color.OrangeRed; lineWidth = 0.1; step = 0.009; f = fun x -> 1.0 / x }] }

let gb8 = Graph.plot { minX = -6.0; maxX = 6.0;
                       minY = -8.0; maxY = 8.0;
                       stepX = 1.0; stepY = 1.0;
                       funs = [ { color = Color.Black; lineWidth = 0.1; step = 0.004; f = fun x -> (1.0 / sin x) * x } 
                                { color = Color.DarkGoldenrod; lineWidth = 0.1; step = 0.009; f = sin } 
                                { color = Color.OrangeRed; lineWidth = 0.1; step = 0.004; f = fun x -> 1.0 / x }] }

(*
Draw.showPicture true gb1
Draw.showPicture true gb2
Draw.showPicture true gb3
Draw.showPicture true gb4 
Draw.showPicture true gb5
Draw.showPicture true gb6
Draw.showPicture true gb7
Draw.showPicture true gb8
*)

(*
Draw.savePicture @"C:\Temp\graph1.jpg" true gb1
Draw.savePicture @"C:\Temp\graph2.jpg" true gb2
Draw.savePicture @"C:\Temp\graph3.jpg" true gb3
*)
