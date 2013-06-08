/// Description: ZX Spectrum keyboard emulator
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using ZXMAK2.Interfaces;


namespace ZXMAK2.MDX
{
    public class DirectKeyboard : IHostKeyboard, IDisposable
    {
        private Form _form;
        private bool kbdActive = false;
        private Device diKeyboard = null;

        private IKeyboardState _state = null;


        public DirectKeyboard(Form mainForm)
        {
            _form = mainForm;
            if (diKeyboard == null)
            {
                diKeyboard = new Device(SystemGuid.Keyboard);
                diKeyboard.SetCooperativeLevel(mainForm, CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Foreground);
                mainForm.Activated += WndActivated;
                mainForm.Deactivate += WndDeactivate;
                WndActivated(null, null);
            }
        }

        public void Dispose()
        {
            _form.Activated -= WndActivated;
            _form.Deactivate -= WndDeactivate;
            if (diKeyboard != null)
            {
                kbdActive = false;
                diKeyboard.Unacquire();
                diKeyboard.Dispose();
                diKeyboard = null;
            }
        }

        public void Scan()
        {
            if (diKeyboard == null)
            {
                _state = KeyboardStateWrapper.Empty;
                return;
            }
            if (!kbdActive)
            {
                WndActivated(null, null);
                return;
            }
            try
            {
                var keyState = diKeyboard.GetCurrentKeyboardState();
                _state = new KeyboardStateWrapper(keyState);
            }
            catch
            {
                WndActivated(null, null); return;
            }
        }

        public IKeyboardState State { get { return _state; } }

        #region private methods

        private void WndActivated(object sender, EventArgs e)
        {
            if (diKeyboard != null)
            {
                try
                {
                    diKeyboard.Acquire();
                    kbdActive = true;
                }
                catch
                {
                    kbdActive = false;
                }
            }
        }

        private void WndDeactivate(object sender, EventArgs e)
        {
            if (diKeyboard != null)
            {
                kbdActive = false;
                try
                {
                    diKeyboard.Unacquire();
                }
                catch
                {
                }
            }
        }

        #endregion

        private class KeyboardStateWrapper : IKeyboardState
        {
            private Microsoft.DirectX.DirectInput.KeyboardState m_state;

            internal KeyboardStateWrapper(Microsoft.DirectX.DirectInput.KeyboardState state)
            {
                m_state = state;
            }

            public bool this[ZXMAK2.Interfaces.Key key]
            {
                get
                {
                    return m_state != null &&
                        m_state[(Microsoft.DirectX.DirectInput.Key)key];
                }
            }

            public static readonly KeyboardStateWrapper Empty = new KeyboardStateWrapper(null);
        }
    }
}
