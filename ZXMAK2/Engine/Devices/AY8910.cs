/// Description: AY8910 emulator
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;
using System.Xml;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Engine.Devices
{
    public class AY8910 : IBusDevice, ISoundRenderer, IConfigurable, IAY8910Device
	{
        #region IBusDevice

        public virtual string Name { get { return "AY8910"; } }
        public virtual string Description { get { return "Standard AY8910 Programmable Sound Generator"; } }
        public BusCategory Category { get { return BusCategory.Music; } }
		private int m_busOrder = 0;
		public int BusOrder { get { return m_busOrder; } set { m_busOrder = value; } }

        public virtual void BusInit(IBusManager bmgr)
        {
			m_cpu = bmgr.CPU;
            IUlaDevice ula = (IUlaDevice)bmgr.FindDevice(typeof(IUlaDevice));
            FrameTactCount = ula.FrameTactCount;
            IMemoryDevice memory = (IMemoryDevice)bmgr.FindDevice(typeof(IMemoryDevice));

            if (memory is ZXMAK2.Engine.Devices.Memory.MemorySpectrum128)
            {
                bmgr.SubscribeWRIO(0xC002, 0xC000, writePortAddr);   // #FFFD (reg#)
                bmgr.SubscribeRDIO(0xC002, 0xC000, readPortData);    // #FFFD (rd data/reg#)
                bmgr.SubscribeWRIO(0xC002, 0x8000, writePortData);   // #BFFD (data)
            }
            else
            {
                bmgr.SubscribeWRIO(0xC0FF, 0xC0FD, writePortAddr);   // #FFFD (reg#)
                bmgr.SubscribeRDIO(0xC0FF, 0xC0FD, readPortData);    // #FFFD (rd data/reg#)
                bmgr.SubscribeWRIO(0xC0FF, 0x80FD, writePortData);   // #BFFD (data)
            }
            
            
            bmgr.SubscribeRESET(busReset);

            bmgr.SubscribeBeginFrame(BeginFrame);
            bmgr.SubscribeEndFrame(EndFrame);
        }

        public virtual void BusConnect()
        {
        }

        public virtual void BusDisconnect()
        {
        }

        #endregion

		#region ISoundRenderer

		public uint[] AudioBuffer { get { return _samples; } }

		public virtual int Volume
		{
			get { return m_volume; }
			set
			{
				if (value < 0)
					value = 0;
				if (value > 100)
					value = 100;
				m_volume = value;

				int VolumeAY = (32767/*9000*/ * m_volume) / 100;
				ushort[] vol_table = YM_tab;
				for (int i = 0; i < 32; i++)
				{
					YM_VolTableA[i] = (uint)((vol_table[i] * VolumeAY / 65535 * mixerPreset[2 * 0] / 100) +
							   ((vol_table[i] * VolumeAY / 65535 * mixerPreset[2 * 0 + 1] / 100) << 16));
					YM_VolTableB[i] = (uint)((vol_table[i] * VolumeAY / 65535 * mixerPreset[2 * 1] / 100) +
							   ((vol_table[i] * VolumeAY / 65535 * mixerPreset[2 * 1 + 1] / 100) << 16));
					YM_VolTableC[i] = (uint)((vol_table[i] * VolumeAY / 65535 * mixerPreset[2 * 2] / 100) +
							   ((vol_table[i] * VolumeAY / 65535 * mixerPreset[2 * 2 + 1] / 100) << 16));
				}
			}
		}
		
		#endregion

		#region IConfigurable Members

		public virtual void LoadConfig(XmlNode itemNode)
		{
			Volume = Utils.GetXmlAttributeAsInt32(itemNode, "volume", Volume);
		}

		public virtual void SaveConfig(XmlNode itemNode)
		{
			Utils.SetXmlAttribute(itemNode, "volume", Volume);
		}

		#endregion


		#region IAY8910Device

		public byte ADDR_REG
		{
			get { return _curReg; }
			set { _curReg = value; }
		}

		public byte DATA_REG
		{
			get
			{
				switch (_curReg)
				{
					case 0: return (byte)(FreqA & 0xFF);
					case 1: return (byte)(FreqA >> 8);
					case 2: return (byte)(FreqB & 0xFF);
					case 3: return (byte)(FreqB >> 8);
					case 4: return (byte)(FreqC & 0xFF);
					case 5: return (byte)(FreqC >> 8);
					case 6: return FreqNoise;
					case 7: return ControlChannels;
					case 8: return VolumeA;
					case 9: return VolumeB;
					case 10: return VolumeC;
					case 11: return (byte)(FreqBend & 0xFF);
					case 12: return (byte)(FreqBend >> 8);
					case 13: return ControlBend;
					case 14: // ay mouse
						OnUpdateIRA(_ira.OutState);
						return _ira.InState;
					case 15:
						OnUpdateIRB(_irb.OutState);
						return _irb.InState;
				}
				return 0;
			}
			set
			{
				if ((_curReg > 7) && (_curReg < 11))
				{
					if ((value & 0x10) != 0) value &= 0x10;
					else value &= 0x0F;
				}
				value &= AY_RegMasks[_curReg & 0x0F];
				switch (_curReg)
				{
					case 0:
						FreqA = (ushort)((FreqA & 0xFF00) | value);
						break;
					case 1:
						FreqA = (ushort)((FreqA & 0x00FF) | (value << 8));
						break;
					case 2:
						FreqB = (ushort)((FreqB & 0xFF00) | value);
						break;
					case 3:
						FreqB = (ushort)((FreqB & 0x00FF) | (value << 8));
						break;
					case 4:
						FreqC = (ushort)((FreqC & 0xFF00) | value);
						break;
					case 5:
						FreqC = (ushort)((FreqC & 0x00FF) | (value << 8));
						break;
					case 6:
						FreqNoise = value;
						break;
					case 7:
						ControlChannels = value;
						break;
					case 8:
						VolumeA = value;
						break;
					case 9:
						VolumeB = value;
						break;
					case 10:
						VolumeC = value;
						break;
					case 11:
						FreqBend = (ushort)((FreqBend & 0xFF00) | value);
						break;
					case 12:
						FreqBend = (ushort)((FreqBend & 0x00FF) | (value << 8));
						break;
					case 13:
						// ATT:
						// 0 - begin down
						// 1 - begin up
						_CounterBend = 0;
						ControlBend = value;
						if (FreqBend != 0)
							if ((value & 0x04) != 0)
							{
								BendVolumeIndex = 0;
								BendStatus = 1;       // up
							}
							else
							{
								BendVolumeIndex = MAX_ENV_VOLTBL;
								BendStatus = 2;       // down
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

		public event AyUpdatePortDelegate UpdateIRA;
		public event AyUpdatePortDelegate UpdateIRB;

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
			UpdateState((int)(m_cpu.Tact % FrameTactCount));
			DATA_REG = value;
		}

		private void readPortData(ushort addr, ref byte value, ref bool iorqge)
		{
			if (!iorqge)
				return;
			iorqge = false;
			value = DATA_REG;
		}

		protected virtual void BeginFrame()
		{
			samplePos = 0;
			UpdateState(0);
		}

		protected virtual void EndFrame()
		{
			UpdateState(_frameTactCount);
		}

		private void busReset()
		{
			for (int i = 0; i < 16; i++)
			{
				ADDR_REG = (byte)i;
				DATA_REG = 0;
			}
			UpdateState((samplePos * _frameTactCount) / _samples.Length);
		}

		#endregion


		private Z80CPU m_cpu;
        
        #region registers

		private ushort FreqA;
		private ushort FreqB;
		private ushort FreqC;
		private byte FreqNoise;        //6
		private byte ControlChannels;  //7
		private byte VolumeA;          //8
		private byte VolumeB;          //9
		private byte VolumeC;          //10
		private ushort FreqBend;       //11
		private byte ControlBend;      //13
		private AyPortState _ira = new AyPortState(0xFF);
		private AyPortState _irb = new AyPortState(0xFF);

		private byte _curReg = 0;


		#endregion

		private int _sampleRate;
		private int _frameTactCount;
		private uint[] _samples = null;
        private int samplePos = 0;
		private int m_volume = 100;

		public AY8910()
		{
            _sampleRate = 44100; // see SMP_T_RELATION
			_samples = new uint[_sampleRate / 50];
			Volume = 50;
		}

        #region Public

		protected int FrameTactCount
		{
			get { return _frameTactCount; }
			set { _frameTactCount = value; }
		}

        public void Reset()
        {
            for (int i = 0; i < 16; i++)
            {
                ADDR_REG = (byte)i;
                DATA_REG = 0;
            }
            UpdateState((samplePos * _frameTactCount) / _samples.Length);
        }

        public void UpdateState(int frameTact)
        {
            if (frameTact > _frameTactCount) 
                frameTact = _frameTactCount;
            GenSignalAY((_samples.Length * frameTact) / _frameTactCount);
        }

        #endregion

        #region private data

        private byte BendStatus = 0;
		private int BendVolumeIndex = 0;
		private uint _CounterBend = 0;

		// signal gen...
		private byte OutputABC = 0;
		private byte OutputNoiseABC = 0;
		private byte MixLineABC = 0;       //  0 or 1 per channel - mixed channel 
		private ushort CounterA = 0;
		private ushort CounterB = 0;
		private ushort CounterC = 0;
		private byte CounterNoise = 0;
		private uint NoiseVal = 0xFFFF;

		#endregion

		protected void OnUpdateIRA(byte outState)
		{
			_ira.DirOut = (ControlChannels & 0x40) != 0;
			_ira.OutState = outState;
			if (_ira.DirOut)
				_ira.InState = outState;
			else
				_ira.InState = 0x00;
			if (UpdateIRA != null)
				UpdateIRA(this, _ira);
		}

		protected void OnUpdateIRB(byte outState)
		{
			_irb.DirOut = (ControlChannels & 0x80) != 0;
			_irb.OutState = outState;
			if (_irb.DirOut)
				_irb.InState = outState;
			else
				_irb.InState = 0x00;  // (see "TAIPAN" #A8CD)
			if (UpdateIRB != null)
				UpdateIRB(this, _irb);
		}

		private byte[] AY_RegMasks = new byte[] 
		{ 
			0xFF, 0x0F, 0xFF, 0x0F, 0xFF, 0x0F, 0x1F, 0xFF, 0x1F, 0x1F, 0x1F, 0xFF, 0xFF, 0x0F, 0xFF, 0xFF 
		};


		private uint[] mixerPreset = new uint[] { 90, 15, 60, 60, 15, 90 };      // ABC   values in 0...100


		private uint[] YM_VolTableA = new uint[32];
		private uint[] YM_VolTableB = new uint[32];
		private uint[] YM_VolTableC = new uint[32];


		private const int SMP_T_RELATION = 5;
		private const int MAX_ENV_VOLTBL = 0x1F;


		// ym (для громкости канала используется 2*i+1) для огибающей - все 32 шага
		private ushort[] YM_tab = new ushort[32]
		{
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
		
		private void GenSignalAY(int toEndPtr)
		{
			byte MixResultA = 0;      // result volume index for channel
			byte MixResultB = 0;
			byte MixResultC = 0;
			lock (this)
			{
                int frameLen = _samples.Length;
                if (toEndPtr > frameLen) toEndPtr = frameLen;  //882
				// mixer
				for (; samplePos < toEndPtr; samplePos++)
				{
					OutputNoiseABC &= (byte)(((ControlChannels & 0x38) >> 3) ^ 7);
					MixLineABC = (byte)((OutputABC & (((ControlChannels & 0x07)) ^ 7)) ^ OutputNoiseABC);

					if ((MixLineABC & 0x01) == 0)
					{
						MixResultA = (byte)((VolumeA & 0x1F) << 1);
						if ((MixResultA & 0x20) != 0)
							MixResultA = (byte)BendVolumeIndex;
						else
							MixResultA++;
					}
					else
						MixResultA = 0;

					if ((MixLineABC & 0x02) == 0)
					{
						MixResultB = (byte)((VolumeB & 0x1F) << 1);
						if ((MixResultB & 0x20) != 0)
							MixResultB = (byte)BendVolumeIndex;
						else
							MixResultB++;
					}
					else
						MixResultB = 0;

					if ((MixLineABC & 0x04) == 0)
					{
						MixResultC = (byte)((VolumeC & 0x1F) << 1);
						if ((MixResultC & 0x20) != 0)
							MixResultC = (byte)BendVolumeIndex;
						else
							MixResultC++;
					}
					else
						MixResultC = 0;

					_samples[samplePos] = YM_VolTableC[MixResultC] + YM_VolTableB[MixResultB] + YM_VolTableA[MixResultA];
					// end of mixer


					// ...counters...
					for (int i = 0; i < SMP_T_RELATION; i++)
					{
						// noise counter...
						if (++CounterNoise >= FreqNoise)
						{
							CounterNoise = 0;
							NoiseVal = (((NoiseVal >> 16) ^ (NoiseVal >> 13)) & 1) ^ ((NoiseVal << 1) + 1);
							OutputNoiseABC = (byte)((NoiseVal >> 16) & 1);
							OutputNoiseABC |= (byte)((OutputNoiseABC << 1) | (OutputNoiseABC << 2));
						}

						// signals counters...
						if (++CounterA >= FreqA)
						{
							CounterA = 0;
							OutputABC ^= 1;
						}
						if (++CounterB >= FreqB)
						{
							CounterB = 0;
							OutputABC ^= 2;
						}
						if (++CounterC >= FreqC)
						{
							CounterC = 0;
							OutputABC ^= 4;
						}
						if (++_CounterBend >= FreqBend)
						{
							_CounterBend = 0;
							ChangeEnvelope();
						}
					}
				}
			}
		}

		#region envelope

		private void ChangeEnvelope()
		{
			if (BendStatus == 0) return;
			// Коррекция амплитуды огибающей:
			if (BendStatus == 1) // AmplUp
			{
				if (++BendVolumeIndex > MAX_ENV_VOLTBL) // DUoverflow
				{
					switch (ControlBend & 0x0F)
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
				if (--BendVolumeIndex < 0)              // UDoverflow
				{
					switch (ControlBend & 0x0F)
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
			BendStatus = 1;
			BendVolumeIndex = 0;
		}
		private void env_UU()
		{
			BendStatus = 0;
			BendVolumeIndex = MAX_ENV_VOLTBL;
		}
		private void env_UD()
		{
			BendStatus = 2;
			BendVolumeIndex = MAX_ENV_VOLTBL;
		}
		private void env_DD()
		{
			BendStatus = 0;
			BendVolumeIndex = 0;
		}

		#endregion
	}

    public class AyPortState
    {
        private byte _outState = 0;
        private byte _oldOutState = 0;
        private byte _inState = 0;
        private bool _dirOut = true;

        public bool DirOut
        {
            get { return _dirOut; }
            set { _dirOut = value; }
        }
        public byte OutState
        {
            get { return _outState; }
            set { _oldOutState = _outState; _outState = value; }
        }
        public byte OldOutState { get { return _oldOutState; } }
        public byte InState
        {
            get { return _inState; }
            set { _inState = value; }
        }

        public AyPortState(byte value)
        {
            _outState = value;
            _oldOutState = value;
            _inState = value;
        }
    }

    public delegate void AyUpdatePortDelegate(AY8910 sender, AyPortState state);
}