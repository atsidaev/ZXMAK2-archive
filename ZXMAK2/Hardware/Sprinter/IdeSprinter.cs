using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;


namespace ZXMAK2.Hardware.Sprinter
{
    public class IdeSprinter : BusDeviceBase
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

        public override string Name { get { return "IDE SPRINTER"; } }
        public override string Description { get { return "SPRINTER IDE controller\r\nPlease edit *.vmide file for configuration settings"; } }
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

            bmgr.SubscribeRdIo(0x00FF, 0x0050, ReadIdeData);
            bmgr.SubscribeWrIo(0x00FF, 0x0050, WriteIdeData);

            bmgr.SubscribeRdIo(0xFEFF, 0x0051, ReadIdeError);
            bmgr.SubscribeWrIo(0xFEFF, 0x0051, WriteIdeError);

            bmgr.SubscribeRdIo(0xFEFF, 0x0052, ReadIdeCounter);
            bmgr.SubscribeWrIo(0xFEFF, 0x0052, WriteIdeCounter);

            bmgr.SubscribeRdIo(0xFEFF, 0x0053, ReadIdeSector);
            bmgr.SubscribeWrIo(0xFEFF, 0x0053, WriteIdeSector);

            bmgr.SubscribeRdIo(0xFEFF, 0x0055, ReadIdeCylHi);
            bmgr.SubscribeWrIo(0xFEFF, 0x0055, WriteIdeCylHi);

            bmgr.SubscribeRdIo(0xFEFF, 0x0054, ReadIdeCylLo);
            bmgr.SubscribeWrIo(0xFEFF, 0x0054, WriteIdeCylLo);

            bmgr.SubscribeRdIo(0xFEFF, 0x4052, ReadIdeControl);
            bmgr.SubscribeWrIo(0xFEFF, 0x4052, WriteIdeControl);
            
            bmgr.SubscribeRdIo(0xFEFF, 0x4053, ReadIdeCommand);
            bmgr.SubscribeWrIo(0xFEFF, 0x4053, WriteIdeCommand);
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

            m_ata.write(1, value);
        }

        protected virtual void ReadIdeError(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(1);
        }

        protected virtual void WriteIdeCounter(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(2, value);
        }

        protected virtual void ReadIdeCounter(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(2);
        }

        protected virtual void WriteIdeSector(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(3, value);
        }

        protected virtual void ReadIdeSector(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(3);
        }

        protected virtual void WriteIdeCylHi(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(4, value);
        }

        protected virtual void ReadIdeCylHi(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(4);
        }

        protected virtual void WriteIdeCylLo(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(5, value);
        }

        protected virtual void ReadIdeCylLo(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(5);
        }

        protected virtual void WriteIdeControl(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(6, value);
        }

        protected virtual void ReadIdeControl(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(6);
        }

        protected virtual void WriteIdeCommand(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            m_ata.write(7, value);
        }

        protected virtual void ReadIdeCommand(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = m_ata.read(7);
        }



        //--
        // ??m_ata.reset();
        // ??value = m_ata.read_intrq() & 0x80
        #endregion Private
    }
}
