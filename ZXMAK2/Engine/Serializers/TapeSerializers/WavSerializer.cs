using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Serializers.SnapshotSerializers;


namespace ZXMAK2.Engine.Serializers.TapeSerializers
{
	public class WavSerializer : FormatSerializer
	{
		private ITapeDevice m_tape;


		public WavSerializer(ITapeDevice tape)
		{
			m_tape = tape;
		}


		#region FormatSerializer

		public override string FormatGroup { get { return "Tape images"; } }
		public override string FormatName { get { return "WAV image"; } }
		public override string FormatExtension { get { return "WAV"; } }

		public override bool CanDeserialize { get { return true; } }

		public override void Deserialize(Stream stream)
		{
			m_tape.Blocks.Clear();
			List<int> pulses = new List<int>();

			WavStreamReader reader = new WavStreamReader(stream);
			int rate = m_tape.TactsPerSecond / reader.Header.sampleRate; // usually 3.5mhz / 44khz
			int smpCounter = 0;
			int state = reader.ReadNext();
			for (int i = 0; i < reader.Count; i++)
			{
				int sample = reader.ReadNext();
				smpCounter++;
				if ((state < 0 && sample < 0) || (state >= 0 && sample >= 0))
					continue;
				pulses.Add(smpCounter * rate);
				smpCounter = 0;
				state = sample;
			}
			pulses.Add(m_tape.TactsPerSecond / 10);

			TapeBlock tb = new TapeBlock();
			tb.Description = "WAV tape image";
			tb.Periods = pulses;
			m_tape.Blocks.Add(tb);
			m_tape.Reset();
		}

		#endregion
	}

	public class WavStreamReader
	{
		private Stream m_stream;
		private WavHeader m_header = new WavHeader();

		public WavStreamReader(Stream stream)
		{
			m_stream = stream;
			m_header.Deserialize(stream);
		}

		public WavHeader Header { get { return m_header; } }

		public int Count { get { return m_header.dataSize / m_header.fmtBlockAlign; } }

		public Int32 ReadNext()
		{
			// check - sample should be in PCM format
			if (m_header.fmtCode != WAVE_FORMAT_PCM &&
				m_header.fmtCode != WAVE_FORMAT_IEEE_FLOAT)
			{
				throw new FormatException(string.Format(
					"Not supported audio format: fmtCode={0}, bitDepth={1}",
					m_header.fmtCode,
					m_header.bitDepth));
			}
			byte[] data = new byte[m_header.fmtBlockAlign];
			m_stream.Read(data, 0, data.Length);
			if (m_header.fmtCode == WAVE_FORMAT_PCM)
			{
				// use first channel only
				if (m_header.bitDepth == 8)
					return getSamplePcm8(data, 0, 0);
				if (m_header.bitDepth == 16)
					return getSamplePcm16(data, 0, 0);
				if (m_header.bitDepth == 24)
					return getSamplePcm24(data, 0, 0);
				if (m_header.bitDepth == 32)
					return getSamplePcm32(data, 0, 0);
			}
			else if (m_header.fmtCode == WAVE_FORMAT_IEEE_FLOAT)
			{
				// use first channel only
				if (m_header.bitDepth == 32)
					return getSampleFloat32(data, 0, 0);
				if (m_header.bitDepth == 64)
					return getSampleFloat64(data, 0, 0);
			}
			throw new NotSupportedException(string.Format(
				"Not supported audio format ({0}/{1} bit)",
				m_header.fmtCode == WAVE_FORMAT_PCM ? "PCM" : "FLOAT",
				m_header.bitDepth));
		}

		private Int32 getSamplePcm8(byte[] bufferRaw, int offset, int channel)
		{
			return bufferRaw[offset + channel] - 128;
		}

		private Int32 getSamplePcm16(byte[] bufferRaw, int offset, int channel)
		{
			return BitConverter.ToInt16(bufferRaw, offset + 2 * channel);
		}

		private Int32 getSamplePcm24(byte[] bufferRaw, int offset, int channel)
		{
			Int32 result;
			int subOffset = offset + channel * 3;
			if (BitConverter.IsLittleEndian)
			{
				result = ((sbyte)bufferRaw[2 + subOffset]) * 0x10000;
				result |= bufferRaw[1 + subOffset] * 0x100;
				result |= bufferRaw[0 + subOffset];
			}
			else
			{
				result = ((sbyte)bufferRaw[0 + subOffset]) * 0x10000;
				result |= bufferRaw[1 + subOffset] * 0x100;
				result |= bufferRaw[2 + subOffset];
			}
			return result;
		}

		private Int32 getSamplePcm32(byte[] bufferRaw, int offset, int channel)
		{
			return BitConverter.ToInt32(bufferRaw, offset + 4 * channel);
		}

		private Int32 getSampleFloat32(byte[] data, int offset, int channel)
		{
			float fSample = BitConverter.ToSingle(data, offset + 4 * channel);
			// convert to 32 bit integer
			return (Int32)(fSample * Int32.MaxValue);
		}

		private Int32 getSampleFloat64(byte[] data, int offset, int channel)
		{
			double fSample = BitConverter.ToDouble(data, offset + 8 * channel);
			// convert to 32 bit integer
			return (Int32)(fSample * Int32.MaxValue);
		}

		private const int WAVE_FORMAT_PCM = 1;              /* PCM */
		private const int WAVE_FORMAT_IEEE_FLOAT = 3;       /* IEEE float */
		private const int WAVE_FORMAT_ALAW = 6;             /* 8-bit ITU-T G.711 A-law */
		private const int WAVE_FORMAT_MULAW = 7;            /* 8-bit ITU-T G.711 µ-law */
		private const int WAVE_FORMAT_EXTENSIBLE = 0xFFFE;  /* Determined by SubFormat */
	}

	public class WavHeader
	{
		// RIFF chunk (12 bytes)
		public Int32 chunkID;           // "RIFF"
		public Int32 fileSize;
		public Int32 riffType;          // "WAVE"

		// Format chunk (24 bytes)
		public Int32 fmtID;             // "fmt "
		public Int32 fmtSize;
		public Int16 fmtCode;
		public Int16 channels;
		public Int32 sampleRate;
		public Int32 fmtAvgBPS;
		public Int16 fmtBlockAlign;
		public Int16 bitDepth;
		public Int16 fmtExtraSize;

		// Data chunk
		public Int32 dataID;            // "data"
		public Int32 dataSize;          // The data size should be file size - 36 bytes.


		public void Deserialize(Stream stream)
		{
			StreamHelper.Read(stream, out chunkID);
			StreamHelper.Read(stream, out fileSize);
			StreamHelper.Read(stream, out riffType);
			if (chunkID != BitConverter.ToInt32(Encoding.ASCII.GetBytes("RIFF"), 0))
			{
				throw new FormatException("Invalid WAV file header");
			}
			if (riffType != BitConverter.ToInt32(Encoding.ASCII.GetBytes("WAVE"), 0))
			{
				throw new FormatException(string.Format(
					"Not supported RIFF type: '{0}'",
					Encoding.ASCII.GetString(BitConverter.GetBytes(riffType))));
			}
			Int32 chunkId;
			Int32 chunkSize;
			while (stream.Position < stream.Length)
			{
				StreamHelper.Read(stream, out chunkId);
				StreamHelper.Read(stream, out chunkSize);
				string strChunkId = Encoding.ASCII.GetString(
					BitConverter.GetBytes(chunkId));
				if (strChunkId == "fmt ")
				{
					read_fmt(stream, chunkId, chunkSize);
				}
				else if (strChunkId == "data")
				{
					read_data(stream, chunkId, chunkSize);
					break;
				}
				else
				{
					stream.Seek(chunkSize, SeekOrigin.Current);
				}
			}
			if (fmtID != BitConverter.ToInt32(Encoding.ASCII.GetBytes("fmt "), 0))
			{
				throw new FormatException("WAV format chunk not found");
			}
			if (dataID != BitConverter.ToInt32(Encoding.ASCII.GetBytes("data"), 0))
			{
				throw new FormatException("WAV data chunk not found");
			}
		}

		private void read_data(Stream stream, int chunkId, int chunkSize)
		{
			dataID = chunkId;
			dataSize = chunkSize;
		}

		private void read_fmt(Stream stream, int chunkId, int chunkSize)
		{
			fmtID = chunkId;
			fmtSize = chunkSize;
			StreamHelper.Read(stream, out fmtCode);
			StreamHelper.Read(stream, out channels);
			StreamHelper.Read(stream, out sampleRate);
			StreamHelper.Read(stream, out fmtAvgBPS);
			StreamHelper.Read(stream, out fmtBlockAlign);
			StreamHelper.Read(stream, out bitDepth);
			if (fmtSize == 18)
			{
				// Read any extra values
				StreamHelper.Read(stream, out fmtExtraSize);
				stream.Seek(fmtExtraSize, SeekOrigin.Current);
			}
		}
	}
}
