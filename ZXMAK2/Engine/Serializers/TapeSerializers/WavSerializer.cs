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
            byte[] data = new byte[m_header.fmtBlockAlign];
            m_stream.Read(data, 0, data.Length);
            if (m_header.bitDepth == 8)
                return getSample8(data, 0, m_header.channels);
            if (m_header.bitDepth == 16)
                return getSample16(data, 0, m_header.channels);
            throw new NotSupportedException("Not supported WAV file format");
        }

        private Int32 getSample8(byte[] bufferRaw, int offset, short channelCount)
        {
            // use first channel only
            return bufferRaw[offset] - 127;
        }
        
        private Int32 getSample16(byte[] bufferRaw, int offset, short channelCount)
        {
            // use first channel only
            return BitConverter.ToInt32(bufferRaw, offset);
        }
    }

    public class WavHeader
    {
        // RIFF chunk (12 bytes)
        public Int32 chunkID;           // "RIFF"
        public Int32 fileSize;
        public Int32 riffType;
        
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
            StreamHelper.Read(stream, out fmtID);
            StreamHelper.Read(stream, out fmtSize);
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
            if (fmtID != BitConverter.ToInt32(Encoding.ASCII.GetBytes("fmt "), 0))
            {
                throw new FormatException("Invalid WAV file header");
            }
            StreamHelper.Read(stream, out dataID);
            StreamHelper.Read(stream, out dataSize);
            if (dataID != BitConverter.ToInt32(Encoding.ASCII.GetBytes("data"), 0))
            {
                throw new FormatException("Invalid WAV file header");
            }

            if (dataSize != fileSize - 36)
            {
                LogAgent.Warn(
                    "WavHeader.Deserialize: invalid dataSize={0}, used dataSize={1}",
                    dataSize,
                    fileSize - 36);
                dataSize = fileSize - 36;
            }
        }
    }
}
