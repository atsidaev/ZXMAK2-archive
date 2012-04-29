/// Description: Direct3D renderer control
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;



namespace ZXMAK2.Controls
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

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);
            try
            {
                init();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
            try
            {
			    free();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
            base.OnHandleDestroyed(e);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
            try
            {
                RenderScene();
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
            }
		}

		private void init()
		{
            lock (SyncRoot)
            {
                CreateFlags flags = CreateFlags.SoftwareVertexProcessing;

                //m_presentParams = new PresentParameters();
                m_presentParams.Windowed = true;
                m_presentParams.SwapEffect = SwapEffect.Discard;
                m_presentParams.BackBufferFormat = Manager.Adapters.Default.CurrentDisplayMode.Format;// : Format.A8R8G8B8;
                m_presentParams.BackBufferCount = 1;
                if (m_vBlankSync)
                    m_presentParams.PresentationInterval = PresentInterval.One;
                else
                    m_presentParams.PresentationInterval = PresentInterval.Immediate;

                m_presentParams.BackBufferWidth = ClientSize.Width > 0 ? ClientSize.Width : 1;
                m_presentParams.BackBufferHeight = ClientSize.Height > 0 ? ClientSize.Height : 1;

                D3D = new Device(0, DeviceType.Hardware, this.Handle, flags, m_presentParams);
                D3D.DeviceResizing += new System.ComponentModel.CancelEventHandler(D3D_DeviceResizing);
                D3D.DeviceReset += new EventHandler(D3D_DeviceReset);
			    OnCreateDevice();
            }
        }

		private void free()
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

		private void D3D_DeviceReset(object sender, EventArgs e) 
        {
			if (DeviceReset != null)
				DeviceReset(this, EventArgs.Empty);
		}

		private void D3D_DeviceResizing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = true;
		}

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (D3D != null)
            {
                try
                {
                    m_presentParams.BackBufferWidth = ClientSize.Width > 0 ? ClientSize.Width : 1;
                    m_presentParams.BackBufferHeight = ClientSize.Height > 0 ? ClientSize.Height : 1;
                    D3D.Reset(m_presentParams);
                }
                catch (Exception ex)
                {
                    LogAgent.Error(ex);
                }
            }
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
                            D3D.Reset(m_presentParams);
                            break;
                        case ResultCode.DeviceLost:
                            // e.g. aquired by other app
                            break;
                        case ResultCode.Success:
                            D3D.Clear(ClearFlags.Target, 0, 1, 0);
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

		public int FrameCounter { get { return m_frameCounter; } }
		public int FrameRate 
		{ 
			get 
			{
				lock(SyncRoot)
                    if (D3D != null)
					    return D3D.DisplayMode.RefreshRate;
				return 0;
			} 
		}

        public bool VBlankSync
        {
            get { return m_vBlankSync; }
            set 
            { 
                if (value != m_vBlankSync) 
                {
                    lock (SyncRoot)
                    {
                        m_vBlankSync = value;

                        if (D3D != null)
                        {
                            m_presentParams.PresentationInterval =
                                m_vBlankSync ? PresentInterval.One : PresentInterval.Immediate;
                            m_presentParams.SwapEffect =
                                m_vBlankSync ? SwapEffect.Flip : SwapEffect.Discard;
                            D3D.Reset(m_presentParams);
                        }
                    }
                } 
            }
        }

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
