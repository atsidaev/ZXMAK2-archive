﻿// Sound resampler used in this class based on Unreal Speccy code created by SMT
// Reference version: us0.37.6
//
using System;
using System.Xml;
using System.Collections.Generic;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine;
using ZXMAK2.Entities;

namespace ZXMAK2.Hardware
{
	public abstract class SoundRendererDeviceBase : BusDeviceBase, ISoundRenderer, IConfigurable
	{
		#region IBusDevice

		public override string Description { get { return "Sound Device"; } }
		public override BusDeviceCategory Category { get { return BusDeviceCategory.Sound; } }

		public override void BusInit(IBusManager bmgr)
		{
			m_sndQueue.Clear();
			m_cpu = bmgr.CPU;
			IUlaDevice ula = (IUlaDevice)bmgr.FindDevice(typeof(IUlaDevice));
			FrameTactCount = ula.FrameTactCount;

			bmgr.SubscribeBeginFrame(BeginFrame);
			bmgr.SubscribeEndFrame(EndFrame);
		}

		//private WavSampleWriter m_wavWriter;
		public override void BusConnect()
		{
			//m_wavWriter = new WavSampleWriter(this.GetType().Name+".wav", 44100, 16, 2); 
		}

		public override void BusDisconnect()
		{
			//m_wavWriter.Dispose();
		}

		#endregion

		#region ISoundRenderer

		public uint[] AudioBuffer
		{
			get
			{
				//LogAgent.Debug("GetBuffer");
				return m_audioBuffer;
			}
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
				int oldVolume = m_volume;
				m_volume = value;
				OnVolumeChanged(oldVolume, m_volume);
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

		#region Bus Handlers

		protected virtual void BeginFrame()
		{
			//int frameTact = (int)((m_cpu.Tact + 1) % FrameTactCount);
			//LogAgent.Debug("BeginFrame @ {0}T", frameTact);
			m_sndout_len = 0;
			m_lastDacTact = 0;
			while (m_sndQueue.Count > 0)
			{
				m_sndout[m_sndout_len] = m_sndQueue.Dequeue();
				m_sndout_len++;
			}
		}

		protected virtual void EndFrame()
		{
			//int frameTact = (int)((m_cpu.Tact + 1) % FrameTactCount);
			//LogAgent.Debug("EndFrame @ {0}T", frameTact);
			render(m_sndout, m_sndout_len, m_frameTactCount);
			//m_wavWriter.Write(m_audioBuffer, 0, m_audioBuffer.Length);
		}

		#endregion


		private Z80CPU m_cpu;

		private int m_volume = 100;
		private int m_frameTactCount = 0;
		private uint[] m_audioBuffer = new uint[882];    // beeper frame sound samples
		private bool m_useFilter = false;

		public SoundRendererDeviceBase()
		{
			Volume = 100;
		}

		protected int FrameTactCount
		{
			get { return m_frameTactCount; }
			set { m_frameTactCount = value; setTimings(value, 44100); }
		}

		protected void UpdateDAC(ushort leftChannel, ushort rightChannel)
		{
			int frameTact = (int)(m_cpu.Tact % FrameTactCount);
			UpdateDAC(frameTact, leftChannel, rightChannel); 
		}

		protected void UpdateDAC(int frameTact, ushort leftChannel, ushort rightChannel)
		{
			if (frameTact < m_lastDacTact)
			{
				SNDOUT sndout = new SNDOUT();
				sndout.timestamp = frameTact;
				sndout.left = leftChannel;
				sndout.right = rightChannel;
				m_sndQueue.Enqueue(sndout);
			}
			else
			{
				m_sndout[m_sndout_len].timestamp = frameTact;
				m_sndout[m_sndout_len].left = leftChannel;
				m_sndout[m_sndout_len].right = rightChannel;
				m_sndout_len++;
			}
			m_lastDacTact = frameTact;
		}

		protected abstract void OnVolumeChanged(int oldVolume, int newVolume);

		private void render(SNDOUT[] src, int len, int clk_ticks)
		{
			try
			{
				start_frame();
				for (int index = 0; index < len; index++)
				{
					// if (src[index].timestamp > clk_ticks) continue; // wrong input data leads to crash
					update(src[index].timestamp, src[index].left, src[index].right);
				}
				end_frame(clk_ticks);
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void start_frame()
		{
			m_dst_start = m_dstpos = 0;
			m_base_tick = m_tick;
		}

		private void update(int timestamp, uint l, uint r)
		{
			if ((l ^ m_mix_l) == 0 && (r ^ m_mix_r) == 0)
				return;

			//[vv]   unsigned endtick = (timestamp * mult_const) >> MULT_C;
			ulong endtick = ((uint)timestamp * (ulong)m_sample_rate * TICK_F) / m_clock_rate;
			flush((uint)(m_base_tick + endtick));
			m_mix_l = l; m_mix_r = r;
		}

		private int end_frame(int clk_ticks)
		{
			// adjusting 'clk_ticks' with whole history will fix accumulation of rounding errors
			//uint64_t endtick = ((passed_clk_ticks + clk_ticks) * mult_const) >> MULT_C;
			ulong endtick = ((m_passed_clk_ticks + (uint)clk_ticks) * (ulong)m_sample_rate * TICK_F) / m_clock_rate;
			flush((uint)(endtick - m_passed_snd_ticks));

			int ready_samples = m_dstpos - m_dst_start;

			m_tick -= (uint)(ready_samples << (int)TICK_FF);
			m_passed_snd_ticks += (uint)(ready_samples << (int)TICK_FF);
			m_passed_clk_ticks += (uint)clk_ticks;

			return ready_samples;
		}

		private void flush(uint endtick)
		{
			uint scale;
			if (((endtick ^ m_tick) & ~(TICK_F - 1)) == 0)
			{
				//same discrete as before
				scale = filter_diff[(endtick & (TICK_F - 1)) + TICK_F] - filter_diff[(m_tick & (TICK_F - 1)) + TICK_F];
				m_s2_l += m_mix_l * scale;
				m_s2_r += m_mix_r * scale;

				scale = filter_diff[endtick & (TICK_F - 1)] - filter_diff[m_tick & (TICK_F - 1)];
				m_s1_l += m_mix_l * scale;
				m_s1_r += m_mix_r * scale;

				m_tick = endtick;
			}
			else /* myfix */ //if (m_dstpos < m_audioBuffer.Length)
			{
				scale = filter_sum_full_u - filter_diff[(m_tick & (TICK_F - 1)) + TICK_F];

				uint sample_value;
				if (m_useFilter)
				{
					/*lame noise reduction by Alone Coder*/
					int templeft = (int)(m_mix_l * scale + m_s2_l);
					/*olduseleft = useleft;
					if (firstsmp)useleft=oldfrmleft,firstsmp--;
					else*/
					m_useleft = (int)(((long)templeft + (long)m_oldleft) / 2);
					m_oldleft = templeft;
					int tempright = (int)(m_mix_r * scale + m_s2_r);
					/*olduseright = useright;
					if (firstsmp)useright=oldfrmright,firstsmp--;
						else*/
					m_useright = (int)(((long)tempright + (long)m_oldright) / 2);
					m_oldright = tempright;
					sample_value = (uint)(m_useleft >> 16) | (uint)(m_useright & 0xFFFF0000);
					/**/
				}
				else
				{
					sample_value = ((m_mix_l * scale + m_s2_l) >> 16) |
						((m_mix_r * scale + m_s2_r) & 0xFFFF0000);
				}

//#if SND_EXTERNAL_BUFFER
				m_audioBuffer[m_dstpos] = sample_value;
				m_dstpos++;
//#endif

				scale = filter_sum_half_u - filter_diff[m_tick & (TICK_F - 1)];
				m_s2_l = m_s1_l + m_mix_l * scale;
				m_s2_r = m_s1_r + m_mix_r * scale;

				m_tick = (m_tick | (TICK_F - 1)) + 1;

				if (((endtick ^ m_tick) & ~(TICK_F - 1)) != 0)
				{
					// assume filter_coeff is symmetric
					uint val_l = m_mix_l * filter_sum_half_u;
					uint val_r = m_mix_r * filter_sum_half_u;
					do
					{
						//if (m_dstpos >= m_audioBuffer.Length) /* myfix */
						//    break;
						uint sample_value2;
						if (m_useFilter)
						{
							/*lame noise reduction by Alone Coder*/
							int templeft = (int)(m_s2_l + val_l);
							/*olduseleft = useleft;
							if (firstsmp)useleft=oldfrmleft,firstsmp--;
							   else*/
							m_useleft = (int)(((long)templeft + (long)m_oldleft) / 2);
							m_oldleft = templeft;
							int tempright = (int)(m_s2_r + val_r);
							/*olduseright = useright;
							if (firstsmp)useright=oldfrmright,firstsmp--;
							   else*/
							m_useright = (int)(((long)tempright + (long)m_oldright) / 2);
							m_oldright = tempright;
							sample_value2 = (uint)(m_useleft >> 16) | (uint)(m_useright & 0xFFFF0000);
							/**/
						}
						else
						{
							sample_value2 = ((m_s2_l + val_l) >> 16) +
										   ((m_s2_r + val_r) & 0xFFFF0000); // save s2+val
						}

//#if SND_EXTERNAL_BUFFER
						m_audioBuffer[m_dstpos] = sample_value2;
						m_dstpos++;
//#endif
						m_tick += TICK_F;
						m_s2_l = val_l; m_s2_r = val_r; // s2=s1, s1=0;

					} while (((endtick ^ m_tick) & ~(TICK_F - 1)) != 0);
				}

				m_tick = endtick;

				scale = filter_diff[(endtick & (TICK_F - 1)) + TICK_F] - filter_sum_half_u;
				m_s2_l += m_mix_l * scale;
				m_s2_r += m_mix_r * scale;

				scale = filter_diff[endtick & (TICK_F - 1)];
				m_s1_l = m_mix_l * scale;
				m_s1_r = m_mix_r * scale;
			}
		}

		private uint m_mix_l, m_mix_r;
		private int m_dstpos, m_dst_start;
		private uint m_clock_rate, m_sample_rate;

		private uint m_tick, m_base_tick;
		private uint m_s1_l, m_s1_r;
		private uint m_s2_l, m_s2_r;
		private int m_oldleft, m_useleft;
		private int m_oldright, m_useright;

		private ulong m_passed_clk_ticks, m_passed_snd_ticks;
		private uint m_mult_const;
		private SNDOUT[] m_sndout;
		private int m_sndout_len;
		private Queue<SNDOUT> m_sndQueue = new Queue<SNDOUT>();
		private int m_lastDacTact;

		private void setTimings(int frameTactCount, int sampleRate)
		{
			m_clock_rate = (uint)frameTactCount * 50;
			m_sample_rate = (uint)sampleRate;
			m_sndout = new SNDOUT[frameTactCount];

			m_tick = 0; m_dstpos = m_dst_start = 0;
			m_passed_snd_ticks = m_passed_clk_ticks = 0;

			m_mult_const = (uint)(((ulong)m_sample_rate << (int)(MULT_C + TICK_FF)) / m_clock_rate);
		}

		private struct SNDOUT
		{
			public int timestamp; // in 'system clock' ticks
			public uint left;
			public uint right;
		}

		#region filter

		static SoundRendererDeviceBase()
		{
			filter_diff = new uint[TICK_F * 2];
			double sum = 0;
			for (int i = 0; i < TICK_F * 2; i++)
			{
				filter_diff[i] = (uint)(int)(sum * 0x10000);
				sum += filter_coeff[i];
			}
		}

		private static uint[] filter_diff;
		private const double filter_sum_full = 1.0, filter_sum_half = 0.5;
		private const uint filter_sum_full_u = (uint)(filter_sum_full * 0x10000),
			filter_sum_half_u = (uint)(filter_sum_half * 0x10000);


		private const uint TICK_FF = 6;			// oversampling ratio: 2^6 = 64
		private const uint TICK_F = 1 << 6;
		private const uint MULT_C = 12;			// fixed point precision for 'system tick -> sound tick'

		private static double[] filter_coeff = new double[]// [TICK_F*2]
		{
   // filter designed with Matlab's DSP toolbox
   0.000797243121022152, 0.000815206499600866, 0.000844792477531490, 0.000886460636664257,
   0.000940630171246217, 0.001007677515787512, 0.001087934129054332, 0.001181684445143001,
   0.001289164001921830, 0.001410557756409498, 0.001545998595893740, 0.001695566052785407,
   0.001859285230354019, 0.002037125945605404, 0.002229002094643918, 0.002434771244914945,
   0.002654234457752337, 0.002887136343664226, 0.003133165351783907, 0.003391954293894633,
   0.003663081102412781, 0.003946069820687711, 0.004240391822953223, 0.004545467260249598,
   0.004860666727631453, 0.005185313146989532, 0.005518683858848785, 0.005860012915564928,
   0.006208493567431684, 0.006563280932335042, 0.006923494838753613, 0.007288222831108771,
   0.007656523325719262, 0.008027428904915214, 0.008399949736219575, 0.008773077102914008,
   0.009145787031773989, 0.009517044003286715, 0.009885804729257883, 0.010251021982371376,
   0.010611648461991030, 0.010966640680287394, 0.011314962852635887, 0.011655590776166550,
   0.011987515680350414, 0.012309748033583185, 0.012621321289873522, 0.012921295559959939,
   0.013208761191466523, 0.013482842243062109, 0.013742699838008606, 0.013987535382970279,
   0.014216593638504731, 0.014429165628265581, 0.014624591374614174, 0.014802262449059521,
   0.014961624326719471, 0.015102178534818147, 0.015223484586101132, 0.015325161688957322,
   0.015406890226980602, 0.015468413001680802, 0.015509536233058410, 0.015530130313785910,
   0.015530130313785910, 0.015509536233058410, 0.015468413001680802, 0.015406890226980602,
   0.015325161688957322, 0.015223484586101132, 0.015102178534818147, 0.014961624326719471,
   0.014802262449059521, 0.014624591374614174, 0.014429165628265581, 0.014216593638504731,
   0.013987535382970279, 0.013742699838008606, 0.013482842243062109, 0.013208761191466523,
   0.012921295559959939, 0.012621321289873522, 0.012309748033583185, 0.011987515680350414,
   0.011655590776166550, 0.011314962852635887, 0.010966640680287394, 0.010611648461991030,
   0.010251021982371376, 0.009885804729257883, 0.009517044003286715, 0.009145787031773989,
   0.008773077102914008, 0.008399949736219575, 0.008027428904915214, 0.007656523325719262,
   0.007288222831108771, 0.006923494838753613, 0.006563280932335042, 0.006208493567431684,
   0.005860012915564928, 0.005518683858848785, 0.005185313146989532, 0.004860666727631453,
   0.004545467260249598, 0.004240391822953223, 0.003946069820687711, 0.003663081102412781,
   0.003391954293894633, 0.003133165351783907, 0.002887136343664226, 0.002654234457752337,
   0.002434771244914945, 0.002229002094643918, 0.002037125945605404, 0.001859285230354019,
   0.001695566052785407, 0.001545998595893740, 0.001410557756409498, 0.001289164001921830,
   0.001181684445143001, 0.001087934129054332, 0.001007677515787512, 0.000940630171246217,
   0.000886460636664257, 0.000844792477531490, 0.000815206499600866, 0.000797243121022152
		};
		#endregion
	}

	//public class WavSampleWriter : IDisposable
	//{
	//    private string m_fileName;
	//    private int m_sampleRate;
	//    private int m_sampleBits;
	//    private int m_channelCount;
	//    private System.IO.FileStream m_fileStream;
	//    private RIFF_HDR m_riffHdr = new RIFF_HDR();
	//    private RIFF_CHUNK_FMT m_riffChunkFmt = new RIFF_CHUNK_FMT();
	//    private RIFF_CHUNK_FACT m_riffChunkFact = new RIFF_CHUNK_FACT();
		
	//    public WavSampleWriter(string fileName, int sampleRate, int sampleBits, int channelCount)
	//    {
	//        m_fileName = fileName;
	//        m_sampleRate = sampleRate;
	//        m_sampleBits = sampleBits;
	//        m_channelCount = channelCount;
	//        m_fileStream = new System.IO.FileStream(
	//            fileName, 
	//            System.IO.FileMode.Create, 
	//            System.IO.FileAccess.Write, 
	//            System.IO.FileShare.Read);

	//        m_riffChunkFact.value = 0;

	//        m_riffChunkFmt.wFormatTag = 0x0001;
	//        m_riffChunkFmt.wChannels = (ushort)channelCount;
	//        m_riffChunkFmt.dwSamplesPerSec = (uint)sampleRate;
	//        m_riffChunkFmt.wBitsPerSample = (ushort)sampleBits;
	//        m_riffChunkFmt.wBlockAlign = (ushort)(m_riffChunkFmt.wChannels * m_riffChunkFmt.wBitsPerSample / 8);  //0x0004;
	//        m_riffChunkFmt.dwAvgBytesPerSec = m_riffChunkFmt.dwSamplesPerSec * m_riffChunkFmt.wBlockAlign;

	//        m_riffHdr.RiffSize = 4 +						// wave_id	
	//            RIFF_CHUNK_FMT.Size +						// FMT
	//            RIFF_CHUNK_FACT.Size;						// FACT

	//        m_fileStream.Seek(0, System.IO.SeekOrigin.Begin);
	//        m_riffHdr.Write(m_fileStream);
	//        m_riffChunkFmt.Write(m_fileStream);
	//        m_riffChunkFact.Write(m_fileStream);
	//        RIFF_CHUNK_DATA riffData = new RIFF_CHUNK_DATA(m_riffChunkFact.value);
	//        riffData.Write(m_fileStream);
	//    }

	//    public void Write(uint[] buffer, int offset, int length)
	//    {
	//        m_fileStream.Seek(0, System.IO.SeekOrigin.End);
	//        for (int i = 0; i < length; i++)
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(m_fileStream, buffer[i+offset]);
			
	//        m_riffHdr.RiffSize = (uint)m_fileStream.Length-8;
	//        m_riffChunkFact.value += (uint)length;
			
	//        m_fileStream.Seek(0, System.IO.SeekOrigin.Begin);
	//        m_riffHdr.Write(m_fileStream);
	//        m_riffChunkFmt.Write(m_fileStream);
	//        m_riffChunkFact.Write(m_fileStream);
	//        RIFF_CHUNK_DATA riffData = new RIFF_CHUNK_DATA(m_riffChunkFact.value * m_riffChunkFmt.wBlockAlign);
	//        riffData.Write(m_fileStream);
	//    }
	

	//    public void  Dispose()
	//    {
	//        m_fileStream.Close();
	//    }


	//    private class RIFF_HDR      // size = 3*4 (align 4)
	//    {
	//        public UInt32 riff_id = BitConverter.ToUInt32(System.Text.Encoding.ASCII.GetBytes("RIFF"), 0);
	//        public UInt32 RiffSize;  // data after riff_hdr length
	//        public UInt32 wave_id = BitConverter.ToUInt32(System.Text.Encoding.ASCII.GetBytes("WAVE"), 0);

	//        public void Write(System.IO.FileStream stream)
	//        {
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, riff_id);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, RiffSize);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, wave_id);
	//        }

	//        public const uint Size = 3*4;
	//    }
	//    private class RIFF_CHUNK_FMT    // size = 2*4 + 0x12
	//    {
	//        public UInt32 id;	// identifier, e.g. "fmt " or "data"
	//        public UInt32 len;	// remaining chunk length after header

	//        public UInt16 wFormatTag;         // Format category
	//        public UInt16 wChannels;          // Number of channels
	//        public UInt32 dwSamplesPerSec;   // Sampling rate
	//        public UInt32 dwAvgBytesPerSec;  // For buffer estimation
	//        public UInt16 wBlockAlign;        // Data block size
	//        public UInt16 wBitsPerSample;     // Sample size
	//        public UInt16 __zero = 0x0000;

	//        public RIFF_CHUNK_FMT()
	//        {
	//            id = BitConverter.ToUInt32(System.Text.Encoding.ASCII.GetBytes("fmt "), 0);
	//            len = 0x12;
	//        }

	//        public void Write(System.IO.FileStream stream)
	//        {
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, id);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, len);
				
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, wFormatTag);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, wChannels);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, dwSamplesPerSec);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, dwAvgBytesPerSec);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, wBlockAlign);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, wBitsPerSample);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, __zero);
	//        }

	//        public const uint Size = 2*4 + 0x12;
	//    }
	//    private class RIFF_CHUNK_FACT    // size = 2*4 + 4
	//    {
	//        public UInt32 id;	// identifier, e.g. "fmt " or "data"
	//        public UInt32 len;	// remaining chunk length after header

	//        public UInt32 value;

	//        public RIFF_CHUNK_FACT()
	//        {
	//            id = BitConverter.ToUInt32(System.Text.Encoding.ASCII.GetBytes("fact"), 0);
	//            len = 4;
	//        }

	//        public void Write(System.IO.FileStream stream)
	//        {
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, id);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, len);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, value);
	//        }

	//        public const uint Size = 2 * 4 + 4;
	//    }
	//    private class RIFF_CHUNK_DATA    // size = 2*4 + ...
	//    {
	//        public UInt32 id;	// identifier, e.g. "fmt " or "data"
	//        public UInt32 len;	// remaining chunk length after header

	//        public RIFF_CHUNK_DATA(uint length)
	//        {
	//            id = BitConverter.ToUInt32(System.Text.Encoding.ASCII.GetBytes("data"), 0);
	//            len = length;
	//        }

	//        public void Write(System.IO.FileStream stream)
	//        {
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, id);
	//            ZXMAK2.Serializers.SnapshotSerializers.StreamHelper.Write(stream, len);
	//        }

	//        public const uint Size = 2*4;
	//    }
	//}
}
