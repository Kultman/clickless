namespace GabeSoft.Clickless

open System
open System.Drawing
open System.Diagnostics
open System.Reflection
open System.Resources
open System.Threading
open System.Windows.Forms

open Microsoft.Win32

open Gma.UserActivityMonitor

type State () =
   let mutable enabled = true
   member u.Enabled 
      with  get () = enabled
      and   set v  = enabled <- v

module Program = 
   let initMenu (state : State) = 
      let menu = new ContextMenu()
      let enable = new MenuItem("Enable")
      let disable = new MenuItem("Disable")
      let exit = new MenuItem("Exit")
      menu.MenuItems.Add(enable) |> ignore
      menu.MenuItems.Add(disable) |> ignore
      menu.MenuItems.Add(exit) |> ignore

      enable.Click.Add(fun _ -> state.Enabled <- true)
      disable.Click.Add(fun _ -> state.Enabled <- false)

      let resources = new ResourceManager("Resources", Assembly.GetExecutingAssembly())
      let icon = new NotifyIcon()
      icon.Icon <- resources.GetObject("Mouse") :?> Icon
      icon.Visible <- true   
      icon.ContextMenu <- menu

      exit.Click.Add(fun _ -> 
         icon.Visible <- false   
         Environment.Exit(0))

      icon.Click.Add(fun _ -> state.Enabled <- not state.Enabled)

   let showCursor (form:CursorForm) x y fx fy =
      form.Location <- new Point(fx, fy)
      form.Show(x, y, fx, fy)

   let showAction (form:ActionForm) x y fx fy =
      form.Location <- new Point(fx, fy)
      form.Show(x, y, fx, fy)

   let allOnScreen (forms:ActionForm seq) =
      forms |> Seq.fold(fun acc f -> acc && f.OnScreen) true

   let showForms (x, y) (x1, x2, y1, y2)
      (cursor:CursorForm, lclick:ActionForm, rclick:ActionForm, dclick:ActionForm, lpress:ActionForm) =
      
      showCursor cursor x y (x - cursor.Width / 2) (y - cursor.Height / 2)
      showAction lclick x y x1 y1
      showAction rclick x y x1 y2
      showAction dclick x y x2 y1
      showAction lpress x y x2 y2
      allOnScreen [ lclick; rclick; dclick; lpress ]

   let showBottomRight x y offset (cursor:CursorForm)  
                           (lclick:ActionForm) (rclick:ActionForm) 
                           (dclick:ActionForm) (lpress:ActionForm) =
      let w = lclick.Width
      let h = lclick.Height
      let o = offset
      showForms   (x, y) (x + o, x + o + w, y + o, y + o + h) 
                  (cursor, lclick, rclick, dclick, lpress)
   
   let showTopRight x y offset (cursor:CursorForm)  
                        (lclick:ActionForm) (rclick:ActionForm) 
                        (dclick:ActionForm) (lpress:ActionForm) =
      let w = lclick.Width
      let h = lclick.Height
      let o = offset
      showForms   (x, y) (x + o, x + o + w, y - o - h, y - o - h * 2)
                  (cursor, lclick, rclick, dclick, lpress)               

   let showTopLeft x y offset (cursor:CursorForm)  
                        (lclick:ActionForm) (rclick:ActionForm) 
                        (dclick:ActionForm) (lpress:ActionForm) =
      let w = lclick.Width
      let h = lclick.Height
      let o = offset
      showForms   (x, y) (x - o - w, x - o - w * 2, y - o - h, y - o - h * 2)
                  (cursor, lclick, rclick, dclick, lpress)               

   let showBottomLeft x y offset (cursor:CursorForm)  
                        (lclick:ActionForm) (rclick:ActionForm) 
                        (dclick:ActionForm) (lpress:ActionForm) =
      let w = lclick.Width
      let h = lclick.Height
      let o = offset
      showForms   (x, y) (x - o - w, x - o - w * 2, y + o, y + o + h)
                  (cursor, lclick, rclick, dclick, lpress)               

   let initEvents (state:State) (cursor:CursorForm) 
                  (lclick:ActionForm) (rclick:ActionForm) (dclick:ActionForm) (lpress:ActionForm)
                  (timer:Timer) =
      let x = ref 0
      let y = ref 0
      let offset = 15
      let activity = ref false
      let pressed = ref false

      let rec show onScreen (fs:list<_>) = 
         if not onScreen && List.length fs > 0 then
            let f = List.head fs
            let ok = f !x !y offset cursor lclick rclick dclick lpress
            show ok (List.tail fs)

      let hideAll ()  =
         cursor.Hide()
         lclick.Hide()
         rclick.Hide()
         dclick.Hide()
         lpress.Hide()               

      timer.Tick.Add(fun _ -> 
         if not state.Enabled || !pressed
         then hideAll ()
         else show false [ showBottomRight; showTopRight; showTopLeft; showBottomLeft ]
         timer.Stop())

      lclick.MouseEnter.Add(fun _ -> 
         activity := true
         timer.Stop())
      lclick.MouseLeave.Add(fun _ -> activity := false)

      HookManager.MouseDown.Add(fun e -> pressed := true)
      HookManager.MouseUp.Add(fun e -> pressed := false)
      HookManager.MouseMove.Add(fun e ->
         timer.Stop()
         x := e.X
         y := e.Y
         timer.Start())
      HookManager.KeyDown.Add(fun e -> 
         if !pressed then Mouse.Up(MouseButtons.Left)
         if e.KeyCode = Keys.Escape || 
            e.KeyCode = Keys.LControlKey then hideAll());

   [<STAThread>]
   [<EntryPoint>]
   let main(args:string[]) =
      let guid = "B3D80749-6400-4844-B1D0-47E0930344FF"
      use sync = new Mutex(false, guid)
      let state = new State()
      use cursor = new CursorForm ()
      use lclick = new ActionForm ("L", fun () -> Mouse.Click(MouseButtons.Left))
      use rclick = new ActionForm ("R", fun () -> Mouse.Click(MouseButtons.Right))
      use dclick = new ActionForm ("D", fun () -> Mouse.DoubleClick())
      use lpress = new ActionForm ("P", fun () -> Mouse.Press(MouseButtons.Left))
      use timer = new Timer()
      timer.Interval <- 200
      state.Enabled <- true

      try 
         if sync.WaitOne(0, false) then
            initMenu state
            initEvents state cursor lclick rclick dclick lpress timer
            SystemEvents.SessionEnded.Add(fun _ -> Application.Exit())
            Application.EnableVisualStyles()         
            Application.Run()
         0
      finally 
         sync.Close()
