using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Hardware.Spectrum;


namespace ZXMAK2.Hardware.Clone
{
    public class UlaByte_Late : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Byte [late model]"; } }
        public override string Description { get { return "БАЙТ [late model]\r\nVersion 1.0"; } }

        public UlaByte_Late()
        {
            InitStaticTables();
        }


        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDNOMREQ(0xC000, 0x4000, NoMreq4000);
            bmgr.SubscribeWRNOMREQ(0xC000, 0x4000, NoMreq4000);
        }

        #endregion

        #region Bus Handlers

        protected override void WriteMem4000(ushort addr, byte value)
        {
            int frameTact = (int)(CPU.Tact % FrameTactCount);
            CPU.Tact += m_contention[frameTact];
            base.WriteMem4000(addr, value);
        }

        protected void ReadMem4000(ushort addr, ref byte value)
        {
            int frameTact = (int)(CPU.Tact % FrameTactCount);
            CPU.Tact += m_contention[frameTact];
        }

        protected void NoMreq4000(ushort addr)
        {
            int frameTact = (int)(CPU.Tact % FrameTactCount);
            CPU.Tact += m_contention[frameTact];
        }

        #endregion


        protected override SpectrumRendererParams CreateSpectrumRendererParams()
        {
            // Байт
            // Total Size:          448 x 312
            // Visible Size:        320 x 240 (32+256+32 x 24+192+24)
            var timing = SpectrumRenderer.CreateParams();
            timing.c_ulaLineTime = 224;
            timing.c_ulaFirstPaperLine = 64;
            timing.c_ulaFirstPaperTact = 56;//54; // +-2?
            timing.c_frameTactCount = 69888;
            timing.c_ulaBorder4T = true;
            timing.c_ulaBorder4Tstage = 0;

            timing.c_ulaBorderTop = 24;
            timing.c_ulaBorderBottom = 24;
            timing.c_ulaBorderLeftT = 16;
            timing.c_ulaBorderRightT = 16;

            timing.c_ulaIntBegin = 0;
            timing.c_ulaIntLength = 42;//32;
            timing.c_ulaFlashPeriod = 25;

            timing.c_ulaWidth = (timing.c_ulaBorderLeftT + 128 + timing.c_ulaBorderRightT) * 2;
            timing.c_ulaHeight = (timing.c_ulaBorderTop + 192 + timing.c_ulaBorderBottom);
            return timing;
        }

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            if ((addr & 0x34) == (0xFE & 0x34))
            {
                base.WritePortFE(addr, value, ref iorqge);
            }
        }

        protected override void OnTimingChanged()
        {
            base.OnTimingChanged();
            //m_contention = UlaSpectrum48.CreateContentionTable(
            //    SpectrumRenderer.Params,
            //    new int[] { 6, 5, 4, 3, 2, 1, 0, 0, });

            m_contention = CreateContentionTable(
                SpectrumRenderer.Params,
                m_dd10,
                m_dd11);
        }

        private static int[] CreateContentionTable(
            SpectrumRendererParams rendererParams,
            byte[] romDd10,
            byte[] romDd11)
        {
            var contention = new int[rendererParams.c_frameTactCount];
            //LogAgent.DumpAppend(
            //    "_dd10log.txt",
            //    "tact =\thsync\tpre34\tblclk\tlatch\thretr\tvsync\tpre56\tibclk\tvretr");
            //LogAgent.DumpAppend(
            //    "_dd10log.txt",
            //    "tact =\tvsync\tpre56\tilclk\tvretr");
            const int dd3dd4set = 0x280 >> 1;
            const int dd5dd6set = 0x0F0;
            var dd10addr = dd3dd4set;
            var dd11addr = dd5dd6set;
            var contAddr = 0;
            var lastPre34 = (romDd10[dd3dd4set] & 2) == 0;
            var lastPre56 = (romDd11[dd5dd6set] & 2) == 0;
            var lastRetrace = (romDd10[dd3dd4set] & 0x80) == 0;
            var lastResetTime = 0;
            var lastResetTime2 = 0;
            var intgt = true;
            for (var i = 0; i < contention.Length; i++)
            {
                for (var j = 0; j < 2; j++)
                {
                    dd10addr++;
                    if ((romDd10[dd10addr & 0x1FF] & 2) != 0 && !lastPre34)
                    {
                        //LogAgent.DumpAppend("_dd10log.txt", "RESET DD10 dt={0}", i - lastResetTime);
                        lastResetTime = i;
                        dd10addr = (dd10addr & 7) | dd3dd4set;
                    }
                    if ((romDd10[dd10addr & 0x1FF] & 0x80) == 0 && lastRetrace)
                    {
                        //LogAgent.DumpAppend("_dd10log.txt", "INC DD5DD6");
                        dd11addr++;
                        if ((romDd11[dd11addr & 0x1FF] & 2) != 0 && !lastPre56)
                        {
                            var dt = i - lastResetTime2;
                            //LogAgent.DumpAppend("_dd10log.txt", "RESET DD11 dt={0} [{1} lines]", dt, dt / 224);
                            lastResetTime2 = i;
                            dd11addr = (dd11addr & 0x100) | dd5dd6set;
                        }
                        //var vsync = (m_dd11[dd11addr & 0x1FF] & 1) != 0;
                        //var pre56 = (m_dd11[dd11addr & 0x1FF] & 2) != 0;
                        //var ibclk = (m_dd11[dd11addr & 0x1FF] & 0x10) != 0;
                        //var vretr = (m_dd11[dd11addr & 0x1FF] & 0x20) != 0;
                        //LogAgent.DumpAppend(
                        //    "_dd10log.txt",
                        //    "{0} =\t{1}\t{2}\t{3}\t{4}",
                        //    i,
                        //    vsync ? 1 : 0,
                        //    pre56 ? 1 : 0,
                        //    ibclk ? 1 : 0,
                        //    vretr ? 1 : 0);
                    }
                    lastPre34 = (romDd10[dd10addr & 0x1FF] & 2) != 0;
                    lastPre56 = (romDd11[dd11addr & 0x1FF] & 2) != 0;
                    lastRetrace = (romDd10[dd10addr & 0x1FF] & 0x80) != 0;
                }

                var vsync = (romDd11[dd11addr & 0x1FF] & 1) != 0;
                var pre56 = (romDd11[dd11addr & 0x1FF] & 2) != 0;
                var ibclk = (romDd11[dd11addr & 0x1FF] & 0x10) != 0;
                var vretr = (romDd11[dd11addr & 0x1FF] & 0x20) != 0;
                var bus75 = (romDd11[dd11addr & 0x1FF] & 0x80) != 0;

                var dd10val = romDd10[dd10addr & 0x1FF];
                var dd11val = romDd11[dd11addr & 0x1FF];
                // D0 - ССИ (horizontal sync pulse)
                var hsync = (dd10val & 1) != 0;
                // D1 - preload DD3/DD4
                var pre34 = (dd10val & 2) != 0;
                // D2 - BUS20 = A1 for DD38-DD41 (vram address generator)
                // D3 - RAS
                // D4 - CAS
                // D5 - BUS23 = block CLK when BUS23=1 and mem access #4000-7FFF
                var blclk = (dd10val & 0x20) != 0;
                // D6 - BUS24 = attr/pixel latch, BUS24=0 -> attr latch
                var latch = (dd10val & 0x40) != 0;
                // D7 - BUS142 = horizontal retrace
                var hretr = (dd10val & 0x80) != 0;

                if (hretr)
                {
                    intgt = true;
                }
                else if (!hsync)
                {
                    intgt = false;
                }
                var intrq = intgt | bus75;

                //LogAgent.DumpAppend(
                //    "_dd10log.txt",
                //    "{0} =\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}",
                //    i,
                //    hsync ? 1 : 0,
                //    pre34 ? 1 : 0,
                //    blclk ? 1 : 0,
                //    latch ? 1 : 0,
                //    hretr ? 1 : 0,
                //    vsync ? 1 : 0,
                //    pre56 ? 1 : 0,
                //    ibclk ? 1 : 0,
                //    vretr ? 1 : 0,
                //    intrq ? 1 : 0);


                //LogAgent.DumpAppend(
                //    "_dd10log.txt",
                //    "{0} =\t{1}\t{2}",
                //    i,
                //    blclk ? 1 : 0,
                //    ibclk ? 1 : 0);
                contention[i] = 0;
                if (blclk && !ibclk)
                {
                    for (var j = contAddr; j <= i; j++)
                    {
                        contention[j]++;
                    }
                }
                else
                {
                    contAddr = i + 1;
                }
            }
            //LogAgent.DumpArray("_contDD10.txt", m_contention);
            return contention;
        }

        private int[] m_contention;
        private byte[] m_dd10 = new byte[0x200];
        private byte[] m_dd11 = new byte[0x200];

        private void InitStaticTables()
        {
            try
            {
                using (var stream = MemoryBase.GetRomFileStream("ZXBYTE/DD10_RT5.bin"))
                {
                    stream.Read(m_dd10, 0, m_dd10.Length);
                }
                using (var stream = MemoryBase.GetRomFileStream("ZXBYTE/DD11_RT5.bin"))
                {
                    stream.Read(m_dd11, 0, m_dd10.Length);
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
        }
    }

    public class UlaByte_Early : UlaByte_Late
    {
        #region IBusDevice

        public override string Name { get { return "Byte [early model]"; } }
        public override string Description { get { return "БАЙТ [early model]\r\nVersion 1.0"; } }


        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
        }

        #endregion

        protected override SpectrumRendererParams CreateSpectrumRendererParams()
        {
            // Байт
            // Total Size:          448 x 312
            // Visible Size:        320 x 240 (32+256+32 x 24+192+24)
            var timing = base.CreateSpectrumRendererParams();

            timing.c_ulaIntBegin = -53 * timing.c_ulaLineTime; // shift all timings on 53 lines (c_ulaFirstPaperLine = 64+53)

            return timing;
        }
    }
}
