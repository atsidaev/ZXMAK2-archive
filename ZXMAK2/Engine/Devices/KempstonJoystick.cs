using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices
{
    public class KempstonJoystick : IBusDevice
    {
        #region IBusDevice

        public string Name { get { return "Kempston Joystick (Stub)"; } }
        public string Description { get { return "Standard Spectrum Joystick\n\nWARNING: This is Stub device (port emulation only)!\nSorry, real joystick read is not implemented yet"; } }
        public BusCategory Category { get { return BusCategory.Other; } }
		private int m_busOrder = 0;
		public int BusOrder { get { return m_busOrder; } set { m_busOrder = value; } }

        public void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRDIO(0x00e0, 0x0000, readPort1F);
        }

        public void BusConnect()
        {
        }

        public void BusDisconnect()
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
