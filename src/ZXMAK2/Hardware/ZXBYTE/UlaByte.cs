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


        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, ReadMem4000);
            bmgr.SubscribeWRMEM(0xC000, 0x4000, WriteMem4000);
            bmgr.SubscribeRDNOMREQ(0xC000, 0x4000, ContendNoMreq);
            bmgr.SubscribeWRNOMREQ(0xC000, 0x4000, ContendNoMreq);
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

        protected void ContendNoMreq(ushort addr)
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
            timing.c_ulaIntLength = 32;
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
            m_contention = UlaSpectrum48.CreateContentionTable(
                SpectrumRenderer.Params,
                new int[] { 6, 5, 4, 3, 2, 1, 0, 0, });
        }

        private int[] m_contention;
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
