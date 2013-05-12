using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;


namespace ZXMAK2.Hardware.Atm
{
    public class IdeAtm : BusDeviceBase
    {
        #region Fields

        private bool m_sandbox = false;
        private IconDescriptor m_iconHdd = new IconDescriptor("HDD", Utils.GetIconStream("hdd.png"));
        private IMemoryDevice m_memory = null;
        private AtaPort m_ata = new AtaPort();
        private string m_ideFileName;
        private byte m_ide_wr_hi;
        private byte m_ide_rd_hi;
        
        #endregion Fields


        #region IBusDevice Members

        public override string Name { get { return "IDE ATM"; } }
        public override string Description { get { return "ATM IDE controller\r\nPlease edit *.vmide file for configuration settings"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_sandbox = bmgr.IsSandbox;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            m_ideFileName = bmgr.GetSatelliteFileName("vmide");

            bmgr.RegisterIcon(m_iconHdd);
            bmgr.SubscribeBeginFrame(BusBeginFrame);
            bmgr.SubscribeEndFrame(BusEndFrame);

            bmgr.SubscribeReset(BusReset);

            const int mask = 0x001F;
            bmgr.SubscribeRdIo(mask, 0xEF & mask, ReadIde);
            bmgr.SubscribeWrIo(mask, 0xEF & mask, WriteIde);
        }

        public override void BusConnect()
        {
            if (!m_sandbox)
            {
                if (m_ideFileName != null)
                {
                    m_ata.Load(m_ideFileName);
                }
            }
        }

        public override void BusDisconnect()
        {
            //if (!m_sandbox)
            //{
            //}
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
        }

        protected virtual void WriteIde(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            if ((addr & 0x0100) != 0)
            {
                m_ide_wr_hi = value;
                return;
            }
            addr >>= 5;
            addr &= 7;
            if (addr != 0)
            {
                m_ata.write(addr, value);
            }
            else
            {
                var data = value | (m_ide_wr_hi << 8);
                m_ata.write_data((ushort)data);
            }
            // ??m_ata.reset();
        }

        protected virtual void ReadIde(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            if ((addr & 0x0100) != 0)
            {
                value = m_ide_rd_hi;
                return;
            }
            addr >>= 5;
            addr &= 7;
            if (addr != 0)
            {
                value = m_ata.read(addr);
            }
            else
            {
                var data = m_ata.read_data();
                m_ide_rd_hi = (byte)(data >> 8);
                value = (byte)data;
            }
            // ??value = m_ata.read_intrq() & 0x80
        }

        #endregion Private
    }
}
