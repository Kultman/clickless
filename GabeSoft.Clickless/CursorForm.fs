namespace GabeSoft.Clickless

open System
open System.Diagnostics
open System.Drawing
open System.Windows.Forms

open GabeSoft.Common
open GabeSoft.Drawing
open GabeSoft.Drawing.Draw

open Gma.UserActivityMonitor

type CursorForm () as this =
   inherit Form ()
   let transparent = Color.FromArgb(192, 192, 255)
   let mutable x = 0
   let mutable y = 0
   let width = 20
   let height = 20

   let MakeTransparent (form : Form) = 
      let style = User32.GetWindowLong(form.Handle, User32.GWL.EXSTYLE)
      User32.SetWindowLong(form.Handle, User32.GWL.EXSTYLE, 
         style ||| 
         uint32 User32.WS_EX.LAYERED ||| 
         uint32 User32.WS_EX.TRANSPARENT |||
         uint32 User32.WS_EX.TOOLWINDOW) |> ignore

   do
      this.FormBorderStyle <- FormBorderStyle.None
      this.ShowIcon <- false
      this.ShowInTaskbar <- false
      this.StartPosition <- FormStartPosition.Manual
      this.TopMost <- false
      this.ClientSize <- new Size(width, height)
      this.TransparencyKey <- transparent
      this.RightToLeftLayout <- false
      this.AllowTransparency <- true
      this.BackColor <- transparent

   override u.OnPaint (e) =
      let pt = point.Make
      let style = linestyle.Default
      let outer = Color.Red
      let inner = Color.DimGray
      let vh = 6.0
      let vl = 3.0

      let cross = [  picture.Draw style inner (sketch.Make [ Seg [ pt -vl 0.0; pt vl 0.0 ] ])
                     picture.Draw style inner (sketch.Make [ Seg [ pt 0.0 -vl; pt 0.0 vl ] ]) 
                     picture.Draw style outer (sketch.Make [ Seg [ pt -vh 0.0; pt -vl 0.0 ] ])
                     picture.Draw style outer (sketch.Make [ Seg [ pt vl 0.0; pt vh 0.0 ] ])
                     picture.Draw style outer (sketch.Make [ Seg [ pt 0.0 -vh; pt 0.0 -vl ] ]) 
                     picture.Draw style outer (sketch.Make [ Seg [ pt 0.0 vl; pt 0.0 vh ] ]) 
                     ] |> picture.Group

      paint transparent cross e.Graphics true false this.Width this.Height             
         
   member u.Show (xm:int, ym:int, xw:int, yw:int) =
      x <- xm
      y <- ym
      User32.SetWindowPlacement(u, User32.SW_SHOWNOACTIVATE)
      User32.SetWindowPos(u.Handle, 
         nativeint User32.HWND_TOPMOST, xw, yw, width, height, 
         uint32 User32.SWP_NOACTIVATE) |> ignore      
      MakeTransparent u
      
      