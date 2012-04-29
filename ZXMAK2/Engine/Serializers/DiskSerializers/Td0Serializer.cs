/// Description: TD0 format serializer
/// Author: Alex Makeev
/// Date: 18.04.2008
using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using ZXMAK2.Engine.Devices.Disk;


namespace ZXMAK2.Engine.Serializers.DiskSerializers
{
	public class Td0Serializer : FormatSerializer
	{
		private DiskImage _diskImage;
		
		public Td0Serializer(DiskImage diskImage)
		{
			_diskImage = diskImage;
		}

		#region FormatSerializer

		public override string FormatGroup { get { return "Disk images"; } }
		public override string FormatName { get { return "TD0 disk image"; } }
		public override string FormatExtension { get { return "TD0"; } }

		public override bool CanDeserialize { get { return true; } }

		public override void Deserialize(Stream stream)
		{
			loadData(stream);
            _diskImage.ModifyFlag = ModifyFlag.None;
            _diskImage.Present = true;
        }

        public override void SetSource(string fileName)
        {
            _diskImage.FileName = fileName;
        }

        public override void SetReadOnly(bool readOnly)
        {
            _diskImage.IsWP = readOnly;
        }

		#endregion

		
		#region private
		
		private bool loadData(Stream stream)
		{
			TD0_MAIN_HEADER mainHdr = TD0_MAIN_HEADER.Deserialize(stream);

			if (mainHdr == null)
				return false;

			if (mainHdr.Ver > 21 || mainHdr.Ver < 10)           // 1.0 <= version <= 2.1...
			{
				DialogProvider.Show(
                    string.Format("Format version is not supported [0x{0:X2}]", mainHdr.Ver), 
                    "TD0 loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
                return false;
			}

			if (mainHdr.DataDOS != 0)
			{
				DialogProvider.Show(
                    "'DOS Allocated sectors were copied' option is not supported!", 
                    "TD0 loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return false;
			}
   
			Stream dataStream = stream;
			if(mainHdr.IsAdvandcedCompression)
			{
				if(mainHdr.Ver < 20 )    // unsupported Old Advanced compression
				{
					DialogProvider.Show(
                        "Old Advanced compression is not implemented!", 
                        "TD0 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
					return false;
				}
				dataStream = new LzssHuffmanStream(stream);
			}


			#region debug info
			//string state = "Main header loaded, ";
			//if (dataStream is LzssHuffmanStream)
			//    state += "compressed+";
			//else
			//    state += "compressed-";
			//state += ", ";
			//if ((mainHdr.Info & 0x80) != 0)
			//    state += "info+";
			//else
			//    state += "info-";
			//DialogProvider.ShowWarning(state, "TD0 loader");
			#endregion

			string description = string.Empty;
			if ((mainHdr.Info & 0x80) != 0)
			{
				byte[] tmp = new byte[4];
				dataStream.Read(tmp, 0, 2);						// crc
				dataStream.Read(tmp, 2, 2);						// length

				byte[] info = new byte[getUInt16(tmp, 2) + 10];
				for (int i = 0; i < 4; i++)
					info[i] = tmp[i];

				dataStream.Read(info, 4, 6);					// year,month,day,hour,min,sec (year is relative to 1900)
				dataStream.Read(info, 10, info.Length - 10);	// description

				if (CalculateTD0CRC(info, 2, 8 + getUInt16(info, 2)) != getUInt16(info, 0))
					DialogProvider.Show(
                        "Info crc wrong", 
                        "TD0 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Warning);
				// combine lines splitted by zero
				StringBuilder builder = new StringBuilder();
				int begin = 10, end=10;
				for (; end < info.Length; end++)
					if (info[end] == 0 && end > begin)
					{
						builder.Append(Encoding.ASCII.GetString(info, begin, end - begin));
						builder.Append("\n");
						begin = end+1;
					}
				description = builder.ToString();
			}
			
			int cylCount = -1;
			int sideCount = -1;
			ArrayList trackList = new ArrayList();
			for (; ; )
			{
				TD0_TRACK track = TD0_TRACK.Deserialize(dataStream);
				if (track.SectorCount == 0xFF) break;
				trackList.Add(track);

				if (cylCount < track.Cylinder) cylCount = track.Cylinder;
				if (sideCount < track.Side) sideCount = track.Side;
			}
			cylCount++;
			sideCount++;

			if (cylCount < 1 || sideCount < 1)
			{
				DialogProvider.Show(
                    "Invalid disk structure", 
                    "td0",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return false;
			}

			_diskImage.SetPhysics(cylCount, sideCount);
			foreach (TD0_TRACK trk in trackList)
				_diskImage.GetTrackImage(trk.Cylinder, trk.Side).AssignSectors(trk.SectorList);
			_diskImage.Description = description;
			return true;
		}

		#endregion

		#region td0 structs

		private class TD0_MAIN_HEADER			// 12 bytes
		{
			private byte[] _buffer = new byte[12];

			public TD0_MAIN_HEADER()
			{
				for (int i = 0; i < _buffer.Length; i++)
					_buffer[i] = 0;
			}

			//public ushort ID		// +0:  0x4454["TD"] - 'Normal'; 0x6474["td"] - packed LZH ('New Advanced data compression')
			//{
			//    get { return getUInt16(_buffer, 0); }
			//    set { setUint16(_buffer, 0, value); }
			//}
			//public byte __t;		// +2:  = 0x00   (id null terminator?)
			//public byte __1;		// +3:  ???
			public byte Ver		// +4:  Source version  (1.0 -> 10, ..., 2.1 -> 21)
			{
				get { return _buffer[4]; }
				set { _buffer[4] = value; }
			}
			//public byte __2;		// +5: 0x00 (except for akai)???
			public byte DiskType	// +6:  Source disk type
			{
				get { return _buffer[6]; }
				set { _buffer[6] = value; }
			}
			public byte Info		// +7:  D7-наличие image info; D0-D6 - буква дисковода
			{
				get { return _buffer[7]; }
				set { _buffer[7] = value; }
			}
			public byte DataDOS	// +8:  if(=0)'All sectors were copied', else'DOS Allocated sectors were copied'
			{
				get { return _buffer[8]; }
				set { _buffer[8] = value; }
			}
			public byte ChkdSides	// +9:  if(=1)'One side was checked', else'Both sides were checked'
			{
				get { return _buffer[9]; }
				set { _buffer[9] = value; }
			}
			//public ushort CRC		// +A:  CRC хидера TD0_MAIN_HEADER (кроме байт с CRC)
			//{
			//    get { return getUInt16(_buffer, 0xA); }
			//    set { setUint16(_buffer, 0xA, value); }
			//}
			//public ushort CalculateCRC()
			//{
			//    return CalculateTD0CRC(_buffer, 0, _buffer.Length-2);
			//}

			public bool IsAdvandcedCompression { get { return getUInt16(_buffer, 0) == 0x6474; } } // "td"

			public static TD0_MAIN_HEADER Deserialize(Stream stream)
			{
				TD0_MAIN_HEADER result = new TD0_MAIN_HEADER();
				stream.Read(result._buffer, 0, result._buffer.Length);
				
				ushort ID = getUInt16(result._buffer, 0);
				if (ID != 0x4454 && ID != 0x6474) // "TD"/"td"
				{
					LogAgent.Error("TD0 loader: Invalid header ID");
					DialogProvider.Show(
                        "Invalid header ID", 
                        "TD0 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
					return null;
				}

				ushort crc = CalculateTD0CRC(result._buffer, 0, result._buffer.Length - 2);
				ushort stampcrc = getUInt16(result._buffer, 0xA);
				if (stampcrc != crc)
				{
					LogAgent.Warn("TD0 loader: Main header had bad CRC=0x"+crc.ToString("X4")+" (stamp crc=0x"+stampcrc.ToString("X4")+")");
					DialogProvider.Show(
                        "Wrong main header CRC", 
                        "TD0 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Warning);
				}
				
				return result;
			}
			public void Serialize(Stream stream)
			{
				stream.Write(_buffer, 0, _buffer.Length);
			}
		}

		private class TD0_TRACK					// 4 bytes+sectors
		{
			private byte[] _rawData = new byte[4];
			private ArrayList _sectorList = new ArrayList();

			public int SectorCount { get { return _rawData[0]; } }
			public int Cylinder { get { return _rawData[1]; } }
			public int Side { get { return _rawData[2]; } }
			// last 1 byte: low byte of crc
			public ArrayList SectorList { get { return _sectorList; } }

			public static TD0_TRACK Deserialize(Stream stream)
			{
				TD0_TRACK hdr = new TD0_TRACK();
				stream.Read(hdr._rawData, 0, 4);
				if (hdr._rawData[0] != 0xFF)			// 0xFF - terminator
				{
					ushort crc = CalculateTD0CRC(hdr._rawData, 0, 3);
					if (hdr._rawData[3] != (crc & 0xFF))
					{
						LogAgent.Warn("TD0 loader: Track header had bad CRC=0x" + crc.ToString("X4") + " (stamp crc=0x" + hdr._rawData[3].ToString("X2") + ") [CYL:0x" + hdr._rawData[1].ToString("X2") + ";SIDE:" + hdr._rawData[2].ToString("X2"));
						DialogProvider.Show(
                            "Track header had bad CRC", 
                            "TD0 loader",
                            DlgButtonSet.OK,
                            DlgIcon.Warning);
					}

					ArrayList sectors = new ArrayList(hdr.SectorCount);
					for (int s = 0; s < hdr.SectorCount; s++)
						hdr._sectorList.Add(TD0_SECTOR.Deserialize(stream));
				}
				return hdr;
			}
		}

		[Flags]
		private enum SectorFlags
		{
			/// <summary>
			/// Sector was duplicated within a track.
			/// The meaning of some of these bits was taken  from  early  Teledisk
			/// documentation,  and may not be accurate - For example,  I've  seen
			/// images where sectors were duplicated within a track and the 01 bit
			/// was NOT set.
			/// </summary>
			DuplicatedWithinTrack = 0x01,
			/// <summary>
			/// Sector was read with a CRC error
			/// </summary>
			BadCrc = 0x02,
			/// <summary>
			/// Sector has a "deleted-data" address mark
			/// </summary>
			DeletedData = 0x04,
			/// <summary>
			/// Sector data was skipped based on DOS allocation.
			/// Bit values 20 or 10 indicate  that  NO  SECTOR  DATA  BLOCK	FOLLOWS.
			/// </summary>
			NoDataBlockDOS = 0x10,
			/// <summary>
			/// Sector had an ID field but not data.
			/// Bit values 20 or 10 indicate  that  NO  SECTOR  DATA  BLOCK	FOLLOWS.
			/// </summary>
			NoDataBlock = 0x20,
			/// <summary>
			/// Sector had data but no ID field (bogus header)
			/// </summary>
			NoAddressBlock = 0x40,
		}

		private class TD0_SECTOR : Sector
		{
			private TD0_SECTOR() { }
			
			private byte[] _admark;
			private byte[] _data;

			public override bool AdPresent { get { return (Td0Flags & SectorFlags.NoAddressBlock) == 0; } }
			public override bool DataPresent { get { return (Td0Flags & (SectorFlags.NoDataBlock | SectorFlags.NoDataBlockDOS)) == 0; } }
			public override bool DataDeleteMark { get { return (Td0Flags & SectorFlags.DeletedData) != 0; } }

			public override byte[] Data { get { return _data; } }
			public override byte C { get { return _admark[0]; } }
			public override byte H { get { return _admark[1]; } }
			public override byte R { get { return _admark[2]; } }
			public override byte N { get { return _admark[3]; } }

			public SectorFlags Td0Flags { get { return (SectorFlags)_admark[4]; } }

			public static TD0_SECTOR Deserialize(Stream stream)
			{
				TD0_SECTOR sector = new TD0_SECTOR();

				// C,H,R,N,Flags,hdrcrc low,hdrcrc high
				byte[] adm = new byte[6];
				stream.Read(adm, 0, 6);
				sector._admark = adm;

				// data size low, data size high, encoding method, rawdata,...
				byte[] datahdr = new byte[2];
				stream.Read(datahdr, 0, 2);
				byte[] rawdata = new byte[getUInt16(datahdr, 0)];
				stream.Read(rawdata, 0, rawdata.Length);
				sector._data = unpackData(rawdata);

				ushort crc = CalculateTD0CRC(sector._data, 0, sector._data.Length);
				if (adm[5] != (crc & 0xFF))
				{
					LogAgent.Warn(
                        "TD0 loader: Sector data had bad CRC=0x{0:X4} (stamp crc=0x{1:X2}) [C:{2:X2};H:{3:X2};R:{4:X2};N:{5:X2}", 
                        crc, 
                        adm[5], 
                        sector.C, 
                        sector.H, 
                        sector.R,
                        sector.N);
					DialogProvider.Show(
                        "Sector data had bad CRC", 
                        "TD0 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Warning);
				}
				
				sector.SetAdCrc(true);
				sector.SetDataCrc((sector.Td0Flags & SectorFlags.BadCrc) == 0);
				return sector;
			}

			private static byte[] unpackData(byte[] buffer)
			{
				List<byte> result = new List<byte>();
				int n;
				switch (buffer[0])
				{
					case 0:
						for (int i = 1; i < buffer.Length; i++)
							result.Add(buffer[i]);
						break;
					case 1:
						n = getUInt16(buffer, 1);
						for (int i = 0; i < n; i++)
						{
							result.Add(buffer[3]);
							result.Add(buffer[4]);
						}
						break;
					case 2:
						int index = 1;
						do
						{
							switch (buffer[index++])
							{
								case 0:
									n = buffer[index++];
									for (int i = 0; i < n; i++)
										result.Add(buffer[index++]);
									break;
								case 1:
									n = buffer[index++];
									for (int i = 0; i < n; i++)
									{
										result.Add(buffer[index]);
										result.Add(buffer[index+1]);
									}
									index += 2;
									break;
								default:
                                    LogAgent.Warn("Unknown sector encoding!");
                                    DialogProvider.Show(
                                        "Unknown sector encoding!", 
                                        "TD0 loader",
                                        DlgButtonSet.OK,
                                        DlgIcon.Warning);
									index = buffer.Length;
									break;
							}
						} while (index < buffer.Length);
						break;
				}
				return result.ToArray();
			}
		}

		#endregion


		#region TD0 CRC
		//----------------------------------------------------------------------------
		//
		// TD0 CRC - table&proc grabed from TDCHECK.EXE
		//
		private static byte[] tbltd0crc = new byte[512] 
		{
			0x00,0x00,0xA0,0x97,0xE1,0xB9,0x41,0x2E,0x63,0xE5,0xC3,0x72,0x82,0x5C,0x22,0xCB,
			0xC7,0xCA,0x67,0x5D,0x26,0x73,0x86,0xE4,0xA4,0x2F,0x04,0xB8,0x45,0x96,0xE5,0x01,
			0x2F,0x03,0x8F,0x94,0xCE,0xBA,0x6E,0x2D,0x4C,0xE6,0xEC,0x71,0xAD,0x5F,0x0D,0xC8,
			0xE8,0xC9,0x48,0x5E,0x09,0x70,0xA9,0xE7,0x8B,0x2C,0x2B,0xBB,0x6A,0x95,0xCA,0x02,
			0x5E,0x06,0xFE,0x91,0xBF,0xBF,0x1F,0x28,0x3D,0xE3,0x9D,0x74,0xDC,0x5A,0x7C,0xCD,
			0x99,0xCC,0x39,0x5B,0x78,0x75,0xD8,0xE2,0xFA,0x29,0x5A,0xBE,0x1B,0x90,0xBB,0x07,
			0x71,0x05,0xD1,0x92,0x90,0xBC,0x30,0x2B,0x12,0xE0,0xB2,0x77,0xF3,0x59,0x53,0xCE,
			0xB6,0xCF,0x16,0x58,0x57,0x76,0xF7,0xE1,0xD5,0x2A,0x75,0xBD,0x34,0x93,0x94,0x04,
			0xBC,0x0C,0x1C,0x9B,0x5D,0xB5,0xFD,0x22,0xDF,0xE9,0x7F,0x7E,0x3E,0x50,0x9E,0xC7,
			0x7B,0xC6,0xDB,0x51,0x9A,0x7F,0x3A,0xE8,0x18,0x23,0xB8,0xB4,0xF9,0x9A,0x59,0x0D,
			0x93,0x0F,0x33,0x98,0x72,0xB6,0xD2,0x21,0xF0,0xEA,0x50,0x7D,0x11,0x53,0xB1,0xC4,
			0x54,0xC5,0xF4,0x52,0xB5,0x7C,0x15,0xEB,0x37,0x20,0x97,0xB7,0xD6,0x99,0x76,0x0E,
			0xE2,0x0A,0x42,0x9D,0x03,0xB3,0xA3,0x24,0x81,0xEF,0x21,0x78,0x60,0x56,0xC0,0xC1,
			0x25,0xC0,0x85,0x57,0xC4,0x79,0x64,0xEE,0x46,0x25,0xE6,0xB2,0xA7,0x9C,0x07,0x0B,
			0xCD,0x09,0x6D,0x9E,0x2C,0xB0,0x8C,0x27,0xAE,0xEC,0x0E,0x7B,0x4F,0x55,0xEF,0xC2,
			0x0A,0xC3,0xAA,0x54,0xEB,0x7A,0x4B,0xED,0x69,0x26,0xC9,0xB1,0x88,0x9F,0x28,0x08,
			0xD8,0x8F,0x78,0x18,0x39,0x36,0x99,0xA1,0xBB,0x6A,0x1B,0xFD,0x5A,0xD3,0xFA,0x44,
			0x1F,0x45,0xBF,0xD2,0xFE,0xFC,0x5E,0x6B,0x7C,0xA0,0xDC,0x37,0x9D,0x19,0x3D,0x8E,
			0xF7,0x8C,0x57,0x1B,0x16,0x35,0xB6,0xA2,0x94,0x69,0x34,0xFE,0x75,0xD0,0xD5,0x47,
			0x30,0x46,0x90,0xD1,0xD1,0xFF,0x71,0x68,0x53,0xA3,0xF3,0x34,0xB2,0x1A,0x12,0x8D,
			0x86,0x89,0x26,0x1E,0x67,0x30,0xC7,0xA7,0xE5,0x6C,0x45,0xFB,0x04,0xD5,0xA4,0x42,
			0x41,0x43,0xE1,0xD4,0xA0,0xFA,0x00,0x6D,0x22,0xA6,0x82,0x31,0xC3,0x1F,0x63,0x88,
			0xA9,0x8A,0x09,0x1D,0x48,0x33,0xE8,0xA4,0xCA,0x6F,0x6A,0xF8,0x2B,0xD6,0x8B,0x41,
			0x6E,0x40,0xCE,0xD7,0x8F,0xF9,0x2F,0x6E,0x0D,0xA5,0xAD,0x32,0xEC,0x1C,0x4C,0x8B,
			0x64,0x83,0xC4,0x14,0x85,0x3A,0x25,0xAD,0x07,0x66,0xA7,0xF1,0xE6,0xDF,0x46,0x48,
			0xA3,0x49,0x03,0xDE,0x42,0xF0,0xE2,0x67,0xC0,0xAC,0x60,0x3B,0x21,0x15,0x81,0x82,
			0x4B,0x80,0xEB,0x17,0xAA,0x39,0x0A,0xAE,0x28,0x65,0x88,0xF2,0xC9,0xDC,0x69,0x4B,
			0x8C,0x4A,0x2C,0xDD,0x6D,0xF3,0xCD,0x64,0xEF,0xAF,0x4F,0x38,0x0E,0x16,0xAE,0x81,
			0x3A,0x85,0x9A,0x12,0xDB,0x3C,0x7B,0xAB,0x59,0x60,0xF9,0xF7,0xB8,0xD9,0x18,0x4E,
			0xFD,0x4F,0x5D,0xD8,0x1C,0xF6,0xBC,0x61,0x9E,0xAA,0x3E,0x3D,0x7F,0x13,0xDF,0x84,
			0x15,0x86,0xB5,0x11,0xF4,0x3F,0x54,0xA8,0x76,0x63,0xD6,0xF4,0x97,0xDA,0x37,0x4D,
			0xD2,0x4C,0x72,0xDB,0x33,0xF5,0x93,0x62,0xB1,0xA9,0x11,0x3E,0x50,0x10,0xF0,0x87,
		};
		private static ushort CalculateTD0CRC(byte[] buffer, int startIndex, int length)
		{
			ushort CRC = 0;
			int j;
			for(int i=0; i < length; i++)
			{
				CRC ^= buffer[startIndex++];
				j = CRC & 0xFF;
				CRC &= 0xFF00;
				CRC = (ushort)((CRC<<8)|(CRC>>8));
				// TODO: replace with ushort array
				CRC ^= getUInt16(tbltd0crc, j * 2);//((ushort*)tbltd0crc)[ j ];
			}
			return (ushort)((CRC << 8) | (CRC >> 8));
		}
		#endregion

	}
}
