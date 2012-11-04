using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;

using ZXMAK2.Interfaces;
using ZXMAK2.Entities;


namespace ZXMAK2.Serializers.TapeSerializers
{
	public class CswSerializer : FormatSerializer
	{
        private ITapeDevice _tape;


        public CswSerializer(ITapeDevice tape)
		{
			_tape = tape;
		}


		#region FormatSerializer

		public override string FormatGroup { get { return "Tape images"; } }
		public override string FormatName { get { return "CSW image"; } }
		public override string FormatExtension { get { return "CSW"; } }

		public override bool CanDeserialize { get { return true; } }

		public override void Deserialize(Stream stream)
		{
            _tape.Blocks.Clear();
            List<int> pulses = new List<int>();
			byte[] hdr = new byte[0x34];
			stream.Read(hdr, 0, 0x20);

			if (Encoding.ASCII.GetString(hdr, 0, 22) != "Compressed Square Wave")
			{
                DialogProvider.Show(
                    "Invalid CSW file, identifier not found! ", 
                    "CSW loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}
			if (hdr[0x17] > 2)
			{
				DialogProvider.Show(
                    string.Format("Format CSW V{0}.{1} not supported!", hdr[0x17], hdr[0x18]), 
                    "CSW loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}
			if (hdr[0x17] == 2)  // CSW V2
			{
				stream.Read(hdr, 0x20, 0x14);
				byte[] extHdr = new byte[hdr[0x23]];
				stream.Read(extHdr, 0, extHdr.Length);
			}
			int cswSampleRate = (hdr[0x17] == 2) ? BitConverter.ToInt32(hdr, 0x19) : BitConverter.ToUInt16(hdr, 0x19);
			int cswCompressionType = (hdr[0x17] == 2) ? hdr[0x21] : hdr[0x1B];

			byte[] buf;
			if (hdr[0x17] == 2)
				buf = new byte[BitConverter.ToInt32(hdr, 0x1D)];
			else
				buf = new byte[stream.Length - 0x20];

			if (cswCompressionType == 1)        // RLE
			{
				stream.Read(buf, 0, buf.Length);
			}
			else if (cswCompressionType == 2)   // Z-RLE
			{
				csw2_uncompress(stream, buf);
			}
			else
			{
				DialogProvider.Show(
                    "Unknown compression type!", 
                    "CSW loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}


			int rate = _tape.TactsPerSecond / cswSampleRate; // usually 3.5mhz / 44khz
			for (int ptr = 0; ptr < buf.Length; )
			{
				int len = buf[ptr++] * rate;
				if (len == 0)
				{
					len = BitConverter.ToInt32(buf, ptr) / rate;
					ptr += 4;
				}
				pulses.Add(len);
			}

			pulses.Add(_tape.TactsPerSecond / 10);
			TapeBlock tb = new TapeBlock();
			tb.Description = "CSW tape image";
			tb.Periods = pulses;
            _tape.Blocks.Add(tb);
			_tape.Reset();
		}

		#endregion


		#region private
		
		/// <summary>
		/// Decompress Z-RLE compressed stream
		/// </summary>
		/// <param name="stream">input stream</param>
		/// <param name="buffer">buffer to decompress</param>
		private int csw2_uncompress(Stream stream, byte[] buffer)
		{
			// fix problem known as "Block length does not match with its complement."
			// see http://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=97064
			stream.ReadByte();
			stream.ReadByte();

			DeflateStream gzip = new DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress, false);
			return gzip.Read(buffer, 0, buffer.Length);
		}

		#endregion
	}
}
