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
using ZXMAK2.Host.WinForms.Views;

namespace ZXMAK2.Hardware.WinForms.General
{
    public partial class FormDebuggerEx : FormView, IDebuggerGeneralView
    {
        public FormDebuggerEx(IDebuggable debugTarget)
        {
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
        }
    }
}
