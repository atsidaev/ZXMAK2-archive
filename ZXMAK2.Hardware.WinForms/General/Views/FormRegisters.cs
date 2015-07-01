using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.WinForms.General.ViewModels;
using ZXMAK2.Host.WinForms.Tools;

namespace ZXMAK2.Hardware.WinForms.General.Views
{
    public partial class FormRegisters : DockContent
    {
        private RegistersViewModel _dataContext;
        
        
        public FormRegisters()
        {
            InitializeComponent();
        }

        public void Attach(IDebuggable target)
        {
            _dataContext = new RegistersViewModel(target, this);
            _dataContext.PropertyChanged += DataContext_OnPropertyChanged;
            _dataContext.TargetStateChanged += DataContext_OnTargetStateChanged;
            _dataContext.Attach();
            Bind();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dataContext.PropertyChanged -= DataContext_OnPropertyChanged;
            _dataContext.TargetStateChanged -= DataContext_OnTargetStateChanged;
            _dataContext.Detach();
            base.OnClosed(e);
        }


        #region Binding

        private void Bind()
        {
            Bind(chkIff1, "Iff1");
            Bind(chkIff2, "Iff2");
            Bind(chkHalt, "Halt");
            Bind(chkBint, "Bint");
            Bind(chkFlagS, "FlagS");
            Bind(chkFlagZ, "FlagZ");
            Bind(chkFlag5, "Flag5");
            Bind(chkFlagH, "FlagH");
            Bind(chkFlag3, "Flag3");
            Bind(chkFlagV, "FlagV");
            Bind(chkFlagN, "FlagN");
            Bind(chkFlagC, "FlagC");
        }

        private void Bind(CheckBox control, string name)
        {
            control.DataBindings.Add("Checked", _dataContext, name, false, DataSourceUpdateMode.OnPropertyChanged);
        }

        private void DataContext_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "IsRunning":
                    var allowEdit = !_dataContext.IsRunning;
                    Controls
                        .OfType<Control>()
                        .ToList()
                        .ForEach(arg => arg.Enabled = allowEdit);
                    Controls
                        .OfType<Control>()
                        .ToList()
                        .ForEach(arg => arg.BackColor = Color.White);
                    break;
                case "Pc": txtRegPc.Text = FormatRegValue(_dataContext.Pc); break;
                case "Sp": txtRegSp.Text = FormatRegValue(_dataContext.Sp); break;
                case "Ir": txtRegIr.Text = FormatRegValue(_dataContext.Ir); break;
                case "Im": txtRegIm.Text = _dataContext.Im.ToString(); break;
                case "Wz": txtRegWz.Text = FormatRegValue(_dataContext.Wz); break;
                case "Lpc": txtRegLpc.Text = FormatRegValue(_dataContext.Lpc); break;
                case "Af": txtRegAf.Text = FormatRegValue(_dataContext.Af); break;
                case "Af_": txtRegAf_.Text = FormatRegValue(_dataContext.Af_); break;
                case "Hl": txtRegHl.Text = FormatRegValue(_dataContext.Hl); break;
                case "Hl_": txtRegHl_.Text = FormatRegValue(_dataContext.Hl_); break;
                case "De": txtRegDe.Text = FormatRegValue(_dataContext.De); break;
                case "De_": txtRegDe_.Text = FormatRegValue(_dataContext.De_); break;
                case "Bc": txtRegBc.Text = FormatRegValue(_dataContext.Bc); break;
                case "Bc_": txtRegBc_.Text = FormatRegValue(_dataContext.Bc_); break;
                case "Ix": txtRegIx.Text = FormatRegValue(_dataContext.Ix); break;
                case "Iy": txtRegIy.Text = FormatRegValue(_dataContext.Iy); break;
            }
        }

        private void DataContext_OnTargetStateChanged(object sender, EventArgs e)
        {
            if (!_dataContext.IsRunning)
            {
                //// reset focus
                //Controls
                //    .OfType<Control>()
                //    .ToList()
                //    .ForEach(arg => arg.Enabled = false);
                //Controls
                //    .OfType<Control>()
                //    .ToList()
                //    .ForEach(arg => arg.Enabled = true);
                //_isSelectionNeeded = true;
            }
        }

        private string FormatRegValue(ushort value)
        {
            return string.Format("#{0:X4}", value);
        }

        #endregion Binding


        #region TextBox Behavior

        private bool _isSelectionNeeded;

        private void txtReg_OnClick(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            //Logger.Debug("txtReg_OnClick: Focused={0}, SelectionStart={1}", textBox.Focused, textBox.SelectionStart);
            if (textBox.Focused && _isSelectionNeeded)
            {
                textBox.SelectAll();
                _isSelectionNeeded = false;
            }
        }

        private void txtReg_OnEnter(object sender, EventArgs e)
        {
            //_isSelectionNeeded = true;
        }

        private void txtReg_OnLeave(object sender, EventArgs e)
        {
            _isSelectionNeeded = true;
        }

        #endregion TextBox Behavior
    }
}
