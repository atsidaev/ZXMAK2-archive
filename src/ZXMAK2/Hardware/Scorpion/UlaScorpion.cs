using System;
using ZXMAK2.Interfaces;

namespace ZXMAK2.Hardware.Scorpion
{
	public class UlaScorpion : UlaDeviceBase
	{
		#region IBusDevice

		public override string Name { get { return "Scorpion"; } }

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);
			bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, busRDM1);
			bmgr.SubscribeRDMEM_M1(0xC000, 0x8000, busRDM1);
			bmgr.SubscribeRDMEM_M1(0xC000, 0xC000, busRDM1);
		}

		#endregion

		public UlaScorpion()
		{
			// Scorpion
			// Total Size:          448 x 312
			// Visible Size:        368 x 296 (48+256+64 x 64+192+40)
			// First Line Border:   0
			// First Line Paper:    64
			// Paper Lines:         192
			// Bottom Border Lines: 40

			c_ulaLineTime = 224;
			c_ulaFirstPaperLine = 64;
			c_ulaFirstPaperTact = 64;      // 64 [40sync+24border+128scr+32border]
			c_frameTactCount = 69888;//+
			c_ulaBorder4T = true;
			c_ulaBorder4Tstage = 3;

			c_ulaBorderTop = 24;//64;
			c_ulaBorderBottom = 24;// 40;
			c_ulaBorderLeftT = 16;// 24;  //24
			c_ulaBorderRightT = 16;// 24; //32

			c_ulaIntBegin = 64 - 3;
			c_ulaIntLength = 32;    // according to fuse

			c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
			c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
		}

		#region Bus Handlers

		private void busRDM1(ushort addr, ref byte value)
		{
			CPU.Tact += CPU.Tact & 1;
		}

		#endregion

		#region UlaDeviceBase

		protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
		{
			if (!Memory.DOSEN && (addr & 0x23) == (0xFE & 0x23))
				base.WritePortFE(addr, value, ref iorqge);
		}

		#endregion
	}

	public class UlaScorpionEx : UlaScorpion
	{
		#region IBusDevice

		public override string Name { get { return "Scorpion [Extended Border]"; } }

		#endregion

		public UlaScorpionEx()
		{
			// Scorpion
			// Total Size:          448 x 312
			// Visible Size:        368 x 296 (48+256+64 x 64+192+40)
			// First Line Border:   0
			// First Line Paper:    64
			// Paper Lines:         192
			// Bottom Border Lines: 40

			c_ulaBorderTop = 64;
			c_ulaBorderBottom = 40;
			c_ulaBorderLeftT = 24;  //24
			c_ulaBorderRightT = 24; //32

			c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
			c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
		}
	}
}
