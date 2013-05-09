using System;
using System.IO;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;


namespace ZXMAK2.Hardware.General
{
    public class CmosGluck : BusDeviceBase
    {
        #region IBusDevice Members

        public override string Name { get { return "GLUCK CMOS"; } }
        public override string Description { get { return "GLUCK CMOS device\r\nPorts:\r\n#DFF7=address (w)\r\n#BFF7=data (r/w)"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            bmgr.SubscribeRdIo(0xF008, 0xB000, ReadPortData);   // DATA IN
            bmgr.SubscribeWrIo(0xF008, 0xB000, WritePortData);  // DATA OUT
            bmgr.SubscribeWrIo(0xF008, 0xD000, WritePortAddr);  // REG

            m_fileName = bmgr.GetSatelliteFileName("cmos");
        }

        public override void BusConnect()
        {
            if (m_fileName != null)
            {
                m_rtc.Load(m_fileName);
            }
        }

        public override void BusDisconnect()
        {
            if (m_fileName != null)
            {
                m_rtc.Save(m_fileName);
            }
        }

        #endregion


        private IMemoryDevice m_memory;
        private RtcChip m_rtc = new RtcChip(RtcChipType.DS12885);
        private string m_fileName = null;


        #region Bus Handlers

        private void WritePortAddr(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            m_rtc.WriteAddr(value);
        }

        private void WritePortData(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            m_rtc.WriteData(value);
        }

        private void ReadPortData(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            m_rtc.ReadData(ref value);
        }

        #endregion
    }
}
