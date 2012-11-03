using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices
{
    public class KempstonJoystick : BusDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Kempston Joystick (Stub)"; } }
        public override string Description { get { return "Standard Spectrum Joystick\n\nWARNING: This is Stub device (port emulation only)!\nSorry, real joystick read is not implemented yet"; } }
        public override BusCategory Category { get { return BusCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRDIO(0x00e0, 0x0000, readPort1F);
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion IBusDevice


		private void readPort1F(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
			iorqge = false;
            value = 0x00;
        }
    }
}
