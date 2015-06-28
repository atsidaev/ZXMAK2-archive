using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZXMAK2.Mvvm;

namespace ZXMAK2.Host.WinForms.Tools
{
    public sealed class BindingManager
    {
        private readonly List<ICommand> _commands = new List<ICommand>();
        private readonly List<ICommandBinder> _binders = new List<ICommandBinder>();
        
        public BindingManager()
        {
            _binders.Add(new ToolStripMenuItemCommandBinder());
            _binders.Add(new ToolStripButtonCommandBinder());
        }

        public void Bind(ICommand command, IComponent component)
        {
            if (!_commands.Contains(command))
            {
                _commands.Add(command);
            }
            var binder = GetBinderFor(component.GetType());
            if (binder == null)
            {
                throw new InvalidOperationException(
                    string.Format("No binding for {0}", component.GetType()));
            }
            binder.Bind(command, component);
        }

        private ICommandBinder GetBinderFor(Type componentType)
        {
            var type = componentType;
            while (type != null)
            {
                var binder = _binders
                    .FirstOrDefault(arg => arg.SourceType == type);
                if (binder != null)
                {
                    return binder;
                }
                type = type.BaseType;
            }
            return null;
        }
    }

    public interface ICommandBinder
    {
        Type SourceType { get; }
        void Bind(ICommand command, object source);
    }

    public abstract class CommandBinder<T> : ICommandBinder
        where T : Component
    {
        public Type SourceType
        {
            get { return typeof(T); }
        }

        public void Bind(ICommand command, object source)
        {
            Bind(command, (T)source);    
        }

        protected abstract void Bind(ICommand command, T source);
    }

    public class ToolStripMenuItemCommandBinder : CommandBinder<ToolStripMenuItem>
    {
        protected override void Bind(ICommand command, ToolStripMenuItem source)
        {
            source.Text = command.Text;
            source.Checked = command.Checked;
            source.Enabled = command.CanExecute(null);
            source.Click += (s, e) => command.Execute(null);
            command.PropertyChanged += (s, e) =>
                {
                    source.Text = command.Text;
                    source.Checked = command.Checked;
                };
            command.CanExecuteChanged += (s, e) =>
                {
                    source.Enabled = command.CanExecute(null);
                };
        }
    }

    public class ToolStripButtonCommandBinder : CommandBinder<ToolStripButton>
    {
        protected override void Bind(ICommand command, ToolStripButton source)
        {
            source.Text = command.Text;
            source.Checked = command.Checked;
            source.Enabled = command.CanExecute(null);
            source.Click += (s, e) => command.Execute(null);
            command.PropertyChanged += (s, e) =>
            {
                source.Text = command.Text;
                source.Checked = command.Checked;
            };
            command.CanExecuteChanged += (s, e) =>
            {
                source.Enabled = command.CanExecute(null);
            };
        }
    }
}
