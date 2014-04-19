using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using ZXMAK2.Interfaces;
using System.Xml;
using System.Reflection;
using ZXMAK2.Entities;


namespace ZXMAK2.XNA4.Host
{
    public class XnaKeyboard : IHostKeyboard, IKeyboardState
    {
        private readonly KeyboardStateMapper<Keys> m_mapper = new KeyboardStateMapper<Keys>();
        private readonly Dictionary<Key, bool> m_state = new Dictionary<Key, bool>();


        public XnaKeyboard()
        {
            m_mapper.LoadMapFromString(
                global::ZXMAK2.XNA4.Properties.Resources.KeyboardMap);
        }

        public void Update(KeyboardState state)
        {
            foreach (var key in m_mapper.Keys)
            {
                m_state[key] = state[m_mapper[key]] == KeyState.Down;
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
    }
}
