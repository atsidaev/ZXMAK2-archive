using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices.Ula
{
    public class UlaLeningrad : UlaDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Leningrad"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            bmgr.SubscribeRDMEM_M1(0x0000, 0x0000, busM1);
        }

        #endregion

        #region Bus Handlers

        private void busM1(ushort addr, ref byte value)
        {
            CPU.Tact += CPU.Tact & 1;
        }
        
        #endregion

        
        public UlaLeningrad()
        {
            // Leningrad 1
            // Total Size:          448 x 320
            // Visible Size:        384 x 304 (72+256+56 x 64+192+48)

			// 224 = 128T scr + 32T right + 32T HSync + 32T left
			// 312 = 192 scr + 40 bottom + 16 VSync + 64 top border

            c_ulaLineTime = 224;        // +
            c_ulaFirstPaperLine = 64;   // +
            c_ulaFirstPaperTact = 64;
            c_frameTactCount = 69888;   // +

            c_ulaBorderTop = 24;// 48;
            c_ulaBorderBottom = 24;// 48;
            c_ulaBorderLeftT = 16;// 32;      // +
            c_ulaBorderRightT = 16;// 32;     // +

            c_ulaIntBegin = 64 + (64+192) * 224;
            c_ulaIntLength = 32;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }
    }
}
