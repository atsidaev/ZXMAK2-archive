/// Description: ZX Spectrum mouse emulator
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Windows.Forms;
using Microsoft.DirectX.DirectInput;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed class DirectMouse : IHostMouse, IDisposable
    {
        private readonly MouseStateWrapper m_state = new MouseStateWrapper();
        private Form m_form;
        private Device m_device;
        private bool m_active;


        #region .ctor

        public DirectMouse(Form form)
        {
            m_form = form;
            if (m_device == null)
            {
                m_device = new Device(SystemGuid.Mouse);
                form.Deactivate += WndDeactivate;
            }
        }

        public void Dispose()
        {
            if (m_device != null)
            {
                m_active = false;
                m_device.Unacquire();
            }
            Dispose(ref m_device);
        }

        #endregion .ctor


        #region IHostMouse

        public IMouseState MouseState
        {
            get { return m_state; }
        }

        public bool IsCaptured
        {
            get { return m_active; }
        }

        public void Scan()
        {
            if (!m_active || m_device==null)
            {
                return;
            }
            try
            {
                m_state.Update(m_device.CurrentMouseState);
            }
            catch (NotAcquiredException)
            {
                Uncapture();
                return;
            }
        }

        public void Capture()
        {
            if (m_device == null || m_active)
            {
                return;
            }
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
                Uncapture();
            }
        }

        public void Uncapture()
        {
            if (m_device == null)
            {
                return;
            }
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


        #endregion IHostMouse


        #region Private

        private void WndDeactivate(object sender, EventArgs e)
        {
            Uncapture();
        }

        private static void Dispose<T>(ref T disposable)
            where T : IDisposable
        {
            var value = disposable;
            disposable = default(T);
            value.Dispose();
        }

        #endregion Private


        private class MouseStateWrapper : IMouseState
        {
            private int m_x = 128;
            private int m_y = 128;
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
