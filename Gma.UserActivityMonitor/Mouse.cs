using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Gma.UserActivityMonitor
{
   public static class Mouse
   {

      [DllImport("user32.dll")]
      private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);

      private const int WM_LBUTTONDOWN = 0x0201;
      private const int WM_LBUTTONUP = 0x0202;
      private const int WM_LBUTTONDBLCLK = 0x0203;
      private const int WM_RBUTTONDOWN = 0x0204;
      private const int WM_RBUTTONUP = 0x0205;
      private const int WM_RBUTTONDBLCLK = 0x0206;
      private const int WM_MOUSEMOVE = 0x0200;
      private const int WM_KEYDOWN = 0x100;
      private const int WM_KEYUP = 0x0101;
      private const int WM_CHAR = 0x0102;

      private const UInt32 MouseEventLeftDown = 0x0002;
      private const UInt32 MouseEventLeftUp = 0x0004;

      [DllImport("user32.dll")]
      private static extern bool SetCursorPos(int X, int Y);

      [DllImport("user32.dll")]
      private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

      [Flags]
      private enum MouseEventFlags
      {
         LEFTDOWN = 0x00000002,
         LEFTUP = 0x00000004,
         MIDDLEDOWN = 0x00000020,
         MIDDLEUP = 0x00000040,
         MOVE = 0x00000001,
         ABSOLUTE = 0x00008000,
         RIGHTDOWN = 0x00000008,
         RIGHTUP = 0x00000010
      }

      public static void Up(MouseButtons button) {
         switch (button) {
            case MouseButtons.Left:
               mouse_event((uint) MouseEventFlags.LEFTUP, 0, 0, 0, 0);
               break;
            case MouseButtons.Right:
               mouse_event((uint) MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
               break;
         }
      }

      public static void Press(MouseButtons button) {
         switch (button) {
            case MouseButtons.Left:
               mouse_event((uint) MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
               break;
            case MouseButtons.Right:
               mouse_event((uint) MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
               break;
         }
      }

      public static void Click(MouseButtons button) {
         switch (button) {
            case MouseButtons.Left:
               mouse_event((uint) MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
               mouse_event((uint) MouseEventFlags.LEFTUP, 0, 0, 0, 0);
               break;
            case MouseButtons.Right:
               mouse_event((uint) MouseEventFlags.RIGHTDOWN, 0, 0, 0, 0);
               mouse_event((uint) MouseEventFlags.RIGHTUP, 0, 0, 0, 0);
               break;
         }
      }

      public static void DoubleClick() {
         Click(MouseButtons.Left);
         Click(MouseButtons.Left);
      }

      public static void Move(int x, int y) {
         Point p = Cursor.Position;
         p.X += x;
         p.Y += y;
         Cursor.Position = p;
      }

      public static void MoveTo(int x, int y) {
         Cursor.Position = new Point(x, y);
      }
   }
}
