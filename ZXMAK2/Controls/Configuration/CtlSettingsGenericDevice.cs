using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ZXMAK2.Engine;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsGenericDevice : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private BusDeviceBase m_device;

        public CtlSettingsGenericDevice()
        {
            InitializeComponent();
        }

        public void Init(BusManager bmgr, BusDeviceBase device)
        {
            m_bmgr = bmgr;
            m_device = device;
            txtDevice.Text = device.Name;
            txtDescription.Text = device.Description.Replace("\n", Environment.NewLine);
        }

        public string DeviceName { get { return m_device.Description; } }
        public string DeviceType { get { return m_device.Name; } }
        
        public override void Apply()
        {
        }
    }
}
