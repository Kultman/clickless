namespace GabeSoft.Drawing

open System
open System.Drawing

open GabeSoft.Common
open GabeSoft.Drawing.Draw

type plotFun = { color: Color; lineWidth: float ; step: float; f: float -> float }
type graphData = {   minX: float; maxX: float 
                     minY: float; maxY: float
                     stepX: float; stepY: float
                     funs: list<plotFun> }
type graphData with
   member d.minPt = point.Make d.minX d.minY
   member d.maxPt = point.Make d.maxX d.maxY

module Graph = 
   let canvasMinX = 0.0
   let canvasMaxX = 1100.0
   let canvasMinY = 0.0
   let canvasMaxY = 650.0

   let canvasTrans (data : graphData) = 
      let translfw = Trans.translation (-data.minPt.xc, -data.minPt.yc)
      let maxpt = Trans.apply translfw data.maxPt
      let scale = Trans.scaling(canvasMaxX / maxpt.xc, canvasMaxY / maxpt.yc)
      let minpt = Trans.apply (Trans.compose [ scale; translfw ]) data.minPt
      let translbk = Trans.translation (minpt.xc, minpt.yc)
      Trans.compose [ translbk; scale; translfw ]

   type graphData with
      member d.XAxisVisible = d.minX <= 0.0
      member d.YAxisVisible = d.minY <= 0.0
      member d.CanvasTrans = canvasTrans d
      member d.CanvasMinPt = Trans.apply d.CanvasTrans d.minPt
      member d.CanvasMaxPt = Trans.apply d.CanvasTrans d.maxPt

   let drawPic color elem = picture.Draw linestyle.Default color (sketch.Make [ elem ])
   let fillPic color elem = picture.Fill Alternate color (sketch.Make [ elem ])
   let drawText value = picture.Text { Style=Arial; Size=10. } Color.Black (string value)

   let graphBackground (data : graphData) =
      let marksColor = Color.Red
      let axesColor = Color.SkyBlue
      let outlineColor = Color.DarkCyan
      let minpt = data.CanvasMinPt
      let maxpt = data.CanvasMaxPt
      let origin = Trans.apply data.CanvasTrans point.Origin
      let arrowOffset1 = 8.0
      let arrowOffset2 = 2.5

      let drawCoord dash textCenter value = 
         let mark = drawPic marksColor dash 
         let text = drawText value
         let center = textCenter text
         picture.Group [ mark; text.CenterAround center ]

      let drawXCoord x y (value : float) = 
         let dash = Seg [ point.Make x (y - 5.); point.Make x (y + 5.) ]
         let center (text : picture) = point.Make x (y - 5. - text.Height / 2. - 2.)
         drawCoord dash center value

      let drawYCoord x y (value : float) =
         let dash = Seg [ point.Make (x - 5.) y; point.Make (x + 5.) y ]
         let center (text : picture) = point.Make (x - 5. - text.Width / 2. - 2.) y
         drawCoord dash center value

      let horiz y = Seg [ point.Make minpt.xc y; point.Make maxpt.xc y ]
      let vert x = Seg [ point.Make x minpt.yc; point.Make x maxpt.yc ]

      let xaxis = 
         if not data.XAxisVisible then picture.Blank else
         let axis = horiz origin.yc
         let arrow = Seg [ point.Make maxpt.xc origin.yc
                           point.Make (maxpt.xc - arrowOffset1) (origin.yc - arrowOffset2)
                           point.Make (maxpt.xc - arrowOffset1) (origin.yc + arrowOffset2)
                           point.Make maxpt.xc origin.yc ]
         let axisPic = drawPic axesColor axis 
         let arrowPic = fillPic marksColor arrow 
         let arrowCenter = point.Make (maxpt.xc + arrowPic.Width / 2.) origin.yc
         picture.Group [ axisPic; arrowPic.CenterAround arrowCenter ]

      let yaxis = 
         if not data.YAxisVisible then picture.Blank else
         let axis = vert origin.xc
         let arrow = Seg [ point.Make origin.xc maxpt.yc;
                           point.Make (origin.xc + arrowOffset2) (maxpt.yc - arrowOffset1)
                           point.Make (origin.xc - arrowOffset2) (maxpt.yc - arrowOffset1)
                           point.Make origin.xc maxpt.yc ]
         let axisPic = drawPic axesColor axis 
         let arrowPic = fillPic marksColor arrow 
         let arrowCenter = point.Make origin.xc (maxpt.yc + arrowPic.Height / 2.)
         picture.Group [ axisPic; arrowPic.CenterAround arrowCenter ]
      
      let bottomOutl = drawPic outlineColor (horiz minpt.yc)
      let leftOutl = drawPic outlineColor (vert minpt.xc)

      let xmarks = 
         [ for i in data.minX .. data.stepX .. data.maxX -> i ]
         |> List.map (fun x -> x, point.Make x data.minY |> Trans.apply data.CanvasTrans)
         |> List.map (fun (v, pt) -> drawXCoord pt.xc pt.yc v)
         |> picture.Group

      let ymarks = 
         [ for i in data.minY .. data.stepY .. data.maxY -> i ]
         |> List.map (fun y -> y, point.Make data.minX y |> Trans.apply data.CanvasTrans)
         |> List.map (fun (v, pt) -> drawYCoord pt.xc pt.yc v)
         |> picture.Group         

      picture.Group [ bottomOutl; leftOutl; xaxis; yaxis; xmarks; ymarks ]

   let drawDot color width pt = 
      let dot = Seg [   point.Make 0.0 0.0; point.Make 0.0 width; 
                        point.Make width width; point.Make width 0.0; point.Make 0.0 0.0 ]
      let dotpic = fillPic color dot 
      dotpic.CenterAround pt

   let plotFun data (pf : plotFun) =
      [ for i in data.minX .. pf.step .. data.maxX -> i ]
      |> List.map (fun x -> pf.f x |> point.Make x)
      |> List.filter (fun pt -> pt.yc >= data.minY && pt.yc <= data.maxY)
      |> List.map (fun pt -> pt |> Trans.apply data.CanvasTrans |> drawDot pf.color pf.lineWidth)
      |> picture.Group

   /// Plots a list of functions as specified by the graph data.
   let plot data = 
      let back = graphBackground data
      let n = data.funs |> List.length    
      let funs = data.funs |> List.map (plotFun data) |> picture.Group
      picture.Group [ back; funs ]