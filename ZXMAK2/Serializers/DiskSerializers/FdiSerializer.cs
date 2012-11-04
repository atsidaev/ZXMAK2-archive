/// Description: FDI load helper
/// Author: Alex Makeev
/// Date: 15.04.2007
using System;
using System.IO;
using System.Text;
using System.Collections;

using ZXMAK2.Entities;


namespace ZXMAK2.Serializers.DiskSerializers
{
	public class FdiSerializer : FormatSerializer
	{
		private DiskImage _diskImage;
		
		
		public FdiSerializer(DiskImage diskImage)
		{
			_diskImage = diskImage;
		}

		
		#region FormatSerializer

		public override string FormatGroup { get { return "Disk images"; } }
		public override string FormatName { get { return "FDI disk image"; } }
		public override string FormatExtension { get { return "FDI"; } }

		public override bool CanDeserialize { get { return true; } }
		
		public override void Deserialize(Stream stream)
		{
			stream.Seek(0, SeekOrigin.Begin);
			loadData(stream);

			_diskImage.SetPhysics(_cylynderImages.Count, _sideCount);
			for (int cyl = 0; cyl < _cylynderImages.Count; cyl++)
			{
				byte[][][] cylynder = (byte[][][])_cylynderImages[cyl];
				for (int side = 0; side < _diskImage.SideCount; side++)
					_diskImage.GetTrackImage(cyl, side).AssignImage(cylynder[side][0], cylynder[side][1]);
			}
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


		#region private data
		private bool _writeProtect = false;
		private string _description = string.Empty;
		private ArrayList _cylynderImages = new ArrayList();
		private int _sideCount = 0;
		#endregion

		
		private void loadData(Stream stream)
		{
			_cylynderImages.Clear();
			_sideCount = 0;

			if (stream.Length < 14)
			{
				DialogProvider.Show(
                    "Corrupted disk image!", 
                    "FDI loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}

			byte[] hdr1 = new byte[14];
			stream.Read(hdr1, 0, 14);

			if (hdr1[0] != 0x46 || hdr1[1] != 0x44 || hdr1[2] != 0x49)
			{
				DialogProvider.Show(
                    "Invalid FDI file!", 
                    "FDI loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return;
			}

			_writeProtect = hdr1[3] != 0;
			int cylCount = hdr1[4] | (hdr1[5] << 8);
			_sideCount = hdr1[6] | (hdr1[7] << 8);

			int descrOffset = hdr1[8] | (hdr1[9] << 8);
			int dataOffset = hdr1[0xA] | (hdr1[0xB] << 8);
			int hdr2len = hdr1[0xC] | (hdr1[0xD] << 8);

			// TODO: check filesize!

			if (hdr2len > 0)
			{
				byte[] hdr2 = new byte[hdr2len];
				stream.Read(hdr2, 0, hdr2len);
			}

			ArrayList trackHeaderList = new ArrayList();
			for (int trk = 0; trk < cylCount; trk++)
				for (int side = 0; side < _sideCount; side++)
					trackHeaderList.Add(readTrackHeader(stream));

			for (int cyl = 0; cyl < cylCount; cyl++)
			{
				byte[][][] cylynderData = new byte[_sideCount][][];
				_cylynderImages.Add(cylynderData);

				for (int side = 0; side < _sideCount; side++)
				{
					ArrayList sectorHeaderList = (ArrayList)trackHeaderList[cyl * _sideCount + side];
					// Вычитываем массивы данных
					for (int sec = 0; sec < sectorHeaderList.Count; sec++)
					{
						SectorHeader sh = (SectorHeader)sectorHeaderList[sec];

						if ((sh.Flags & 0x40) != 0)   // нет массива данных?
							continue;

						int dataArrayLen = 128 << sh.N;
						//if ((sh.Flags & 0x01) != 0)      // CRC 128 OK?
						//   dataArrayLen = 128;
						//if ((sh.Flags & 0x02) != 0)      // CRC 256 OK?
						//   dataArrayLen = 256;
						//if ((sh.Flags & 0x04) != 0)      // CRC 1024 OK?
						//   dataArrayLen = 1024;
						//if ((sh.Flags & 0x08) != 0)      // CRC 2048 OK?
						//   dataArrayLen = 2048;
						//if ((sh.Flags & 0x10) != 0)      // CRC 4096 OK?
						//   dataArrayLen = 4096;
						sh.crcOk = (sh.Flags & 0x1F) != 0;

						sh.DataArray = new byte[dataArrayLen];
						stream.Seek(dataOffset + sh.DataOffset, SeekOrigin.Begin);
						stream.Read(sh.DataArray, 0, dataArrayLen);
					}

					// Формируем дорожку
					cylynderData[side] = generateTrackImage(sectorHeaderList);
				}
			}
		}


		#region private methods

		private byte[][] generateTrackImage(ArrayList sectorHeaderList)
		{
			byte[][] trackImage = new byte[2][];

			// Вычисляем необходимое число байт под данные:
			int imageSize = 6250;

			int SecCount = sectorHeaderList.Count;
			int trkdatalen = 0;
			for (int ilsec = 0; ilsec < SecCount; ilsec++)
			{
				SectorHeader hdr = (SectorHeader)sectorHeaderList[ilsec];

				trkdatalen += 2 + 6;     // for marks:   0xA1, 0xFE, 6bytes
				int SL = 128 << hdr.N;

				if ((hdr.Flags & 0x40) != 0)   // заголовок без массива данных
					SL = 0;
				else
					trkdatalen += 4;       // for data header/crc: 0xA1, 0xFB, ...,2bytes

				trkdatalen += SL;
			}

			int FreeSpace = imageSize - (trkdatalen + SecCount * (3 + 2));  // 3x4E & 2x00 per sector
			int SynchroPulseLen = 1; // 1 уже учтен в trkdatalen...
			int FirstSpaceLen = 1;
			int SecondSpaceLen = 1;
			int ThirdSpaceLen = 1;
			int SynchroSpaceLen = 1;
			FreeSpace -= FirstSpaceLen + SecondSpaceLen + ThirdSpaceLen + SynchroSpaceLen;
			if (FreeSpace < 0)
			{
				imageSize += -FreeSpace;
				FreeSpace = 0;
			}
			// Распределяем длины пробелов и синхропромежутка:
			while (FreeSpace > 0)
			{
				if (FreeSpace >= (SecCount * 2)) // Synchro for ADMARK & DATA
					if (SynchroSpaceLen < 12)
					{
						SynchroSpaceLen++;
						FreeSpace -= SecCount * 2;
					}
				if (FreeSpace < SecCount) break;

				if (FirstSpaceLen < 10) { FirstSpaceLen++; FreeSpace -= SecCount; }
				if (FreeSpace < SecCount) break;
				if (SecondSpaceLen < 22) { SecondSpaceLen++; FreeSpace -= SecCount; }
				if (FreeSpace < SecCount) break;
				if (ThirdSpaceLen < 60) { ThirdSpaceLen++; FreeSpace -= SecCount; }
				if (FreeSpace < SecCount) break;

				if ((SynchroSpaceLen >= 12) && (FirstSpaceLen >= 10) &&
					(SecondSpaceLen >= 22) && (ThirdSpaceLen >= 60))
					break;
			}
			// по возможности делаем три синхроимпульса...
			if (FreeSpace > (SecCount * 2) + 10) { SynchroPulseLen++; FreeSpace -= SecCount; }
			if (FreeSpace > (SecCount * 2) + 9) SynchroPulseLen++;
			if (FreeSpace < 0)
			{
				imageSize += -FreeSpace;
				FreeSpace = 0;
			}


			// Форматируем дорожку...
			trackImage[0] = new byte[imageSize];
			trackImage[1] = new byte[trackImage[0].Length / 8 + (((trackImage[0].Length & 7) != 0) ? 1 : 0)];

			int r, tptr = 0;
			for (int sec = 0; sec < SecCount; sec++)
			{
				SectorHeader hdr = (SectorHeader)sectorHeaderList[sec];

				for (r = 0; r < FirstSpaceLen; r++)        // Первый пробел
				{
					trackImage[0][tptr] = 0x4E;
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
				}
				for (r = 0; r < SynchroSpaceLen; r++)        // Синхропромежуток
				{
					trackImage[0][tptr] = 0x00;
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
				}
				int ptrcrc = tptr;
				for (r = 0; r < SynchroPulseLen; r++)        // Синхроимпульс
				{
					trackImage[0][tptr] = 0xA1;
					trackImage[1][tptr / 8] |= (byte)(1 << (tptr & 7));
					tptr++;
				}
				trackImage[0][tptr] = 0xFE;               // Метка "Адрес"
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;

				trackImage[0][tptr] = hdr.C;              // cyl
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;
				trackImage[0][tptr] = hdr.H;              // head
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;
				trackImage[0][tptr] = hdr.R;              // sector #
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;
				trackImage[0][tptr] = hdr.N;              // len code
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;

				ushort vgcrc = WD1793_CRC(trackImage[0], ptrcrc, tptr - ptrcrc);
				trackImage[0][tptr] = (byte)vgcrc;        // crc
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;
				trackImage[0][tptr] = (byte)(vgcrc >> 8);   // crc
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;

				for (r = 0; r < SecondSpaceLen; r++)        // Второй пробел
				{
					trackImage[0][tptr] = 0x4E;
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
				}
				for (r = 0; r < SynchroSpaceLen; r++)        // Синхропромежуток
				{
					trackImage[0][tptr] = 0x00;
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
				}

				byte fdiSectorFlags = hdr.Flags;
				// !!!!!!!!!
				// !WARNING! this feature of FDI format is NOT FULL DOCUMENTED!!!
				// !!!!!!!!!
				//
				//  Flags::bit6 - Возможно, 1 в данном разряде
				//                будет обозначать адресный маркер без области данных.
				//
				if ((fdiSectorFlags & 0x40) == 0) // oh-oh, data area can be not present... ;-) 
				{
					ptrcrc = tptr;
					for (r = 0; r < SynchroPulseLen; r++)        // Синхроимпульс
					{
						trackImage[0][tptr] = 0xA1;
						trackImage[1][tptr / 8] |= (byte)(1 << (tptr & 7));
						tptr++;
					}

					if ((fdiSectorFlags & 0x80) != 0)            // Метка "Удаленные данные"
						trackImage[0][tptr] = 0xF8;
					else
						trackImage[0][tptr] = 0xFB;            // Метка "Данные"
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;

					//TODO: sector len from crc flags?
					int SL = 128 << hdr.N;

					for (r = 0; r < SL; r++)        // сектор SL байт
					{
						trackImage[0][tptr] = hdr.DataArray[r];
						trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
						tptr++;
					}

					vgcrc = WD1793_CRC(trackImage[0], ptrcrc, tptr - ptrcrc);

					if ((fdiSectorFlags & 0x3F) == 0)         // CRC not correct?
						vgcrc ^= (ushort)0xFFFF;            // oh-oh, high technology... CRC bad... ;-)

					trackImage[0][tptr] = (byte)vgcrc;        // crc
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
					trackImage[0][tptr] = (byte)(vgcrc >> 8);   // crc
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
				}


				for (r = 0; r < ThirdSpaceLen; r++)        // Третий пробел
				{
					trackImage[0][tptr] = 0x4E;
					trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
					tptr++;
				}
			}
			for (int eoftrk = tptr; eoftrk < trackImage[0].Length; eoftrk++)
			{
				trackImage[0][tptr] = 0x4E;
				trackImage[1][tptr / 8] &= (byte)~(1 << (tptr & 7));
				tptr++;
			}

			return trackImage;
		}

		private ArrayList readTrackHeader(Stream f)
		{
			byte[] buf = new byte[7];
			ArrayList sectorHeaderList = new ArrayList();

			f.Read(buf, 0, 4);   // data offset in data block
			int dataOffset = buf[0] | (buf[1] << 8) | (buf[2] << 16) | (buf[3] << 24);

			f.Read(buf, 0, 2);   // reserved

			f.Read(buf, 0, 1);   // sector count
			int sectorCount = buf[0];

			for (int i = 0; i < sectorCount; i++)
			{
				f.Read(buf, 0, 7);
				SectorHeader sh = new SectorHeader();
				sh.C = buf[0];
				sh.H = buf[1];
				sh.R = buf[2];
				sh.N = buf[3];
				sh.Flags = buf[4];
				sh.DataOffset = dataOffset + (buf[5] | (buf[6] << 8));
				sectorHeaderList.Add(sh);
			}
			return sectorHeaderList;
		}

		static private ushort WD1793_CRC(byte[] data, int startIndex, int size)   // full CRC !!!
		{
			ushort CRC = 0xFFFF;
			while (size-- > 0)
			{
				CRC ^= (ushort)(data[startIndex++] << 8);
				for (int j = 0; j < 8; j++)
				{
					if ((CRC & 0x8000) != 0)
						CRC = (ushort)((CRC << 1) ^ 0x1021);
					else
						CRC <<= 1;
				}
			}
			return (ushort)((CRC >> 8) | (CRC << 8));
		}

		#endregion

		private class SectorHeader
		{
			public byte C;   // std data CYLYNDER?
			public byte H;   // std data HEAD?
			public byte R;   // std data
			public byte N;   // std data DATA ARRAY LEN

			public byte Flags;      // 012345=crcok(128,256,1024,2048,4096); 6=no data array; 7=0:normal marker/1:deleted marker
			public int DataOffset;  // sector data offset in track data block (dataOffset+trackOffset)
			public byte[] DataArray = null;

			public bool crcOk;
		}
	}
}
