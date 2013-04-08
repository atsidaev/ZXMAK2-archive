using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Hardware.Scorpion
{
    public class UlaScorpionGreen : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion [Green]"; } }

        #endregion

        public UlaScorpionGreen()
        {
            // Scorpion [Green]
            // Total Size:          448 x 316
            // Visible Size:        ?368 x 296 (48+256+64 x 64+192+40)
            // First Line Border:   ?0
            // First Line Paper:    80
            // Paper Lines:         192
            // Bottom Border Lines: ?40

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 80;
            c_ulaFirstPaperTact = 64;      // ?64 [40sync+24border+128scr+32border]
            c_frameTactCount = 70784;
            c_ulaBorder4T = true;
            c_ulaBorder4Tstage = 3;

            c_ulaBorderTop = 24;
            c_ulaBorderBottom = 24;
            c_ulaBorderLeftT = 16;
            c_ulaBorderRightT = 16;

            c_ulaIntBegin = 64 - 3;
            c_ulaIntLength = 32;    // according to fuse

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }

        #region UlaDeviceBase

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            if (!Memory.DOSEN && (addr & 0x23) == (0xFE & 0x23))
                base.WritePortFE(addr, value, ref iorqge);
        }

        #endregion
    }
}
