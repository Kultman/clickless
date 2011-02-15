namespace GabeSoft.Clickless

open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

open GabeSoft.Common
open GabeSoft.Drawing
open GabeSoft.Drawing.Draw

open Gma.UserActivityMonitor

type ActionForm (caption:string, click:unit -> unit) as this =
   inherit Form ()
   let transparent = Color.FromArgb(192, 192, 255)
   let background = Color.DimGray
   let mutable x = 0
   let mutable y = 0
   let width = 40
   let height = 25
   let mutable mousein = false

   do
      this.FormBorderStyle <- FormBorderStyle.None
      this.ShowIcon <- false
      this.ShowInTaskbar <- false
      this.StartPosition <- FormStartPosition.Manual
      this.TopMost <- true 
      this.ClientSize <- new Size(width, height)
      this.Size <- this.ClientSize
      this.TransparencyKey <- transparent
      this.RightToLeftLayout <- false
      this.AllowTransparency <- true
      this.BackColor <- background

      this.MouseHover.Add(fun e -> 
         this.Invalidate()
         Mouse.MoveTo(x, y)
         click ()) 
      this.MouseEnter.Add(fun e -> 
         mousein <- true
         this.Invalidate())
      this.MouseLeave.Add(fun e -> 
         mousein <- false
         this.Invalidate())
         
   override u.ShowWithoutActivation 
      with get () = true

   override u.OnPaint (e) =
      let pt = point.Make
      let lstyle = { Width = 3.; Cap = Squarecap; Join = Beveljoin; Dashpat = [] }
      let fstyle = fillstyle.Alternate
      let font = font.Make Arial 12.0
      let bcol = Color.Red
      let tcol = if mousein then Color.Red else Color.DimGray
      let w = float (width - 3)
      let h = float (height - 3)
      let wi = w - 1.0
      let hi = h - 1.0
      let center (pic:picture) = pic.CenterAround point.Origin

      let border = [ picture.Draw lstyle bcol (sketch.Make [ Seg [ pt 0.0 0.0; pt w 0.0 ] ])
                     picture.Draw lstyle bcol (sketch.Make [ Seg [ pt w 0.0; pt w h ] ])
                     picture.Draw lstyle bcol (sketch.Make [ Seg [ pt w h; pt 0.0 h ] ]) 
                     picture.Draw lstyle bcol (sketch.Make [ Seg [ pt 0.0 h; pt 0.0 0.0 ] ]) 
                     ] |> picture.Group 
      let rect = Seg [  pt 1.0 1.0; 
                        pt wi 1.0; 
                        pt wi hi; 
                        pt 1.0 hi;
                        pt 1.0 1.0 ]

      let text = picture.Text font tcol caption
      let back = picture.Fill fstyle background (sketch.Make [ rect ])
      let pic =   if mousein 
                  then picture.Group [ center border; center back; center text ] 
                  else picture.Group [ center border; center text ]
      paint transparent pic e.Graphics true false this.Width this.Height        
      if mousein then u.Opacity <- 1.0 else u.Opacity <- 0.2

   override u.OnLoad (e) =
      base.OnLoad (e)
      u.Size <- new Size(width, height)
         
   member u.Show (xm:int, ym:int, xw:int, yw:int) =
      x <- xm
      y <- ym
      User32.SetWindowPlacement(u, User32.SW_SHOWNOACTIVATE)
      User32.SetWindowPos(u.Handle, 
         nativeint User32.HWND_TOPMOST, xw, yw, width, height, 
         uint32 User32.SWP_NOACTIVATE) |> ignore      
      let style = User32.GetWindowLong(u.Handle, User32.GWL.EXSTYLE)
      User32.SetWindowLong(u.Handle, User32.GWL.EXSTYLE, 
         style ||| uint32 User32.WS_EX.TOOLWINDOW) |> ignore

   member u.OnScreen 
      with get () =
         Screen.AllScreens
         |> Seq.fold (fun acc s ->
               let top = new Point(u.Left, u.Top)
               acc || s.WorkingArea.Contains(top)) false
   