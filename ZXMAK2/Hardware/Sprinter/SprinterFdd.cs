using System.Xml;
using ZXMAK2.Model.Disk;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Hardware.Circuits.Fdd;
using ZXMAK2.Serializers;
using ZXMAK2.Resources;


namespace ZXMAK2.Hardware.Sprinter
{
    public class SprinterFdd : BusDeviceBase, IBetaDiskDevice
    {
        #region Fields

        private bool m_sandbox = false;
        private IconDescriptor m_iconRd = new IconDescriptor("FDDRD", ImageResources.FddRd_128x128);
        private IconDescriptor m_iconWr = new IconDescriptor("FDDWR", ImageResources.FddWr_128x128);
        protected CpuUnit m_cpu;
        protected IMemoryDevice m_memory;
        protected Wd1793 m_wd = new Wd1793(2);

        private byte m_bdimode;
        
        #endregion

        public SprinterFdd()
        {
            LoadManager = new DiskLoadManager(m_wd.FDD[0]);
        }



        #region Properties

        public ISerializeManager LoadManager { get; private set; }

        public bool OpenPorts { get; set; }

        #endregion


        #region BusDeviceBase

        public override string Description { get { return "Sprinter FDD controller WD1793"; } }
        public override string Name { get { return "FDD SPRINTER"; } }
        public override BusDeviceCategory Category {  get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_cpu = bmgr.CPU;
            m_sandbox = bmgr.IsSandbox;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            bmgr.RegisterIcon(m_iconRd);
            bmgr.RegisterIcon(m_iconWr);
            bmgr.SubscribeBeginFrame(BusBeginFrame);
            bmgr.SubscribeEndFrame(BusEndFrame);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRam);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRam);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRam);

            OnSubscribeIo(bmgr);

            bmgr.SubscribeReset(BusReset);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);

            foreach (var fs in LoadManager.GetSerializers())
            {
                bmgr.AddSerializer(fs);
            }
        }

        public override void BusConnect()
        {
            if (!m_sandbox)
            {
                foreach (DiskImage di in FDD)
                    di.Connect();
            }
        }

        public override void BusDisconnect()
        {
            if (!m_sandbox)
            {
                foreach (DiskImage di in FDD)
                    di.Disconnect();
            }
            if (m_memory != null)
            {
                m_memory.DOSEN = false;
            }
        }

        protected override void OnConfigLoad(XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            NoDelay = Utils.GetXmlAttributeAsBool(itemNode, "noDelay", false);
            LogIo = Utils.GetXmlAttributeAsBool(itemNode, "logIo", false);
            for (var i = 0; i < m_wd.FDD.Length; i++)
            {
                var inserted = false;
                var readOnly = true;
                var fileName = string.Empty;
                var node = itemNode.SelectSingleNode(string.Format("Drive[@index='{0}']", i));
                if (node != null)
                {
                    inserted = Utils.GetXmlAttributeAsBool(node, "inserted", inserted);
                    readOnly = Utils.GetXmlAttributeAsBool(node, "readOnly", readOnly);
                    fileName = Utils.GetXmlAttributeAsString(node, "fileName", fileName);
                }
                // will be opened on Connect
                m_wd.FDD[i].FileName = fileName;
                m_wd.FDD[i].IsWP = readOnly;
                m_wd.FDD[i].Present = inserted;
            }
        }

        protected override void OnConfigSave(XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "noDelay", NoDelay);
            Utils.SetXmlAttribute(itemNode, "logIo", LogIo);
            for (var i = 0; i < m_wd.FDD.Length; i++)
            {
                if (m_wd.FDD[i].Present)
                {
                    XmlNode xn = itemNode.AppendChild(itemNode.OwnerDocument.CreateElement("Drive"));
                    Utils.SetXmlAttribute(xn, "index", i);
                    Utils.SetXmlAttribute(xn, "inserted", m_wd.FDD[i].Present);
                    Utils.SetXmlAttribute(xn, "readOnly", m_wd.FDD[i].IsWP);
                    if (!string.IsNullOrEmpty(m_wd.FDD[i].FileName))
                    {
                        Utils.SetXmlAttribute(xn, "fileName", m_wd.FDD[i].FileName);
                    }
                }
            }
        }

        #endregion


        #region IBetaDiskInterface

        public bool DOSEN
        {
            get { return m_memory.DOSEN; }
            set { m_memory.DOSEN = value; }
        }

        public DiskImage[] FDD
        {
            get { return m_wd.FDD; }
        }

        public bool NoDelay
        {
            get { return m_wd.NoDelay; }
            set { m_wd.NoDelay = value; }
        }

        public bool LogIo { get; set; }

        #endregion

        #region Private

        protected virtual void OnSubscribeIo(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x83, 0x1F & 0x83, BusWritePortFdc);
            bmgr.SubscribeRdIo(0x83, 0x1F & 0x83, BusReadPortFdc);
            bmgr.SubscribeWrIo(0x83, 0xFF & 0x83, BusWritePortSys);
            bmgr.SubscribeRdIo(0x83, 0xFF & 0x83, BusReadPortSys);
            bmgr.SubscribeWrIo(0xFF, 0xBD, BusWriteBDIMode);
        }

        protected virtual void BusBeginFrame()
        {
            m_wd.LedRd = false;
            m_wd.LedWr = false;
        }

        protected virtual void BusEndFrame()
        {
            m_iconWr.Visible = m_wd.LedWr;
            m_iconRd.Visible = !m_wd.LedWr && m_wd.LedRd;
        }

        protected virtual void BusReadPortFdc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge && (this.DOSEN || this.OpenPorts))
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                value = m_wd.Read(m_cpu.Tact, (WD93REG)fdcReg);
                LogIoRead(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }

        protected virtual void BusReadPortSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge && (this.DOSEN || this.OpenPorts))
            {
                iorqge = false;
                value = m_wd.Read(m_cpu.Tact, WD93REG.SYS);
                LogIoRead(m_cpu.Tact, WD93REG.SYS, value);
            }
        }


        protected virtual void BusWriteBDIMode(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge && (this.DOSEN || this.OpenPorts))
            {
                iorqge = false;
                //0x21 - Set 1440 
                //0x01 - Set  720
                m_bdimode = value;
                
                if (LogIo)
                {
                    Logger.Debug(
                        "WD93 BDI MODE <== #{0:X2} [PC=#{1:X4}, T={2}]",
                        value,
                        m_cpu.regs.PC,
                        m_cpu.Tact);
                }
            }
        }


        protected virtual void BusWritePortFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge && (this.DOSEN || this.OpenPorts))
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                LogIoWrite(m_cpu.Tact, (WD93REG)fdcReg, value);
                m_wd.Write(m_cpu.Tact, (WD93REG)fdcReg, value);
            }
        }


        protected virtual void BusWritePortSys(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge && (this.DOSEN || this.OpenPorts))
            {
                iorqge = false;
                LogIoWrite(m_cpu.Tact, WD93REG.SYS, value);
                m_wd.Write(m_cpu.Tact, WD93REG.SYS, value);
            }
        }

        protected virtual void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (m_memory.IsRom48)
            {
                DOSEN = true;
            }
        }

        protected virtual void BusReadMemRam(ushort addr, ref byte value)
        {
            if (DOSEN)
            {
                DOSEN = false;
            }
        }

        protected virtual void BusReset()
        {
            DOSEN = false;
            m_bdimode = 1; // Возможно не верное значение
        }

        protected virtual void BusNmiRq(BusCancelArgs e)
        {
            e.Cancel = DOSEN;
        }

        protected virtual void BusNmiAck()
        {
            DOSEN = true;
        }

        protected void LogIoWrite(long tact, WD93REG reg, byte value)
        {
            if (LogIo)
            {
                Logger.Debug(
                    "WD93 {0} <== #{1:X2} [PC=#{2:X4}, T={3}]",
                    reg,
                    value,
                    m_cpu.regs.PC,
                    tact);
            }
        }

        protected void LogIoRead(long tact, WD93REG reg, byte value)
        {
            if (LogIo)
            {
                Logger.Debug(
                    "WD93 {0} ==> #{1:X2} [PC=#{2:X4}, T={3}]",
                    reg,
                    value,
                    m_cpu.regs.PC,
                    tact);
            }
        }

        #endregion
    }
}
