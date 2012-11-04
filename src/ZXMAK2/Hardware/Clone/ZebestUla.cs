using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Hardware.Pentagon;

namespace ZXMAK2.Hardware.Clone
{
    public class ZebestUla : UlaPentagon
    {
        public override string Name { get { return "Zebest ULA (Pentagon+BRIGHT)"; } }
        public override string Description { get { return "ULA device based on Pentagon + border bright mod\nbit 6 of port #FE = bright bit for border"; } }

        public ZebestUla()
        {
            c_ulaBorderTop = 64;
            c_ulaBorderBottom = 48;
            c_ulaBorderLeftT = 28;
            c_ulaBorderRightT = 28;

            c_ulaWidth = (c_ulaBorderLeftT + 128 + c_ulaBorderRightT) * 2;
            c_ulaHeight = (c_ulaBorderTop + 192 + c_ulaBorderBottom);
        }

        public override byte PortFE
        {
            set
            {
                base.PortFE = value;
                int borderAttr = (value & 7) | ((value & 0x40 ) >> 3);
                _borderColor = Palette[borderAttr];
            }
        }
    }
}
