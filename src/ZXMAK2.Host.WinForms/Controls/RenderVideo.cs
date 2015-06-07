using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.WinForms.Mdx;
using ZXMAK2.Host.WinForms.Mdx.Renderers;
using ZXMAK2.Host.WinForms.Tools;

namespace ZXMAK2.Host.WinForms.Controls
{
    public class RenderVideo : Control, IHostVideo
    {
        private readonly AllocatorPresenter _allocator;
        private readonly VideoRenderer _videoLayer;
        private readonly OsdRenderer _osdLayer;
        private readonly IconRenderer _iconLayer;
        private readonly FrameResampler _frameResampler = new FrameResampler(50);
        private readonly AutoResetEvent _frameEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _cancelEvent = new AutoResetEvent(false);

        #region .ctor

        public RenderVideo()
        {
            SetStyle(
                ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, 
                true);
            
            _allocator = new AllocatorPresenter();
            _videoLayer = new VideoRenderer(_allocator);
            _osdLayer = new OsdRenderer(_allocator);
            _iconLayer = new IconRenderer(_allocator);
            _videoLayer.IsVisible = true;

            _allocator.Register(_videoLayer);
            _allocator.Register(_osdLayer);
            _allocator.Register(_iconLayer);

            _allocator.PresentCompleted += AllocatorPresenter_OnPresentCompleted;

            IsSyncSupported = true;
            VideoFilter = VideoFilter.None;
            ScaleMode = ScaleMode.FixedPixelSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _allocator.Dispose();
                _frameEvent.Dispose();
                _cancelEvent.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion .ctor


        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MimicTv
        {
            get { return _videoLayer.MimicTv; }
            set { _videoLayer.MimicTv = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AntiAlias
        {
            get { return _videoLayer.AntiAlias; }
            set { _videoLayer.AntiAlias = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScaleMode ScaleMode
        {
            get { return _videoLayer.ScaleMode; }
            set { _videoLayer.ScaleMode = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VideoFilter VideoFilter
        {
            get { return _videoLayer.VideoFilter; }
            set { _videoLayer.VideoFilter = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DebugInfo
        {
            get { return _osdLayer.IsVisible; }
            set { _osdLayer.IsVisible = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayIcon 
        {
            get { return _iconLayer.IsVisible; }
            set { _iconLayer.IsVisible = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSyncSupported { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size FrameSize
        {
            get { return _osdLayer.FrameSize; }
            private set { _osdLayer.FrameSize = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRunning
        {
            get { return _osdLayer.IsRunning; }
            set { _osdLayer.IsRunning = value; }
        }

        public void InitWnd()
        {
            _allocator.Attach(Handle);
        }

        public void FreeWnd()
        {
            _allocator.Dispose();
        }

        #endregion Properties


        #region IHostVideo

        public bool IsSynchronized { get; set; }

        public void CancelWait()
        {
            _cancelEvent.Set();
        }

        public void PushFrame(IFrameInfo info, IFrameVideo frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }
            if (_frameResampler.SourceRate > 0 && IsSynchronized && !info.IsRefresh)
            {
                do
                {
                    var waitEvents = new[] 
                    { 
                        _frameEvent, 
                        _cancelEvent 
                    };
                    if (WaitHandle.WaitAny(waitEvents) != 0)
                    {
                        return;
                    }
                } while (!_frameResampler.Next());
            }
            _osdLayer.FrameStartTact = info.StartTact;
            _osdLayer.SampleRate = info.SampleRate;
            if (!info.IsRefresh)
            {
                _osdLayer.UpdateFrame(info.UpdateTime);
            }
            FrameSize = new Size(
                frame.Size.Width,
                (int)(frame.Size.Height * frame.Ratio + 0.5F));
            _videoLayer.Update(frame);
            _iconLayer.Update(info.Icons);
        }

        #endregion IHostVideo

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Black, e.ClipRectangle);
            e.Graphics.DrawImage(SystemIcons.Warning.ToBitmap(), new Point(20, 20));
            using (var font = new System.Drawing.Font(Font.FontFamily, 20))
            {
                e.Graphics.DrawString(
                    "Failed to initialize DirectX!",
                    font,
                    Brushes.White,
                    new PointF(20 + SystemIcons.Warning.Width + 20, 20));
            }
        }

        private void AllocatorPresenter_OnPresentCompleted(object sender, EventArgs e)
        {
            _osdLayer.UpdatePresent();
            _frameEvent.Set();
            _frameResampler.SourceRate = _allocator.Device.DisplayMode.RefreshRate;
        }
    }
}
