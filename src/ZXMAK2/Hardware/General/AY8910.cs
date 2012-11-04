/// Description: AY8910 emulator
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;
using System.Xml;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine;

namespace ZXMAK2.Hardware.General
{
	public class AY8910 : BusDeviceBase, ISoundRenderer, IConfigurable, IAyDevice
	{
		#region IBusDevice

		public override string Name { get { return "AY8910"; } }
		public override string Description { get { return "Standard AY8910 Programmable Sound Generator"; } }
		public override BusCategory Category { get { return BusCategory.Music; } }

		public override void BusInit(IBusManager bmgr)
		{
			m_cpu = bmgr.CPU;
			IUlaDevice ula = (IUlaDevice)bmgr.FindDevice(typeof(IUlaDevice));
			IMemoryDevice memory = (IMemoryDevice)bmgr.FindDevice(typeof(IMemoryDevice));
			initTiming(m_sampleRate, ula.FrameTactCount);

			if (memory is ZXMAK2.Hardware.Spectrum.MemorySpectrum128 ||
				memory is ZXMAK2.Hardware.Spectrum.MemoryPlus3)
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

		public override void BusConnect()
		{
		}

		public override void BusDisconnect()
		{
		}

		#endregion

		#region ISoundRenderer

		public uint[] AudioBuffer
		{
			get { return m_audioBuffer; }
		}

		public int Volume
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
				ushort[] vol_table = s_volumeTable;
				for (int i = 0; i < 32; i++)
				{
					m_volTableA[i] = (uint)((vol_table[i] * VolumeAY / 65535 * m_mixerPreset[2 * 0] / 100) +
							   ((vol_table[i] * VolumeAY / 65535 * m_mixerPreset[2 * 0 + 1] / 100) << 16));
					m_volTableB[i] = (uint)((vol_table[i] * VolumeAY / 65535 * m_mixerPreset[2 * 1] / 100) +
							   ((vol_table[i] * VolumeAY / 65535 * m_mixerPreset[2 * 1 + 1] / 100) << 16));
					m_volTableC[i] = (uint)((vol_table[i] * VolumeAY / 65535 * m_mixerPreset[2 * 2] / 100) +
							   ((vol_table[i] * VolumeAY / 65535 * m_mixerPreset[2 * 2 + 1] / 100) << 16));
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
			UpdateAudioBuffer((int)(m_cpu.Tact % m_frameTactCount));
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
			m_audioBufferPos = 0;
			UpdateAudioBuffer(0);
		}

		protected virtual void EndFrame()
		{
			UpdateAudioBuffer(m_frameTactCount);
		}

		private void busReset()
		{
			m_noiseVal = 0x0FFFF;
			m_outNoiseABC = 0;
			m_outABC = 0;
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
			UpdateAudioBuffer((m_audioBufferPos * m_frameTactCount) / m_audioBuffer.Length);
		}

		#endregion

		public AY8910()
		{
			initTiming(44100, 70000);
			Volume = 50;
		}


		private Z80CPU m_cpu;

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

		private int m_frameTactCount;
		private int m_sampleRate;
		private int m_tactsPerSample;
		private uint[] m_audioBuffer;
		private int m_audioBufferPos;
		private int m_volume;

		#region private data

		private byte m_bendStatus = 0;
		private int m_bendVolumeIndex = 0;
		private uint m_counterBend = 0;

		// signal gen...
		private byte m_outABC = 0;
		private byte m_outNoiseABC = 0;
		private byte m_mixLineABC = 0;       //  0 or 1 per channel - mixed channel 
		private ushort m_counterA = 0;
		private ushort m_counterB = 0;
		private ushort m_counterC = 0;
		private byte m_counterNoise = 0;
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

		private void initTiming(int sampleRate, int frameTactCount)
		{
			m_sampleRate = sampleRate;
			m_frameTactCount = frameTactCount;

			m_tactsPerSample = (int)Math.Round(((double)m_frameTactCount * 50D) / (16D * (double)m_sampleRate),
				MidpointRounding.AwayFromZero);
			m_audioBuffer = new uint[m_sampleRate / 50];
			m_audioBufferPos = 0;
		}

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

		private unsafe void UpdateAudioBuffer(int frameTact)
		{
			if (frameTact > m_frameTactCount)
				frameTact = m_frameTactCount;
			int frameLen = m_audioBuffer.Length;
			int toEndPtr = (frameLen * frameTact) / m_frameTactCount;

			byte MixResultA = 0;      // result volume index for channel
			byte MixResultB = 0;
			byte MixResultC = 0;
			lock (this)
			{
				if (toEndPtr > frameLen) toEndPtr = frameLen;  //882
				// mixer
				fixed (uint* pAudioBuffer = m_audioBuffer)
					for (; m_audioBufferPos < toEndPtr; m_audioBufferPos++)
					{
						m_outNoiseABC &= (byte)(((m_controlChannels & 0x38) >> 3) ^ 7);
						m_mixLineABC = (byte)((m_outABC & (((m_controlChannels & 0x07)) ^ 7)) ^ m_outNoiseABC);

						if ((m_mixLineABC & 0x01) == 0)
						{
							MixResultA = (byte)((m_volumeA & 0x1F) << 1);
							if ((MixResultA & 0x20) != 0)
								MixResultA = (byte)m_bendVolumeIndex;
							else
								MixResultA++;
						}
						else
							MixResultA = 0;

						if ((m_mixLineABC & 0x02) == 0)
						{
							MixResultB = (byte)((m_volumeB & 0x1F) << 1);
							if ((MixResultB & 0x20) != 0)
								MixResultB = (byte)m_bendVolumeIndex;
							else
								MixResultB++;
						}
						else
							MixResultB = 0;

						if ((m_mixLineABC & 0x04) == 0)
						{
							MixResultC = (byte)((m_volumeC & 0x1F) << 1);
							if ((MixResultC & 0x20) != 0)
								MixResultC = (byte)m_bendVolumeIndex;
							else
								MixResultC++;
						}
						else
							MixResultC = 0;

						pAudioBuffer[m_audioBufferPos] = m_volTableC[MixResultC] + m_volTableB[MixResultB] + m_volTableA[MixResultA];
						// end of mixer


						// ...counters...
						for (int i = 0; i < m_tactsPerSample; i++)
						{
							// noise counter...
							if (++m_counterNoise >= m_freqNoise)
							{
								m_counterNoise = 0;
								m_noiseVal &= 0x1FFFF;
								m_noiseVal = (((m_noiseVal >> 16) ^ (m_noiseVal >> 13)) & 1) ^ ((m_noiseVal << 1) + 1);
								m_outNoiseABC = (byte)((m_noiseVal >> 16) & 1);
								m_outNoiseABC |= (byte)((m_outNoiseABC << 1) | (m_outNoiseABC << 2));
							}

							// signals counters...
							if (++m_counterA >= m_freqA)
							{
								m_counterA = 0;
								m_outABC ^= 1;
							}
							if (++m_counterB >= m_freqB)
							{
								m_counterB = 0;
								m_outABC ^= 2;
							}
							if (++m_counterC >= m_freqC)
							{
								m_counterC = 0;
								m_outABC ^= 4;
							}
							if (++m_counterBend >= m_freqBend)
							{
								m_counterBend = 0;
								ChangeEnvelope();
							}
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