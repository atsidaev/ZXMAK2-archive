using System;
using System.Xml;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Entities;
using ZXMAK2.Hardware.Circuits;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Resources;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.Evo
{
    public class IdePentEvo : BusDeviceBase
    {
        #region Fields

        private bool m_sandbox = false;
        private CpuUnit m_cpu;
        private IconDescriptor m_iconHdd = new IconDescriptor("HDD", ImageResources.HddRd_128x128);
        private AtaPort m_ata = new AtaPort();
        private string m_ideFileName;
        private int m_ide_write;
        private int m_ide_hi_byte_w;
        private int m_ide_hi_byte_w1;
        private int m_ide_hi_byte_r;
        private int m_ide_read;

        #endregion Fields


        #region IBusDevice Members

        public override string Name { get { return "IDE PentEvo"; } }
        public override string Description { get { return "PentEvo IDE controller\r\nPlease edit *.vmide file for configuration settings"; } }
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

            bmgr.SubscribeRdIo(0x1E, 0x10, ReadIde);
            bmgr.SubscribeWrIo(0x1E, 0x10, WriteIde);

            bmgr.SubscribeRdIo(0xFF, 0xC8, ReadIdeAltStatus);
            bmgr.SubscribeWrIo(0xFF, 0xC8, WriteIdeAltStatus);
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

        protected virtual void WriteIde(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            if ((addr & 0xFF) == 0x11)
            {
                m_ide_write = value;
                m_ide_hi_byte_w = 0;
                m_ide_hi_byte_w1 = 1;
                return;
            }
            var data = value;
            if ((addr & 0xFE) == 0x10)
            {
                m_ide_hi_byte_w ^= 1;

                if (m_ide_hi_byte_w1 != 0) // Была запись в порт 0x11 (старший байт уже запомнен)
                {
                    m_ide_hi_byte_w1 = 0;
                }
                else
                {
                    if (m_ide_hi_byte_w != 0) // Запоминаем младший байт
                    {
                        m_ide_write = data;
                        return;
                    }
                    else // Меняем старший и младший байты местами (как этого ожидает write_hdd_5)
                    {
                        var tmp = (byte)m_ide_write;
                        m_ide_write = data;
                        data = tmp;
                    }
                }
            }
            else
            {
                m_ide_hi_byte_w = 0;
            }
            addr >>= 5;
            addr &= 7;
            if (addr != 0)
            {
                AtaWrite((AtaReg)addr, data);
            }
            else
            {
                var dataWord = (ushort)(data | (m_ide_write << 8));
                if (LogIo)
                {
                    Logger.Info("IDE WR DATA: #{0:X4} @ PC=#{1:X4}", dataWord, m_cpu.regs.PC);
                }
                m_ata.WriteData(dataWord);
            }
        }

        protected virtual void ReadIde(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;
            
            if ((addr & 0xFF) == 0x11)
            {
                m_ide_hi_byte_r = 0;
                value = (byte)m_ide_read;
                return;
            }

            if ((addr & 0xFE) == 0x10)
            {
                m_ide_hi_byte_r ^= 1;
                if (m_ide_hi_byte_r==0)
                {
                    value = (byte)m_ide_read;
                    return;
                }
            }
            else
            {
                m_ide_hi_byte_r = 0;
            }
            addr >>= 5;
            addr &= 7;
            if (addr != 0)
            {
                value = AtaRead((AtaReg)addr);
                return;
            }
            var dataWord = m_ata.ReadData();
            if (LogIo)
            {
                Logger.Info("IDE RD DATA: #{0:X4} @ PC=#{1:X4}", dataWord, m_cpu.regs.PC);
            }
            m_ide_read = (dataWord >> 8) & 0xFF;
            value = (byte)dataWord;
        }

        protected virtual void WriteIdeAltStatus(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            AtaWrite(AtaReg.ControlAltStatus, value);
        }

        protected virtual void ReadIdeAltStatus(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
            {
                return;
            }
            iorqge = false;

            value = AtaRead(AtaReg.ControlAltStatus);
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
