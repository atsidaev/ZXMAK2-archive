using System;
using System.Text;

using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Engine.Devices.Ula
{
    public class UlaSpectrum48_Early : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "ZX Spectrum 48 [early model]"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);

            bmgr.SubscribeRDNOMREQ(0xC000, 0x4000, ContendNoMreq);
            bmgr.SubscribeWRNOMREQ(0xC000, 0x4000, ContendNoMreq);

            bmgr.SubscribeRDIO(0x0000, 0x0000, ReadPortAll);
            bmgr.SubscribeWRIO(0x0000, 0x0000, WritePortAll);
        }

        #endregion

        public UlaSpectrum48_Early()
        {
            // ZX Spectrum 48
            // Total Size:          //+ 448 x 312
            // Visible Size:        //+ 352 x 303 (48+256+48 x 55+192+56)

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 64;
            c_ulaFirstPaperTact = 64;      // 64 [40sync+24border+128scr+32border]
            c_frameTactCount = 69888;
            c_ulaBorder4T = true;
            c_ulaBorder4Tstage = 0;

            c_ulaBorderTop = 55;      //56 (at least 48=border, other=retrace or border)
            c_ulaBorderBottom = 56;   //
            c_ulaBorderLeftT = 24;    //16T
            c_ulaBorderRightT = 24;   //32T

            c_ulaIntBegin = 64;
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

        protected void ReadMem4000(ushort addr, ref byte value)
        {
            contendMemory();
        }

        #region The same as 128

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
            contendPortLate(addr);
            if ((addr & 0x0001) == 0)
            {
                int frameTact = (int)((CPU.Tact - 2) % FrameTactCount);
                UpdateState(frameTact);
                PortFE = value;
            }
        }

        private void ReadPortAll(ushort addr, ref byte value, ref bool iorqge)
        {
            contendPortEarly(addr);
            contendPortLate(addr);
            int frameTact = (int)((CPU.Tact - 1) % FrameTactCount);
            base.ReadPortFF(frameTact, ref value);
        }

        #endregion

        #endregion

        private bool IsContended(int addr)
        {
            int test = addr & 0xC000;
            return (test == 0x4000);
        }

        private bool IsPortUla(int addr)
        {
            return (addr & 1) == 0;
        }

        #region The same as 128

        private void contendMemory()
        {
            int frameTact = (int)(CPU.Tact % c_frameTactCount);
            CPU.Tact += m_contention[frameTact];
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
            int shift = 1;
            int frameTact = (int)((CPU.Tact + shift) % c_frameTactCount);

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

        protected override void OnTimingChanged()
        {
            base.OnTimingChanged();
            
            // build early model table...
            m_contention = new int[c_frameTactCount];
            int[] byteContention = new int[] { 6, 5, 4, 3, 2, 1, 0, 0, };
            for (int t = 0; t < c_frameTactCount; t++)
            {
                int shifted = (t + 1) + c_ulaIntBegin;
                // check overflow
                if (shifted < 0)
                    shifted += c_frameTactCount;
                shifted %= c_frameTactCount;

                m_contention[t] = 0;
                int line = shifted / c_ulaLineTime;
                int pix = shifted % c_ulaLineTime;
                if (line < c_ulaFirstPaperLine || line >= (c_ulaFirstPaperLine + 192))
                {
                    m_contention[t] = 0;
                    continue;
                }
                int scrPix = pix - c_ulaFirstPaperTact;
                if (scrPix < 0 || scrPix >= 128)
                {
                    m_contention[t] = 0;
                    continue;
                }
                int pixByte = scrPix % 8;

                m_contention[t] = byteContention[pixByte];
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

    public class UlaSpectrum48 : UlaSpectrum48_Early
    {
        public override string Name { get { return "ZX Spectrum 48 [late model]"; } }

        public UlaSpectrum48()
        {
            c_ulaFirstPaperTact += 1;
            c_ulaBorder4Tstage = (c_ulaBorder4Tstage + 1) & 3;
        }
    }
}
