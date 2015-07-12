﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.WinForms.General.ViewModels;
using ZXMAK2.Mvvm;
using ZXMAK2.Mvvm.BindingTools;
using ZXMAK2.Host.WinForms.Tools;
using ZXMAK2.Host.WinForms.BindingTools;


namespace ZXMAK2.Hardware.WinForms.General.Views
{
    public partial class FormRegisters : DockContent
    {
        private RegistersViewModel _dataContext;
        private IValueConverter _regToStringConverter = new IntegerToStringConverter() { IsHex = true, DigitCount = 4 };
        private BindingService _binding = new BindingService();
        
        
        public FormRegisters()
        {
            InitializeComponent();
            _binding.RegisterAdapterFactory<Control>(
                arg => new ControlBindingAdapter(arg));
        }


        public void Attach(IDebuggable target)
        {
            _dataContext = new RegistersViewModel(target, this);
            _dataContext.PropertyChanged += DataContext_OnPropertyChanged;
            _dataContext.Attach();
            Bind();
            _binding.DataContext = _dataContext;
        }

        protected override void OnClosed(EventArgs e)
        {
            _dataContext.PropertyChanged -= DataContext_OnPropertyChanged;
            _dataContext.Detach();
            _binding.Dispose();
            base.OnClosed(e);
        }


        #region Binding

        private void Bind()
        {
            var pText = "Text";
            _binding.Bind(txtRegPc, pText, "Pc", _regToStringConverter);
            _binding.Bind(txtRegSp, pText, "Sp", _regToStringConverter);
            _binding.Bind(txtRegIr, pText, "Ir", _regToStringConverter);
            _binding.Bind(txtRegIm, pText, "Im");
            _binding.Bind(txtRegWz, pText, "Wz", _regToStringConverter);
            _binding.Bind(txtRegLpc, pText, "Lpc", _regToStringConverter);
            _binding.Bind(txtRegAf, pText, "Af", _regToStringConverter);
            _binding.Bind(txtRegAf_, pText, "Af_", _regToStringConverter);
            _binding.Bind(txtRegHl, pText, "Hl", _regToStringConverter);
            _binding.Bind(txtRegHl_, pText, "Hl_", _regToStringConverter);
            _binding.Bind(txtRegDe, pText, "De", _regToStringConverter);
            _binding.Bind(txtRegDe_, pText, "De_", _regToStringConverter);
            _binding.Bind(txtRegBc, pText, "Bc", _regToStringConverter);
            _binding.Bind(txtRegBc_, pText, "Bc_", _regToStringConverter);
            _binding.Bind(txtRegIx, pText, "Ix", _regToStringConverter);
            _binding.Bind(txtRegIy, pText, "Iy", _regToStringConverter);
            _binding.Bind(lblRzxFetchValue, pText, "RzxFetch");
            _binding.Bind(lblRzxInputValue, pText, "RzxInput");
            _binding.Bind(lblRzxFrameValue, pText, "RzxFrame");
            var pChecked = "Checked";
            _binding.Bind(chkIff1, pChecked, "Iff1");
            _binding.Bind(chkIff2, pChecked, "Iff2");
            _binding.Bind(chkHalt, pChecked, "Halt");
            _binding.Bind(chkBint, pChecked, "Bint");
            _binding.Bind(chkFlagS, pChecked, "FlagS");
            _binding.Bind(chkFlagZ, pChecked, "FlagZ");
            _binding.Bind(chkFlag5, pChecked, "Flag5");
            _binding.Bind(chkFlagH, pChecked, "FlagH");
            _binding.Bind(chkFlag3, pChecked, "Flag3");
            _binding.Bind(chkFlagV, pChecked, "FlagV");
            _binding.Bind(chkFlagN, pChecked, "FlagN");
            _binding.Bind(chkFlagC, pChecked, "FlagC");
            var pVisible = "Visible";
            _binding.Bind(lblTitleRzx, pVisible, "IsRzxAvailable");
            _binding.Bind(sepRzx, pVisible, "IsRzxAvailable");
            _binding.Bind(lblRzxFetch, pVisible, "IsRzxAvailable");
            _binding.Bind(lblRzxInput, pVisible, "IsRzxAvailable");
            _binding.Bind(lblRzxFrame, pVisible, "IsRzxAvailable");
            _binding.Bind(lblRzxFetchValue, pVisible, "IsRzxAvailable");
            _binding.Bind(lblRzxInputValue, pVisible, "IsRzxAvailable");
            _binding.Bind(lblRzxFrameValue, pVisible, "IsRzxAvailable");
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
