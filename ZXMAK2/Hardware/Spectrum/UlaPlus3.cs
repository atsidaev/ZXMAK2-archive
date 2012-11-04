using System;
using ZXMAK2.Interfaces;

// Contended memory info links:
//  +3: http://scratchpad.wikia.com/wiki/Contended_memory#Instruction_breakdown
// 128: http://www.worldofspectrum.org/faq/reference/128kreference.htm
//  48: http://www.worldofspectrum.org/faq/reference/48kreference.htm
// http://www.zxdesign.info/dynamicRam.shtml
// examples: http://zxm.speccy.cz/realspec/

namespace ZXMAK2.Hardware.Spectrum
{
	public class UlaPlus3 : UlaDeviceBase
	{
		#region IBusDevice

		public override string Name { get { return "ZX Spectrum +2A/+3"; } }

		public override void BusInit(IBusManager bmgr)
		{
			base.BusInit(bmgr);
			bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
			bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
			bmgr.SubscribeRDMEM(0xC000, 0xC000, ReadMemC000);
			bmgr.SubscribeRDMEM_M1(0xC000, 0xC000, ReadMemC000);

			bmgr.SubscribeRDIO(0x0000, 0x0000, ReadPortAll);
			bmgr.SubscribeWRIO(0x0000, 0x0000, WritePortAll);
		}

		#endregion

		public UlaPlus3()
		{
			// ZX Spectrum +3
			// Total Size:          //+ 456 x 311
			// Visible Size:        //+ 352 x 303 (48+256+48 x 55+192+56)

			c_ulaLineTime = 228;
			c_ulaFirstPaperLine = 63;
			c_ulaFirstPaperTact = 64;      // 64 [40sync+24border+128scr+32border]
			c_frameTactCount = 70908;
			c_ulaBorder4T = true;
			c_ulaBorder4Tstage = 2;

			c_ulaBorderTop = 55;      //56
			c_ulaBorderBottom = 56;   //
			c_ulaBorderLeftT = 24;    //16T
			c_ulaBorderRightT = 24;   //32T

			c_ulaIntBegin = 64 + 2;
			c_ulaIntLength = 32;    // according to fuse

			c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
			c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
		}


		#region Bus Handlers

		protected override void WriteMem4000(ushort addr, byte value)
		{
			contendMemory();
			base.WriteMem4000(addr, value);
		}

		protected override void WriteMemC000(ushort addr, byte value)
		{
			if ((m_pageC000 & 1) != 0)
				contendMemory();
			base.WriteMemC000(addr, value);
		}

		protected void ReadMem4000(ushort addr, ref byte value)
		{
			contendMemory();
		}

		protected void ReadMemC000(ushort addr, ref byte value)
		{
			if ((m_pageC000 & 1) != 0)
				contendMemory();
		}

		#region The same as 48

		protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
		{
		}

		private void WritePortAll(ushort addr, byte value, ref bool iorqge)
		{
			if ((addr & 0x0001) == 0)
			{
				int frameTact = (int)((CPU.Tact - 2) % FrameTactCount);
				UpdateState(frameTact);
				PortFE = value;
			}
		}

		private void ReadPortAll(ushort addr, ref byte value, ref bool iorqge)
		{
			int frameTact = (int)((CPU.Tact - 1) % FrameTactCount);
			base.ReadPortFF(frameTact, ref value);
		}

		#endregion

		#endregion

		#region The same as 48

		private void contendMemory()
		{
			int frameTact = (int)(CPU.Tact % c_frameTactCount);
			CPU.Tact += m_contention[frameTact];
		}

		protected override void OnTimingChanged()
		{
			base.OnTimingChanged();

			// build early model table...
			m_contention = new int[c_frameTactCount];
			int[] byteContention = new int[] { 1, 0, 7, 6, 5, 4, 3, 2, };
			for (int t = 0; t < c_frameTactCount; t++)
			{
				int shifted = t - c_ulaIntBegin;
				if (shifted < 0)
					shifted += c_frameTactCount;

				m_contention[shifted] = 0;
				int line = t / c_ulaLineTime;
				int pix = t % c_ulaLineTime;
				if (line < c_ulaFirstPaperLine || line >= (c_ulaFirstPaperLine + 192))
				{
					m_contention[shifted] = 0;
					continue;
				}
				int scrPix = pix - c_ulaFirstPaperTact + 1;
				if (scrPix < 0 || scrPix >= 128)
				{
					m_contention[shifted] = 0;
					continue;
				}
				int pixByte = scrPix % 8;

				m_contention[shifted] = byteContention[pixByte];
			}
		}

		private int[] m_contention;

		#endregion

		//protected override void EndFrame()
		//{
		//    base.EndFrame();
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F1))
		//    {
		//        c_ulaBorder4T = true;
		//        c_ulaBorder4Tstage = 0;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F2))
		//    {
		//        c_ulaBorder4T = true;
		//        c_ulaBorder4Tstage = 1;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F3))
		//    {
		//        c_ulaBorder4T = true;
		//        c_ulaBorder4Tstage = 2;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F4))
		//    {
		//        c_ulaBorder4T = true;
		//        c_ulaBorder4Tstage = 3;
		//        OnTimingChanged();
		//    }
		//    if (IsKeyPressed(System.Windows.Forms.Keys.F5))
		//    {
		//        c_ulaBorder4T = false;
		//        OnTimingChanged();
		//    }
		//}
		//private static bool IsKeyPressed(System.Windows.Forms.Keys key)
		//{
		//    return (GetKeyState((int)key) & 0xFF00) != 0;
		//}
		//[System.Runtime.InteropServices.DllImport("user32")]
		//private static extern short GetKeyState(int vKey);
	}
}
