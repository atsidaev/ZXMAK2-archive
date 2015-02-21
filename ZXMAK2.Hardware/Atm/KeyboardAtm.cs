using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.Atm
{
    public class KeyboardAtm : BusDeviceBase, IKeyboardDevice
    {
        #region Fields

        private IKeyboardState m_keyboardState;
        private long m_intState; // Each 5 bits represents: #7FFE, #BFFE, #DFFE, #EFFE, #F7FE, #FBFE, #FDFE, #FEFE.

        #endregion Fields


        public KeyboardAtm()
        {
            Category = BusDeviceCategory.Keyboard;
            Name = "KEYBOARD ATM";
            Description = "ATM Keyboard\r\nPort: #FE\r\nMask: #05";
        }
        

        #region IBusDevice

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeRdIo(0x0005, 0xFE & 0x0005, BusReadKeyboard);
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
            set { m_keyboardState = value; m_intState = ScanState(m_keyboardState); }
        }

        #endregion


        #region Bus Handlers

        private void BusReadKeyboard(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            value &= 0xE0;
            value |= (byte)(ScanKbdPort(addr) & 0x1F);
        }

        #endregion


        #region Scan

        protected int ScanKbdPort(ushort port)
        {
            byte val = 0x1F;
            var mask = 0x0100;
            for (var i = 0; i < 8; i++, mask <<= 1)
            {
                if ((port & mask) == 0)
                {
                    val &= (byte)(((m_intState >> (i * 5)) ^ 0x1F) & 0x1F);
                }
            }
            return val;
        }

        private static long ScanState(IKeyboardState state)
        {
            if (state == null || ((state[Key.LeftAlt] || state[Key.RightAlt]) && state[Key.Return]))
                return 0;

            long value = 0;
            value = (value << 5) | Scan7ffe(state);
            value = (value << 5) | ScanBffe(state);
            value = (value << 5) | ScanDffe(state);
            value = (value << 5) | ScanEffe(state);
            value = (value << 5) | ScanF7fe(state);
            value = (value << 5) | ScanFbfe(state);
            value = (value << 5) | ScanFdfe(state);
            value = (value << 5) | ScanFefe(state);
            return value;
        }

        // #7FFE: BNM[symbol][space]    +'
        private static byte Scan7ffe(IKeyboardState state)
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

        // #BFFE: HJKL[enter]
        private static byte ScanBffe(IKeyboardState state)
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

        // #DFFE: YUIOP    +",'
        private static byte ScanDffe(IKeyboardState state)
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

        // #EFFE: 67890    +down,up,right, bksp
        private static byte ScanEffe(IKeyboardState state)
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

        // #F7FE: 54321    +left
        private static byte ScanF7fe(IKeyboardState state)
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

        // #FBFE: QWERT    +period,comma
        private static byte ScanFbfe(IKeyboardState state)
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

        // #FDFE: GFDSA
        private static byte ScanFdfe(IKeyboardState state)
        {
            byte res = 0;
            if (state[Key.A]) res |= 1;
            if (state[Key.S]) res |= 2;
            if (state[Key.D]) res |= 4;
            if (state[Key.F]) res |= 8;
            if (state[Key.G]) res |= 16;
            return res;
        }

        // #FEFE: VCXZ<caps>     +left,right,up,down,bksp
        private static byte ScanFefe(IKeyboardState state)
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
