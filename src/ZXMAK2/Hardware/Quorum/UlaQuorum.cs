using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Engine.Devices.Ula
{
    public class UlaQuorum : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "QUORUM"; } }

        #endregion

        public UlaQuorum()
        {
            // Кворум БК-04
            // Total Size:          448 x 312
            // Visible Size:        384 x 296 (72+256+56 x 64+192+40)

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 80;      // proof???80
            c_ulaFirstPaperTact = 68;      // proof???68 [32sync+36border+128scr+28border]
            c_frameTactCount = 69888;      // for pentagon mod = 71680

            c_ulaBorderTop = 24;//64;
            c_ulaBorderBottom = 24;//40;
            c_ulaBorderLeftT = 16;
            c_ulaBorderRightT = 16;

            c_ulaIntBegin = 0;
            c_ulaIntLength = 32;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            if((addr & 0x99)==(0xFE&0x99))
                base.WritePortFE(addr, value, ref iorqge);
        }
    }
}
