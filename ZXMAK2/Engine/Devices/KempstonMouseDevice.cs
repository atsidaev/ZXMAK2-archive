using System;

using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Engine.Devices
{
    public class KempstonMouseDevice : IBusDevice, IMouseDevice
    {
        #region IBusDevice Members

        string IBusDevice.Name { get { return "Kempston Mouse"; } }
        string IBusDevice.Description { get { return "Standard Kempston Mouse\n#FADF - buttons\n#FBDF - X coord\n#FFDF - Y coord"; } }
        BusCategory IBusDevice.Category { get { return BusCategory.Mouse; } }
		private int m_busOrder = 0;
		public int BusOrder { get { return m_busOrder; } set { m_busOrder = value; } }

        public void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRDIO(0xFFFF, 0xFADF, readPortFADF);
            bmgr.SubscribeRDIO(0xFFFF, 0xFBDF, readPortFBDF);
            bmgr.SubscribeRDIO(0xFFFF, 0xFFDF, readPortFFDF);
        }

        public void BusConnect()
        {
        }

        public void BusDisconnect()
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
