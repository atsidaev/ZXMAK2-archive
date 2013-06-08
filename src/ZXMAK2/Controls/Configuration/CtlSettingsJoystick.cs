using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Hardware;
using ZXMAK2.MDX;

namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsJoystick : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private IJoystickDevice m_device;

        public CtlSettingsJoystick()
        {
            InitializeComponent();
            BindTypeList();
        }

        private void BindTypeList()
        {
            cbxType.Items.Clear();
            foreach (var hdi in DirectJoystick.Select())
            {
                cbxType.Items.Add(hdi);
            }
            //cbxType.Sorted = true;
        }

        public void Init(BusManager bmgr, IJoystickDevice device)
        {
            m_bmgr = bmgr;
            m_device = device;

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
            m_device.HostId = hdi.HostId;
            Init(m_bmgr, m_device);
        }

        private void cbxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //var hdi = (HostDeviceInfo)cbxType.SelectedItem;
        }
    }
}
