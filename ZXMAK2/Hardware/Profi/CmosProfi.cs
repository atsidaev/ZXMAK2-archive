using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;

namespace ZXMAK2.Hardware.Profi
{
    public class CmosProfi : BusDeviceBase
    {
        #region IBusDevice Members

        public override string Name { get { return "PROFI CMOS"; } }
        public override string Description { get { return "PROFI CMOS device\nPort:\t#9F"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            bmgr.SubscribeWrIo(0x009F, 0x009F, BusWriteRtc);
            bmgr.SubscribeRdIo(0x009F, 0x009F, BusReadRtc);

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

        private void BusWriteRtc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            if (rom48 && cpm)
            {
                iorqge = false;
                if ((addr & 0x20) != 0)
                {
                    m_rtc.WriteAddr(value);
                }
                else
                {
                    m_rtc.WriteData(value);
                }
            }
        }

        private void BusReadRtc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            bool rom48 = (m_memory.CMR0 & 0x10) != 0;
            bool cpm = (m_memory.CMR1 & 0x20) != 0;
            if (rom48 && cpm && (addr & 0x20) == 0)
            {
                iorqge = false;
                m_rtc.ReadData(ref value);
            }
        }

        #endregion
    }
}
