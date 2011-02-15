
#r "bin\Debug\GabeSoft.Common.dll"
open GabeSoft.Common
#r "bin\Debug\GabeSoft.Drawing.dll"
open GabeSoft.Drawing
#r "bin\Debug\Gma.UserActivityMonitor.dll"
open Gma.UserActivityMonitor
#load "ActionForm.fs"
open GabeSoft.Clickless

open System
open System.Drawing
open System.Reflection
open System.Resources
open System.Threading
open System.Windows.Forms

let getAction () = 
   let action = new ActionForm ("T", fun () -> ())
   action.Location <- new Point(1300, 300)
   action.Size <- new Size(10, 30)
   action
   
(*
let a = getAction()
a.Show()
a.Close()
*)