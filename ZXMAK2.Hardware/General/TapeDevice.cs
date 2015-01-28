/// Description: Tape emulator
/// Author: Alex Makeev
/// Date: 16.04.2007
using System;
using System.Xml;
using System.Collections.Generic;

using ZXMAK2.Dependency;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Model.Tape.Interfaces;
using ZXMAK2.Serializers.TapeSerializers;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.Presentation;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Resources;


namespace ZXMAK2.Hardware.General
{
    public class TapeDevice : SoundDeviceBase, ITapeDevice
    {
        #region Fields

        private CpuUnit m_cpu;
        private IMemoryDevice m_memory;
        private IconDescriptor m_iconTape = new IconDescriptor("TAPE", ImageResources.TapeRd_128x128);
        private bool m_trapsAllowed = true;
        private bool m_autoPlay = true;
        private readonly int m_frequency = 3500000;

        // sound related
        private ushort m_dacValue0 = 0;
        private ushort m_dacValue1 = 0x1FFF;

        private int m_index = 0;
        private int m_playPosition = 0;

        private bool m_isPlay = false;

        private long m_lastTact = 0;
        private int m_waitEdge = 0;
        private bool m_state = false;

        private IViewHolder m_viewHolder;

        #endregion Fields

        
        public TapeDevice()
        {
            Category = BusDeviceCategory.Tape;
            Name = "TAPE PLAYER";
            Description = "Common Tape Device (port #FE, mask #01)";

            Blocks = new List<ITapeBlock>();
            Volume = 5;
            CreateViewHolder();
        }


        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_cpu = bmgr.CPU;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            bmgr.SubscribeRdIo(0x0001, 0x0000, readPortFE);

            bmgr.SubscribePreCycle(busPreCycle);

            bmgr.AddSerializer(new TapSerializer(this));
            bmgr.AddSerializer(new TzxSerializer(this));
            bmgr.AddSerializer(new CswSerializer(this));
            bmgr.AddSerializer(new WavSerializer(this));
            bmgr.RegisterIcon(m_iconTape);
            if (m_viewHolder != null)
            {
                bmgr.AddCommandUi(m_viewHolder.CommandOpen);
            }
        }

        public override void BusDisconnect()
        {
            base.BusDisconnect();
            if (m_viewHolder != null)
            {
                m_viewHolder.Close();
            }
        }

        protected override void OnConfigLoad(XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            UseTraps = Utils.GetXmlAttributeAsBool(itemNode, "useTraps", UseTraps);
            UseAutoPlay = Utils.GetXmlAttributeAsBool(itemNode, "useAutoPlay", UseAutoPlay);
        }

        protected override void OnConfigSave(XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "useTraps", UseTraps);
            Utils.SetXmlAttribute(itemNode, "useAutoPlay", UseAutoPlay);
        }

        #endregion

        #region ITapeDevice

        public bool UseTraps
        {
            get { return m_trapsAllowed; }
            set { m_trapsAllowed = value; OnConfigChanged(); }
        }

        public bool UseAutoPlay
        {
            get { return m_autoPlay; }
            set { m_autoPlay = value; OnConfigChanged(); detectorReset(); }
        }

        #endregion

        protected override void OnVolumeChanged(int oldVolume, int newVolume)
        {
            m_dacValue0 = ushort.MinValue;
            m_dacValue1 = (ushort)((ushort.MaxValue * newVolume) / 100);
        }

        #region Bus Handlers

        private void readPortFE(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge || m_memory.DOSEN)
                return;
            //iorqge = false;

            if (tape_bit(m_cpu.Tact))
                value |= 0x40;
            else
                value &= 0xBF;
            detectorRead();
        }

        private void busPreCycle(int frameTact)
        {
            if (!m_isPlay)
                return;
            //bmgr.SubscribeRDMEM_M1(0xFFFF, 0x056B, tapeTrap);
            //bmgr.SubscribeRDMEM_M1(0xFFFF, 0x059E, tapeTrap);

            var val = tape_bit(m_cpu.Tact) ? m_dacValue1 : m_dacValue0;
            UpdateDac(val, val);

            ushort addr = m_cpu.regs.PC;
            if (!UseTraps || !m_memory.IsRom48 ||
                !(addr == 0x056B || addr == 0x059E) ||
                (m_cpu.regs._AF & (int)CpuFlags.C) == 0) // verify?
            {
                return;
            }

            var tb = Blocks[CurrentBlock];
            var tapData = tb.GetData();
            if (tapData == null || tapData.Length < 2)
            {
                return;
            }

            var length = tapData.Length;
            var read = length - 1;
            read = read < m_cpu.regs.DE ? read : m_cpu.regs.DE;
            if (read <= 0)
            {
                return;
            }
            var parity = tapData[0];
            if (parity != m_cpu.regs._AF >> 8)
            {
                return;
            }
            m_cpu.regs._AF = 0x0145;

            /* Loading or verifying determined by the carry flag of F' */
            for (var i = 0; i < read; i++)
            {
                var value = tapData[i + 1];
                m_cpu.regs.L = value;
                parity ^= value;
                m_cpu.WRMEM((ushort)(m_cpu.regs.IX + i), value);
            }
            var pc = (ushort)0x05DF;
            /* If |DE| bytes have been read and there's more data, do the parity check */
            if (m_cpu.regs.DE == read && read + 1 < length)
            {
                var value = tapData[read + 1];
                m_cpu.regs.L = value;
                parity ^= value;
                m_cpu.regs.B = 0xB0;
            }
            else
            {
                /* Failure to read first bit of the next byte (ref. 48K ROM, 0x5EC) */
                m_cpu.regs.L = 1;
                m_cpu.regs.A = parity;
                m_cpu.regs.F = 0x50;
                m_cpu.regs.B = 0;
                pc = 0x05EE;
            }

            m_cpu.regs.H = parity;
            m_cpu.regs.DE -= (ushort)read;
            m_cpu.regs.IX += (ushort)read;
            m_cpu.regs.C = 0x01;

            m_cpu.regs.PC = pc;//0x05DF;//0x05DF - 1;

            int newBlock = CurrentBlock + 1;
            if (newBlock < Blocks.Count)
            {
                CurrentBlock = newBlock;
            }
            else
            {
                Stop();
                Rewind();
            }
        }

        protected override void OnBeginFrame()
        {
            base.OnBeginFrame();
            m_iconTape.Visible = false;
        }

        protected override void OnEndFrame()
        {
            //ushort val = tape_bit(m_cpu.Tact) ? m_dacValue1 : m_dacValue0;
            //UpdateDAC(val, val);
            base.OnEndFrame();
            detectorFrame();
        }

        #endregion


        #region Events

        public event EventHandler TapeStateChanged;

        protected virtual void OnTapeStateChanged()
        {
            if (TapeStateChanged != null)
                TapeStateChanged(this, EventArgs.Empty);
        }

        #endregion


        #region Properties

        public List<ITapeBlock> Blocks { get; private set; }

        public int CurrentBlock
        {
            get
            {
                if (Blocks.Count > 0)
                    return m_index;
                else
                    return -1;
            }
            set
            {
                if (value == m_index)
                    return;
                if (value >= 0 && value < Blocks.Count)
                {
                    m_index = value;
                    m_playPosition = 0;
                    OnTapeStateChanged();
                    //if(Play)
                    //   _currentBlock = _blocks[_index] as TapeBlock;
                }
            }
        }

        public int Position
        {
            get
            {
                if (m_playPosition >= Blocks[m_index].Count)
                    return 0;
                return m_playPosition;
            }
        }

        public bool IsPlay
        {
            get { return m_isPlay; }
        }

        public int TactsPerSecond
        {
            get { return FrameTactCount * 50; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called on load new image
        /// </summary>
        public void Reset()
        {
            m_index = -1;
            Stop();
        }

        public void Rewind()
        {
            m_index = -1;
            Stop();
        }

        public void Play()
        {
            m_lastTact = m_cpu.Tact;
            m_waitEdge = 0;
            m_playPosition = 0;

            if (Blocks.Count > 0 && m_index >= 0)
            {
                while (m_playPosition >= Blocks[m_index].Count)
                {
                    m_playPosition = 0;
                    m_index++;
                    if (m_index >= Blocks.Count)
                        break;
                }
                if (m_index >= Blocks.Count)
                {
                    // end of tape -> rewind & stop
                    m_index = -1;
                    Stop();
                    return;
                }

                //m_state = !m_state;
                m_waitEdge = Blocks[m_index].GetPeriod(m_playPosition, m_frequency);
                m_isPlay = true;
                OnTapeStateChanged();
            }
        }

        public void Stop()
        {
            m_isPlay = false;
            if (m_index >= 0 && m_index < Blocks.Count &&
                m_playPosition >= Blocks[m_index].Count - 1)
            {
                m_index++;
                if (m_index >= Blocks.Count)
                    m_index = -1;
            }
            m_lastTact = m_cpu.Tact;
            m_waitEdge = 0;
            m_playPosition = 0;
            if (m_index < 0 && Blocks.Count > 0)
                m_index = 0;
            OnTapeStateChanged();
        }

        #endregion

        #region private methods

        private bool tape_bit(long globalTact)
        {
            int delta = (int)(globalTact - m_lastTact);

            if (!m_isPlay)
            {
                m_lastTact = globalTact;
                return false;
            }
            if (m_index < 0)
            {
                // end of tape -> rewind & stop
                Stop();
                return m_state;
            }

            while (delta >= m_waitEdge)
            {
                delta -= m_waitEdge;
                m_state = !m_state;

                m_playPosition++;
                if (m_playPosition >= Blocks[m_index].Count) // endof block?
                {
                    while (m_playPosition >= Blocks[m_index].Count)
                    {
                        // skip empty blocks
                        m_playPosition = 0;
                        m_index++;
                        if (m_index >= Blocks.Count)
                            break;
                    }
                    if (m_index >= Blocks.Count)
                    {
                        // end of tape -> rewind & stop
                        m_index = -1;
                        Stop();
                        return m_state;
                    }
                    OnTapeStateChanged();
                }
                m_waitEdge = Blocks[m_index].GetPeriod(m_playPosition, m_frequency);
            }
            m_lastTact = globalTact - (long)delta;
            return m_state;
        }

        #endregion


        #region AutoPlay

        private long m_lastInTact = 0;
        private int m_detectCounter;
        private int m_detectTimeOut;
        private ushort m_lastPC;
        private byte[] m_lastRegs;

        private void detectorReset()
        {
            m_lastInTact = 0;
            m_detectCounter = 0;
            m_lastPC = 0;
            m_lastRegs = null;
        }

        private void detectorRead()
        {
            long cpuTact = m_cpu.Tact;
            int delta = (int)(cpuTact - m_lastInTact);
            m_lastInTact = cpuTact;

            byte[] newRegs = new byte[] {
				m_cpu.regs.A,
				m_cpu.regs.B, m_cpu.regs.C,
				m_cpu.regs.D, m_cpu.regs.E,
				m_cpu.regs.H, m_cpu.regs.L,
			};
            if (delta > 0 && delta < 96 && m_cpu.regs.PC == m_lastPC && m_lastRegs != null)
            {
                int diffCount = 0;
                int diffValue = 0;
                for (int i = 0; i < newRegs.Length; i++)
                {
                    if (m_lastRegs[i] != newRegs[i])
                    {
                        diffValue = m_lastRegs[i] - newRegs[i];
                        diffCount++;
                    }
                }
                if (diffCount == 1 && (diffValue == 1 || diffValue == -1))
                {
                    m_iconTape.Visible = true;
                    m_detectCounter++;
                    if (m_detectCounter >= 8 && m_autoPlay)
                    {
                        if (!m_isPlay)
                            Play();
                        m_detectTimeOut = 50;
                    }
                }
                else
                {
                    m_detectCounter = 0;
                }
            }
            m_lastRegs = newRegs;
            m_lastPC = m_cpu.regs.PC;
        }

        private void detectorFrame()
        {
            if (m_isPlay && m_autoPlay)
            {
                m_detectTimeOut--;
                if (m_detectTimeOut < 0)
                    Stop();
            }
        }

        #endregion


        #region IGuiExtension Members

        private void CreateViewHolder()
        {
            try
            {
                m_viewHolder = new ViewHolder<ITapeView>(
                    "Tape",
                    new Argument("tapeDevice", this));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        #endregion
    }
}
