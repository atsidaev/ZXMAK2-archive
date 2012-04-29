using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Engine.Devices
{
    public class BeeperScorpion : BeeperDevice
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion Beeper"; } }
        public override string Description { get { return "Simple Scorpion ZS Beeper"; } }

        #endregion

        #region BeeperDevice

        protected override void WritePortFE(ushort addr, byte value, ref bool iorqge)
        {
            if ((addr & 0x23) == (0xFE & 0x23))  // TODO: check no trdos port activated
                base.WritePortFE(addr, value, ref iorqge);
        }

        #endregion
    }
}
