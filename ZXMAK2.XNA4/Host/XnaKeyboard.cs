using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using ZXMAK2.Interfaces;


namespace ZXMAK2.XNA4.Host
{
    public class XnaKeyboard : IHostKeyboard, IKeyboardState
    {
        private readonly Dictionary<Key, Keys> m_map = new Dictionary<Key, Keys>();
        private readonly Dictionary<Key, bool> m_state = new Dictionary<Key, bool>();


        public XnaKeyboard()
        {
            CreateKeyMap();
        }

        public void Update(KeyboardState state)
        {
            foreach (var key in m_map.Keys)
            {
                m_state[key] = state[m_map[key]] == KeyState.Down;
            }
        }


        #region IKeyboardState

        public bool this[Key key]
        {
            get { return m_state.ContainsKey(key) && m_state[key]; }
        }

        #endregion IKeyboardState


        #region IHostKeyboard

        public IKeyboardState State
        {
            get { return this; }
        }

        public void Scan()
        {
        }

        #endregion IHostKeyboard


        #region Map

        private void CreateKeyMap()
        {
            // ZXMAK2 to XNA4 mapping
            m_map[Key.D1] = Keys.D1;
            m_map[Key.D2] = Keys.D2;
            m_map[Key.D3] = Keys.D3;
            m_map[Key.D4] = Keys.D4;
            m_map[Key.D5] = Keys.D5;
            m_map[Key.D6] = Keys.D6;
            m_map[Key.D7] = Keys.D7;
            m_map[Key.D8] = Keys.D8;
            m_map[Key.D9] = Keys.D9;
            m_map[Key.D0] = Keys.D0;
            m_map[Key.Q] = Keys.Q;
            m_map[Key.W] = Keys.W;
            m_map[Key.E] = Keys.E;
            m_map[Key.R] = Keys.R;
            m_map[Key.T] = Keys.T;
            m_map[Key.Y] = Keys.Y;
            m_map[Key.U] = Keys.U;
            m_map[Key.I] = Keys.I;
            m_map[Key.O] = Keys.O;
            m_map[Key.P] = Keys.P;
            m_map[Key.A] = Keys.A;
            m_map[Key.S] = Keys.S;
            m_map[Key.D] = Keys.D;
            m_map[Key.F] = Keys.F;
            m_map[Key.G] = Keys.G;
            m_map[Key.H] = Keys.H;
            m_map[Key.J] = Keys.J;
            m_map[Key.K] = Keys.K;
            m_map[Key.L] = Keys.L;
            m_map[Key.Z] = Keys.Z;
            m_map[Key.X] = Keys.X;
            m_map[Key.C] = Keys.C;
            m_map[Key.V] = Keys.V;
            m_map[Key.B] = Keys.B;
            m_map[Key.N] = Keys.N;
            m_map[Key.M] = Keys.M;
            m_map[Key.Space] = Keys.Space;
            m_map[Key.Return] = Keys.Enter;  //+
            m_map[Key.F1] = Keys.F1;
            m_map[Key.F2] = Keys.F2;
            m_map[Key.F3] = Keys.F3;
            m_map[Key.F4] = Keys.F4;
            m_map[Key.F5] = Keys.F5;
            m_map[Key.F6] = Keys.F6;
            m_map[Key.F7] = Keys.F7;
            m_map[Key.F8] = Keys.F8;
            m_map[Key.F9] = Keys.F9;
            m_map[Key.F10] = Keys.F10;
            m_map[Key.F11] = Keys.F11;
            m_map[Key.F12] = Keys.F12;
            m_map[Key.F13] = Keys.F13;
            m_map[Key.F14] = Keys.F14;
            m_map[Key.F15] = Keys.F15;
            m_map[Key.LeftShift] = Keys.LeftShift;
            m_map[Key.RightShift] = Keys.RightShift;
            m_map[Key.LeftAlt] = Keys.LeftAlt;
            m_map[Key.RightAlt] = Keys.RightAlt;
            m_map[Key.LeftControl] = Keys.LeftControl;
            m_map[Key.RightControl] = Keys.RightControl;
            //m_map[Key.LeftMenu] = Keys.LeftMenu;
            //m_map[Key.RightMenu] = Keys.RightMenu;
            m_map[Key.LeftWindows] = Keys.LeftWindows;
            m_map[Key.RightWindows] = Keys.RightWindows;
            m_map[Key.UpArrow] = Keys.Up;                   //+
            m_map[Key.LeftArrow] = Keys.Left;               //+
            m_map[Key.RightArrow] = Keys.Right;             //+
            m_map[Key.DownArrow] = Keys.Down;               //+
            m_map[Key.Insert] = Keys.Insert;
            m_map[Key.Delete] = Keys.Delete;
            m_map[Key.Home] = Keys.Home;
            m_map[Key.End] = Keys.End;
            m_map[Key.PageUp] = Keys.PageUp;
            m_map[Key.PageDown] = Keys.PageDown;
            m_map[Key.Escape] = Keys.Escape;
            m_map[Key.Tab] = Keys.Tab;
            //m_map[Key.Minus] = Keys.???;                  //?
            //m_map[Key.Equals] = Keys.???;                 //?
            m_map[Key.BackSpace] = Keys.Back;               //+
            m_map[Key.CapsLock] = Keys.CapsLock;
            m_map[Key.NumPadPlus] = Keys.OemPlus;           //+?
            m_map[Key.NumPadMinus] = Keys.OemMinus;         //+?
            //m_map[Key.NumPadStar] = Keys.???;             //?
            //m_map[Key.NumPadSlash] = Keys.???;            //?
            m_map[Key.Period] = Keys.OemPeriod;             //+?
            m_map[Key.Comma] = Keys.OemComma;               //+?
            m_map[Key.SemiColon] = Keys.OemSemicolon;       //+?
            //m_map[Key.Apostrophe] = Keys.???;             //?
            //m_map[Key.Slash] = Keys.???;                  //?
            m_map[Key.LeftBracket] = Keys.OemOpenBrackets;  //+?
            m_map[Key.RightBracket] = Keys.OemCloseBrackets;//+?
            //m_map[Key.NumPadEnter] = Keys.???;            //?
            m_map[Key.BackSlash] = Keys.OemBackslash;       //+?
            //m_map[Key.Grave] = Keys.???;                  //+
            m_map[Key.NumPad0] = Keys.NumPad0;
            m_map[Key.NumPad1] = Keys.NumPad1;
            m_map[Key.NumPad2] = Keys.NumPad2;
            m_map[Key.NumPad3] = Keys.NumPad3;
            m_map[Key.NumPad4] = Keys.NumPad4;
            m_map[Key.NumPad5] = Keys.NumPad5;
            m_map[Key.NumPad6] = Keys.NumPad6;
            m_map[Key.NumPad7] = Keys.NumPad7;
            m_map[Key.NumPad8] = Keys.NumPad8;
            m_map[Key.NumPad9] = Keys.NumPad9;
            m_map[Key.NumPadComma] = Keys.OemComma;         //+?
            m_map[Key.NumPadPeriod] = Keys.OemPeriod;       //+?
        }

        #endregion Map
    }
}
