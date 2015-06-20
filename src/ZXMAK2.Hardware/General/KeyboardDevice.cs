using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.Tools;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.General
{
    public class KeyboardDevice : BusDeviceBase, IKeyboardDevice
    {
        private readonly KeyboardMatrix _matrix;
        private int[] _rows;
        private IMemoryDevice m_memory;
        private IKeyboardState m_keyboardState = null;

        
        public KeyboardDevice()
        {
            Category = BusDeviceCategory.Keyboard;
            Name = "KEYBOARD";
            Description = "Standard Spectrum Keyboard\r\nPort: #FE\r\nMask: #01";
            _matrix = KeyboardMatrix.Deserialize(
                KeyboardMatrix.DefaultRows,
                Path.Combine(Utils.GetAppFolder(), "Keyboard.config"));
        }

        
        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
			bmgr.SubscribeRdIo(0x0001, 0x0000, ReadPortFe);
        }

        public override void BusConnect()
        {
        }

        public override void BusDisconnect()
        {
        }

        #endregion IBusDevice

        #region IKeyboardDevice

        public IKeyboardState KeyboardState
        {
            get { return m_keyboardState; }
			set 
            { 
                m_keyboardState = value;
                _rows = _matrix.Scan(value);//new MockState(Key.A, Key.B));
            }
        }

        #endregion

		#region Bus Handlers

		private void ReadPortFe(ushort addr, ref byte value, ref bool iorqge)
		{
			if (!iorqge || m_memory.DOSEN)
				return;
			//iorqge = false;
			value &= 0xE0;
			value |= (byte)(~KeyboardMatrix.ScanPort(_rows, addr) & 0x1F);
		}
		
		#endregion
	}
}
