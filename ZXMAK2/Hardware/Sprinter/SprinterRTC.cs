using ZXMAK2.Entities;
using ZXMAK2.Hardware.Circuits;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterRTC : BusDeviceBase
    {
        #region IBusDevice Members

        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }
        public override string Description { get { return "Sprinter RTC"; } }
        public override string Name { get { return "CMOS SPRINTER"; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_isSandBox = bmgr.IsSandbox;
            bmgr.SubscribeReset(BusReset);
            bmgr.SubscribeWrIo(0xFFFF, 0xBFBD, BusWriteData);  //CMOS_DWR
            bmgr.SubscribeWrIo(0xFFFF, 0xDFBD, BusWriteAddr);  //CMOS_AWR
            bmgr.SubscribeRdIo(0xFFFF, 0xFFBD, BusReadData);  //CMOS_DRD

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

        private bool m_isSandBox;
        private RtcChip m_rtc = new RtcChip(RtcChipType.DS12885);
        private string m_fileName = null;


        #region Bus

        private void BusReset()
        {
            m_rtc.WriteAddr(0);
        }


        /// <summary>
        /// RTC address port
        /// </summary>
        private void BusWriteAddr(ushort addr, byte val, ref bool iorqge)
        {
            m_rtc.WriteAddr(val);
        }

        /// <summary>
        /// RTC write data port
        /// </summary>
        private void BusWriteData(ushort addr, byte val, ref bool iorqge)
        {
            m_rtc.WriteData(val);
        }

        /// <summary>
        /// RTC read data port
        /// </summary>
        private void BusReadData(ushort addr, ref byte val, ref bool iorqge)
        {
            m_rtc.ReadData(ref val);
        }

        #endregion
    }
}
