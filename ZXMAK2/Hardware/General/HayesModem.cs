// Hayes modem connected using Kondratyev scheme. 
// Only ZXMC/ZXMC2 features are emulated.
// (C) Alexander Tsidaev, 2013

using System;
using System.Collections.Generic;
using ZXMAK2;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.General
{
    public class HayesModem : BusDeviceBase
    {
        #region Public

        public string PortName { get; set; }

        #endregion

        #region private

        IBusManager m_bmgr;
        System.IO.Ports.SerialPort port;
        
        const byte DLAB = 7; // 7th bit of REG_LINE_CTRL

        int REG_LINE_CTRL = 0x03; // 8N1
        int REG_DIV = 0;
        int REG_IRQ_ENABLE = 0;

        bool RTS = false;

        void UpdateSerialPortParameters()
        {
            if (port != null)
            {
                try
                {
                    if (REG_DIV > 0)
                        port.BaudRate = 115200 / REG_DIV;
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        #endregion

        #region BusDeviceBase

        public override string Name { get { return "HAYES MODEM"; } }
        public override string Description { get { return "Hayes Modem connected using Kondratyev's scheme"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_bmgr = bmgr;

            bmgr.SubscribeWrIo(0xFFFF, 0xF8EF, writeREG_IO);
            bmgr.SubscribeRdIo(0xFFFF, 0xF8EF, readREG_IO);
            bmgr.SubscribeWrIo(0xFFFF, 0xF8EF + 0x100, writeREG_IRQ_ENABLE);
            bmgr.SubscribeWrIo(0xFFFF, 0xF8EF + 0x300, writeREG_LINE_CTRL);
            bmgr.SubscribeWrIo(0xFFFF, 0xF8EF + 0x400, writeREG_MDM_CTRL);
            bmgr.SubscribeRdIo(0xFFFF, 0xF8EF + 0x500, readREG_LINE_STATUS);
            bmgr.SubscribeRdIo(0xFFFF, 0xF8EF + 0x600, readREG_MDM_STATUS);
        }

        public override void BusConnect()
        {
            if (!m_bmgr.IsSandbox && !String.IsNullOrEmpty(PortName))
            {
                try
                {
                    port = new System.IO.Ports.SerialPort(PortName);
                    port.RtsEnable = true;
                    port.Open();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    port = null;
                }
            }
        }

        public override void BusDisconnect()
        {
            if (!m_bmgr.IsSandbox && port != null && port.IsOpen)
                port.Close();
        }

        #endregion

        #region Hayes modem port activity

        private void readREG_IO(ushort addr, ref byte value, ref bool iorqge)
        {
            value = 0;
            if (port != null && port.IsOpen && port.BytesToRead != 0)
            {
                var result = port.ReadByte();
                value = (byte)(result == -1 ? 0 : result);
            }
        }

        private void writeREG_IO(ushort addr, byte value, ref bool iorqge)
        {
            if ((REG_LINE_CTRL & (1 << DLAB)) > 0)
            {
                REG_DIV &= 0xFF00; // clear REG_DIV_H
                REG_DIV |= value; // store REG_DIV_H
                UpdateSerialPortParameters();
            }
            else
            {
                if (port != null && port.IsOpen)
                    port.Write(new byte[] { value }, 0, 1);
            }
        }

        private void readREG_LINE_STATUS(ushort addr, ref byte value, ref bool iorqge)
        {
            value = 0;

            if (port != null && port.IsOpen)
            {
                if (port.BytesToRead > 0)
                    value |= 0x01;

                if (port.BytesToWrite == 0)
                    value |= 0x60;
            }
        }

        private void readREG_MDM_STATUS(ushort addr, ref byte value, ref bool iorqge)
        {
            value = 0;
            
            if (port != null && port.IsOpen && port.BytesToWrite == 0)
                value |= 0x10;
        }

        private void writeREG_MDM_CTRL(ushort addr, byte value, ref bool iorqge)
        {
            RTS = ((value & 2) >> 1) != 0;
        }

        private void writeREG_LINE_CTRL(ushort addr, byte value, ref bool iorqge)
        {
            // Can be used for setting DLAB only
            REG_LINE_CTRL = value;
        }

        private void writeREG_IRQ_ENABLE(ushort addr, byte value, ref bool iorqge)
        {
            if ((REG_LINE_CTRL & (1 << DLAB)) > 0)
            {
                REG_DIV &= 0xFF; // clear REG_DIV_H
                REG_DIV |= (value << 8); // store REG_DIV_H
                UpdateSerialPortParameters();
            }
            else
                REG_IRQ_ENABLE = value; // Not used in ZXMC
        }

        #endregion

        protected override void OnConfigLoad(System.Xml.XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            PortName = Utils.GetXmlAttributeAsString(itemNode, "PortName", String.Empty);
        }

        protected override void OnConfigSave(System.Xml.XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "PortName", PortName);
        }
    }
}
