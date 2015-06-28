using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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


        public FormDebuggerEx(IDebuggable debugTarget)
        {
            _dataContext = new DebuggerViewModel(debugTarget, this);
            _dataContext.Attach();
            
            InitializeComponent();
            dockPanel.DocumentStyle = DocumentStyle.DockingMdi;

            var dasm = new FormDisassembly();
            var memr = new FormMemory();
            var regs = new FormRegisters();
            var stat = new FormState();

            regs.Show(dockPanel, DockState.DockRight);
            stat.Show(regs.Pane, DockAlignment.Bottom, 0.3);
            dasm.Show(dockPanel, DockState.Document);
            memr.Show(dasm.Pane, DockAlignment.Bottom, 0.3);

            Bind();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _dataContext.Detach();
            base.OnFormClosed(e);
        }

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
        }
    }
}
