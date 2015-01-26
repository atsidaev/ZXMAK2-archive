/// Description: AY8910 emulator
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;
using System.Xml;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.General
{
    public class AY8910 : SoundDeviceBase, IAyDevice
    {
        public AY8910()
        {
            Category = BusDeviceCategory.Music;
            Name = "AY8910";
            Description = "Standard AY8910 Programmable Sound Generator";

            ChipFrequency = 3548160;//1774400*2;
            Volume = 50;
        }

        
        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            var ula = bmgr.FindDevice<IUlaDevice>();
            var memory = bmgr.FindDevice<IMemoryDevice>();
            Frequency = ChipFrequency / 16;

            if (memory is ZXMAK2.Hardware.Spectrum.MemorySpectrum128 ||
                memory is ZXMAK2.Hardware.Spectrum.MemoryPlus3)
            {
                bmgr.SubscribeWrIo(0xC002, 0xC000, writePortAddr);   // #FFFD (reg#)
                bmgr.SubscribeRdIo(0xC002, 0xC000, readPortData);    // #FFFD (rd data/reg#)
                bmgr.SubscribeWrIo(0xC002, 0x8000, writePortData);   // #BFFD (data)
            }
            else
            {
                bmgr.SubscribeWrIo(0xC0FF, 0xC0FD, writePortAddr);   // #FFFD (reg#)
                bmgr.SubscribeRdIo(0xC0FF, 0xC0FD, readPortData);    // #FFFD (rd data/reg#)
                bmgr.SubscribeWrIo(0xC0FF, 0x80FD, writePortData);   // #BFFD (data)
            }
            bmgr.SubscribeReset(busReset);
        }

        #endregion

        #region SoundDeviceBase

        protected override void OnBeginFrame()
        {
            base.OnBeginFrame();
        }

        protected override void OnEndFrame()
        {
            m_lastTime += TimeStep;
            m_lastTime -= Math.Floor(m_lastTime);
            UpdateAudioBuffer(1D);
            base.OnEndFrame();
        }

        protected override void OnVolumeChanged(int oldVolume, int newVolume)
        {
            var volume = (32767/*9000*/ * newVolume) / 100;
            var vol_table = s_volumeTable;
            for (int i = 0; i < 32; i++)
            {
                m_volTableA[i] = (uint)((vol_table[i] * volume / 65535 * m_mixerPreset[2 * 0] / 100) +
                           ((vol_table[i] * volume / 65535 * m_mixerPreset[2 * 0 + 1] / 100) << 16));
                m_volTableB[i] = (uint)((vol_table[i] * volume / 65535 * m_mixerPreset[2 * 1] / 100) +
                           ((vol_table[i] * volume / 65535 * m_mixerPreset[2 * 1 + 1] / 100) << 16));
                m_volTableC[i] = (uint)((vol_table[i] * volume / 65535 * m_mixerPreset[2 * 2] / 100) +
                           ((vol_table[i] * volume / 65535 * m_mixerPreset[2 * 2 + 1] / 100) << 16));
            }
        }

        #endregion SoundDeviceBase


        #region IAY8910Device

        public byte ADDR_REG
        {
            get { return m_curReg; }
            set { m_curReg = value; }
        }

        public byte DATA_REG
        {
            get
            {
                switch (m_curReg)
                {
                    case 0: return (byte)(m_freqA & 0xFF);
                    case 1: return (byte)(m_freqA >> 8);
                    case 2: return (byte)(m_freqB & 0xFF);
                    case 3: return (byte)(m_freqB >> 8);
                    case 4: return (byte)(m_freqC & 0xFF);
                    case 5: return (byte)(m_freqC >> 8);
                    case 6: return m_freqNoise;
                    case 7: return m_controlChannels;
                    case 8: return m_volumeA;
                    case 9: return m_volumeB;
                    case 10: return m_volumeC;
                    case 11: return (byte)(m_freqBend & 0xFF);
                    case 12: return (byte)(m_freqBend >> 8);
                    case 13: return m_controlBend;
                    case 14: // ay mouse
                        OnUpdateIRA(m_iraState.OutState);
                        return m_iraState.InState;
                    case 15:
                        OnUpdateIRB(m_irbState.OutState);
                        return m_irbState.InState;
                }
                return 0;
            }
            set
            {
                if ((m_curReg > 7) && (m_curReg < 11))
                {
                    if ((value & 0x10) != 0) value &= 0x10;
                    else value &= 0x0F;
                }
                value &= s_regMask[m_curReg & 0x0F];
                switch (m_curReg)
                {
                    case 0:
                        m_freqA = (ushort)((m_freqA & 0xFF00) | value);
                        break;
                    case 1:
                        m_freqA = (ushort)((m_freqA & 0x00FF) | (value << 8));
                        break;
                    case 2:
                        m_freqB = (ushort)((m_freqB & 0xFF00) | value);
                        break;
                    case 3:
                        m_freqB = (ushort)((m_freqB & 0x00FF) | (value << 8));
                        break;
                    case 4:
                        m_freqC = (ushort)((m_freqC & 0xFF00) | value);
                        break;
                    case 5:
                        m_freqC = (ushort)((m_freqC & 0x00FF) | (value << 8));
                        break;
                    case 6:
                        m_freqNoise = value;
                        break;
                    case 7:
                        m_controlChannels = value;
                        break;
                    case 8:
                        m_volumeA = value;
                        break;
                    case 9:
                        m_volumeB = value;
                        break;
                    case 10:
                        m_volumeC = value;
                        break;
                    case 11:
                        m_freqBend = (ushort)((m_freqBend & 0xFF00) | value);
                        break;
                    case 12:
                        m_freqBend = (ushort)((m_freqBend & 0x00FF) | (value << 8));
                        break;
                    case 13:
                        // ATT:
                        // 0 - begin down
                        // 1 - begin up
                        m_counterBend = 0;
                        m_controlBend = value;
                        if (m_freqBend != 0)
                            if ((value & 0x04) != 0)
                            {
                                m_bendVolumeIndex = 0;
                                m_bendStatus = 1;       // up
                            }
                            else
                            {
                                m_bendVolumeIndex = MAX_ENV_VOLTBL;
                                m_bendStatus = 2;       // down
                            }
                        break;
                    case 14:
                        OnUpdateIRA(value);
                        break;
                    case 15:
                        OnUpdateIRB(value);
                        break;
                }
            }
        }

        private int m_chipFrequency;
        
        public int ChipFrequency 
        {
            get { return m_chipFrequency; }
            set { m_chipFrequency = Math.Max(496, value); }
        }

        public event AyUpdatePortHandler UpdateIRA;
        public event AyUpdatePortHandler UpdateIRB;

        #endregion

        #region Bus Handlers

        private void writePortAddr(ushort addr, byte value, ref bool iorqge)
        {
            //if (!iorqge)
            //    return;
            //iorqge = false;
            ADDR_REG = value;
        }

        private void writePortData(ushort addr, byte value, ref bool iorqge)
        {
            //if (!iorqge)
            //    return;
            //iorqge = false;
            UpdateAudioBuffer(GetFrameTime());
            DATA_REG = value;
        }

        private void readPortData(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            value = DATA_REG;
        }

        private void busReset()
        {
            m_noiseVal = 0x0FFFF;
            m_stateNoise = 0;
            m_stateGen = 0;
            m_counterNoise = 0;
            m_counterA = 0;
            m_counterB = 0;
            m_counterC = 0;
            m_counterBend = 0;
            for (int i = 0; i < 16; i++)
            {
                ADDR_REG = (byte)i;
                DATA_REG = 0;
            }
        }

        #endregion

        #region registers

        private ushort m_freqA;
        private ushort m_freqB;
        private ushort m_freqC;
        private byte m_freqNoise;        //6
        private byte m_controlChannels;  //7
        private byte m_volumeA;          //8
        private byte m_volumeB;          //9
        private byte m_volumeC;          //10
        private ushort m_freqBend;       //11
        private byte m_controlBend;      //13
        private AyPortState m_iraState = new AyPortState(0xFF);
        private AyPortState m_irbState = new AyPortState(0xFF);

        private byte m_curReg = 0;


        #endregion

        private double m_lastTime;

        #region private data

        private byte m_bendStatus;
        private int m_bendVolumeIndex;
        private uint m_counterBend;

        // signal gen...
        private byte m_stateGen;      // ABC generators state
        private byte m_stateNoise;    // ABC noise state
        private ushort m_counterA;
        private ushort m_counterB;
        private ushort m_counterC;
        private byte m_counterNoise;
        private uint m_noiseVal = 0x0FFFF;

        #endregion

        private static byte[] s_regMask = new byte[] 
		{ 
			0xFF, 0x0F, 0xFF, 0x0F, 0xFF, 0x0F, 0x1F, 0xFF, 
            0x1F, 0x1F, 0x1F, 0xFF, 0xFF, 0x0F, 0xFF, 0xFF, 
		};


        private uint[] m_mixerPreset = new uint[] { 90, 15, 60, 60, 15, 90 };      // ABC   values in 0...100


        private uint[] m_volTableA = new uint[32];
        private uint[] m_volTableB = new uint[32];
        private uint[] m_volTableC = new uint[32];


        private const int MAX_ENV_VOLTBL = 0x1F;


        // ym (для громкости канала используется 2*i+1) для огибающей - все 32 шага
        private static ushort[] s_volumeTable = new ushort[32]
		{
            //ZXMAK1
            0x0000, 0x0000, 0x00F8, 0x01C2, 0x029E, 0x033A, 0x03F2, 0x04D7,
            0x0610, 0x077F, 0x090A, 0x0A42, 0x0C3B, 0x0EC2, 0x1137, 0x13A7,
            0x1750, 0x1BF9, 0x20DF, 0x2596, 0x2C9D, 0x3579, 0x3E55, 0x4768,
            0x54FF, 0x6624, 0x773B, 0x883F, 0xA1DA, 0xC0FC, 0xE094, 0xFFFF
            
            //us037-3
            //0x0000, 0x0000, 0x00EF, 0x01D0, 0x0290, 0x032A, 0x03EE, 0x04D2, 
            //0x0611, 0x0782, 0x0912, 0x0A36, 0x0C31, 0x0EB6, 0x1130, 0x13A0,
            //0x1751, 0x1BF5, 0x20E2, 0x2594, 0x2CA1, 0x357F, 0x3E45, 0x475E,
            //0x5502, 0x6620, 0x7730, 0x8844, 0xA1D2, 0xC102, 0xE0A2, 0xFFFF
		};

        protected void OnUpdateIRA(byte outState)
        {
            m_iraState.DirOut = (m_controlChannels & 0x40) != 0;
            m_iraState.OutState = outState;
            if (m_iraState.DirOut)
                m_iraState.InState = outState;
            else
                m_iraState.InState = 0x00;
            if (UpdateIRA != null)
                UpdateIRA(this, m_iraState);
        }

        protected void OnUpdateIRB(byte outState)
        {
            m_irbState.DirOut = (m_controlChannels & 0x80) != 0;
            m_irbState.OutState = outState;
            if (m_irbState.DirOut)
                m_irbState.InState = outState;
            else
                m_irbState.InState = 0x00;  // (see "TAIPAN" #A8CD)
            if (UpdateIRB != null)
                UpdateIRB(this, m_irbState);
        }

        private unsafe void UpdateAudioBuffer(double frameTime)
        {
            lock (this)
            {
                for (var t = m_lastTime; t < frameTime; t += TimeStep)
                {
                    var outGen = m_stateGen | (m_controlChannels & 7);
                    var outNoise = m_stateNoise | ((m_controlChannels & 0x38) >> 3);
                    var outMix = outGen & outNoise;

                    var mixA = 0;      // result volume index for channel
                    if ((outMix & 0x01) != 0)
                    {
                        mixA = (m_volumeA & 0x1F) << 1;
                        mixA = (mixA & 0x20) == 0 ? mixA + 1 : m_bendVolumeIndex;
                    }

                    var mixB = 0;
                    if ((outMix & 0x02) != 0)
                    {
                        mixB = (m_volumeB & 0x1F) << 1;
                        mixB = (mixB & 0x20) == 0 ? mixB + 1 : m_bendVolumeIndex;
                    }

                    var mixC = 0;
                    if ((outMix & 0x04) != 0)
                    {
                        mixC = (m_volumeC & 0x1F) << 1;
                        mixC = (mixC & 0x20) == 0 ? mixC + 1 : m_bendVolumeIndex;
                    }
                    
                    var sample = m_volTableC[mixC] + m_volTableB[mixB] + m_volTableA[mixA];
                    UpdateDac(t, (ushort)(sample & 0xFFFF), (ushort)(sample >> 16));
                    m_lastTime = t;

                    // end of mixer

                    // noise counter...
                    var nfq = m_freqNoise & 0x1F;
                    if (++m_counterNoise >= nfq)
                    {
                        m_counterNoise = 0;

                        m_noiseVal &= 0x1FFFF;
                        m_noiseVal = (((m_noiseVal >> 16) ^ (m_noiseVal >> 13)) & 1) ^ ((m_noiseVal << 1) + 1);
                        m_stateNoise = (byte)((m_noiseVal >> 16) & 1);
                        m_stateNoise |= (byte)((m_stateNoise << 1) | (m_stateNoise << 2));

                        //var output = (m_noiseVal + 1) & 2;
                        //m_outNoiseABC ^= (byte)((output << 1) | output | (output >> 1));
                        //m_noiseVal = (m_noiseVal >> 1) ^ ((m_noiseVal & 1) << 14) ^ ((m_noiseVal & 1) << 16);
                    } 

                    // signals counters...
                    if (++m_counterA >= m_freqA)
                    {
                        m_counterA = 0;
                        m_stateGen ^= 1;
                    }
                    if (++m_counterB >= m_freqB)
                    {
                        m_counterB = 0;
                        m_stateGen ^= 2;
                    }
                    if (++m_counterC >= m_freqC)
                    {
                        m_counterC = 0;
                        m_stateGen ^= 4;
                    }
                    if (++m_counterBend >= m_freqBend)
                    {
                        m_counterBend = 0;
                        ChangeEnvelope();
                    }
                }
            }
        }

        #region envelope

        private void ChangeEnvelope()
        {
            if (m_bendStatus == 0) return;
            // Коррекция амплитуды огибающей:
            if (m_bendStatus == 1) // AmplUp
            {
                if (++m_bendVolumeIndex > MAX_ENV_VOLTBL) // DUoverflow
                {
                    switch (m_controlBend & 0x0F)
                    {
                        case 0: env_DD(); break; // not used
                        case 1: env_DD(); break; // not used
                        case 2: env_DD(); break; // not used
                        case 3: env_DD(); break; // not used
                        case 4: env_DD(); break;
                        case 5: env_DD(); break;
                        case 6: env_DD(); break;
                        case 7: env_DD(); break;
                        case 8: env_UD(); break; // not used
                        case 9: env_DD(); break; // not used
                        case 10: env_UD(); break;
                        case 11: env_UU(); break;// not used
                        case 12: env_DU(); break;
                        case 13: env_UU(); break;
                        case 14: env_UD(); break;
                        case 15: env_DD(); break;
                    }
                }
            }
            else                 // AmplDown
            {
                if (--m_bendVolumeIndex < 0)              // UDoverflow
                {
                    switch (m_controlBend & 0x0F)
                    {
                        case 0: env_DD(); break;
                        case 1: env_DD(); break;//env_UU(); break;
                        case 2: env_DD(); break;
                        case 3: env_DD(); break;
                        case 4: env_DD(); break;  // not used
                        case 5: env_DD(); break;  // not used
                        case 6: env_DD(); break;  // not used
                        case 7: env_DD(); break;  // not used
                        case 8: env_UD(); break;
                        case 9: env_DD(); break;
                        case 10: env_DU(); break;
                        case 11: env_UU(); break;
                        case 12: env_DU(); break; // not used
                        case 13: env_UU(); break; // not used
                        case 14: env_DU(); break;
                        case 15: env_DD(); break; // not used
                    }
                }
            }
        }
        private void env_DU()
        {
            m_bendStatus = 1;
            m_bendVolumeIndex = 0;
        }
        private void env_UU()
        {
            m_bendStatus = 0;
            m_bendVolumeIndex = MAX_ENV_VOLTBL;
        }
        private void env_UD()
        {
            m_bendStatus = 2;
            m_bendVolumeIndex = MAX_ENV_VOLTBL;
        }
        private void env_DD()
        {
            m_bendStatus = 0;
            m_bendVolumeIndex = 0;
        }

        #endregion
    }

    public class AyPortState
    {
        private byte m_outState = 0;
        private byte m_oldOutState = 0;
        private byte m_inState = 0;
        private bool m_dirOut = true;

        public bool DirOut
        {
            get { return m_dirOut; }
            set { m_dirOut = value; }
        }

        public byte OutState
        {
            get { return m_outState; }
            set { m_oldOutState = m_outState; m_outState = value; }
        }

        public byte OldOutState
        {
            get { return m_oldOutState; }
        }

        public byte InState
        {
            get { return m_inState; }
            set { m_inState = value; }
        }

        public AyPortState(byte value)
        {
            m_outState = value;
            m_oldOutState = value;
            m_inState = value;
        }
    }

    public delegate void AyUpdatePortHandler(AY8910 sender, AyPortState state);
}