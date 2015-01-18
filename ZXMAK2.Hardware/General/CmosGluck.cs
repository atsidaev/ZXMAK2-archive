using ZXMAK2.Hardware.Circuits;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.General
{
    public class CmosGluck : BusDeviceBase
    {
        public CmosGluck()
        {
            Category = BusDeviceCategory.Other;
            Name = "CMOS GLUCK";
            Description = "GLUCK CMOS device\r\nPorts:\r\n#DFF7=address (w)\r\n#BFF7=data (r/w)";
        }
        

        #region IBusDevice Members

        public override void BusInit(IBusManager bmgr)
        {
            m_isSandBox = bmgr.IsSandbox;
            bmgr.SubscribeRdIo(0xF008, 0xB000, BusReadData);   // DATA IN
            bmgr.SubscribeWrIo(0xF008, 0xB000, BusWriteData);  // DATA OUT
            bmgr.SubscribeWrIo(0xF008, 0xD000, BusWriteAddr);  // REG

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


        #region Bus Handlers

        private void BusWriteAddr(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            m_rtc.WriteAddr(value);
        }

        private void BusWriteData(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            m_rtc.WriteData(value);
        }

        private void BusReadData(ushort addr, ref byte value, ref bool iorqge)
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
