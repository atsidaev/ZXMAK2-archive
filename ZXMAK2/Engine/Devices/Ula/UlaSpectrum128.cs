using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices.Ula
{
    public class UlaSpectrum128 : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "ZX Spectrum 128"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM(0xC000, 0xC000, ReadMemC000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0xC000, ReadMemC000);

            bmgr.SubscribeRDNOMREQ(0xC000, 0x4000, ContendNoMreq);
            bmgr.SubscribeWRNOMREQ(0xC000, 0x4000, ContendNoMreq);
            bmgr.SubscribeRDNOMREQ(0xC000, 0xC000, ContendNoMreq);
            bmgr.SubscribeWRNOMREQ(0xC000, 0xC000, ContendNoMreq);

            bmgr.SubscribeRDIO(0x0000, 0x0000, ReadPortAll);
            bmgr.SubscribeWRIO(0x0000, 0x0000, WritePortAll);
        }

        #endregion

        public UlaSpectrum128()
        {
            // ZX Spectrum 128
            // Total Size:          //+ 456 x 311
            // Visible Size:        //+ 352 x 303 (48+256+48 x 55+192+56)

            c_ulaLineTime = 228;
            c_ulaFirstPaperLine = 63;
            c_ulaFirstPaperTact = 64;      // 64 [40sync+24border+128scr+32border]
			c_frameTactCount = 70908;

            c_ulaBorderTop = 55;      //56
            c_ulaBorderBottom = 56;   //
            c_ulaBorderLeftT = 24;    //16T
            c_ulaBorderRightT = 24;   //32T

            c_ulaIntBegin = 64 + 1;
            c_ulaIntLength = 36;    // according to fuse

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
            
            fillTable(true);
        }


        #region Bus Handlers

		protected override void WriteMem4000(ushort addr, byte value)
		{
			contendMemory();
			base.WriteMem4000(addr, value);
		}

		protected override void WriteMemC000(ushort addr, byte value)
		{
            if ( (m_pageC000&1) != 0 /*m_pageC000 == 1 || m_pageC000 == 3 || m_pageC000 == 5 || m_pageC000 == 7*/)
                contendMemory();
            base.WriteMemC000(addr, value);
		}

		protected void ReadMem4000(ushort addr, ref byte value)
		{
            contendMemory();
        }

		protected void ReadMemC000(ushort addr, ref byte value)
		{
			if ((m_pageC000 & 1) != 0 /*m_pageC000 == 1 || m_pageC000 == 3 || m_pageC000 == 5 || m_pageC000 == 7*/)
                contendMemory();
        }

        protected void ContendNoMreq(ushort addr)
        {
            if (IsContended(addr))
                contendMemory();
        }

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
        }

        private void WritePortAll(ushort addr, byte value, ref bool iorqge)
        {
			contendPortEarly(addr);
			if ((addr & 0x0001) == 0)
			{
				UpdateState((int)((CPU.Tact + 1) % FrameTactCount));  // -2 should be good for 4T border
				PortFE = value;
			}
			contendPortLate(addr);
		}

        private void ReadPortAll(ushort addr, ref byte value, ref bool iorqge)
        {
			contendPortEarly(addr);
			contendPortLate(addr);
            int frameTact = (int)((CPU.Tact - 1) % FrameTactCount);
            base.ReadPortFF(frameTact, ref value);
        }


        #endregion

		private void contendMemory()
		{
            int frameTact = (int)(CPU.Tact % c_frameTactCount);
            CPU.Tact += m_contention[frameTact];
		}

		private bool IsContended(int addr)
		{
			int test = addr & 0xC000;
			return (test == 0x4000 || (test == 0xC000 && (m_pageC000&1)!=0/*(m_pageC000 == 1 || m_pageC000 == 3 || m_pageC000 == 5 || m_pageC000 == 7)*/));
		}

		private bool IsPortUla(int addr)
		{
			return (addr & 1)==0;
		}

        private void contendPortEarly(int addr)
        {
            if (IsContended(addr))
            {
                int frameTact = (int)(CPU.Tact % c_frameTactCount);
                CPU.Tact += m_contention[frameTact];
            }
        }

        private void contendPortLate(int addr)
        {
			int shift = 1;// 2;
            int frameTact = (int)((CPU.Tact + shift) % c_frameTactCount);

            // if( port_from_ula( addr ) )...
            if (IsPortUla(addr))
            {
                CPU.Tact += m_contention[frameTact];
            }
			else if (IsContended(addr))
			{
				CPU.Tact += m_contention[frameTact]; frameTact += m_contention[frameTact]; frameTact++; frameTact %= c_frameTactCount;
				CPU.Tact += m_contention[frameTact]; frameTact += m_contention[frameTact]; frameTact++; frameTact %= c_frameTactCount;
				CPU.Tact += m_contention[frameTact]; frameTact += m_contention[frameTact]; frameTact++; frameTact %= c_frameTactCount;
			}
        }


        private void fillTable(bool lateModel)
		{
			m_contention = new int[c_frameTactCount];
            int[] byteContention = new int[] { 6, 5, 4, 3, 2, 1, 0, 0, };
            for (int t = 0; t < c_frameTactCount; t++)
			{
                int shifted = t - c_ulaIntBegin;
                if (!lateModel)
                    shifted -= 1;
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
                int scrPix = pix - c_ulaFirstPaperTact+1;
				if (scrPix < 0 || scrPix >= 128)
				{
					m_contention[shifted] = 0;
					continue;
				}
				int pixByte = scrPix % 8;

				m_contention[shifted] = byteContention[pixByte];
			}
		}

        // 128: http://www.worldofspectrum.org/faq/reference/128kreference.htm
        //  48: http://www.worldofspectrum.org/faq/reference/48kreference.htm
        // http://www.zxdesign.info/dynamicRam.shtml

        private int[] m_contention;


        #region Fuse

        /* Contention patterns */
        //private static int[] contention_pattern_65432100 = new int[] { 5, 4, 3, 2, 1, 0, 0, 6 }; //128
        //private static int[] contention_pattern_76543210 = new int[] { 0, 7, 6, 5, 4, 3, 2, 1 }; //???
        //return timings[ tstates_through_line % 8 ];

        #endregion
    }
}
