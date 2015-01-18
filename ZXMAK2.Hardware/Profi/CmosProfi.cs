﻿using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Hardware.Circuits;


namespace ZXMAK2.Hardware.Profi
{
    public class CmosProfi : BusDeviceBase
    {
        #region Fields

        private bool m_isSandBox;
        private IMemoryDevice m_memory;
        private RtcChip m_rtc = new RtcChip(RtcChipType.DS12885);
        private string m_fileName = null;

        #endregion Fields


        public CmosProfi()
        {
            Category = BusDeviceCategory.Other;
            Name = "CMOS PROFI";
            Description = "PROFI CMOS device\nPort:\t#9F";
        }


        #region IBusDevice Members

        public override void BusInit(IBusManager bmgr)
        {
            m_isSandBox = bmgr.IsSandbox;
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            bmgr.SubscribeWrIo(0x009F, 0x009F, BusWriteRtc);
            bmgr.SubscribeRdIo(0x009F, 0x009F, BusReadRtc);

            m_fileName = bmgr.GetSatelliteFileName("cmos");
        }

        public override void BusConnect()
        {
            if (!m_isSandBox && m_fileName != null)
            {
                m_rtc.Load(m_fileName);
            }
        }

        public override void BusDisconnect()
        {
            if (!m_isSandBox && m_fileName != null)
            {
                m_rtc.Save(m_fileName);
            }
        }

        #endregion


        #region Bus Handlers

        protected virtual bool IsExtendedMode
        {
            get
            {
                var cpm = (m_memory.CMR1 & 0x20) != 0;
                var rom48 = (m_memory.CMR0 & 0x10) != 0;
                var csExtended = cpm && rom48;
                return csExtended;
            }
        }

        protected virtual void BusWriteRtc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge || !IsExtendedMode)
            {
                return;
            }

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

        protected virtual void BusReadRtc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !IsExtendedMode)
            {
                return;
            }

            if ((addr & 0x20) == 0)
            {
                iorqge = false;
                m_rtc.ReadData(ref value);
            }
        }

        #endregion
    }
}