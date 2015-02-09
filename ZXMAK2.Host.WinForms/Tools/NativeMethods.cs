using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Diagnostics;


namespace ZXMAK2.Host.WinForms.Tools
{
    internal sealed class NativeMethods
    {
        #region P/Invoke

        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern void CopyMemory(int* destination, int* source, int length);

        [DllImport("kernel32.dll", SetLastError = true)]
        public unsafe static extern void CopyMemory(uint* destination, uint* source, int length);


        private const uint TIMERR_NOERROR = 0;
        private const uint TIMERR_NOCANDO = 96 + 1;

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint uPeriod);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint uPeriod);


        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner


            public Point Location
            {
                get { return new Point(Left, Top); }
            }

            public Size Size
            {
                get { return new Size(Right - Left, Bottom - Top); }
            }
        }

        #endregion P/Invoke


        #region Wrappers

        public static uint TimeBeginPeriod(uint uPeriod)
        {
            try
            {
                var result = timeBeginPeriod(uPeriod);
                if (result != TIMERR_NOERROR)
                {
                    Logger.Debug("timeBeginPeriod({0}): error {1}", result);
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.Debug("{0}: {1}", ex.GetType().Name, ex.Message);
            }
            return 0xFFFFFFFF;
        }

        public static uint TimeEndPeriod(uint uPeriod)
        {
            try
            {
                var result = timeEndPeriod(uPeriod);
                if (result != TIMERR_NOERROR)
                {
                    Logger.Debug("timeBeginPeriod({0}): error {1}", result);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("{0}: {1}", ex.GetType().Name, ex.Message);
            }
            return 0xFFFFFFFF;
        }

        public static Rectangle GetWindowRect(IntPtr hWnd)
        {
            RECT wndRect;
            if (!NativeMethods.GetWindowRect(hWnd, out wndRect))
            {
                Trace.WriteLine("GetWindowRect failed");
                return Rectangle.Empty;
            }
            return new Rectangle(wndRect.Location, wndRect.Size);
        }

        #endregion Wrappers
    }
}
