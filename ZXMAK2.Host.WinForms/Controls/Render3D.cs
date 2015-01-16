/// Description: Direct3D renderer control
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;



namespace ZXMAK2.Host.WinForms.Controls
{
    public class Render3D : Control
    {
        #region Fields

        private readonly PresentParameters m_presentParams = new PresentParameters();
        protected readonly object SyncRoot = new object();
        protected Device D3D;

        #endregion Fields


        public Render3D()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
        }


        #region Properties

        public event EventHandler DeviceReset;

        #endregion Properties


        #region Public

        public void InitWnd()
        {
            lock (SyncRoot)
            {
                if (D3D != null)
                {
                    return;
                }
                InitDevice();
            }
        }

        public void FreeWnd()
        {
            lock (SyncRoot)
            {
                if (D3D == null)
                {
                    return;
                }
                FreeDevice();
            }
        }

        #endregion Public


        #region Private

        private void InitDevice()
        {
            try
            {
                lock (SyncRoot)
                {
                    D3D = CreateDirect3D();
                    D3D.DeviceResizing += new System.ComponentModel.CancelEventHandler(Device_OnDeviceResizing);
                    D3D.DeviceReset += new EventHandler(Device_OnDeviceReset);
                    OnCreateDevice();
                    RenderScene();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void FreeDevice()
        {
            try
            {
                lock (SyncRoot)
                {
                    if (D3D == null)
                    {
                        return;
                    }
                    OnDestroyDevice();
                    D3D.Dispose();
                    D3D = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void ResetDevice()
        {
            try
            {
                lock (SyncRoot)
                {
                    if (D3D == null)
                    {
                        return;
                    }
                    ConfigureDeviceParams(false);
                    D3D.Reset(m_presentParams);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void Device_OnDeviceReset(object sender, EventArgs e)
        {
            try
            {
                if (DeviceReset == null)
                {
                    return;
                }
                DeviceReset(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void Device_OnDeviceResizing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
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
                    RenderError(e, "Direct3D initialization failed!\nProbably DirectX 9 (June 2010) is not installed");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ResetDevice();
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
                    if (D3D == null ||
                        !Visible ||
                        ClientSize.Width <= 0 ||
                        ClientSize.Height <= 0)
                    {
                        return;
                    }
                    int resultCodeInt;
                    D3D.CheckCooperativeLevel(out resultCodeInt);
                    var resultCode = (ResultCode)resultCodeInt;
                    switch (resultCode)
                    {
                        case ResultCode.DeviceNotReset:
                            //LogAgent.Debug("DeviceNotReset");
                            ResetDevice();
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
                            Logger.Info("CheckCooperativeLevel = {0}", resultCode);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        protected void RenderError(PaintEventArgs e, string message)
        {
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
            e.Graphics.DrawImage(SystemIcons.Warning.ToBitmap(), new Point(10, 10));
            using (var font = new System.Drawing.Font(Font.FontFamily, 20))
            {
                e.Graphics.DrawString(
                    message,
                    font,
                    Brushes.White,
                    new PointF(10 + SystemIcons.Warning.Width + 10, 10));
            }
        }

        #endregion Private


        #region Direct3D Initialization

        private int GetAdapterId()
        {
            var screen = Screen.FromControl(this);
            var adapterInfo = Manager.Adapters
                .OfType<AdapterInformation>()
                .FirstOrDefault(ai => ai.Information.DeviceName == screen.DeviceName);
            adapterInfo = adapterInfo ?? Manager.Adapters.Default;
            return adapterInfo.Adapter;
        }

        private void ConfigureDeviceParams(bool vBlankSync)
        {
            m_presentParams.Windowed = true;
            m_presentParams.PresentationInterval =
                vBlankSync ? PresentInterval.One : PresentInterval.Immediate;
            m_presentParams.SwapEffect =
                vBlankSync ? SwapEffect.Flip : SwapEffect.Discard;
            m_presentParams.BackBufferCount = 1;
            m_presentParams.BackBufferFormat = Format.A8R8G8B8;
            m_presentParams.BackBufferWidth = ClientSize.Width > 0 ? ClientSize.Width : 1;
            m_presentParams.BackBufferHeight = ClientSize.Height > 0 ? ClientSize.Height : 1;
            m_presentParams.EnableAutoDepthStencil = false;
        }

        private Device CreateDirect3D()
        {
            ConfigureDeviceParams(false);
            var adapterId = GetAdapterId();
            var caps = Manager.GetDeviceCaps(adapterId, DeviceType.Hardware);
            var flags = CreateFlags.MultiThreaded;
            if (caps.DeviceCaps.SupportsHardwareTransformAndLight)
                flags |= CreateFlags.HardwareVertexProcessing;
            else
                flags |= CreateFlags.SoftwareVertexProcessing;
            return new Device(
                adapterId,
                DeviceType.Hardware,
                this.Handle,
                flags,
                m_presentParams);
        }

        #endregion Direct3D Initialization


        #region WndProc

        // Disable 'Ding' sound on Alt+Enter
        protected const int WM_SYSCHAR = 0x106;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCHAR && (int)m.WParam == 13)
            {
                return;
            }
            base.WndProc(ref m);
        }

        #endregion WndProc
    }
}
