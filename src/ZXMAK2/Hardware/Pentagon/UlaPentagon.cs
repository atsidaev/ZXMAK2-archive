using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Hardware.Pentagon
{
    public class UlaPentagon : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Pentagon"; } }

        #endregion

        public UlaPentagon()
        {
            // Pentagon 128K
            // Total Size:          448 x 320
            // Visible Size:        384 x 304 (72+256+56 x 64+192+48)

            c_ulaLineTime = 224;
            c_ulaFirstPaperLine = 80;
            c_ulaFirstPaperTact = 68;      // 68 [32sync+36border+128scr+28border]
            c_frameTactCount = 71680;

            c_ulaBorderTop = 24;//64;
            c_ulaBorderBottom = 24;//48;
            c_ulaBorderLeftT = 16;//28; //36;
            c_ulaBorderRightT = 16;// 28;

            c_ulaIntBegin = 0;
            c_ulaIntLength = 32;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }
    }

    public class UlaPentagonEx : UlaPentagon
    {
        #region IBusDevice

        public override string Name { get { return "Pentagon [Extended Border]"; } }

        #endregion

        public UlaPentagonEx()
        {
            // Pentagon 128K
            // Total Size:          448 x 320
            // Visible Size:        320 x 240 (32+256+32 x 24+192+24)

            c_ulaBorderTop = 64;
            c_ulaBorderBottom = 48;
            c_ulaBorderLeftT = 28;
            c_ulaBorderRightT = 28;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }
    }
}
