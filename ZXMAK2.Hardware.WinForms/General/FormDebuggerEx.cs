using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Host.WinForms.Tools;
using ZXMAK2.Host.WinForms.Views;

namespace ZXMAK2.Hardware.WinForms.General
{
    public partial class FormDebuggerEx : FormView, IDebuggerGeneralView
    {
        private DebuggerViewModel _dataContext;
        private BindingManager _manager = new BindingManager();
        private bool _isCloseRequest;
        private bool _isUiRequest;
        private bool _isCloseCalled;


        public FormDebuggerEx(IDebuggable debugTarget)
        {
            _dataContext = new DebuggerViewModel(debugTarget, this);
            _dataContext.Attach();
            _dataContext.CloseRequest += DataContext_OnCloseRequest;
            
            InitializeComponent();
            dockPanel.DocumentStyle = DocumentStyle.DockingWindow;// .DockingMdi;

            var dasm = new FormDisassembly();
            var memr = new FormMemory();
            var regs = new FormRegisters();
            var stat = new FormState();

            //regs.Show(dockPanel, DockState.DockRight);
            //stat.Show(regs.Pane, DockAlignment.Bottom, 0.3);
            //dasm.Show(dockPanel, DockState.Document);
            //memr.Show(dasm.Pane, DockAlignment.Bottom, 0.3);
            
            // Mono compatible
            dasm.Show(dockPanel, DockState.Document);
            memr.Show(dasm.Pane, DockAlignment.Bottom, 0.3);
            regs.Show(dasm.Pane, DockAlignment.Right, 0.2);
            stat.Show(memr.Pane, DockAlignment.Right, 0.2);


            Bind();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _dataContext.Detach();
            base.OnFormClosed(e);
        }

        
        #region Close Behavior

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //Logger.Debug("OnFormClosing: reason={0}, isRequest={1}, isUi={2}", e.CloseReason, _isCloseRequest, _isUiRequest);
            if (!_isCloseRequest &&
                _dataContext != null &&
                _dataContext.CommandClose != null)
            {
                _isCloseCalled = false;
                _isUiRequest = true;
                var canClose = _dataContext.CommandClose.CanExecute(null);
                if (canClose)
                {
                    _dataContext.CommandClose.Execute(null);
                    canClose = _isCloseCalled;
                }
                _isUiRequest = false;
                if (!canClose)
                {
                    // WARN: Mono runtime has a bug, so if the user will
                    // close parent window, it will be closed although Cancel=true.
                    // In such case, attempt to show the window will cause 
                    // fatal Mono runtime bug (it requires OS reboot).
                    // So, we call it async to avoid such issues.
                    e.Cancel = true;
                    // show & highlight blocking window
                    BeginInvoke(new Action(() =>
                        {
                            Show();
                            WindowState = FormWindowState.Normal;
                            Activate();
                        }), null);
                }
            }
            base.OnFormClosing(e);
        }
        
        private void DataContext_OnCloseRequest(object sender, EventArgs e)
        {
            if (_isUiRequest)
            {
                _isCloseCalled = true;
                return;
            }
            _isCloseRequest = true;
            Close();
            _isCloseRequest = false;
        }

        #endregion Close Behavior


        private void Bind()
        {
            _manager.Bind(_dataContext.CommandContinue, menuDebugContinue);
            _manager.Bind(_dataContext.CommandContinue, toolStripContinue);
            _manager.Bind(_dataContext.CommandBreak, menuDebugBreak);
            _manager.Bind(_dataContext.CommandBreak, toolStripBreak);
            _manager.Bind(_dataContext.CommandStepInto, menuDebugStepInto);
            _manager.Bind(_dataContext.CommandStepInto, toolStripStepInto);
            _manager.Bind(_dataContext.CommandStepOver, menuDebugStepOver);
            _manager.Bind(_dataContext.CommandStepOver, toolStripStepOver);
            _manager.Bind(_dataContext.CommandStepOut, menuDebugStepOut);
            _manager.Bind(_dataContext.CommandStepOut, toolStripStepOut);

            _manager.Bind(_dataContext.CommandClose, menuFileClose);
            _dataContext.CommandClose.CanExecuteChanged += DataContextCommandClose_OnCanExecuteChanged;
            DataContextCommandClose_OnCanExecuteChanged(this, EventArgs.Empty);
        }

        private void DataContextCommandClose_OnCanExecuteChanged(object sender, EventArgs e)
        {
            var canExecute = _dataContext.CommandClose.CanExecute(null);
            if (canExecute)
            {
                NativeMethods.EnableCloseButton(Handle);
            }
            else
            {
                NativeMethods.DisableCloseButton(Handle);
            }
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal class NativeMethods
    {
        private static readonly Dictionary<IntPtr, int> s_windowSystemMenuHandle = new Dictionary<IntPtr, int>();

        public static bool IsWinApiNotAvailable { get; private set; }

        public static void DisableCloseButton(IntPtr hWnd)
        {
            if (IsWinApiNotAvailable)
            {
                return;
            }
            try
            {
                if (s_windowSystemMenuHandle.ContainsKey(hWnd))
                {
                    return;
                }
                var hMenu = GetSystemMenu(hWnd, false);
                s_windowSystemMenuHandle[hWnd] = hMenu;
                DeleteMenu(hMenu, 6, 1024);
            }
            catch (EntryPointNotFoundException ex)
            {
                Logger.Warn(ex);
                IsWinApiNotAvailable = true;
            }
        }

        public static void EnableCloseButton(IntPtr hWnd)
        {
            if (IsWinApiNotAvailable)
            {
                return;
            }
            try
            {
                if (!s_windowSystemMenuHandle.ContainsKey(hWnd))
                {
                    return;
                }
                var hMenu = GetSystemMenu(hWnd, true);
                DeleteMenu(hMenu, 6, 1024);
                s_windowSystemMenuHandle.Remove(hWnd);
            }
            catch (EntryPointNotFoundException ex)
            {
                Logger.Warn(ex);
                IsWinApiNotAvailable = true;
            }
        }    

        [DllImport("user32", SetLastError=true)]
        private static extern int GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32", SetLastError = true)]
        private static extern bool DeleteMenu(int hMenu, int uPosition, int uFlags);
    }
}
