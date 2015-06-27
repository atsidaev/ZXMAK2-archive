/// Description: AY8910 emulator
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;
using System.Xml;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Hardware.Circuits.Sound;
using ZXMAK2.Dependency;


namespace ZXMAK2.Hardware.General
{
    public class AY8910 : SoundDeviceBase, IPsgDevice
    {
        #region Fields

        private readonly IPsgChip m_chip;
        private readonly PsgPortState m_iraState = new PsgPortState(0xFF);
        private readonly PsgPortState m_irbState = new PsgPortState(0xFF);

        private int m_maskAddrReg;
        private int m_portAddrReg;
        private int m_maskDataReg;
        private int m_portDataReg;
        private double m_lastTime;

        #endregion Fields


        public AY8910()
        {
            m_chip = Locator.Resolve<IPsgChip>();
            m_chip.UpdateHandler = UpdateDac;
            m_chip.Volume = Volume;

            Category = BusDeviceCategory.Music;
            Name = "AY8910";
            Description = "Standard AY8910 Programmable Sound Generator";

            MaskAddrReg = 0xC0FF;       // for compatibility (Quorum for example)
            MaskDataReg = 0xC0FF;       // for compatibility (Quorum for example)
            PortAddrReg = 0xFFFD;
            PortDataReg = 0xBFFD;
        }


        #region Public

        public int MaskAddrReg
        {
            get { return m_maskAddrReg; }
            set
            {
                m_maskAddrReg = value;
                OnConfigChanged();
            }
        }

        public int PortAddrReg
        {
            get { return m_portAddrReg; }
            set
            {
                m_portAddrReg = value;
                OnConfigChanged();
            }
        }

        public int MaskDataReg
        {
            get { return m_maskDataReg; }
            set
            {
                m_maskDataReg = value;
                OnConfigChanged();
            }
        }

        public int PortDataReg
        {
            get { return m_portDataReg; }
            set
            {
                m_portDataReg = value;
                OnConfigChanged();
            }
        }

        public byte RegAddr
        {
            get { return m_chip.RegAddr; }
            set { m_chip.RegAddr = value; }
        }

        public byte GetReg(int index)
        {
            return m_chip.GetReg(index);
        }

        public void SetReg(int index, byte value)
        {
            m_chip.SetReg(index, value);
            index &= 0x0F;
            if (index == PsgRegId.IRA)
            {
                OnWriteIra(value);
            }
            else if (index == PsgRegId.IRB)
            {
                OnWriteIrb(value);
            }
        }

        public event Action<IPsgDevice, PsgPortState> IraHandler;
        public event Action<IPsgDevice, PsgPortState> IrbHandler;

        #endregion Public


        #region SoundDeviceBase

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_lastTime = 0D;
            bmgr.Events.SubscribeWrIo(MaskAddrReg, PortAddrReg & MaskAddrReg, WritePortAddr);   // #FFFD (reg#)
            bmgr.Events.SubscribeRdIo(MaskAddrReg, PortAddrReg & MaskAddrReg, ReadPortData);    // #FFFD (rd data/reg#)
            bmgr.Events.SubscribeWrIo(MaskDataReg, PortDataReg & MaskDataReg, WritePortData);   // #BFFD (data)
            bmgr.Events.SubscribeReset(Bus_OnReset);
        }

        protected override void OnEndFrame()
        {
            m_lastTime = m_chip.Update(m_lastTime, 1D);
            if (m_lastTime >= 1D)
            {
                m_lastTime -= Math.Floor(m_lastTime);
            }
            else
            {
                Logger.Warn("EndFrame: m_lastTime={0:F9}", m_lastTime);
            }
            base.OnEndFrame();
        }

        protected override void OnConfigLoad(XmlNode node)
        {
            base.OnConfigLoad(node);
            m_chip.ChipFrequency = Utils.GetXmlAttributeAsInt32(node, "frequency", m_chip.ChipFrequency);
            m_chip.AmpType = Utils.GetXmlAttributeAsEnum<AmpType>(node, "ampType", m_chip.AmpType);
            m_chip.PanType = Utils.GetXmlAttributeAsEnum<PanType>(node, "panType", m_chip.PanType);
            MaskAddrReg = Utils.GetXmlAttributeAsInt32(node, "maskAddrReg", MaskAddrReg);
            MaskDataReg = Utils.GetXmlAttributeAsInt32(node, "maskDataReg", MaskDataReg);
            PortAddrReg = Utils.GetXmlAttributeAsInt32(node, "portAddrReg", PortAddrReg);
            PortDataReg = Utils.GetXmlAttributeAsInt32(node, "portDataReg", PortDataReg);
        }

        protected override void OnConfigSave(XmlNode node)
        {
            base.OnConfigSave(node);
            Utils.SetXmlAttribute(node, "frequency", m_chip.ChipFrequency);
            Utils.SetXmlAttributeAsEnum(node, "ampType", m_chip.AmpType);
            Utils.SetXmlAttributeAsEnum(node, "panType", m_chip.PanType);
            Utils.SetXmlAttribute(node, "maskAddrReg", MaskAddrReg);
            Utils.SetXmlAttribute(node, "maskDataReg", MaskDataReg);
            Utils.SetXmlAttribute(node, "portAddrReg", PortAddrReg);
            Utils.SetXmlAttribute(node, "portDataReg", PortDataReg);
        }

        protected override void OnVolumeChanged(int oldVolume, int newVolume)
        {
            if (m_chip != null)
            {
                m_chip.Volume = newVolume;
            }
        }

        private void WritePortAddr(ushort addr, byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            //handled = true;
            m_chip.RegAddr = value;
        }

        private void WritePortData(ushort addr, byte value, ref bool handled)
        {
            //if (handled)
            //    return;
            //handled = true;
            var index = m_chip.RegAddr;
            m_lastTime = m_chip.SetReg(m_lastTime, GetFrameTime(), index, value);
            index &= 0x0F;
            if (index == PsgRegId.IRA)
            {
                OnWriteIra(value);
            }
            else if (index == PsgRegId.IRB)
            {
                OnWriteIrb(value);
            }
        }

        private void ReadPortData(ushort addr, ref byte value, ref bool handled)
        {
            if (handled)
                return;
            handled = true;

            var index = m_chip.RegAddr;
            var indexF = index & 0x0F;
            if (indexF == PsgRegId.IRA)
            {
                value = OnReadIra();
                return;
            }
            else if (indexF == PsgRegId.IRB)
            {
                value = OnReadIrb();
                return;
            }
            value = m_chip.GetReg(m_chip.RegAddr);
        }

        private void Bus_OnReset()
        {
            m_chip.Reset();
        }

        private byte OnReadIra()
        {
            m_iraState.DirOut = (m_chip.GetReg(PsgRegId.MIXER_CONTROL) & 0x40) != 0;
            m_iraState.InState = m_iraState.DirOut ? m_iraState.OutState : (byte)0;
            var iraHandler = IraHandler;
            if (iraHandler != null)
            {
                iraHandler(this, m_iraState);
            }
            return m_iraState.InState;
        }

        private byte OnReadIrb()
        {
            m_irbState.DirOut = (m_chip.GetReg(PsgRegId.MIXER_CONTROL) & 0x80) != 0;
            m_irbState.InState = m_irbState.DirOut ? m_irbState.OutState : (byte)0;
            var irbHandler = IrbHandler;
            if (irbHandler != null)
            {
                irbHandler(this, m_irbState);
            }
            return m_irbState.InState;
        }

        private void OnWriteIra(byte value)
        {
            m_iraState.DirOut = (m_chip.GetReg(PsgRegId.MIXER_CONTROL) & 0x40) != 0;
            m_iraState.OutState = value;
            var iraHandler = IraHandler;
            if (iraHandler != null)
            {
                m_iraState.InState = m_iraState.DirOut ? m_iraState.OutState : (byte)0;
                iraHandler(this, m_iraState);
            }
        }

        private void OnWriteIrb(byte value)
        {
            m_irbState.DirOut = (m_chip.GetReg(PsgRegId.MIXER_CONTROL) & 0x80) != 0;
            m_irbState.OutState = value;
            var irbHandler = IrbHandler;
            if (irbHandler != null)
            {
                m_irbState.InState = m_irbState.DirOut ? m_irbState.OutState : (byte)0;
                irbHandler(this, m_irbState);
            }
        }

        #endregion SoundDeviceBase
    }
}