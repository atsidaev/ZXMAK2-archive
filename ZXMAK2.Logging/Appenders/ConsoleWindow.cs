using System;
using System.Runtime.InteropServices;
using System.Reflection;


namespace ZXMAK2.Logging.Appenders
{
    public class ConsoleWindow : IDisposable
    {
        private static readonly object _syncRoot = new object();
        private GCHandle _callbackHandle;
        private IntPtr _pinnedCallback;
        private IntPtr _handle;
        private bool _isOwner;
        private bool _isShown;
        private bool _isInitialized;

        
        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (!_isInitialized || !_isOwner)
                {
                    return;
                }
                _isOwner = false;
                WinApi.SetConsoleCtrlHandler(_pinnedCallback, false);
                WinApi.FreeConsole();
                _isShown = false;
                if (_callbackHandle.IsAllocated)
                {
                    _callbackHandle.Free();
                }
                _isInitialized = false;
            }
        }

        public void Show()
        {
            lock (_syncRoot)
            {
                Allocate();
                if (!_isInitialized || !_isOwner || _isShown)
                {
                    return;
                }
                WinApi.ShowWindow(_handle, SW_SHOWNOACTIVATE);
                _isShown = true;
            }
        }

        public void Hide()
        {
            lock (_syncRoot)
            {
                if (!_isInitialized || !_isOwner || !_isShown)
                {
                    return;
                }
                WinApi.ShowWindow(_handle, SW_HIDE);
                _isShown = false;
            }
        }


        #region Private

        private void Allocate()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;
            _handle = WinApi.GetConsoleWindow();
            if (_handle != IntPtr.Zero)
            {
                return;
            }
            if (!WinApi.AllocConsole())
            {
                return;
            }
            _handle = WinApi.GetConsoleWindow();
            if (_handle == IntPtr.Zero)
            {
                return;
            }
            _isShown = true;
            ReinitConsole();
            Console.Title = string.Format("{0} [Ctrl+C to hide]", Console.Title);
            _isOwner = true;
            ApplyCloseBehavior();
        }

        private void ReinitConsole()
        {
            // Under debugger with no console, handle will be redirected to VS
            // workaround to recreate handle inside Console class
            var handleFieldInfo = typeof(Console).GetField("_consoleOutputHandle", BindingFlags.Static | BindingFlags.NonPublic);
            if (handleFieldInfo != null)
            {
                handleFieldInfo.SetValue(null, IntPtr.Zero);
            }
            // workaround to recreate Out/Error writers inside Console class
            Console.OutputEncoding = Console.OutputEncoding;
        }

        private void ApplyCloseBehavior()
        {
            var hMenu = WinApi.GetSystemMenu(_handle, false);
            WinApi.DeleteMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);

            var callbackHandler = new HandlerRoutine(ConsoleHandlerCallback);
            _callbackHandle = GCHandle.Alloc(callbackHandler);
            _pinnedCallback = Marshal.GetFunctionPointerForDelegate(callbackHandler);
            WinApi.SetConsoleCtrlHandler(_pinnedCallback, true);
        }

        private bool ConsoleHandlerCallback(int dwCtrlType)
        {
            if (dwCtrlType == CTRL_C_EVENT ||
                dwCtrlType == CTRL_BREAK_EVENT)
            {
                Hide();
            }
            return true;
        }

        #endregion Private

        
        #region WinApi

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

        #endregion Private
    }
}
