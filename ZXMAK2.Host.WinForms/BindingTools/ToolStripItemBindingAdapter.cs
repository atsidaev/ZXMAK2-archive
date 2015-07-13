using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZXMAK2.Mvvm;
using ZXMAK2.Mvvm.BindingTools;

namespace ZXMAK2.Host.WinForms.BindingTools
{
    public class ToolStripItemBindingAdapter : BaseBindingAdapter
    {
        public ToolStripItemBindingAdapter(ToolStripItem control)
            : base(control)
        {
            control.Click += ToolStrip_OnClick;    
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
                return;
            if (Command != null)
            {
                Command.CanExecuteChanged -= Command_OnCanExecuteChanged;
            }
            var control = (ToolStripItem)Target;
            control.Click -= ToolStrip_OnClick;    
        }

        public override Type GetTargetPropertyType(string name)
        {
            if (name == "Command")  // virtual property
            {
                return typeof(ICommand);
            }
            return base.GetTargetPropertyType(name);
        }

        public override object GetTargetPropertyValue(string name)
        {
            if (name == "Command")  // virtual property
            {
                return Command;
            }
            return base.GetTargetPropertyValue(name);
        }

        public override void SetTargetPropertyValue(string name, object value)
        {
            if (name == "Command")  // virtual property
            {
                Command = (ICommand)value;
                return;
            }
            // cache property set to eliminate redundant UI updates
            if (name == "Text")
            {
                var control = (ToolStripItem)Target;
                if (control.Text != (string)value)
                {
                    control.Text = (string)value;
                }
                return;
            }
            if (name == "Visible")
            {
                var control = (ToolStripItem)Target;
                if (control.Visible != (bool)value)
                {
                    control.Visible = (bool)value;
                }
                return;
            }
            if (name == "Checked")
            {
                var menuItem = Target as ToolStripMenuItem;
                if (menuItem.Checked != (bool)value)
                {
                    menuItem.Checked = (bool)value;
                }
                var button = Target as ToolStripButton;
                if (button.Checked != (bool)value)
                {
                    button.Checked = (bool)value;
                }
                return;
            }
            base.SetTargetPropertyValue(name, value);
        }

        // Virtual property
        private ICommand _command;

        private ICommand Command
        {
            get { return _command; }
            set 
            {
                if (_command == value)
                {
                    return;
                }
                if (_command != null)
                {
                    _command.CanExecuteChanged -= Command_OnCanExecuteChanged;
                }
                _command = value;
                if (_command != null)
                {
                    _command.CanExecuteChanged += Command_OnCanExecuteChanged;
                    Command_OnCanExecuteChanged(_command, EventArgs.Empty);
                }
            }
        }

        private void Command_OnCanExecuteChanged(object sender, EventArgs e)
        {
            var control = (ToolStripItem)Target;
            if (Command != null)
            {
                control.Enabled = Command.CanExecute(null);
            }
        }

        private void ToolStrip_OnClick(object sender, EventArgs e)
        {
            if (Command != null && Command.CanExecute(null))
            {
                Command.Execute(null);    
            }
        }
    }
}
