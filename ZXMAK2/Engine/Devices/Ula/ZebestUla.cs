using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Engine.Devices.Ula
{
    public class ZebestUla : UlaPentagon
    {
        public override string Name { get { return "Zebest ULA (Pentagon+BRIGHT)"; } }
        public override string Description { get { return "ULA device based on Pentagon + border bright mod\nbit 7 of port #FE = inversed bright bit for border"; } }

        public override byte PortFE
        {
            set
            {
                base.PortFE = value;
                int borderAttr = (value & 7) | (((value & 0x80) /*^ 0x80*/) >> 4);
                _borderColor = Palette[borderAttr];
            }
        }
    }
}
