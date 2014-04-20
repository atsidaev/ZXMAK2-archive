using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Hardware;

namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsJoystick : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private IHost m_host;
        private IJoystickDevice m_device;

        public CtlSettingsJoystick()
        {
            InitializeComponent();
        }

        public void Init(BusManager bmgr, IHost host, IJoystickDevice device)
        {
            m_bmgr = bmgr;
            m_host = host;
            m_device = device;

            cbxType.Items.Clear();
            if (m_host != null && m_host.Joystick != null)
            {
                foreach (var hdi in m_host.Joystick.GetAvailableJoysticks())
                {
                    cbxType.Items.Add(hdi);
                }
            }
            //cbxType.Sorted = true;

            cbxType.SelectedIndex = -1;
            for (var i = 0; i < cbxType.Items.Count; i++)
            {
                var hdi = (HostDeviceInfo)cbxType.Items[i];
                if (m_device.HostId == hdi.HostId)
                {
                    cbxType.SelectedIndex = i;
                    break;
                }
            }
            cbxType_SelectedIndexChanged(this, EventArgs.Empty);
        }

        public override void Apply()
        {
            var hdi = (HostDeviceInfo)cbxType.SelectedItem;
            if (hdi != null)
            {
                m_device.HostId = hdi.HostId;
            }
            else
            {
                m_device.HostId = string.Empty;
            }
            Init(m_bmgr, m_host, m_device);
        }

        private void cbxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var hdi = (HostDeviceInfo)cbxType.SelectedItem;
        }
    }
}
