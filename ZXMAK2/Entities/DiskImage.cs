/// Description: Disk image emulation
/// Author: Alex Makeev
/// Date: 31.01.2008
using System;
using System.Collections;
using System.IO;

using ZXMAK2.Engine.Serializers;
using System.Collections.Generic;


namespace ZXMAK2.Engine.Devices.Disk
{
	public enum ModifyFlag
	{
		None = 0,
		SectorLevel = 1,
		TrackLevel = 2
	}
	public delegate void SaveDiskDelegate(DiskImage sender);

	// TODO: DiskImage.Present logic
	public class DiskImage
	{
		#region private data

		private bool _present = false;
		private long _rotateTime;
		private long _indexTime;
		private List<Track[]> _cylynderList = new List<Track[]>();     // SECHDR[2] list

		private int _headSide = 0;
		private int _headCylynder = 0;
		private int _sideCount = 0;
		private bool _writeProtect = true;
		private Track _nullTrack;

		private ModifyFlag _modifyFlag = ModifyFlag.None;
		private string _fileName = string.Empty;
		#endregion

		//#BEGIN us0374
		public Track t;
		//#END us0374


		#region public

		private DiskLoadManager m_serializer;

		public DiskImage()
		{
		}

		public void Init(long rotateTime)
		{
			_rotateTime = rotateTime;
			_indexTime = rotateTime / 50;
			_nullTrack = new Track(rotateTime);
			m_serializer = new DiskLoadManager(this);
		}

		public DiskLoadManager SerializeManager { get { return m_serializer; } }

		public string Description = string.Empty;

		public bool Present
		{
			get { return _present; }
			set { _present = value; }
		}

		public event SaveDiskDelegate SaveDisk;
		protected void OnSaveDisk()
		{
			if (SaveDisk != null)
				SaveDisk(this);
		}

		public string FileName
		{
			get { return _fileName; }
			set { _fileName = value; }
		}

		public long motor = 0;

		public int CylynderCount { get { return _cylynderList.Count; } }

		public int SideCount { get { return _sideCount; } }

		public int HeadSide
		{
			get { return _headSide; }
			set { _headSide = value & 1; }
		}

		public int HeadCylynder
		{
			get { return _headCylynder; }
			set
			{
				if (value >= _cylynderList.Count)
					value = _cylynderList.Count - 1;
				if (value < 0)
					value = 0;
				_headCylynder = value;
			}
		}

		public Track CurrentTrack
		{
			get
			{
				if (_headCylynder >= _cylynderList.Count || _headSide >= _sideCount)
					return _nullTrack;
				return _cylynderList[_headCylynder][_headSide];
			}
		}

		public void SetPhysics(int cylynderCount, int sideCount)
		{
			_cylynderList.Clear();
			_sideCount = sideCount;
			for (int i = 0; i < cylynderCount; i++)
			{
				Track[] cyl = new Track[_sideCount];
				for (int j = 0; j < cyl.Length; j++)
				{
					cyl[j] = new Track(_rotateTime);
					byte[] rawImage = new byte[6400];
					byte[] rawClock = new byte[rawImage.Length / 8 + (((rawImage.Length & 7) != 0) ? 1 : 0)];
					cyl[j].AssignImage(rawImage, rawClock);
				}
				_cylynderList.Add(cyl);
			}
			_modifyFlag = ModifyFlag.None;
		}

		public void Format()
		{
			SetPhysics(80, 2);
			format_trdos();
		}

		public bool IsREADY { get { return _cylynderList.Count > 0; } }

		public bool IsTRK00 { get { return _headCylynder == 0; } }

		public bool IsINDEX(long time)
		{
			return (time % _rotateTime) < _indexTime;
		}

		public bool IsWP
		{
			get { return _writeProtect; }
			set { _writeProtect = value; }
		}

		public ModifyFlag ModifyFlag
		{
			get { return _modifyFlag; }
			set { _modifyFlag = value; }
		}
		#endregion

		#region Loader methods (logical accessors, track image getter)
		private byte[] il = new byte[] //format_trdos interleave table
		{ 
			1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16
		};
		public void format_trdos()
		{
			// format empty disk...
			for (int cyl = 0; cyl < _cylynderList.Count; cyl++)
			{
				Track[] cylinder = (Track[])_cylynderList[cyl];
				for (int side = 0; side < cylinder.Length; side++)
				{
					ArrayList sectorList = new ArrayList();
					for (int sec = 0; sec < 16; sec++)	// 16 sectors per track
					{
						SimpleSector sector = new SimpleSector(cyl, 0, il[sec], 1, new byte[256]);
						for (int i = 0; i < 256; i++)
							sector.Data[i] = 0x00;
						sector.SetAdCrc(true);
						sector.SetDataCrc(true);
						sectorList.Add(sector);
					}
					cylinder[side].AssignSectors(sectorList);
				}
			}


			// TRDOS level format: 2544 secs (80 cyls, 2 sides)
			byte[] trsec = new byte[256];
			for (int i = 0; i < trsec.Length; i++)
				trsec[i] = 0x00;

			trsec[0xE2] = 0x01; trsec[0xE3] = 0x16;
			trsec[0xE5] = 0xF0; trsec[0xE6] = 0x09;
			trsec[0xE7] = 0x10;
			for (int i = 0xEA; i <= 0xF2; i++)
				trsec[i] = 0x20;
			trsec[0xF5] = 0x5A;  // Z
			trsec[0xF6] = 0x58;  // X
			trsec[0xF7] = 0x4D;  // M
			trsec[0xF8] = 0x41;  // A
			trsec[0xF9] = 0x4B;  // K
			trsec[0xFA] = 0x32;  // 2
			trsec[0xFB] = 0x20;
			trsec[0xFC] = 0x20;
			writeLogicalSector(0, 0, 9, trsec);
			_modifyFlag = ModifyFlag.None;
		}

		public void writeLogicalSector(int cyl, int side, int sec, byte[] buffer)
		{
			if (cyl < 0 || cyl >= _cylynderList.Count)
				return;
			Track[] tracks = (Track[])_cylynderList[cyl];
			if (side < 0 || side >= _sideCount)
				return;
			Track trk = tracks[side];

			// find sector...
			for (int sh = 0; sh < trk.HeaderList.Count; sh++)
			{
				SECHDR h = (SECHDR)trk.HeaderList[sh];
				if (h.n == sec && h.c == cyl && h.dataOffset > 0 && h.datlen >= buffer.Length)
				{
					int doLen = buffer.Length;
					if (doLen > h.datlen)
						doLen = h.datlen;
					for (int i = 0; i < doLen; i++)
						trk.RawWrite(h.dataOffset + i, buffer[i], false);
					//TODO: fix crc for non 3-byte sunchropulse
					ushort crc = trk.WD1793_CRC(h.dataOffset - 1, h.datlen + 1);
					trk.RawWrite(h.dataOffset + h.datlen, (byte)crc, false);
					trk.RawWrite(h.dataOffset + h.datlen + 1, (byte)(crc >> 8), false);
					h.crc2 = crc;
					h.c2 = true;
					break;
				}
			}
		}

		public void readLogicalSector(int cyl, int side, int sec, byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; i++)
				buffer[i] = 0x00;

			if (cyl < 0 || cyl >= _cylynderList.Count)
				return;
			Track[] tracks = (Track[])_cylynderList[cyl];
			if (side < 0 || side >= _sideCount)
				return;
			Track trk = tracks[side];

			// find sector...
			for (int sh = 0; sh < trk.HeaderList.Count; sh++)
			{
				SECHDR h = (SECHDR)trk.HeaderList[sh];
				if (h.n == sec && h.c == cyl && h.dataOffset > 0)
				{
					int doLen = h.datlen;
					if (doLen > buffer.Length)
						doLen = buffer.Length;
					for (int i = 0; i < doLen; i++)
						buffer[i] = trk.RawRead(h.dataOffset + i);
					break;
				}
			}
		}

		public Track GetTrackImage(int cyl, int side)
		{
			return ((Track[])_cylynderList[cyl])[side];
		}
		#endregion

		public void Connect()
		{
			if (Present)
			{
				if (!string.IsNullOrEmpty(FileName))
				{
					SerializeManager.OpenFileName(FileName, IsWP);
				}
				else
				{
					SetPhysics(80, 2);
					format_trdos();
				}
			}
			m_isConnected = true;
		}

		public void Disconnect()
		{
			if (Present && !IsWP && ModifyFlag != Disk.ModifyFlag.None)
				OnSaveDisk();
			m_isConnected = false;
		}

		private bool m_isConnected = false;
		public bool IsConnected { get { return m_isConnected; } }
	}
}