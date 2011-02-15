using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Gma.UserActivityMonitor
{
   [StructLayout(LayoutKind.Sequential)]
   public struct TRACKMOUSEEVENT
   {
      public UInt32 cbSize;
      public UInt32 dwFlags;
      public IntPtr hWnd;
      public UInt32 dwHoverTime;

      public TRACKMOUSEEVENT(UInt32 dwFlags, IntPtr hWnd, UInt32 dwHoverTime) {
         this.cbSize = 16;
         this.dwFlags = dwFlags;
         this.hWnd = hWnd;
         this.dwHoverTime = dwHoverTime;
      }
   }
}
