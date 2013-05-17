using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.IC;
using ZXMAK2.Engine.Z80;
using System.Xml;


namespace ZXMAK2.Hardware.Atm
{
    public class IdeAtm : BusDeviceBase, IConfigurable
    {
        #region Fields

        private bool m_sandbox = false;
        private Z80CPU m_cpu;
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
            m_cpu = bmgr.CPU;
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


        #region IConfigurable

        public void LoadConfig(XmlNode itemNode)
        {
            LogIo = Utils.GetXmlAttributeAsBool(itemNode, "logIo", false);
        }

        public void SaveConfig(XmlNode itemNode)
        {
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

        protected virtual void WriteIde(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            if ((addr & 0x0100) != 0)
            {
                if (LogIo)
                {
                    LogAgent.Info("IDE WR DATA HI: #{0:X2} @ PC=#{1:X4}", value, m_cpu.regs.PC);
                }
                m_ide_wr_hi = value;
                return;
            }
            addr >>= 5;
            addr &= 7;
            if (addr != 0)
            {
                AtaWrite((AtaReg)addr, value);
            }
            else
            {
                var data = value | (m_ide_wr_hi << 8);
                if (LogIo)
                {
                    LogAgent.Info("IDE WR DATA LO: #{0:X2} @ PC=#{1:X4} [#{2:X4}]", value, m_cpu.regs.PC, data);
                }
                m_ata.WriteData((ushort)data);
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
                if (LogIo)
                {
                    LogAgent.Info("IDE RD DATA HI: #{0:X2} @ PC=#{1:X4}", value, m_cpu.regs.PC);
                }
                return;
            }
            addr >>= 5;
            addr &= 7;
            if (addr != 0)
            {
                value = AtaRead((AtaReg)addr);
            }
            else
            {
                var data = m_ata.ReadData();
                m_ide_rd_hi = (byte)(data >> 8);
                value = (byte)data;
                if (LogIo)
                {
                    LogAgent.Info("IDE RD DATA LO: #{0:X2} @ PC=#{1:X4} [#{2:X4}]", value, m_cpu.regs.PC, data);
                }
            }
            // ??value = m_ata.read_intrq() & 0x80
        }

        private void AtaWrite(AtaReg ataReg, byte value)
        {
            if (LogIo)
            {
                LogAgent.Info("IDE WR {0,-13}: #{1:X2} @ PC=#{2:X4}", ataReg, value, m_cpu.regs.PC);
            }
            m_ata.Write(ataReg, value);
        }

        private byte AtaRead(AtaReg ataReg)
        {
            var value = m_ata.Read(ataReg);
            if (LogIo)
            {
                LogAgent.Info("IDE RD {0,-13}: #{1:X2} @ PC=#{2:X4}", ataReg, value, m_cpu.regs.PC);
            }
            return value;
        }

        #endregion Private
    }
}
