namespace GabeSoft.Drawing

open System
open System.Drawing
open System.Drawing.Drawing2D

type point = { xc: float; yc: float }

type geometricElement = 
   /// Open polygon formed from a sequence of
   /// straight line seqments.
   | Seg of point list
   /// Arc of a circle defined by its center, 
   /// radius, and two angles.
   | Arc of point * float * float * float
   /// Beziers curve defined by its origin,
   /// two control points, and its endpoint.
   | Bez of point * point * point * point
   /// A sequence of beziers curves defined by a list of points.
   /// The list should be specified as the following example illustrates:
   /// [ ptA; ctrl1A; ctrl2A; ptB; ctrl1B; ctrl2B; ptC; ctrl1C; ctrl2C; ptD ]
   | Bezs of point list

type sketch = { Elements : geometricElement list }

type sketch with 
   static member Make elements = { Elements = elements }
   static member Group s1 s2 = { Elements = s1.Elements @ s2.Elements }

type transformation = {
   m11: float; m12: float; m13: float;
   m21: float; m22: float; m23: float }

type transformation with
   member x.ToMatrix =
      new Matrix( float32 x.m11, 
                  float32 x.m21, 
                  float32 x.m12, 
                  float32 x.m22, 
                  float32 x.m13, 
                  float32 x.m23)


type linecap = Buttcap | Squarecap | Roundcap
type linejoin = Beveljoin | Roundjoin | Miterjoin
type linestyle = {
   Width: float;
   Cap: linecap;
   Join: linejoin;
   Dashpat: int list }
type fillstyle = Alternate | Winding
type fontStyle = Helvetica | Symbol | Arial
type font = {  Style: fontStyle; Size: float }

type point with
   static member Make x y = { xc = x; yc = y }
   static member Origin = { xc = 0.; yc = 0. }

type font with
   static member Make style size = { Style = style; Size = size }

type linestyle with
   static member Default = { Width = 1.; Cap = Buttcap; Join = Beveljoin; Dashpat = [] }

type pictureSettings = PLine of linestyle | PFill of fillstyle | PFont of font 

type pictureItem = {
   Settings : pictureSettings;
   Contents : GraphicsPath;
   Color : Color }

type picture = { Items : pictureItem list }
  
type picture with
   static member Make items = { Items = items }
   member x.Bounds =        
      use path = new GraphicsPath()
      List.iter (fun e -> path.AddPath(e.Contents, false)) x.Items
      path.GetBounds()
   member x.Height = float x.Bounds.Height
   member x.Width = float x.Bounds.Width
   member x.Center = 
      use path = new GraphicsPath()
      List.iter (fun e -> path.AddPath(e.Contents, false)) x.Items
      let bounds = path.GetBounds()
      let x = bounds.X + bounds.Width / float32 2.
      let y = bounds.Y + bounds.Height / float32 2.
      point.Make (float x) (float -y)

type btreestyle = {
   /// Vertical distance between a node and its children
   Vdist: float;
   /// The spread between the two children of the root
   Hdist: float;
   /// Coefficients that indicate for each level n the 
   /// ratio between the spread at level n + 1 and n   
   Coefs: float list;
   /// Line style for the branches of the tree
   Tlstyl: linestyle;
   /// The color in which the branches of the 
   /// tree will be drawn
   Tcolor: Color }

type gtreestyle = {
   /// Vertical distance between a node and its children
   Vdist: float;
   /// The spread between the two children of the root
   Hdist: float;
   /// Coefficients that indicate for each level n the 
   /// ratio between the spread at level n + 1 and n   
   Coefs: float list;
   /// Line style for the branches of the tree
   Tlstyl: linestyle;
   /// The color in which the branches of the 
   /// tree will be drawn
   Tcolor: Color }
