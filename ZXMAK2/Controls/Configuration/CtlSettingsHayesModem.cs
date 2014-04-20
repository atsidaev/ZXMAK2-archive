using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using ZXMAK2.Engine;
using ZXMAK2.Hardware.General;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Controls.Configuration
{
    public partial class CtlSettingsHayesModem : ConfigScreenControl
    {
        private BusManager m_bmgr;
        private HayesModem m_modem;

        public CtlSettingsHayesModem()
        {
            InitializeComponent();
        }

        public void Init(BusManager bmgr, IHost host, HayesModem modem)
        {
            m_bmgr = bmgr;
            m_modem = modem;

            BindComPorts();
        }

        public void BindComPorts()
        {
            cmbPorts.Items.Clear();
            cmbPorts.Items.Add("NONE");

            cmbPorts.SelectedIndex = 0;

            var ports = System.IO.Ports.SerialPort.GetPortNames();
            foreach (var port in ports)
                cmbPorts.Items.Add(port);

            if (!String.IsNullOrEmpty(m_modem.PortName) && cmbPorts.Items.Contains(m_modem.PortName))
                cmbPorts.SelectedIndex = cmbPorts.Items.IndexOf(m_modem.PortName);
        }

        public override void Apply()
        {
            if (cmbPorts.SelectedIndex == 0)
                m_modem.PortName = String.Empty;
            else
                m_modem.PortName = cmbPorts.SelectedItem.ToString();
        }
    }
}
