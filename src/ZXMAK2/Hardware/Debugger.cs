using System;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices
{
    public class Debugger : BusDeviceBase, IJtagDevice, IGuiExtension
    {
        private IDebuggable m_target;

        #region BusDeviceBase

        public override string Name { get { return "Debugger"; } }
        public override string Description { get { return "Default Debugger"; } }
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
        private object m_subMenuItem;
        private object m_form;

        public void AttachGui(GuiData guiData)
        {
            m_guiData = guiData;
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
                if (menuItem != null)
                {
                    m_subMenuItem = new System.Windows.Forms.MenuItem("Debugger", menu_Click);
                    menuItem.MenuItems.Add((System.Windows.Forms.MenuItem)m_subMenuItem);
                }
            }
        }

        public void DetachGui()
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem subMenuItem = m_subMenuItem as System.Windows.Forms.MenuItem;
                System.Windows.Forms.Form form = m_form as System.Windows.Forms.Form;
                if (subMenuItem != null)
                {
                    subMenuItem.Parent.MenuItems.Remove(subMenuItem);
                    subMenuItem.Dispose();
                    m_subMenuItem = null;
                }
                if (form != null)
                {
                    Controls.Debugger.FormCpu formCpu = form as Controls.Debugger.FormCpu;
                    formCpu.AllowClose = true;
                    formCpu.Close();
                    m_form = null;
                }
            }
            m_guiData = null;
        }

        private void menu_Click(object sender, EventArgs e)
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                Controls.Debugger.FormCpu form = m_form as Controls.Debugger.FormCpu;
                if (form == null)
                {
                    form = new Controls.Debugger.FormCpu();
                    form.Init(m_target);
                    form.FormClosed += delegate(object obj, System.Windows.Forms.FormClosedEventArgs arg)
                    {
                        m_form = null;
                    };
                    m_form = form;
                    form.Show((System.Windows.Forms.Form)m_guiData.MainWindow);
                }
                else
                {
                    form.Show();
                    form.Activate();
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
