using System.Xml;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.Circuits;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Resources;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Hardware.Sprinter
{
    public class IdeSprinter : BusDeviceBase
    {
        #region Fields

        private bool m_sandbox = false;
        private CpuUnit m_cpu;
        private IconDescriptor m_iconHdd = new IconDescriptor("HDD", ImageResources.HddRd_128x128);
        private AtaPort m_ata = new AtaPort();
        private string m_ideFileName;
        private byte m_ide_wr_lo;
        private byte m_ide_rd_hi;

        #endregion Fields

        
        #region IBusDevice Members

        public override string Name { get { return "IDE SPRINTER"; } }
        public override string Description { get { return "SPRINTER IDE controller\r\nPlease edit *.vmide file for configuration settings"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_sandbox = bmgr.IsSandbox;
            m_cpu = bmgr.CPU;

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

        protected override void OnConfigLoad(XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            LogIo = Utils.GetXmlAttributeAsBool(itemNode, "logIo", false);
        }

        protected override void OnConfigSave(XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "logIo", LogIo);
        }

        #endregion

        
        #region Properties

        public bool LogIo 
        {
            get { return m_ata.LogIo; }
            set { m_ata.LogIo = value; } 
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

            if ((addr & 0x0100) == 0)
            {
                m_ide_wr_lo = value;
                if (LogIo)
                {
                    Logger.Info("IDE WR DATA LO: #{0:X2} @ PC=#{1:X4}", value, m_cpu.regs.PC);
                }
                return;
            }
            var data = (value << 8) | m_ide_wr_lo; //value | (m_ide_wr_hi << 8);
            if (LogIo)
            {
                Logger.Info("IDE WR DATA HI: #{0:X2} @ PC=#{1:X4} [{2:X4}]", value, m_cpu.regs.PC, data);
            }
            m_ata.WriteData((ushort)data);
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
                if (LogIo)
                {
                    Logger.Info("IDE RD DATA HI: #{0:X2} @ PC=#{1:X4}", m_ide_rd_hi, m_cpu.regs.PC);
                }
                return;
            }
            var data = m_ata.ReadData();
            m_ide_rd_hi = (byte)(data >> 8);
            value = (byte)data;
            if (LogIo)
            {
                Logger.Info("IDE RD DATA LO: #{0:X2} @ PC=#{1:X4} [#{2:X4}]", value, m_cpu.regs.PC, data);
            }
        }

        protected virtual void WriteIdeError(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            
            AtaWrite(AtaReg.FeatureError, value);
        }

        protected virtual void ReadIdeError(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.FeatureError);
        }

        protected virtual void WriteIdeCounter(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.SectorCount, value);
        }

        protected virtual void ReadIdeCounter(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.SectorCount);
        }

        protected virtual void WriteIdeSector(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.SectorNumber, value);
        }

        protected virtual void ReadIdeSector(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.SectorNumber);
        }

        protected virtual void WriteIdeCylHi(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.CylinderHigh, value);
        }

        protected virtual void ReadIdeCylHi(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.CylinderHigh);
        }

        protected virtual void WriteIdeCylLo(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.CylinderLow, value);
        }

        protected virtual void ReadIdeCylLo(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.CylinderLow);
        }

        protected virtual void WriteIdeControl(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.HeadAndDrive, value);
        }

        protected virtual void ReadIdeControl(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.HeadAndDrive);
        }

        protected virtual void WriteIdeCommand(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.CommandStatus, value);
        }

        protected virtual void ReadIdeCommand(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.CommandStatus);
        }


        private void AtaWrite(AtaReg ataReg, byte value)
        {
            if (LogIo)
            {
                Logger.Info("IDE WR {0,-13}: #{1:X2} @ PC=#{2:X4}", ataReg, value, m_cpu.regs.PC);
            }
            m_ata.Write(ataReg, value);
        }

        private byte AtaRead(AtaReg ataReg)
        {
            var value = m_ata.Read(ataReg);
            if (LogIo)
            {
                Logger.Info("IDE RD {0,-13}: #{1:X2} @ PC=#{2:X4}", ataReg, value, m_cpu.regs.PC);
            }
            return value;
        }

        //--
        // ??m_ata.reset();
        // ??value = m_ata.read_intrq() & 0x80
        #endregion Private
    }
}
