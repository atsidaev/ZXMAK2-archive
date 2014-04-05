using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ZXMAK2.Interfaces;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.Entities;

namespace ZXMAK2.MVP.WinForms
{
    public class ViewHolder<T> : IViewHolder
        where T : Form
    {
        private readonly string m_name;
        private object[] m_args;
        private IWin32Window m_hostWindow;
        private ICommand m_command;
        private T m_form;
        private bool m_canClose;
        

        public ViewHolder(string name, params object[] args)
        {
            m_name = name;
            m_args = args;
        }

        public object[] Arguments
        {
            get { return m_args; }
            set { m_args = value; }
        }

        public ICommand CommandOpen
        {
            get
            {
                if (m_command != null)
                {
                    return m_command;
                }
                m_command = new CommandDelegate(Command_OnExecute, Command_OnCanExecute, m_name);
                return m_command;
            }
        }

        public void Close()
        {
            if (m_form != null)
            {
                m_canClose = true;
                m_form.Close();
                m_form = null;
                m_command = null;
            }
        }

        private void CreateTargetForm()
        {
            m_canClose = false;
            if (m_args != null && m_args.Length > 0)
            {
                m_form = (T)Activator.CreateInstance(typeof(T), m_args);
            }
            else
            {
                m_form = Activator.CreateInstance<T>();
            }
            m_form.FormClosed += (s,e) => 
            {
                m_form = null;
            };
            m_form.FormClosing += (s, e) =>
            {
                if (m_form != null && !m_canClose && e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    m_form.Hide();
                }
            };
        }

        private bool Command_OnCanExecute(Object arg)
        {
            return arg is IWin32Window;
        }

        private void Command_OnExecute(Object arg)
        {
            try
            {
                var hostWindow = arg as IWin32Window;
                m_hostWindow = hostWindow ?? m_hostWindow;
                if (m_form == null)
                {
                    CreateTargetForm();
                }
                if (arg == null || m_form == null)
                {
                    return;
                }
                if (!m_form.Visible)
                {
                    m_form.Show(m_hostWindow);
                }
                else
                {
                    m_form.Show();
                    m_form.Activate();
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }

        public void Show()
        {
            var hostWindow = m_hostWindow;
            var form = m_form;
            if (hostWindow == null || form == null)
            {
                return;
            }
            var action = new Action(()=>Command_OnExecute(hostWindow));
            if (form.InvokeRequired)
            {
                form.BeginInvoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
