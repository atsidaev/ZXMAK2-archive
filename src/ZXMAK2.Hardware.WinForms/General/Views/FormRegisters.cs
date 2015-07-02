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
using ZXMAK2.Mvvm;
using System.Globalization;

namespace ZXMAK2.Hardware.WinForms.General.Views
{
    public partial class FormRegisters : DockContent
    {
        private RegistersViewModel _dataContext;
        private IValueConverter _regToStringConverter = new IntegerToStringConverter() { IsHex = true, DigitCount = 4 };
        
        
        public FormRegisters()
        {
            InitializeComponent();
        }

        public void Attach(IDebuggable target)
        {
            _dataContext = new RegistersViewModel(target, this);
            _dataContext.PropertyChanged += DataContext_OnPropertyChanged;
            _dataContext.Attach();
            Bind();
        }

        protected override void OnClosed(EventArgs e)
        {
            _dataContext.PropertyChanged -= DataContext_OnPropertyChanged;
            _dataContext.Detach();
            base.OnClosed(e);
        }


        #region Binding

        private void Bind()
        {
            Bind(txtRegPc, "Pc", _regToStringConverter);
            Bind(txtRegSp, "Sp", _regToStringConverter);
            Bind(txtRegIr, "Ir", _regToStringConverter);
            Bind(txtRegIm, "Im");
            Bind(txtRegWz, "Wz", _regToStringConverter);
            Bind(txtRegLpc, "Lpc", _regToStringConverter);
            Bind(txtRegAf, "Af", _regToStringConverter);
            Bind(txtRegAf_, "Af_", _regToStringConverter);
            Bind(txtRegHl, "Hl", _regToStringConverter);
            Bind(txtRegHl_, "Hl_", _regToStringConverter);
            Bind(txtRegDe, "De", _regToStringConverter);
            Bind(txtRegDe_, "De_", _regToStringConverter);
            Bind(txtRegBc, "Bc", _regToStringConverter);
            Bind(txtRegBc_, "Bc_", _regToStringConverter);
            Bind(txtRegIx, "Ix", _regToStringConverter);
            Bind(txtRegIy, "Iy", _regToStringConverter);
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
            Bind(lblRzxFetchValue, "RzxFetch");
            Bind(lblRzxInputValue, "RzxInput");
            Bind(lblRzxFrameValue, "RzxFrame");
            BindVisible(lblTitleRzx, "IsRzxAvailable");
            BindVisible(sepRzx, "IsRzxAvailable");
            BindVisible(lblRzxFetch, "IsRzxAvailable");
            BindVisible(lblRzxInput, "IsRzxAvailable");
            BindVisible(lblRzxFrame, "IsRzxAvailable");
            BindVisible(lblRzxFetchValue, "IsRzxAvailable");
            BindVisible(lblRzxInputValue, "IsRzxAvailable");
            BindVisible(lblRzxFrameValue, "IsRzxAvailable");
        }

        private void BindVisible(Control control, string name)
        {
            var binding = new Binding("Visible", _dataContext, name, false);
            control.DataBindings.Add(binding);
        }

        private void Bind(Label control, string name)
        {
            var binding = new Binding("Text", _dataContext, name, false);
            control.DataBindings.Add(binding);
        }

        private void Bind(TextBox control, string name)
        {
            var binding = new Binding("Text", _dataContext, name, false, DataSourceUpdateMode.OnValidation);
            control.DataBindings.Add(binding);
        }

        private void Bind(TextBox control, string name, IValueConverter converter)
        {
            var binding = new Binding("Text", _dataContext, name, true, DataSourceUpdateMode.OnValidation);
            binding.Format += (s, e) => e.Value = converter.Convert(e.Value, e.DesiredType, null, CultureInfo.CurrentCulture);
            binding.Parse += (s, e) => e.Value = converter.ConvertBack(e.Value, e.DesiredType, null, CultureInfo.CurrentCulture);
            control.DataBindings.Add(binding);
        }

        private void Bind(CheckBox control, string name)
        {
            control.DataBindings.Add(
                "Checked", 
                _dataContext, 
                name, 
                false, 
                DataSourceUpdateMode.OnPropertyChanged);
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
            }
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

        private void txtReg_KeyPress(object sender, KeyPressEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (e.KeyChar == 0x0D)
            {
                e.Handled = true;
                Validate();
            }
        }

        #endregion TextBox Behavior
    }
}
