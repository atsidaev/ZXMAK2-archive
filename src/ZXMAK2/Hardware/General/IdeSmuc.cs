using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;


namespace ZXMAK2.Hardware.General
{
    /// <summary>
    /// http://zx.pk.ru/attachment.php?attachmentid=13640&d=1254911208
    /// </summary>
    public class IdeSmuc : BusDeviceBase
    {
        #region Fields

        private bool m_sandbox = false;
        private IconDescriptor m_iconHdd = new IconDescriptor("HDD", Utils.GetIconStream("hdd.png"));
        private IMemoryDevice m_memory = null;
        private AtaPort m_ata = new AtaPort();
        private RtcChip m_rtc = new RtcChip(RtcChipType.DS12885);
        private NvramChip m_nvram = new NvramChip();
        private string m_rtcFileName;
        private string m_nvramFileName;
        private string m_ideFileName;
        private byte m_ide_rd_hi;
        private byte m_ide_wr_hi;
        private byte m_sys;
        private byte m_fdd;

        #endregion Fields


        #region IBusDevice Members

        public override string Name { get { return "IDE SMUC"; } }
        public override string Description { get { return "Spectrum Multi Unit Controller"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_sandbox = bmgr.IsSandbox;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            m_rtcFileName = bmgr.GetSatelliteFileName("cmos");
            m_nvramFileName = bmgr.GetSatelliteFileName("nvram");
            m_ideFileName = bmgr.GetSatelliteFileName("vmide");

            bmgr.RegisterIcon(m_iconHdd);
            bmgr.SubscribeBeginFrame(BusBeginFrame);
            bmgr.SubscribeEndFrame(BusEndFrame);

            bmgr.SubscribeReset(BusReset);

            const int mask = 0xB8E7;//0xA044;
            bmgr.SubscribeRdIo(mask, 0x5FBA & mask, ReadVer);
            bmgr.SubscribeRdIo(mask, 0x5FBE & mask, ReadRev);
            bmgr.SubscribeRdIo(mask, 0x7FBA & mask, ReadFdd);
            bmgr.SubscribeRdIo(mask, 0x7FBE & mask, ReadPic);
            bmgr.SubscribeRdIo(mask, 0xDFBA & mask, ReadRtc);
            bmgr.SubscribeRdIo(mask, 0xD8BE & mask, ReadIdeHi);
            bmgr.SubscribeRdIo(mask, 0xFFBA & mask, ReadSys);
            bmgr.SubscribeRdIo(mask, 0xFFBE & mask, ReadIde);

            bmgr.SubscribeWrIo(mask, 0x7FBA & mask, WriteFdd);
            bmgr.SubscribeWrIo(mask, 0xDFBA & mask, WriteRtc);
            bmgr.SubscribeWrIo(mask, 0xD8BE & mask, WriteIdeHi);
            bmgr.SubscribeWrIo(mask, 0xFFBA & mask, WriteSys);
            bmgr.SubscribeWrIo(mask, 0xFFBE & mask, WriteIde);
        }

        public override void BusConnect()
        {
            if (!m_sandbox)
            {
                if (m_rtcFileName != null)
                    m_rtc.Load(m_rtcFileName);
                if (m_nvramFileName != null)
                    m_nvram.Load(m_nvramFileName);
                if (m_ideFileName != null)
                    m_ata.Load(m_ideFileName);
            }
        }

        public override void BusDisconnect()
        {
            if (!m_sandbox)
            {
                if (m_rtcFileName != null)
                    m_rtc.Save(m_rtcFileName);
                if (m_nvramFileName != null)
                    m_nvram.Save(m_nvramFileName);
            }
        }

        #endregion


        #region Private

        protected virtual void BusBeginFrame()
        {
            m_ata.LedIo = false;
        }

        protected virtual void BusEndFrame()
        {
            m_iconHdd.Visible = m_ata.LedIo;
        }

        protected virtual void BusReset()
        {
            m_rtc.Reset();
            m_sys = 0x00;
        }

        #endregion Private


        #region Read I/O

        protected virtual void ReadVer(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            value = 0x57;//0x3F;	  // D7,D6,D5,D3 (see table, there is no direct encoding)
        }

        protected virtual void ReadRev(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            value = 0x17;//0x57;
        }

        protected virtual void ReadFdd(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            // D7=0 => fdd A virtual
            // D6=0 => fdd B virtual
            // D3=0 => HDD present
            value = (byte)(m_fdd | 0x37);	// us |= 0x3F
        }

        protected virtual void ReadPic(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            int ab0 = (addr >> 8) & 1;
            // not installed
            value = 0x57; // ???
        }

        protected virtual void ReadRtc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            if ((m_sys & 0x80) == 0)
                m_rtc.ReadData(ref value);
        }

        protected virtual void ReadIdeHi(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            value = m_ide_rd_hi;
        }

        protected virtual void ReadSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            value = m_nvram.Read();
            value &= 0x7F;
            value |= (byte)(m_ata.read_intrq() & 0x80);
        }

        protected virtual void ReadIde(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            int ab = (addr >> 8) & 7;

            if ((m_sys & 0x80) == 0)
            {
                if (ab == 0)
                {
                    UInt16 rd = m_ata.ReadData();
                    m_ide_rd_hi = (byte)(rd >> 8);
                    value = (byte)rd;
                }
                else
                {
                    value = m_ata.Read((AtaReg)ab);
                }
            }
            else if (/*ab==6*/ (ab & 1) == 0)
            {
                value = m_ata.Read(AtaReg.ControlAltStatus);
            }
        }

        #endregion

        
        #region Write I/O

        protected virtual void WriteFdd(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            m_fdd = value;
        }

        protected virtual void WriteRtc(ushort addr, byte value, ref bool iorqge)
        {
            if (!m_memory.DOSEN)
                return;
            iorqge = false;
            if ((m_sys & 0x80) == 0)
                m_rtc.WriteAddr(value);
            else
                m_rtc.WriteData(value);
        }

        protected virtual void WriteIdeHi(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;

            m_ide_wr_hi = value;
        }

        protected virtual void WriteSys(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;

            if ((value & 1) != 0)
                m_ata.Reset();
            m_nvram.Write(value);
            m_sys = value;
        }

        protected virtual void WriteIde(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge || !m_memory.DOSEN)
                return;
            iorqge = false;
            int ab = (addr >> 8) & 7;

            if ((m_sys & 0x80) == 0)
            {
                if (ab == 0)
                {
                    m_ata.WriteData((UInt16)((m_ide_wr_hi << 8) | value));
                }
                else
                {
                    m_ata.Write((AtaReg)ab, value);
                }
            }
            else if (/*ab==6*/ (ab & 1) == 0)
            {
                m_ata.Write(AtaReg.ControlAltStatus, value);
            }
        }

        #endregion
    }
}
