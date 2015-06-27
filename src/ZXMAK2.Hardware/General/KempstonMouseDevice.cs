using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.General
{
    public class KempstonMouseDevice : BusDeviceBase, IMouseDevice
    {
        #region Fields

        private IMemoryDevice m_memory;
        
        #endregion Fields


        public KempstonMouseDevice()
        {
            Category = BusDeviceCategory.Mouse;
            Name = "MOUSE KEMPSTON";
            Description = "Standard Kempston Mouse\n#FADF - buttons\n#FBDF - X coord\n#FFDF - Y coord";
        }


        #region IBusDevice Members

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
            var mask = 0xFFFF;// 0x05FF;
            bmgr.Events.SubscribeRdIo(mask, 0xFADF & mask, ReadPortBtn);
            bmgr.Events.SubscribeRdIo(mask, 0xFBDF & mask, ReadPortX);
            bmgr.Events.SubscribeRdIo(mask, 0xFFDF & mask, ReadPortY);
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion

        #region IMouseDevice Members

		public IMouseState MouseState { get; set; }
		
        #endregion

		


        #region Private

        private void ReadPortBtn(ushort addr, ref byte value, ref bool handled)
        {
            if (handled || m_memory.DOSEN)
                return;
            handled = true;
			
            var b = MouseState != null ? MouseState.Buttons : 0;
			b = ((b & 1) << 1) | ((b & 2) >> 1) | (b & 0xFC);			// D0 - right, D1 - left, D2 - middle
			value = (byte)(b ^ 0xFF);     //  Kempston mouse buttons
        }

        private void ReadPortX(ushort addr, ref byte value, ref bool handled)
        {
            if (handled || m_memory.DOSEN)
                return;
            handled = true;
			
            var x = MouseState != null ? MouseState.X : 0;
            value = (byte)(x / 3);			//  Kempston mouse X        
        }

        private void ReadPortY(ushort addr, ref byte value, ref bool handled)
        {
            if (handled || m_memory.DOSEN)
                return;
            handled = true;
			
            var y = MouseState != null ? MouseState.Y : 0;
			value = (byte)(-y / 3);			//	Kempston mouse Y
        }

        #endregion
    }
}
