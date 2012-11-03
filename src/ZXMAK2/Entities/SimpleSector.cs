using System;


namespace ZXMAK2.Engine.Devices.Disk
{
	public class SimpleSector : Sector
	{
		private bool _adPresent = false;
		private bool _dataPresent = false;
		private byte[] _adMark = new byte[4];
		private byte[] _data = new byte[0];

		public override byte[] Data { get { return _data; } }
		public override bool DataPresent { get { return _dataPresent; } }

		public override bool AdPresent { get { return _adPresent; } }
		public override byte C { get { return _adMark[0]; } }
		public override byte H { get { return _adMark[1]; } }
		public override byte R { get { return _adMark[2]; } }
		public override byte N { get { return _adMark[3]; } }


		/// <summary>
		/// Make sector
		/// </summary>
		/// <param name="cc">Cylinder</param>
		/// <param name="hh">Head</param>
		/// <param name="rr">Sector Number</param>
		/// <param name="nn">Sector Size Code</param>
		public SimpleSector(int cc, int hh, int rr, int nn, byte[] data)
		{
			_adPresent = true;
			_adMark[0] = (byte)cc;
			_adMark[1] = (byte)hh;
			_adMark[2] = (byte)rr;
			_adMark[3] = (byte)nn;
			if (data != null)
			{
				_dataPresent = true;
				_data = data;
			}
			else
			{
				_dataPresent = false;
			}
		}

		public SimpleSector(int cc, int hh, int rr, int nn) 
			: this(cc,hh,rr,nn,new byte[128 << (nn&7)]) { }

		public SimpleSector(byte[] data)
			: this(0,0,0,0, data)
		{
			_adPresent = false;		
		}
	}
}
