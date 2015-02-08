/// Description: Direct3D renderer control
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;
using System.Threading;
using System.Diagnostics;



namespace ZXMAK2.Host.WinForms.Controls
{
    public class Render3D : Control
    {
        #region Fields

        private readonly PresentParameters _presentParams = new PresentParameters();
        protected readonly object _syncRoot = new object();
        protected Device _device;
        private Thread _threadRender;

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
            InitDevice();
        }

        public void FreeWnd()
        {
            FreeDevice();
        }

        #endregion Public


        #region Private

        private void InitDevice()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_device != null)
                    {
                        return;
                    }
                    _device = CreateDirect3D();
                    _device.DeviceResizing += new System.ComponentModel.CancelEventHandler(Device_OnDeviceResizing);
                    _device.DeviceReset += new EventHandler(Device_OnDeviceReset);
                    OnLoadResources();
                    RenderScene();
                    _renderEvent.Reset();
                    _threadRender = new Thread(RenderProc);
                    _threadRender.Name = "Render";
                    _threadRender.Priority = ThreadPriority.AboveNormal;
                    _threadRender.Start();
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
                var thread = default(Thread);
                lock (_syncRoot)
                {
                    thread = _threadRender;
                    _threadRender = null;
                }
                if (thread != null)
                {
                    _renderEvent.Set();
                    thread.Join();
                }
                lock (_syncRoot)
                {
                    if (_device == null)
                    {
                        return;
                    }
                    OnUnloadResources();
                    Dispose(ref _device);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private long? _resetStartStamp; 

        private void ResetDevice()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_device == null)
                    {
                        return;
                    }
                    OnUnloadResources();
                    ConfigureDeviceParams(false);
                    _device.Reset(_presentParams);
                    OnLoadResources();
                    _resetStartStamp = null;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private readonly AutoResetEvent _renderEvent = new AutoResetEvent(false);

        private void RenderProc()
        {
            while (_threadRender != null)
            {
                try
                {
                    _renderEvent.WaitOne();
                    RenderScene();
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        protected void RenderAsync()
        {
            _renderEvent.Set();
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
                if (_device != null)
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

        protected virtual void OnLoadResources()
        {
        }

        protected virtual void OnUnloadResources()
        {
        }

        protected virtual void OnRenderScene()
        {
        }

        protected virtual bool CanRender()
        {
            return true;
        }

        protected void RenderScene()
        {
            try
            {
                lock (_syncRoot)
                {
                    if (_device == null ||
                        !Visible ||
                        ClientSize.Width <= 0 ||
                        ClientSize.Height <= 0)
                    {
                        return;
                    }
                    var hr = TestCooperativeLevel();
                    if (hr == ResultCode.Success)
                    {
                        OnRenderScene();
                    }
                    else if (hr == ResultCode.DeviceNotReset)
                    {
                        //Logger.Debug("DeviceReset: TODO - sync with UI");

                        if (!_resetStartStamp.HasValue)
                        {
                            _resetStartStamp = Stopwatch.GetTimestamp();
                            BeginInvoke(new Action(ResetDevice));
                        }
                        else
                        {
                            var delta = Stopwatch.GetTimestamp() - _resetStartStamp;
                            if (delta * 1000 / Stopwatch.Frequency > 300)
                            {
                                _resetStartStamp = null;
                            }
                        }
                    }
                    else if (hr == ResultCode.DeviceLost)
                    {
                        Thread.Sleep(10);
                    }
                    else
                    {
                        Logger.Warn("TestCooperativeLevel() = {0}", hr);
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
            _presentParams.Windowed = true;
            _presentParams.PresentationInterval =
                vBlankSync ? PresentInterval.One : PresentInterval.Immediate;
            _presentParams.SwapEffect =
                vBlankSync ? SwapEffect.Flip : SwapEffect.Discard;
            _presentParams.BackBufferCount = 1;
            _presentParams.BackBufferFormat = Format.A8R8G8B8;
            _presentParams.BackBufferWidth = ClientSize.Width > 0 ? ClientSize.Width : 1;
            _presentParams.BackBufferHeight = ClientSize.Height > 0 ? ClientSize.Height : 1;
            _presentParams.EnableAutoDepthStencil = false;
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
                _presentParams);
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


        #region Helpers

        protected ResultCode TestCooperativeLevel()
        {
            if (_device == null)
            {
                return ResultCode.NotAvailable;
            }
            int hr;
            _device.CheckCooperativeLevel(out hr);
            return (ResultCode)hr;
        }

        protected static void Dispose<T>(ref T disposable)
            where T : IDisposable
        {
            var value = disposable;
            disposable = default(T);
            if (value != null)
            {
                value.Dispose();
            }
        }

        #endregion Helpers
    }
}
