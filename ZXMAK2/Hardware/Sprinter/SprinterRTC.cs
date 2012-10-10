using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Hardware.Sprinter
{
	public class SprinterRTC : BusDeviceBase
	{
		#region IBusDevice Members

		public override BusCategory Category { get { return BusCategory.Other; } }
		public override string Description { get { return "Sprinter RTC"; } }
		public override string Name { get { return "Sprinter RTC"; } }

		public override void BusInit(IBusManager bmgr)
		{
			m_sandbox = bmgr.IsSandbox;
			m_bus = bmgr;

			m_bus.SubscribeRESET(Reset);
			m_bus.SubscribeWRIO(0xFFFF, 0xBFBD, CMOS_DWR);  //CMOS_DWR
			m_bus.SubscribeWRIO(0xFFFF, 0xDFBD, CMOS_AWR);  //CMOS_AWR
			m_bus.SubscribeRDIO(0xFFFF, 0xFFBD, CMOS_DRD);  //CMOS_DRD

			m_fileName = bmgr.GetSatelliteFileName("cmos");
		}

		public override void BusConnect()
		{
			if (!m_sandbox && m_fileName != null)
				load(m_fileName);
		}

		public override void BusDisconnect()
		{
			if (!m_sandbox && m_fileName != null)
				save(m_fileName);
		}

		#endregion

		private IBusManager m_bus;
		private bool m_sandbox;
		private byte[] m_eeprom = new byte[256];
		private byte m_addr;

		private void load(string fileName)
		{
			try
			{
				if (!File.Exists(fileName))
					return;
				using (FileStream eepromFile = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read))
				{
					if (eepromFile.Length < 256)
						eepromFile.Write(m_eeprom, 0, 256);
					else
						eepromFile.Read(m_eeprom, 0, 256);
					eepromFile.Flush();
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void save(string fileName)
		{
			try
			{
				using (FileStream eepromFile = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
				{
					eepromFile.Write(m_eeprom, 0, 256);
					eepromFile.Flush();
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}


		#region Bus

		void Reset()
		{
			m_addr = 0;
		}


		/// <summary>
		/// RTC control port
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="val"></param>
		/// <param name="iorqge"></param>


		/// <summary>
		/// RTC address port
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="val"></param>
		/// <param name="iorqge"></param>
		void CMOS_AWR(ushort addr, byte val, ref bool iorqge)
		{
			this.m_addr = val;
		}

		/// <summary>
		/// RTC write data port
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="val"></param>
		/// <param name="iorqge"></param>
		void CMOS_DWR(ushort addr, byte val, ref bool iorqge)
		{
			WrCMOS(val);
		}

		/// <summary>
		/// RTC read data port
		/// </summary>
		/// <param name="addr"></param>
		/// <param name="val"></param>
		/// <param name="iorqge"></param>
		void CMOS_DRD(ushort addr, ref byte val, ref bool iorqge)
		{
			val = RdCMOS();
		}

		#endregion


		#region RTC emu

		DateTime dt = DateTime.Now;
		bool UF = false;
		private string m_fileName = null;

		byte RdCMOS()
		{
			var curDt = DateTime.Now;

			if (curDt.Subtract(dt).Seconds > 0 || curDt.Millisecond / 500 != dt.Millisecond / 500)
			{
				dt = curDt;
				UF = true;
			}

			switch (m_addr)
			{
				case 0x00:
					return BDC(dt.Second);
				case 0x02:
					return BDC(dt.Minute);
				case 0x04:
					return BDC(dt.Hour);
				case 0x06:
					return (byte)(dt.DayOfWeek);
				case 0x07:
					return BDC(dt.Day);
				case 0x08:
					return BDC(dt.Month);
				case 0x09:
					return BDC(dt.Year % 100);
				case 0x0A:
					return 0x00;
				case 0x0B:
					return 0x02;
				case 0x0C:
					var res = (byte)(UF ? 0x1C : 0x0C);
					UF = false;
					return res;
				case 0x0D:
					return 0x80;

				default:
					return m_eeprom[m_addr];
			}
		}

		void WrCMOS(byte val)
		{
			if (m_addr < 0xF0)
				m_eeprom[m_addr] = val;
		}

		byte BDC(int val)
		{
			var res = val;

			if ((m_eeprom[11] & 4) == 0)
			{
				var rem = 0;
				res = Math.DivRem(val, 10, out rem);
				res = (res * 16 + rem);
			}

			return (byte)res;
		}

		#endregion
	}
}
