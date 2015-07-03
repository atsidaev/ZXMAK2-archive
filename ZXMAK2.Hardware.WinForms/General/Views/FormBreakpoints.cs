using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Hardware.WinForms.General.ViewModels;

namespace ZXMAK2.Hardware.WinForms.General.Views
{
    public partial class FormBreakpoints : DockContent
    {
        private BreakpointsViewModel _dataContext;

        public FormBreakpoints()
        {
            InitializeComponent();

            // Workaround #2 for binding.
            // ListBox does not expose SelectedItemChanged event,
            // so DataBinding have no idea that something has changed in the control.
            // And DataBinding didn't set property on view model.
            // There is workaround to fix it:
            lstItems.SelectedIndexChanged += (s, e) =>
            {
                var binding = DataBindings["SelectedItem"];
                if (binding != null)
                {
                    binding.WriteValue();
                }
            };
        }

        public void Attach(IDebuggable target)
        {
            _dataContext = new BreakpointsViewModel(target, this);
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

        private void Bind()
        {
            lstItems.DataSource = _dataContext.Breakpoints;
            Bind(lstItems, "Breakpoints", "SelectedBreakpoint");
        }

        private void Bind(ListBox control, string nameSource, string nameSelectedItem)
        {
            var source = new BindingSource(_dataContext, nameSource);
            control.DataSource = source;
            var binding = new Binding("SelectedItem", _dataContext, nameSelectedItem, true, DataSourceUpdateMode.OnPropertyChanged);
            binding.Format += (s, e) => e.Value = e.Value ?? DBNull.Value;
            binding.Parse += (s, e) => e.Value = e.Value == DBNull.Value ? null : e.Value;
            control.DataBindings.Add(binding);
        }

        private void DataContext_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }
    }

    /// <summary>
    /// ListBox with fixed binding
    /// </summary>
    public class ListBoxEx : ListBox
    {
        public override int SelectedIndex
        {
            set
            {
                // Workaround #1 for internal binding bug.
                // Actually all works without it.
                // But this workaround helps to avoid 
                // first chance exception which happens inside ListBox.
                // So, it eliminates ArgumentOutOfRangeException: "InvalidArgument=Value of '0' is not valid for 'SelectedIndex'."
                // when ListBox is empty
                base.SelectedIndex = value < Items.Count ? value : -1;
            }
        }
    }
}
