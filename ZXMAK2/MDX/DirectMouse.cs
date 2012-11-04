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
		private Form _form;
		private bool _active = false;
		private Device _device = null;


		public DirectMouse(Form mainForm)
		{
			_form = mainForm;
			if (_device == null)
			{
				_device = new Device(SystemGuid.Mouse);
				//            mainForm.Activated += WndActivated;
				mainForm.Deactivate += WndDeactivate;
				//            WndActivated(null, null);
			}
		}
		public void Dispose()
		{
			if (_device != null)
			{
				_active = false;
				_device.Unacquire();
				_device.Dispose();
				_device = null;
			}
		}

		private void WndActivated(object sender, EventArgs e)
		{
		}
		private void WndDeactivate(object sender, EventArgs e)
		{
			StopCapture();
		}

		public void StartCapture()
		{
			if (_device != null && !_active)
			{
				try
				{
					_device.SetCooperativeLevel(
					   _form,
					   CooperativeLevelFlags.Exclusive | CooperativeLevelFlags.Foreground
					   );
					_device.Acquire();
					_active = true;
				}
				catch { StopCapture(); }
			}
		}
		public void StopCapture()
		{
			if (_device != null)
			{
				try
				{
					if (_active)
						_device.Unacquire();
					_device.SetCooperativeLevel(_form,
					   CooperativeLevelFlags.NonExclusive | CooperativeLevelFlags.Foreground
					   );
					_active = false;
				}
				catch { }
			}
		}

		private MouseStateWrapper m_state = new MouseStateWrapper();
		public ZXMAK2.Interfaces.IMouseState MouseState { get { return m_state; } }

		public void Scan()
		{
			if (_active)
			{
				try
				{
					m_state.Update(_device.CurrentMouseState);
				}
				catch (NotAcquiredException) { StopCapture(); return; }
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
