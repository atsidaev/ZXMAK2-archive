/// Description: ZX Spectrum keyboard emulator
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using Microsoft.DirectX.DirectInput;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities.Tools;
using ZxmakKey = ZXMAK2.Host.Entities.Key;
using MdxKey = Microsoft.DirectX.DirectInput.Key;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class DirectKeyboard : IHostKeyboard, IKeyboardState, IDisposable
    {
        private readonly Form m_form;
        private Device m_device = null;
        private KeyboardStateMapper<MdxKey> m_mapper = new KeyboardStateMapper<MdxKey>();
        private readonly Dictionary<ZxmakKey, bool> m_state = new Dictionary<ZxmakKey, bool>();
        private bool m_isActive = false;



        public DirectKeyboard(Form mainForm)
        {
            m_form = mainForm;
            if (m_device == null)
            {
                m_device = new Device(SystemGuid.Keyboard);
                m_device.SetCooperativeLevel(mainForm, CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Foreground);
                mainForm.Activated += WndActivated;
                mainForm.Deactivate += WndDeactivate;
                WndActivated(null, null);
            }
            m_mapper.LoadMapFromString(
                global::ZXMAK2.Host.WinForms.Properties.Resources.Keyboard_Mdx);
        }

        public void Dispose()
        {
            m_form.Activated -= WndActivated;
            m_form.Deactivate -= WndDeactivate;
            if (m_device != null)
            {
                m_isActive = false;
                m_device.Unacquire();
                m_device.Dispose();
                m_device = null;
            }
        }

        #region IHostKeyboard

        public IKeyboardState State
        {
            get { return this; }
        }

        public void Scan()
        {
            if (m_device == null)
            {
                foreach (var key in m_mapper.Keys)
                {
                    m_state[key] = false;
                }
                return;
            }
            if (!m_isActive)
            {
                WndActivated(null, null);
                return;
            }
            try
            {
                var state = m_device.GetCurrentKeyboardState();
                foreach (var key in m_mapper.Keys)
                {
                    m_state[key] = state[m_mapper[key]];
                }
            }
            catch
            {
                WndActivated(null, null); return;
            }
        }

        public void LoadConfiguration(string fileName)
        {
            using (var reader = (TextReader)new StreamReader(fileName))
            {
                var xml = reader.ReadToEnd();
                var mapper = new KeyboardStateMapper<MdxKey>();
                mapper.LoadMapFromString(xml);
                m_mapper = mapper;
            }
        }

        #endregion IHostKeyboard


        #region IKeyboardState

        public bool this[ZxmakKey key]
        {
            get { return m_state.ContainsKey(key) && m_state[key]; }
        }

        #endregion IKeyboardState


        #region private methods

        private void WndActivated(object sender, EventArgs e)
        {
            if (m_device != null)
            {
                try
                {
                    m_device.Acquire();
                    m_isActive = true;
                }
                catch
                {
                    m_isActive = false;
                }
            }
        }

        private void WndDeactivate(object sender, EventArgs e)
        {
            if (m_device != null)
            {
                m_isActive = false;
                try
                {
                    m_device.Unacquire();
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
