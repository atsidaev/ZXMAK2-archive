using System;

using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.General
{
    public class KeyboardDevice : BusDeviceBase, IKeyboardDevice
    {
        #region IBusDevice

        public override string Name { get { return "KEYBOARD"; } }
        public override string Description { get { return "Standard Spectrum Keyboard\r\nPort: #FE\r\nMask: #01"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Keyboard; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_memory = bmgr.FindDevice<IMemoryDevice>();
			bmgr.SubscribeRdIo(0x0001, 0x0000, readPortFE);
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
			set { m_keyboardState = value; m_intState = scanState(m_keyboardState); }
        }

        #endregion

		#region Bus Handlers

		private void readPortFE(ushort addr, ref byte value, ref bool iorqge)
		{
			if (!iorqge || m_memory.DOSEN)
				return;
			//iorqge = false;
			value &= 0xE0;
			value |= (byte)(ScanKbdPort(addr) & 0x1F);
		}
		
		#endregion

		private IMemoryDevice m_memory;
		private IKeyboardState m_keyboardState = null;

		#region Comment
		/// <summary>
		/// Spectrum keyboard state. 
		/// Each 5 bits represents: #7FFE, #BFFE, #DFFE, #EFFE, #F7FE, #FBFE, #FDFE, #FEFE.
		/// </summary>
		#endregion
		private long m_intState = 0;

        #region Comment
        /// <summary>
        /// Scans keyboard state for specified port
        /// </summary>
        /// <param name="ADDR">Port address</param>
        #endregion
        protected int ScanKbdPort(ushort port)
        {
            byte val = 0x1F;
            int msk = 0x0100;
            for (int i = 0; i < 8; i++, msk <<= 1)
                if ((port & msk) == 0)
                    val &= (byte)(((m_intState >> (i * 5)) ^ 0x1F) & 0x1F);
            return val;
		}

		#region Scan

		private static long scanState(IKeyboardState state)
		{
			if (state==null || ((state[Key.LeftAlt] || state[Key.RightAlt])&&state[Key.Return]))
				return 0;
			
            long value = 0;
			value = (value << 5) | scan_7FFE(state);
			value = (value << 5) | scan_BFFE(state);
			value = (value << 5) | scan_DFFE(state);
			value = (value << 5) | scan_EFFE(state);
			value = (value << 5) | scan_F7FE(state);
			value = (value << 5) | scan_FBFE(state);
			value = (value << 5) | scan_FDFE(state);
			value = (value << 5) | scan_FEFE(state);
			return value;
		}

		#region Comment
		/// <summary>
		/// #7FFE: BNM[symbol][space]    +'
		/// </summary>
		#endregion
		private static byte scan_7FFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.Space]) res |= 1;
			if (state[Key.RightShift]) res |= 2;
			if (state[Key.M]) res |= 4;
			if (state[Key.N]) res |= 8;
			if (state[Key.B]) res |= 16;

			if (state[Key.CapsLock] || state[Key.NumPadPlus] || state[Key.NumPadMinus] || state[Key.NumPadStar] || state[Key.NumPadSlash])
				res |= 2;                                                            // numpad CapsLock +-*/
			if (state[Key.Period] || state[Key.Comma] || state[Key.SemiColon] ||
				state[Key.Apostrophe] || state[Key.Slash] ||
				state[Key.Minus] || state[Key.Equals] || state[Key.LeftBracket] ||
				state[Key.RightBracket])
				res |= 2;                                                            // SS for .,;"/-=[]

			if (state[Key.NumPadStar]) res |= 16;                                    // * = SS+B
			if (!state[Key.RightShift])
			{
				if (state[Key.Period]) res |= 4;
				if (state[Key.Comma]) res |= 8;
			}
			return res;
		}

		#region Comment
		/// <summary>
		/// #BFFE: HJKL[enter]
		/// </summary>
		#endregion
		private static byte scan_BFFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.Return]) res |= 1;
			if (state[Key.L]) res |= 2;
			if (state[Key.K]) res |= 4;
			if (state[Key.J]) res |= 8;
			if (state[Key.H]) res |= 16;

			if (state[Key.NumPadEnter]) res |= 1;
			if (state[Key.NumPadMinus]) res |= 8;
			if (state[Key.NumPadPlus]) res |= 4;
			if (state[Key.RightShift])
			{
				if (state[Key.Equals]) res |= 4;
			}
			else
			{
				if (state[Key.Minus]) res |= 8;
				if (state[Key.Equals]) res |= 2;
			}
			return res;
		}

		#region Comment
		/// <summary>
		/// #DFFE: YUIOP    +",'
		/// </summary>
		#endregion
		private static byte scan_DFFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.P]) res |= 1;
			if (state[Key.O]) res |= 2;
			if (state[Key.I]) res |= 4;
			if (state[Key.U]) res |= 8;
			if (state[Key.Y]) res |= 16;

            if (state[Key.Tab]) res |= 4;
            if (state[Key.RightShift])
			{
				if (state[Key.Apostrophe]) res |= 1;            // " = SS+P
			}
			else
			{
				if (state[Key.SemiColon]) res |= 2;             // ; = SS+O
			}
			return res;
		}

		#region Comment
		/// <summary>
		/// #EFFE: 67890    +down,up,right, bksp
		/// </summary>
		#endregion
		private static byte scan_EFFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.D0]) res |= 1;
			if (state[Key.D9]) res |= 2;
			if (state[Key.D8]) res |= 4;
			if (state[Key.D7]) res |= 8;
			if (state[Key.D6]) res |= 16;

			if (state[Key.RightArrow]) res |= 4;
			if (state[Key.UpArrow]) res |= 8;
			if (state[Key.DownArrow]) res |= 16;
			if (state[Key.BackSpace]) res |= 1;
			if (state[Key.RightShift])
			{
				if (state[Key.Minus]) res |= 1;                 // _ = SS+0
			}
			else
			{
				if (state[Key.Apostrophe]) res |= 8;            // ' = SS+7
			}
			return res;
		}

		#region Comment
		/// <summary>
		/// #F7FE: 54321    +left
		/// </summary>
		#endregion
		private static byte scan_F7FE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.D1]) res |= 1;
			if (state[Key.D2]) res |= 2;
			if (state[Key.D3]) res |= 4;
			if (state[Key.D4]) res |= 8;
			if (state[Key.D5]) res |= 16;

            if (state[Key.Escape]) res |= 1;
            if (state[Key.LeftArrow]) res |= 16;
			return res;
		}

		#region Comment
		/// <summary>
		/// #FBFE: QWERT    +period,comma
		/// </summary>
		#endregion
		private static byte scan_FBFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.Q]) res |= 1;
			if (state[Key.W]) res |= 2;
			if (state[Key.E]) res |= 4;
			if (state[Key.R]) res |= 8;
			if (state[Key.T]) res |= 16;

			if (state[Key.RightShift])
			{
				if (state[Key.Period]) res |= 16;
				if (state[Key.Comma]) res |= 8;
			}
			return res;
		}

		#region Comment
		/// <summary>
		/// #FDFE: GFDSA
		/// </summary>
		#endregion
		private static byte scan_FDFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.A]) res |= 1;
			if (state[Key.S]) res |= 2;
			if (state[Key.D]) res |= 4;
			if (state[Key.F]) res |= 8;
			if (state[Key.G]) res |= 16;
			return res;
		}

		#region Comment
		/// <summary>
		/// #FEFE: VCXZ<caps>     +left,right,up,down,bksp
		/// </summary>
		#endregion
		private static byte scan_FEFE(IKeyboardState state)
		{
			byte res = 0;
			if (state[Key.LeftShift]) res |= 1;
			if (state[Key.Z]) res |= 2;
			if (state[Key.X]) res |= 4;
			if (state[Key.C]) res |= 8;
			if (state[Key.V]) res |= 16;

            if (state[Key.Escape]) res |= 1;
            if (state[Key.LeftArrow]) res |= 1;
			if (state[Key.RightArrow]) res |= 1;
			if (state[Key.UpArrow]) res |= 1;
			if (state[Key.DownArrow]) res |= 1;
			if (state[Key.BackSpace]) res |= 1;
			if (state[Key.CapsLock]) res |= 1;
            if (state[Key.Tab]) res |= 1;
			if (state[Key.NumPadSlash]) res |= 16;
			if (state[Key.RightShift])
			{
				if (state[Key.SemiColon]) res |= 2;
				if (state[Key.Slash]) res |= 8;
			}
			else
			{
				if (state[Key.Slash]) res |= 16;
			}
			return res;
		}

		#endregion
	}
}
