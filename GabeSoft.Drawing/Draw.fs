namespace GabeSoft.Drawing

open System
open System.Drawing
open System.Drawing.Drawing2D

module Draw =
   let toPicCoord = 
       Trans.apply { m11=1.; m12=0.; m13=0.; m21=0.; m22= -1.; m23=0. }
   let fromPicCoord = 
      Trans.apply { m11=1.; m12=0.; m13=0.; m21=0.; m22= -1.; m23=0. }
   let picTranslation (x, y) = Trans.translation (x, -y)
   let picRotation p angle_deg = Trans.rotation (toPicCoord p) -angle_deg
   let picScaling (sx, sy) = Trans.scaling (sx, sy)

   type PointF with
      static member Make point = 
         let pc = toPicCoord point
         new PointF (float32 pc.xc, float32 pc.yc)

   let addSketchToPath s (path:GraphicsPath) = 
      for e in s.Elements do
         use epath = new GraphicsPath()
         match e with
         | Arc (center, radius, tetha1, tetha2) -> 
            let height = float32 (radius * 2.)
            let width = float32 (radius * 2.)
            let top = toPicCoord (point.Make (center.xc - radius) (center.yc + radius))
            let sweep = float32 (tetha2 - tetha1)
            let start = float32 (360. - tetha1) - sweep
            epath.AddArc(float32 top.xc, float32 top.yc, width, height, start, sweep)
         | Bez (p1, p2, p3, p4) -> 
            let startp = PointF.Make p1
            let controlp1 = PointF.Make p2
            let controlp2 = PointF.Make p3
            let endp = PointF.Make p4
            epath.AddBezier(startp, controlp1, controlp2, endp)
         | Bezs l ->
            let pts = List.map (fun p -> PointF.Make p) l
            epath.AddBeziers(Array.ofList pts)
         | Seg l ->
            let pts = List.map (fun p -> PointF.Make p) l
            epath.AddLines(Array.ofList pts)                        
         path.AddPath(epath, false)

   let private makeDrawPic style color sketch : picture = 
      let path = new GraphicsPath()
      addSketchToPath sketch path
      picture.Make [ {  Settings = PLine style; 
                        Contents = path;
                        Color = color } ]

   let private makeFillPic style color sketch : picture =
      let path = new GraphicsPath()
      addSketchToPath sketch path   
      picture.Make [ {  Settings = PFill style;
                        Contents = path; 
                        Color = color } ]

   let private makeTextPic ({ Style=sty; Size=sz } as f : font) color text : picture = 
      let path = new GraphicsPath()
      let font_family = function 
         | Helvetica -> new FontFamily("Helvetica")
         | Symbol -> new FontFamily("Symbol")
         | Arial -> new FontFamily("Arial")
      path.AddString(text, font_family sty, int FontStyle.Regular, float32 sz, 
                     PointF.Make point.Origin, StringFormat.GenericDefault)
      picture.Make [ {  Settings = PFont f;
                        Contents = path;
                        Color = color } ]

   let private makeBlankPic = 
      makeDrawPic { Width=0.;  Cap=Buttcap;  Join=Beveljoin;  Dashpat=[] } 
                  Color.Black 
                  (sketch.Make [ Seg [ point.Origin; point.Origin ] ])

   let transformPic (trans : transformation) (pic : picture) = 
      pic.Items
      |> List.map (fun e ->   
            let c = e.Contents.Clone() :?> GraphicsPath
            c.Transform(trans.ToMatrix)
            { Settings=e.Settings; Contents=c; Color=e.Color }) 
      |> picture.Make

   let centerPic center (pic : picture) =
      let pt = toPicCoord center   
      let bounds = pic.Bounds
      let centerTrans = Trans.translation ((float bounds.Width) / 2., 
                                             (float bounds.Height) / 2.)
      let top = (point.Make (float bounds.X) (float bounds.Y))
      let center = Trans.apply centerTrans top
      let trans = Trans.translation ((pt.xc - center.xc), (pt.yc - center.yc))
      let matrix = trans.ToMatrix
      pic.Items
      |> List.map (fun e -> 
            let centered = e.Contents.Clone() :?> GraphicsPath
            centered.Transform(matrix)
            { Settings=e.Settings; Contents=centered; Color=e.Color }) 
      |> picture.Make
   
   type picture with
      /// Creates a draw picture.
      static member Draw = makeDrawPic
      /// Creates a fill picture.
      static member Fill = makeFillPic
      /// Creates a text picture.
      static member Text = makeTextPic
      /// Creates a blank picture.
      static member Blank = makeBlankPic
      /// Groups a list of pictures into one.
      static member Group pictures =         
         pictures
         |> List.map (fun p -> p.Items)
         |> List.concat
         |> List.map (fun e -> { Settings=e.Settings; 
                                 Contents=e.Contents.Clone() :?> GraphicsPath;
                                 Color=e.Color })
         |> picture.Make
      /// Center this picture around the given center point.
      member x.CenterAround center = centerPic center x
      /// Applies the given transformation to this picture.
      member x.Transform (trans : transformation) = transformPic trans x
      
   let paint backgroundColor (pic : picture) (g : Graphics) centerToCanvas scaleUp width height =
      g.Clear(backgroundColor)
      let cap = function
         | Buttcap   -> LineCap.Flat
         | Squarecap -> LineCap.Square
         | Roundcap  -> LineCap.Round
      let join = function
         | Beveljoin -> LineJoin.Bevel
         | Roundjoin -> LineJoin.Round
         | Miterjoin -> LineJoin.Miter  
      let fill = function
         | Alternate -> FillMode.Alternate
         | Winding   -> FillMode.Winding       

      let paintItem (g:Graphics) e = 
         use pen = new Pen(e.Color)
         let path = e.Contents
         match e.Settings with
         | PLine style -> 
            pen.Width <- float32 style.Width
            pen.EndCap <- cap style.Cap
            pen.LineJoin <- join style.Join
            if List.length style.Dashpat > 0 
            then pen.DashPattern <- style.Dashpat |> List.map float32 |> Array.ofList
            g.DrawPath(pen, path)
         | PFill style -> 
            path.FillMode <- fill style
            use brush = new SolidBrush(e.Color)
            g.FillPath(brush, path)
            g.DrawPath(pen, path)
         | PFont _ -> g.DrawPath(pen, path)

      let border = 10.0
      let center = fromPicCoord (point.Make (float width / 2.) (float height / 2.))
      let centerTrans = Trans.translation (0., (float height))
      let scaleValue = min ((float width - border) / pic.Width) ((float height - border) / pic.Height)
      let scaleToFit = scaleUp || (float width < pic.Width || float height < pic.Height)
      let scaleTrans = picScaling (scaleValue, scaleValue)
      let centerPic (p : picture) = if centerToCanvas then p.CenterAround center else p
      let scalePic (p : picture) = if scaleToFit then p.Transform scaleTrans else p
      let pic = pic |> scalePic |> transformPic centerTrans |> centerPic
      g.SmoothingMode <- SmoothingMode.AntiAlias
      List.iter (fun e -> paintItem g e) pic.Items
      
   open System.Windows.Forms
         
   let showPictureEx backgroundColor centerToCanvas scaleUp (pic : picture) = 
      let panel = new Panel(BackColor=SystemColors.ControlLight, Dock=DockStyle.Fill)
      panel.Paint.Add(fun e -> paint backgroundColor pic e.Graphics centerToCanvas scaleUp panel.Width panel.Height)
      panel.Resize.Add(fun _ -> panel.Invalidate())

      let add_exit (form:Form) =
         let exit = new Button(Visible=true, Enabled=true)
         exit.Location <- new Point(-100, -100)
         exit.Click.Add(fun _ -> form.Close(); form.Dispose() |> ignore)
         form.Controls.Add(exit)
         form.CancelButton <- exit

      let form = new Form(Text="Canvas", 
                        TopMost=true,
                        Padding=new Padding(20))   
      add_exit form
      form.Controls.Add(panel)   
      form.Size <- new Size (1200, 800)
      form.Show()

   open System.IO
   
   let savePictureEx backgroundColor (path : string) centerToCanvas scaleUp (pic : picture) =
      use bmp = new Bitmap(2560, 1600)
      use g = Graphics.FromImage(bmp)
      paint backgroundColor pic g centerToCanvas scaleUp bmp.Width bmp.Height
      
      let dir = Path.GetDirectoryName(path)
      Directory.CreateDirectory(dir) |> ignore
      bmp.Save(path, Imaging.ImageFormat.Jpeg)

   let showPicture centerToCanvas pic = showPictureEx Color.FloralWhite centerToCanvas false pic
   let savePicture path centerToCanvas pic = savePictureEx Color.FloralWhite path centerToCanvas true pic