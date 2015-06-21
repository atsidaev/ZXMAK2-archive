using ZXMAK2.Hardware.Circuits;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterRTC : BusDeviceBase
    {
        #region Fields

        private readonly RtcChip m_rtc = new RtcChip(RtcChipType.DS12885);
        private bool m_isSandBox;
        private string m_fileName;

        #endregion Fields


        public SprinterRTC()
        {
            Category = BusDeviceCategory.Other;
            Name = "CMOS SPRINTER";
            Description = "Sprinter RTC";
        }
        

        #region IBusDevice Members

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


        #region Bus

        private void BusReset()
        {
            m_rtc.WriteAddr(0);
        }


        /// <summary>
        /// RTC address port
        /// </summary>
        private void BusWriteAddr(ushort addr, byte val, ref bool handled)
        {
            m_rtc.WriteAddr(val);
        }

        /// <summary>
        /// RTC write data port
        /// </summary>
        private void BusWriteData(ushort addr, byte val, ref bool handled)
        {
            m_rtc.WriteData(val);
        }

        /// <summary>
        /// RTC read data port
        /// </summary>
        private void BusReadData(ushort addr, ref byte val, ref bool handled)
        {
            m_rtc.ReadData(ref val);
        }

        #endregion
    }
}
