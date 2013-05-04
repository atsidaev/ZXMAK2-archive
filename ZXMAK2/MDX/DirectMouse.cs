/// Description: ZX Spectrum mouse emulator
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;


namespace ZXMAK2.MDX
{
    public class DirectMouse : IDisposable
    {
        private Form m_form;
        private bool m_active = false;
        private Device m_device = null;
        private MouseStateWrapper m_state = new MouseStateWrapper();

        
        public ZXMAK2.Interfaces.IMouseState MouseState
        {
            get { return m_state; }
        }

        public bool IsCaptured
        {
            get { return m_active; }
        }


        public DirectMouse(Form mainForm)
        {
            m_form = mainForm;
            if (m_device == null)
            {
                m_device = new Device(SystemGuid.Mouse);
                mainForm.Deactivate += WndDeactivate;
            }
        }

        public void Dispose()
        {
            if (m_device != null)
            {
                m_active = false;
                m_device.Unacquire();
                m_device.Dispose();
                m_device = null;
            }
        }

        private void WndDeactivate(object sender, EventArgs e)
        {
            StopCapture();
        }

        public void StartCapture()
        {
            if (m_device != null && !m_active)
            {
                try
                {
                    m_device.SetCooperativeLevel(
                        m_form,
                        CooperativeLevelFlags.Exclusive |
                            CooperativeLevelFlags.Foreground);
                    m_device.Acquire();
                    m_active = true;
                }
                catch
                {
                    StopCapture();
                }
            }
        }

        public void StopCapture()
        {
            if (m_device != null)
            {
                try
                {
                    if (m_active)
                    {
                        m_device.Unacquire();
                    }
                    m_device.SetCooperativeLevel(
                        m_form,
                        CooperativeLevelFlags.NonExclusive | 
                            CooperativeLevelFlags.Foreground);
                    m_active = false;
                }
                catch
                {
                }
            }
        }

        public void Scan()
        {
            if (m_active)
            {
                try
                {
                    m_state.Update(m_device.CurrentMouseState);
                }
                catch (NotAcquiredException)
                {
                    StopCapture();
                    return;
                }
            }
        }

        private class MouseStateWrapper : ZXMAK2.Interfaces.IMouseState
        {
            private int m_x = 0;
            private int m_y = 0;
            private int m_b = 0;

            internal MouseStateWrapper()
            {
            }

            internal void Update(MouseState state)
            {
                m_x += state.X;
                m_y += state.Y;

                m_b = 0;
                byte[] buttonState = state.GetMouseButtons();
                if ((buttonState[0] & 0x80) != 0) m_b |= 1;
                if ((buttonState[1] & 0x80) != 0) m_b |= 2;
                if ((buttonState[2] & 0x80) != 0) m_b |= 4;
                if ((buttonState[3] & 0x80) != 0) m_b |= 8;
                if ((buttonState[4] & 0x80) != 0) m_b |= 16;
                if ((buttonState[5] & 0x80) != 0) m_b |= 32;
            }

            public int X { get { return m_x; } }
            public int Y { get { return m_y; } }
            public int Buttons { get { return m_b; } }
        }
    }
}
