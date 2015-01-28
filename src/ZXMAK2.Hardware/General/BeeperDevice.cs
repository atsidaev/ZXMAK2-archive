using System;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine;
using System.Xml;

namespace ZXMAK2.Hardware.General
{
	public class BeeperDevice : SoundDeviceBase
	{
        #region Fields

        private int m_mask;
        private int m_port;
        private int m_bit;
        private int m_bitMask;

        private int _portFE = 0;
        private ushort m_dacValue0 = 0;
        private ushort m_dacValue1 = 0x1FFF;

        #endregion Fields

        
        public BeeperDevice()
        {
            Category = BusDeviceCategory.Sound;
            Name = "BEEPER";
            Description = "Standard Beeper";

            Mask = 0x01;
            Port = 0xFE;
            Bit = 4;
        }

        #region Properties

        public int Mask
        {
            get { return m_mask; }
            set
            {
                m_mask = value;
                OnConfigChanged();
            }
        }

        public int Port
        {
            get { return m_port; }
            set
            {
                m_port = value;
                OnConfigChanged();
            }
        }

        public int Bit
        {
            get { return m_bit; }
            set
            {
                value = value < 0 ? 0 : value;
                value = value > 7 ? 7 : value;
                m_bit = value;
                m_bitMask = 1 << m_bit;
                OnConfigChanged();
            }
        }

        #endregion Properties


        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);

			bmgr.SubscribeWrIo(Mask, Port & Mask, WritePortFE);
		}

		#endregion

		#region Bus Handlers

		protected virtual void WritePortFE(ushort addr, byte value, ref bool iorqge)
		{
			//if (!iorqge)
			//	return;
			//iorqge = false;
			//PortFE = value;
			var maskedValue = value & m_bitMask;
            if (maskedValue == _portFE)
            {
                return;
            }
			_portFE = maskedValue;
			var v = _portFE != 0 ? m_dacValue1 : m_dacValue0;
			UpdateDac(v, v);
		}

		#endregion

		protected override void OnVolumeChanged(int oldVolume, int newVolume)
		{
			m_dacValue0 = 0;
			m_dacValue1 = (ushort)((0x7FFF * newVolume) / 100);
		}

        protected override void OnConfigLoad(XmlNode node)
        {
            base.OnConfigLoad(node);
            Mask = Utils.GetXmlAttributeAsInt32(node, "mask", Mask);
            Port = Utils.GetXmlAttributeAsInt32(node, "port", Port);
            Bit = Utils.GetXmlAttributeAsInt32(node, "bit", Bit);
        }

        protected override void OnConfigSave(XmlNode node)
        {
            base.OnConfigSave(node);
            Utils.SetXmlAttribute(node, "mask", Mask);
            Utils.SetXmlAttribute(node, "port", Port);
            Utils.SetXmlAttribute(node, "bit", Bit);
        }
	}
}
