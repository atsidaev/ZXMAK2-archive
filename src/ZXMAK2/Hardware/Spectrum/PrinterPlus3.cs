using System;
using System.IO;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;

namespace ZXMAK2.Hardware.Spectrum
{
	public class PrinterPlus3 : BusDeviceBase
	{
		#region IBusDevice

		public override string Name { get { return "Printer Plus-3 (Centronix)"; } }

		public override string Description { get { return "Printer to file (settings not implemented yet)"; } }

		public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

		public override void BusInit(IBusManager bmgr)
		{
			bmgr.SubscribeRdIo(0xF002, 0x0000, portDataRead);
			bmgr.SubscribeWrIo(0xF002, 0x0000, portDataWrite);
			bmgr.SubscribeWrIo(0xF002, 0x1000, portStrbWrite);
		}

		public override void BusConnect()
		{
		}

		public override void BusDisconnect()
		{
		}

		#endregion

		private byte m_data = 0;
		private byte m_strb = 0;
		
		private void portDataWrite(ushort addr, byte value, ref bool iorqge)
		{
			if (!iorqge)
				return;
			iorqge = false;
			m_data = value;
		}

		private void portDataRead(ushort addr, ref byte value, ref bool iorqge)
		{
			if (!iorqge)
				return;
			iorqge = false;
			value &= 0xFE;	// reset BUSY flag to show ready state
		}

		private void portStrbWrite(ushort addr, byte value, ref bool iorqge)
		{
			if ((m_strb&0x10)==0 && (value&0x10)!=0)
			{
				//using (FileStream fs = new FileStream("C:\\ZXPRN.TXT", FileMode.Append, FileAccess.Write, FileShare.Read))
				//{
				//    fs.WriteByte(m_data);
				//}
			}
			m_strb = value;
		}
	}
}
