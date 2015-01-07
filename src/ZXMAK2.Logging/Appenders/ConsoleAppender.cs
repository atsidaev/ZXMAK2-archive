using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Appender;
using log4net.Core;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace ZXMAK2.Logging.Appenders
{
    public class ConsoleAppender : ColoredConsoleAppender
    {
        private bool _isAllocated;
        private bool _isOwner;
        private GCHandle _callbackHandle;
        private IntPtr _pinnedCallback;
        private bool _isShown;
        
        public ConsoleAllocMode AllocMode { get; set; }
        public Level AutoLevel { get; set; }


        public ConsoleAppender()
        {
            AutoLevel = Level.Debug;
        }


        public override void ActivateOptions()
        {
            if (AllocMode == ConsoleAllocMode.Always)
            {
                Allocate();
            }
            base.ActivateOptions();
        }

        protected override void OnClose()
        {
            base.OnClose();
            if (_isAllocated)
            {
                Deallocate();
            }
        }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (AllocMode == ConsoleAllocMode.Auto &&
                loggingEvent != null && 
                loggingEvent.Level >= AutoLevel)
            {
                Allocate();
            }
            base.Append(loggingEvent);
        }

        protected override void Append(LoggingEvent[] loggingEvents)
        {
            if (loggingEvents == null)
            {
                return;
            }
            foreach (var logEvent in loggingEvents)
            {
                Append(loggingEvents);
            }
        }

        private void Allocate()
        {
            var handle = WinApi.GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                if (!_isShown)
                {
                    WinApi.ShowWindow(handle, SW_SHOWNOACTIVATE);
                    _isShown = true;
                }
                return;
            }
            if (_isAllocated)
            {
                return;
            }
            _isAllocated = true;
            if (!WinApi.AllocConsole())
            {
                return;
            }
            _isOwner = true;
            handle = WinApi.GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                Console.Title = string.Format("{0} [Ctrl+C to hide]", Console.Title);

                WinApi.ShowWindow(handle, SW_SHOWNOACTIVATE);
                _isShown = true;
                var hMenu = WinApi.GetSystemMenu(handle, false);
                WinApi.DeleteMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);

                var callbackHandler = new HandlerRoutine(ConsoleHandlerCallback);
                _callbackHandle = GCHandle.Alloc(callbackHandler);
                _pinnedCallback = Marshal.GetFunctionPointerForDelegate(callbackHandler);
                WinApi.SetConsoleCtrlHandler(_pinnedCallback, true);
            }
            base.ActivateOptions();
        }

        private void Deallocate()
        {
            if (!_isAllocated)
            {
                return;
            }
            _isAllocated = false;
            if (!_isOwner)
            {
                return;
            }
            WinApi.SetConsoleCtrlHandler(_pinnedCallback, false);
            WinApi.FreeConsole();
            _isShown = false;
            if (_callbackHandle.IsAllocated)
            {
                _callbackHandle.Free();
            }
        }

        private bool ConsoleHandlerCallback(int dwCtrlType)
        {
            if (dwCtrlType == CTRL_C_EVENT ||
                dwCtrlType == CTRL_BREAK_EVENT)
            {
                var handle = WinApi.GetConsoleWindow();
                WinApi.ShowWindow(handle, SW_HIDE);
                _isShown = false;
            }
            return true;
        }


        private const int CTRL_C_EVENT = 0;
        private const int CTRL_BREAK_EVENT = 1;
        private const int CTRL_CLOSE_EVENT = 2;
        private const int CTRL_LOGOFF_EVENT = 5;
        private const int CTRL_SHUTDOWN_EVENT = 6;

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_SHOWNOACTIVATE = 4;

        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint SC_CLOSE = 0xF060;

        private delegate bool HandlerRoutine(int dwCtrlType);

        private static class WinApi
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FreeConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetConsoleWindow();

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleCtrlHandler(IntPtr handler, bool add);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int DeleteMenu(
                IntPtr hMenu,
                uint nPosition,
                uint wFlags);
        }
    }
}
