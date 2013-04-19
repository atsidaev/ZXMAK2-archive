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
