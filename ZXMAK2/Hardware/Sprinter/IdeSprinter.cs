using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;
using ZXMAK2.Engine.Z80;


namespace ZXMAK2.Hardware.Sprinter
{
    public class IdeSprinter : BusDeviceBase
    {
        #region Fields

        private bool m_sandbox = false;
        private IconDescriptor m_iconHdd = new IconDescriptor("HDD", Utils.GetIconStream("hdd.png"));
        private AtaPort m_ata = new AtaPort();
        private string m_ideFileName;
        private byte m_ide_wr_hi;
        private byte m_ide_rd_hi;

        #endregion Fields


        #region IBusDevice Members

        public override string Name { get { return "IDE SPRINTER"; } }
        public override string Description { get { return "SPRINTER IDE controller\r\nPlease edit *.vmide file for configuration settings"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_sandbox = bmgr.IsSandbox;

            m_ideFileName = bmgr.GetSatelliteFileName("vmide");

            bmgr.RegisterIcon(m_iconHdd);
            bmgr.SubscribeBeginFrame(BusBeginFrame);
            bmgr.SubscribeEndFrame(BusEndFrame);

            bmgr.SubscribeReset(BusReset);

            var dataMask = 0x00E7;//0x00FF;
            var regMask = 0xE1E7;//0x00E7;//0xFEFF;
            bmgr.SubscribeRdIo(dataMask, 0x0050 & dataMask, ReadIdeData);
            bmgr.SubscribeWrIo(dataMask, 0x0050 & dataMask, WriteIdeData);

            bmgr.SubscribeRdIo(regMask, 0x0051 & regMask, ReadIdeError);
            bmgr.SubscribeWrIo(regMask, 0x0151 & regMask, WriteIdeError);

            bmgr.SubscribeRdIo(regMask, 0x0052 & regMask, ReadIdeCounter);
            bmgr.SubscribeWrIo(regMask, 0x0152 & regMask, WriteIdeCounter);

            bmgr.SubscribeRdIo(regMask, 0x0053 & regMask, ReadIdeSector);
            bmgr.SubscribeWrIo(regMask, 0x0153 & regMask, WriteIdeSector);

            bmgr.SubscribeRdIo(regMask, 0x0055 & regMask, ReadIdeCylHi);
            bmgr.SubscribeWrIo(regMask, 0x0155 & regMask, WriteIdeCylHi);

            bmgr.SubscribeRdIo(regMask, 0x0054 & regMask, ReadIdeCylLo);
            bmgr.SubscribeWrIo(regMask, 0x0154 & regMask, WriteIdeCylLo);

            bmgr.SubscribeRdIo(regMask, 0x4052 & regMask, ReadIdeControl);
            bmgr.SubscribeWrIo(regMask, 0x4152 & regMask, WriteIdeControl);

            bmgr.SubscribeRdIo(regMask, 0x4053 & regMask, ReadIdeCommand);
            bmgr.SubscribeWrIo(regMask, 0x4153 & regMask, WriteIdeCommand);
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


        private const int ATA_REG_ERROR = 1;
        private const int ATA_REG_COUNT = 2;
        private const int ATA_REG_SECTR = 3;
        private const int ATA_REG_CYLLO = 4;
        private const int ATA_REG_CYLHI = 5;
        private const int ATA_REG_CNTRL = 6;
        private const int ATA_REG_COMND = 7;

        protected virtual void WriteIdeData(ushort addr, byte value, ref bool iorqge)
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
            var data = value | (m_ide_wr_hi << 8);
            m_ata.write_data((ushort)data);
        }

        protected virtual void ReadIdeData(ushort addr, ref byte value, ref bool iorqge)
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
            var data = m_ata.read_data();
            m_ide_rd_hi = (byte)(data >> 8);
            value = (byte)data;
        }

        protected virtual void WriteIdeError(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_ERROR, value);
        }

        protected virtual void ReadIdeError(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_ERROR);
        }

        protected virtual void WriteIdeCounter(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_COUNT, value);
        }

        protected virtual void ReadIdeCounter(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_COUNT);
        }

        protected virtual void WriteIdeSector(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_SECTR, value);
        }

        protected virtual void ReadIdeSector(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_SECTR);
        }

        protected virtual void WriteIdeCylHi(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_CYLHI, value);
        }

        protected virtual void ReadIdeCylHi(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_CYLHI);
        }

        protected virtual void WriteIdeCylLo(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_CYLLO, value);
        }

        protected virtual void ReadIdeCylLo(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_CYLLO);
        }

        protected virtual void WriteIdeControl(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_CNTRL, value);
        }

        protected virtual void ReadIdeControl(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_CNTRL);
        }

        protected virtual void WriteIdeCommand(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(ATA_REG_COMND, value);
        }

        protected virtual void ReadIdeCommand(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(ATA_REG_COMND);
        }



        //--
        // ??m_ata.reset();
        // ??value = m_ata.read_intrq() & 0x80
        #endregion Private
    }
}
