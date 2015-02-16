//#define SHOW_LATENCY
/// Description: Video renderer control
/// Author: Alex Makeev
/// Date: 27.03.2008
using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ZXMAK2.Engine;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.WinForms.Tools;
using D3dFont = Microsoft.DirectX.Direct3D.Font;
using D3dSprite = Microsoft.DirectX.Direct3D.Sprite;
using D3dTexture = Microsoft.DirectX.Direct3D.Texture;
using D3dTextureLoader = Microsoft.DirectX.Direct3D.TextureLoader;
using ZXMAK2.Host.WinForms.Mdx;


namespace ZXMAK2.Host.WinForms.Controls
{
    public class RenderVideo : Render3D, IHostVideo
    {
        #region Constants

        private const int GraphLength = 150;

        #endregion Constants


        #region Fields

        private readonly FrameResampler _frameResampler = new FrameResampler(50);
        private readonly AutoResetEvent _eventFrame = new AutoResetEvent(false);
        private readonly AutoResetEvent _eventCancel = new AutoResetEvent(false);

        private RenderVideoObject _renderVideo = new RenderVideoObject();
        private RenderIconsObject _renderIcons = new RenderIconsObject();
        private RenderDebugObject _renderDebug = new RenderDebugObject();

        #endregion Fields


        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MimicTv 
        { 
            get { return _renderVideo.MimicTv; }
            set { _renderVideo.MimicTv = value; }
        }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AntiAlias 
        { 
            get { return _renderVideo.AntiAlias; }
            set { _renderVideo.AntiAlias = value; }
        }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScaleMode ScaleMode 
        {
            get { return _renderVideo.ScaleMode; }
            set { _renderVideo.ScaleMode = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VideoFilter VideoFilter
        {
            get { return _renderVideo.VideoFilter; }
            set { _renderVideo.VideoFilter = value; }
        }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DebugInfo 
        {
            get { return _renderDebug.IsEnabled; }
            set { _renderDebug.IsEnabled = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayIcon { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSyncSupported { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size FrameSize 
        {
            get { return _renderDebug.FrameSize; }
            private set { _renderDebug.FrameSize = value; } 
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRunning
        {
            get { return _renderDebug.IsRunning; }
            set { _renderDebug.IsRunning = value; }
        }

        #endregion Properties

        
        public RenderVideo()
        {
            VideoFilter = VideoFilter.None;
            ScaleMode = ScaleMode.FixedPixelSize;
            DisplayIcon = true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventCancel.Set();
                _eventCancel.Dispose();
                _eventFrame.Dispose();
                _renderVideo.Dispose();
                _renderIcons.Dispose();
                _renderDebug.Dispose();
            }
            base.Dispose(disposing);
        }


        #region IHostVideo

        public bool IsSynchronized { get; set; }

        public void CancelWait()
        {
            _eventCancel.Set();
        }

        public void PushFrame(IVideoFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }
            if (_frameResampler.SourceRate > 0 && IsSynchronized && !frame.IsRefresh)
            {
                do
                {
                    if (WaitHandle.WaitAny(new[] { _eventFrame, _eventCancel }) != 0)
                    {
                        return;
                    }
                } while (!_frameResampler.Next());
            }
            _renderDebug.FrameStartTact = frame.StartTact;
            if (!frame.IsRefresh)
            {
                _renderDebug.UpdateFrame(frame.InstantUpdateTime);
            }
            FrameSize = new Size(
                frame.VideoData.Size.Width,
                (int)(frame.VideoData.Size.Height * frame.VideoData.Ratio + 0.5F));
            _renderVideo.Update(frame.VideoData);
            _renderIcons.Update(frame.Icons);
        }

        #endregion IHostVideo

        
        #region Private

        protected override void OnLoadResources()
        {
            base.OnLoadResources();
            IsSyncSupported = true;// _device.DeviceCaps.DriverCaps.ReadScanLine;

            _frameResampler.SourceRate = _device.DisplayMode.RefreshRate;
            _device.SetRenderState(RenderStates.AlphaBlendEnable, true);
            _device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
            _device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
            //_device.SetRenderState(RenderStates.BlendOperation, (int)BlendOperation.Add);
        }

        protected override void OnUnloadResources()
        {
            _renderVideo.ReleaseResources();
            _renderIcons.ReleaseResources();
            _renderDebug.ReleaseResources();
            base.OnUnloadResources();
        }

        protected override void OnRenderScene()
        {
            _device.Clear(ClearFlags.Target, Color.Black, 1F, 0);
            _device.BeginScene();
            try
            {
                var wndSize = new Size(
                    _device.PresentationParameters.BackBufferWidth,
                    _device.PresentationParameters.BackBufferHeight);
                _renderVideo.Render(_device, wndSize);
                _renderDebug.Render(_device, wndSize);
                if (DisplayIcon)
                {
                    _renderIcons.Render(_device, wndSize);
                }
            }
            finally
            {
                _device.EndScene();
            }
            try
            {
                _eventFrame.Set();
                //var limit = 1000 / _device.DisplayMode.RefreshRate;
                //var delta = (int)((Stopwatch.GetTimestamp() - _lastFrameTick) * 1000 / Stopwatch.Frequency);
                //delta -= 1;
                //if (delta > 3 && delta < 20)
                //{
                //    Thread.Sleep(delta - 1);
                //}
                _device.Present();
                _renderDebug.UpdatePresent();
                _lastFrameTick = Stopwatch.GetTimestamp();
            }
            catch (Exception ex)
            {
                // DeviceLostException
                // GraphicsException [-2005530510]
                Logger.Debug(ex);
            }
        }

        private long _lastFrameTick;

        #endregion Private
    }

    public enum ScaleMode
    {
        SquarePixelSize = 0,
        FixedPixelSize,
        KeepProportion,
        Stretch,
    }

    public enum VideoFilter
    {
        None = 0,
        NoFlick,
    }
}
