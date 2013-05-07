using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Entities;

namespace ZXMAK2.Hardware.General
{
    public class KempstonJoystick : BusDeviceBase
    {
        #region IBusDevice

        public override string Name { get { return "Kempston Joystick (Stub)"; } }
        public override string Description { get { return "Standard Spectrum Joystick\n\nWARNING: This is Stub device (port emulation only)!\nSorry, real joystick read is not implemented yet"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Other; } }

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRdIo(0x00e0, 0x0000, readPort1F);
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
