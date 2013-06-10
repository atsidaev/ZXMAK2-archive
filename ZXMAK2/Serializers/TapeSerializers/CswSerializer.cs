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
            var hdr = new byte[0x34];
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
            var version = hdr[0x17];
            if (version > 2)
            {
                DialogProvider.Show(
                    string.Format("Format CSW V{0}.{1} not supported!", hdr[0x17], hdr[0x18]),
                    "CSW loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
                return;
            }
            if (version == 2)  // CSW V2
            {
                stream.Read(hdr, 0x20, 0x14);
                byte[] extHdr = new byte[hdr[0x23]];
                stream.Read(extHdr, 0, extHdr.Length);
            }
            var cswSampleRate = version == 2 ?
                BitConverter.ToInt32(hdr, 0x19) : 
                BitConverter.ToUInt16(hdr, 0x19);
            var cswCompressionType = version == 2 ?
                hdr[0x21] : 
                hdr[0x1B];

            var dataSize = version == 2 ?
                BitConverter.ToInt32(hdr, 0x1D) :
                stream.Length - 0x20;
            var buf = new byte[dataSize];

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

            var tactsPerSecond = _tape.TactsPerSecond;
            var rate = tactsPerSecond / (double)cswSampleRate; // usually 3.5mhz / 44khz

            var list = new List<TapeBlock>();
            var pulses = new List<int>();
            var blockTime = 0;
            var blockCounter = 0;

            for (var ptr = 0; ptr < buf.Length; )
            {
                double rle = buf[ptr++];
                if (rle == 0x00)
                {
                    rle = BitConverter.ToInt32(buf, ptr);
                    ptr += 4;
                }
                var len = (int)Math.Round(rle * rate, MidpointRounding.AwayFromZero);
                pulses.Add(len);

                blockTime += len;
                if (blockTime >= tactsPerSecond * 2)
                {
                    var tb = new TapeBlock();
                    tb.Description = string.Format("CSW-{0:D3}", blockCounter++);
                    tb.Periods = new List<int>(pulses);
                    list.Add(tb);
                    blockTime = 0;
                    pulses.Clear();
                }
            }
            if (pulses.Count > 0)
            {
                var tb = new TapeBlock();
                tb.Description = string.Format("CSW-{0:D3}", blockCounter++);
                tb.Periods = new List<int>(pulses);
                list.Add(tb);
            }
            _tape.Blocks.Clear();
            _tape.Blocks.AddRange(list);
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
