using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;

namespace ZXMAK2.Hardware.Adlers
{
    public class Debugger : BusDeviceBase, IJtagDevice, IGuiExtension
    {
        private IDebuggable m_target;

        #region BusDeviceBase

        public override string Name { get { return "Debugger Adlers"; } }
        public override string Description { get { return "Debugger Adlers"; } }
        public override BusCategory Category { get { return BusCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion

        #region IJtagDevice

        public void Attach(IDebuggable dbg)
        {
            m_target = dbg;
            m_target.Breakpoint += new EventHandler(OnBreakpoint);
        }

        public void Detach()
        {
            m_target.Breakpoint -= new EventHandler(OnBreakpoint);
        }

        #endregion

        #region IGuiExtension Members

        private GuiData m_guiData;
		private System.Windows.Forms.MenuItem m_subMenuItem;
		private UI.FormCpu m_form;

        public void AttachGui(GuiData guiData)
        {
            m_guiData = guiData;
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
                if (menuItem != null)
                {
                    m_subMenuItem = new System.Windows.Forms.MenuItem("Debugger", menu_Click);
                    menuItem.MenuItems.Add(m_subMenuItem);
                }
            }
        }

        public void DetachGui()
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_subMenuItem != null)
                {
                    m_subMenuItem.Parent.MenuItems.Remove(m_subMenuItem);
                    m_subMenuItem.Dispose();
                    m_subMenuItem = null;
                }
                if (m_form != null)
                {
                    m_form.AllowClose = true;
                    m_form.Close();
                    m_form = null;
                }
            }
            m_guiData = null;
        }

        private void menu_Click(object sender, EventArgs e)
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_form == null)
                {
					m_form = new UI.FormCpu();
                    m_form.Init(m_target);
                    m_form.FormClosed += delegate(object obj, System.Windows.Forms.FormClosedEventArgs arg)
                    {
                        m_form = null;
                    };
                    m_form.Show((System.Windows.Forms.Form)m_guiData.MainWindow);
                }
                else
                {
                    m_form.Show();
                    m_form.Activate();
                }
            }
        }

        protected virtual void OnBreakpoint(object sender, EventArgs e)
        {
            System.Windows.Forms.Form mainForm = m_guiData.MainWindow as System.Windows.Forms.Form;
            if (mainForm != null)
            {
                if (mainForm.InvokeRequired)
                {
                    mainForm.BeginInvoke(new EventHandler(OnBreakpoint), sender, e);
                    return;
                }
                menu_Click(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
