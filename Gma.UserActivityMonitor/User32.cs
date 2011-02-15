using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gma.UserActivityMonitor
{
   public static class User32
   {
      #region Structures
      [StructLayout(LayoutKind.Sequential)]
      public struct COLORREF
      {
         public byte R;
         public byte G;
         public byte B;

         public static COLORREF Create(int r, int g, int b) {
            return new COLORREF { R = (byte)r, G = (byte)g, B = (byte)b };
         }
      }

      /// <summary>
      /// Contains information about the placement of a window on the screen.
      /// </summary>
      [Serializable]
      [StructLayout(LayoutKind.Sequential)]
      public struct WINDOWPLACEMENT
      {
         /// <summary>
         /// The length of the structure, in bytes. Before calling the GetWindowPlacement or SetWindowPlacement functions, set this member to sizeof(WINDOWPLACEMENT).
         /// <para>
         /// GetWindowPlacement and SetWindowPlacement fail if this member is not set correctly.
         /// </para>
         /// </summary>
         public int Length;

         /// <summary>
         /// Specifies flags that control the position of the minimized window and the method by which the window is restored.
         /// </summary>
         public int Flags;

         /// <summary>
         /// The current show state of the window.
         /// </summary>
         public int ShowCmd;

         /// <summary>
         /// The coordinates of the window's upper-left corner when the window is minimized.
         /// </summary>
         public System.Drawing.Point MinPosition;

         /// <summary>
         /// The coordinates of the window's upper-left corner when the window is maximized.
         /// </summary>
         public System.Drawing.Point MaxPosition;

         /// <summary>
         /// The window's coordinates when the window is in the restored position.
         /// </summary>
         public System.Drawing.Rectangle NormalPosition;

         /// <summary>
         /// Gets the default (empty) value.
         /// </summary>
         public static WINDOWPLACEMENT Default {
            get {
               WINDOWPLACEMENT result = new WINDOWPLACEMENT();
               result.Length = Marshal.SizeOf(result);
               return result;
            }
         }
      }
      #endregion

      #region Enums
      public enum GWL
      {
         WNDPROC = (-4),
         HINSTANCE = (-6),
         HWNDPARENT = (-8),
         STYLE = (-16),
         EXSTYLE = (-20),
         USERDATA = (-21),
         ID = (-12)
      }

      [Flags]
      public enum WS : uint
      {
         OVERLAPPED = 0x00000000,
         POPUP = 0x80000000,
         CHILD = 0x40000000,
         MINIMIZE = 0x20000000,
         VISIBLE = 0x10000000,
         DISABLED = 0x08000000,
         CLIPSIBLINGS = 0x04000000,
         CLIPCHILDREN = 0x02000000,
         MAXIMIZE = 0x01000000,
         BORDER = 0x00800000,
         DLGFRAME = 0x00400000,
         VSCROLL = 0x00200000,
         HSCROLL = 0x00100000,
         SYSMENU = 0x00080000,
         THICKFRAME = 0x00040000,
         GROUP = 0x00020000,
         TABSTOP = 0x00010000,

         MINIMIZEBOX = 0x00020000,
         MAXIMIZEBOX = 0x00010000,

         CAPTION = BORDER | DLGFRAME,
         TILED = OVERLAPPED,
         ICONIC = MINIMIZE,
         SIZEBOX = THICKFRAME,
         TILEDWINDOW = OVERLAPPEDWINDOW,

         OVERLAPPEDWINDOW = OVERLAPPED | CAPTION | SYSMENU | THICKFRAME | MINIMIZEBOX | MAXIMIZEBOX,
         POPUPWINDOW = POPUP | BORDER | SYSMENU,
         CHILDWINDOW = CHILD,
      }

      [Flags]
      public enum WS_EX : uint
      {
         None = 0,
         DLGMODALFRAME = 0x00000001,
         NOPARENTNOTIFY = 0x00000004,
         TOPMOST = 0x00000008,
         ACCEPTFILES = 0x00000010,
         TRANSPARENT = 0x00000020,
         MDICHILD = 0x00000040,
         TOOLWINDOW = 0x00000080,
         WINDOWEDGE = 0x00000100,
         CLIENTEDGE = 0x00000200,
         CONTEXTHELP = 0x00000400,
         RIGHT = 0x00001000,
         LEFT = 0x00000000,
         RTLREADING = 0x00002000,
         LTRREADING = 0x00000000,
         LEFTSCROLLBAR = 0x00004000,
         RIGHTSCROLLBAR = 0x00000000,
         CONTROLPARENT = 0x00010000,
         STATICEDGE = 0x00020000,
         APPWINDOW = 0x00040000,
         LAYERED = 0x00080000,
         NOINHERITLAYOUT = 0x00100000, // Disable inheritence of mirroring by children
         LAYOUTRTL = 0x00400000, // Right to left mirroring
         COMPOSITED = 0x02000000,
         NOACTIVATE = 0x08000000,
         OVERLAPPEDWINDOW = (WINDOWEDGE | CLIENTEDGE),
         PALETTEWINDOW = (WINDOWEDGE | TOOLWINDOW | TOPMOST),
      }

      [Flags]
      public enum LWA : uint
      {
         ColorKey = 0x00000001,
         Alpha = 0x00000002
      }
      #endregion

      #region Constants
      /// <summary>
      /// Places the window at the bottom of the Z order. 
      /// If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at 
      /// the bottom of all other windows.
      /// </summary>
      public const int HWND_BOTTOM = 1;
      /// <summary>
      /// Places the window at the top of the Z order.
      /// </summary>
      public const int HWND_TOP = 0;
      /// <summary>
      /// Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
      /// </summary>
      public const int HWND_TOPMOST = -1;
      /// <summary>
      /// Places the window above all non-topmost windows (that is, behind all topmost windows). 
      /// This flag has no effect if the window is already a non-topmost window.
      /// </summary>
      public const int HWND_NOTOPMOST = -2;
      /// <summary>
      /// If the calling thread and the thread that owns the window are attached to different input queues, 
      /// the system posts the request to the thread that owns the window. 
      /// This prevents the calling thread from blocking its execution while other threads process the request. 
      /// </summary>
      public const int SWP_ASYNCWINDOWPOS = 0x4000;
      /// <summary>
      /// Prevents generation of the WM_SYNCPAINT message. 
      /// </summary>
      public const int SWP_DEFERERASE = 0x2000;
      /// <summary>
      /// Draws a frame (defined in the window's class description) around the window.
      /// </summary>
      public const int SWP_DRAWFRAME = 0x0020;
      /// <summary>
      /// Applies new frame styles set using the SetWindowLong function. 
      /// Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. 
      /// If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
      /// </summary>
      public const int SWP_FRAMECHANGED = 0x0020;
      /// <summary>
      /// Hides the window.
      /// </summary>
      public const int SWP_HIDEWINDOW = 0x0080;
      /// <summary>
      /// Does not activate the window. If this flag is not set, the window is activated 
      /// and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
      /// </summary>
      public const int SWP_NOACTIVATE = 0x0010;
      /// <summary>
      /// Discards the entire contents of the client area. 
      /// If this flag is not specified, the valid contents of the client area are saved 
      /// and copied back into the client area after the window is sized or repositioned.
      /// </summary>
      public const int SWP_NOCOPYBITS = 0x0100;
      /// <summary>
      /// Retains the current position (ignores X and Y parameters).
      /// </summary>
      public const int SWP_NOMOVE = 0x0002;
      /// <summary>
      /// Does not change the owner window's position in the Z order.
      /// </summary>
      public const int SWP_NOOWNERZORDER = 0x0200;
      /// <summary>
      /// Does not redraw changes. If this flag is set, no repainting of any kind occurs. 
      /// This applies to the client area, the nonclient area (including the title bar and scroll bars), 
      /// and any part of the parent window uncovered as a result of the window being moved. 
      /// When this flag is set, the application must explicitly invalidate or redraw any parts 
      /// of the window and parent window that need redrawing.
      /// </summary>
      public const int SWP_NOREDRAW = 0x0008;
      /// <summary>
      /// Same as the SWP_NOOWNERZORDER flag.
      /// </summary>
      public const int SWP_NOREPOSITION = 0x0200;
      /// <summary>
      /// Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
      /// </summary>
      public const int SWP_NOSENDCHANGING = 0x0400;
      /// <summary>
      /// Retains the current size (ignores the cx and cy parameters).
      /// </summary>
      public const int SWP_NOSIZE = 0x0001;
      /// <summary>
      /// Retains the current Z order (ignores the hWndInsertAfter parameter).
      /// </summary>
      public const int SWP_NOZORDER = 0x0004;
      /// <summary>
      /// Displays the window.
      /// </summary>
      public const int SWP_SHOWWINDOW = 0x0040;

      /// <summary>
      /// Activates and displays a window. If the window is minimized or maximized, 
      /// Windows restores it to its original size and position. An application should specify this 
      /// flag when displaying the window for the first time.
      /// </summary>
      public const int SW_SHOWNORMAL = 1;
      /// <summary>
      /// Activates the window and displays it as a minimized window.
      /// </summary>
      public const int SW_SHOWMINIMIZED = 2;
      /// <summary>
      /// Activates the window and displays it as a maximized window.
      /// </summary>
      public const int SW_SHOWMAXIMIZED = 3;
      /// <summary>
      /// Displays a window in its most recent size and position. The active window remains active.
      /// </summary>
      public const int SW_SHOWNOACTIVATE = 4;
      /// <summary>
      /// Activates the window and displays it in its current size and position.
      /// </summary>
      public const int SW_SHOW = 5;
      #endregion

      #region Public Methods
      /// <summary>
      /// Changes the size, position, and Z order of a child, pop-up, or top-level window. 
      /// These windows are ordered according to their appearance on the screen. 
      /// The topmost window receives the highest rank and is the first window in the Z order.
      /// </summary>
      /// <param name="hWnd">A handle to the window.</param>
      /// <param name="hWndInsertAfter">
      /// A handle to the window to precede the positioned window in the Z order. 
      /// This parameter must be a window handle or one of the following values:
      /// <see cref="User32.HWND_BOTTOM"/>
      /// <see cref="User32.HWND_NOTOPMOST"/>
      /// <see cref="User32.HWND_TOP"/>
      /// <see cref="User32.HWND_TOPMOST"/>
      /// </param>
      /// <param name="X">The new position of the left side of the window, in client coordinates.</param>
      /// <param name="Y">The new position of the top of the window, in client coordinates. </param>
      /// <param name="cx">The new width of the window, in pixels.</param>
      /// <param name="cy">The new height of the window, in pixels.</param>
      /// <param name="uFlags">
      /// The window sizing and positioning flags. This parameter can be a combination of the following values:
      /// <see cref="User32.SWP_ASYNCWINDOWPOS"/>
      /// <see cref="SWP_DEFERERASE "/>
      /// <see cref="SWP_DRAWFRAME "/>
      /// <see cref="SWP_FRAMECHANGED "/>
      /// <see cref="SWP_HIDEWINDOW "/>
      /// <see cref="SWP_NOACTIVATE "/>
      /// <see cref="SWP_NOCOPYBITS "/>
      /// <see cref="SWP_NOMOVE "/>
      /// <see cref="SWP_NOOWNERZORDER "/>
      /// <see cref="SWP_NOREDRAW "/>
      /// <see cref="SWP_NOREPOSITION "/>
      /// <see cref="SWP_NOSENDCHANGING "/>
      /// <see cref="SWP_NOSIZE "/>
      /// <see cref="SWP_NOZORDER "/>
      /// <see cref="SWP_SHOWWINDOW "/>
      /// </param>
      /// <returns></returns>
      [DllImport("user32.dll")]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

      /// <summary>
      /// Retrieves the show state and the restored, minimized, and maximized positions of the specified window.
      /// </summary>
      /// <param name="hWnd">
      /// A handle to the window.
      /// </param>
      /// <param name="lpwndpl">
      /// A pointer to the WINDOWPLACEMENT structure that receives the show state and position information.
      /// <para>
      /// Before calling GetWindowPlacement, set the length member to sizeof(WINDOWPLACEMENT). GetWindowPlacement fails if lpwndpl-> length is not set correctly.
      /// </para>
      /// </param>
      /// <returns>
      /// If the function succeeds, the return value is nonzero.
      /// <para>
      /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
      /// </para>
      /// </returns>
      [DllImport("user32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

      /// <summary>
      /// Sets the show state and the restored, minimized, and maximized positions of the specified window.
      /// </summary>
      /// <param name="hWnd">
      /// A handle to the window.
      /// </param>
      /// <param name="lpwndpl">
      /// A pointer to a WINDOWPLACEMENT structure that specifies the new show state and window positions.
      /// <para>
      /// Before calling SetWindowPlacement, set the length member of the WINDOWPLACEMENT structure to sizeof(WINDOWPLACEMENT). SetWindowPlacement fails if the length member is not set correctly.
      /// </para>
      /// </param>
      /// <returns>
      /// If the function succeeds, the return value is nonzero.
      /// <para>
      /// If the function fails, the return value is zero. To get extended error information, call GetLastError.
      /// </para>
      /// </returns>
      [DllImport("user32.dll", SetLastError = true)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

      /// <summary>
      /// Sets the show state and the restored, minimized, and maximized positions of the specified window.
      /// </summary>
      /// <param name="form">The form.</param>
      /// <param name="flags">
      /// The flags one of the following: 
      /// <see cref="SW_SHOWNORMAL" />
      /// <see cref="SW_SHOWMINIMIZED" />
      /// <see cref="SW_SHOWMAXIMIZED" />
      /// <see cref="SW_SHOWNOACTIVATE" />
      /// <see cref="SW_SHOW" />
      /// </param>
      public static void SetWindowPlacement(Form form, int flags) {
         var placement = WINDOWPLACEMENT.Default;
         GetWindowPlacement(form.Handle, out placement);
         placement.Length = Marshal.SizeOf(placement);
         placement.Flags = 0;
         placement.ShowCmd = flags;
         SetWindowPlacement(form.Handle, ref placement);
      }

      [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
      public static extern int SetWindowLong(IntPtr hWnd, User32.GWL gwlIndex, uint dwNewLong);

      [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
      public static extern uint GetWindowLong(IntPtr hWnd, User32.GWL gwlIndex);

      [DllImport("user32.dll")]
      public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, COLORREF crKey, byte bAlpha, uint dwFlags);
      #endregion
   }
}
