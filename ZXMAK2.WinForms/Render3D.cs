/// Description: Direct3D renderer control
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;



namespace ZXMAK2.WinForms
{
	public class Render3D : Control
	{
		protected readonly object SyncRoot = new object();

		private int m_frameCounter = 0;
		private bool m_vBlankSync = false;
		protected Device D3D = null;
		private PresentParameters m_presentParams = new PresentParameters();

		public event EventHandler DeviceReset;

		public Render3D()
		{
			SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
		}

		public void InitWnd()
		{
			lock (SyncRoot)
			{
				if (D3D == null)
					init();
			}
		}

		public void FreeWnd()
		{
			lock (SyncRoot)
			{
				if (D3D != null)
					free();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			try
			{
				if (D3D != null)
				{
					RenderScene();
				}
				else
				{
					e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
					e.Graphics.DrawImage(SystemIcons.Warning.ToBitmap(), new Point(10, 10));
					e.Graphics.DrawString(
						"Direct3D not initialized!",
						new System.Drawing.Font(Font.FontFamily, 20),
						Brushes.White,
						new PointF(10 + SystemIcons.Warning.Width + 10, 10));
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void init()
		{
			try
			{
				lock (SyncRoot)
				{
					int adapter = Manager.Adapters.Default.Adapter;
					Caps caps = Manager.GetDeviceCaps(adapter, DeviceType.Hardware);
					CreateFlags flags = CreateFlags.MultiThreaded;
					if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
						flags |= CreateFlags.HardwareVertexProcessing;
					else
						flags |= CreateFlags.SoftwareVertexProcessing;

					m_presentParams.Windowed = true;
					m_presentParams.PresentationInterval =
						m_vBlankSync ? PresentInterval.One : PresentInterval.Immediate;
					m_presentParams.SwapEffect =
						m_vBlankSync ? SwapEffect.Flip : SwapEffect.Discard;
					m_presentParams.BackBufferCount = 1;
					m_presentParams.BackBufferFormat = Format.A8R8G8B8;
					m_presentParams.BackBufferWidth = ClientSize.Width > 0 ? ClientSize.Width : 1;
					m_presentParams.BackBufferHeight = ClientSize.Height > 0 ? ClientSize.Height : 1;
					m_presentParams.EnableAutoDepthStencil = false;

					D3D = new Device(
						adapter,
						DeviceType.Hardware,
						this.Handle,
						flags,
						m_presentParams);
					D3D.DeviceResizing += new System.ComponentModel.CancelEventHandler(D3D_DeviceResizing);
					D3D.DeviceReset += new EventHandler(D3D_DeviceReset);
					OnCreateDevice();
					RenderScene();
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void free()
		{
			try
			{
				lock (SyncRoot)
				{
					if (D3D != null)
					{
						OnDestroyDevice();
						D3D.Dispose();
						D3D = null;
					}
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void reset()
		{
			try
			{
				lock (SyncRoot)
				{
					if (D3D == null)
						return;

					m_presentParams.Windowed = true;
					m_presentParams.PresentationInterval =
						m_vBlankSync ? PresentInterval.One : PresentInterval.Immediate;
					m_presentParams.SwapEffect =
						m_vBlankSync ? SwapEffect.Flip : SwapEffect.Discard;
					m_presentParams.BackBufferCount = 1;
					m_presentParams.BackBufferFormat = Format.A8R8G8B8;
					m_presentParams.BackBufferWidth = ClientSize.Width > 0 ? ClientSize.Width : 1;
					m_presentParams.BackBufferHeight = ClientSize.Height > 0 ? ClientSize.Height : 1;
					m_presentParams.EnableAutoDepthStencil = false;

					D3D.Reset(m_presentParams);
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void D3D_DeviceReset(object sender, EventArgs e)
		{
			try
			{
				if (DeviceReset != null)
					DeviceReset(this, EventArgs.Empty);
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		private void D3D_DeviceResizing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			reset();
		}

		protected virtual void OnCreateDevice()
		{
		}

		protected virtual void OnDestroyDevice()
		{
		}

		protected virtual void OnRenderScene()
		{
		}

		protected void RenderScene()
		{
			try
			{
				lock (SyncRoot)
				{
					if (D3D != null && Visible && ClientSize.Width > 0 && ClientSize.Height > 0)
					{
						int resultCodeInt;
						D3D.CheckCooperativeLevel(out resultCodeInt);
						ResultCode resultCode = (ResultCode)resultCodeInt;
						switch (resultCode)
						{
							case ResultCode.DeviceNotReset:
								//LogAgent.Debug("DeviceNotReset");
								reset();
								break;
							case ResultCode.DeviceLost:
								//LogAgent.Debug("DeviceLost");
								// e.g. aquired by other app
								break;
							case ResultCode.Success:
								D3D.Clear(ClearFlags.Target, Color.Black, 1, 0);
								D3D.BeginScene();
								OnRenderScene();
								D3D.EndScene();
								D3D.Present();
								break;
							default:
								LogAgent.Info("CheckCooperativeLevel = {0}", resultCode);
								break;
						}
						m_frameCounter++;
					}
				}
			}
			catch (Exception ex)
			{
				LogAgent.Error(ex);
			}
		}

		public int FrameCounter { get { return m_frameCounter; } }
		public int FrameRate
		{
			get
			{
				lock (SyncRoot)
					if (D3D != null)
						return D3D.DisplayMode.RefreshRate;
				return 0;
			}
		}

        //public bool VBlankSync
        //{
        //    get { return m_vBlankSync; }
        //    set
        //    {
        //        if (value != m_vBlankSync)
        //        {
        //            m_vBlankSync = value;
        //            reset();
        //        }
        //    }
        //}

		// Disable 'Ding' sound on Alt+Enter
		protected const int WM_SYSCHAR = 0x106;
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == WM_SYSCHAR)
			{
				if ((int)m.WParam == 13)
					return;

			}
			base.WndProc(ref m);
		}
	}
}
