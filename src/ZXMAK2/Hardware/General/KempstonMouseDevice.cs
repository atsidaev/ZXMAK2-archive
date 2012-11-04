using System;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine;


namespace ZXMAK2.Hardware.General
{
    public class KempstonMouseDevice : BusDeviceBase, IMouseDevice
    {
        #region IBusDevice Members

        public override string Name { get { return "Kempston Mouse"; } }
        public override string Description { get { return "Standard Kempston Mouse\n#FADF - buttons\n#FBDF - X coord\n#FFDF - Y coord"; } }
        public override BusCategory Category { get { return BusCategory.Mouse; } }

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRDIO(0xFFFF, 0xFADF, readPortFADF);
            bmgr.SubscribeRDIO(0xFFFF, 0xFBDF, readPortFBDF);
            bmgr.SubscribeRDIO(0xFFFF, 0xFFDF, readPortFFDF);
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion

        #region IMouseDevice Members

		public IMouseState MouseState
		{
			get { return m_mouseState; }
			set { m_mouseState = value; }
		}
		
        #endregion

		
		private IMouseState m_mouseState = null;


        #region Private

		private void readPortFADF(ushort addr, ref byte value, ref bool iorqge)
        {
			if (!iorqge)
				return;
			iorqge = false;
			int b = MouseState != null ? MouseState.Buttons : 0;
			b = ((b & 1) << 1) | ((b & 2) >> 1) | (b & 0xFC);			// D0 - right, D1 - left, D2 - middle
			value = (byte)(b ^ 0xFF);     //  Kempston mouse buttons
        }

		private void readPortFBDF(ushort addr, ref byte value, ref bool iorqge)
        {
			if (!iorqge)
				return;
			iorqge = false;
			int x = MouseState != null ? MouseState.X : 0;
			value = (byte)(x / 3);			//  Kempston mouse X        
        }

		private void readPortFFDF(ushort addr, ref byte value, ref bool iorqge)
        {
			if (!iorqge)
				return;
			iorqge = false;
			int y = MouseState != null ? MouseState.Y : 0;
			value = (byte)(-y / 3);			//	Kempston mouse Y
        }

        #endregion
    }
}
