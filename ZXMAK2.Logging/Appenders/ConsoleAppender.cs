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
            if (!_isAllocated && 
                AllocMode == ConsoleAllocMode.Auto &&
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
            if (_isAllocated)
            {
                return;
            }
            _isAllocated = true;
            //WinApi.AttachConsole(ATTACH_PARENT_PROCESS)
            var handle = WinApi.GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                return;
            }
            if (!WinApi.AllocConsole())
            {
                return;
            }
            _isOwner = true;
            handle = WinApi.GetConsoleWindow();
            if (handle != IntPtr.Zero)
            {
                //WinApi.SetConsoleTitle(Name);
                WinApi.ShowWindow(handle, SW_SHOWNOACTIVATE);
                var hMenu = WinApi.GetSystemMenu(handle, false);
                WinApi.DeleteMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
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
            WinApi.FreeConsole();
        }

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_SHOWNOACTIVATE = 4;
        private const UInt32 STD_INPUT_HANDLE = 0xFFFFFFF6;
        private const UInt32 STD_OUTPUT_HANDLE = 0xFFFFFFF5;
        private const UInt32 STD_ERROR_HANDLE = 0xFFFFFFF4;
        private const UInt32 ATTACH_PARENT_PROCESS = 0xFFFFFFFF;
        private const UInt32 ALLOCATED_HANDLE = 7;

        private const int MF_BYCOMMAND = 0x00000000;
        public const int SC_CLOSE = 0xF060;

        private static class WinApi
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AttachConsole(UInt32 dwProcessId);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool FreeConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr GetConsoleWindow();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool SetConsoleTitle(string lpConsoleTitle);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        }
    }
}
