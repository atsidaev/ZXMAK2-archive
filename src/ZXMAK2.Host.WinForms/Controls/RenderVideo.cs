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


namespace ZXMAK2.Host.WinForms.Controls
{
    public class RenderVideo : Render3D, IHostVideo
    {
        #region Constants

        private const byte MimicTvRatio = 4;      // mask size 1/x of pixel
        private const byte MimicTvAlpha = 0x90;   // mask alpha
        private const int GraphLength = 150;

        #endregion Constants


        #region Fields

        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(true);
        private readonly Dictionary<IIconDescriptor, IconTextureWrapper> _iconTextures = new Dictionary<IIconDescriptor, IconTextureWrapper>();
        private readonly FrameResampler _frameResampler = new FrameResampler(50);

        private D3dSprite _sprite;
        private D3dSprite _spriteTv;
        private D3dSprite _spriteIcon;
        private D3dTexture _texture0;
        private D3dTexture _textureMaskTv;
        private D3dFont _font;
        private Size _surfaceSize;
        private Size _maskTvSize;
        private int _texturePitch;

        private VideoFilterDelegate _videoFilter;
        private readonly GraphMonitor _graphRender = new GraphMonitor(GraphLength);
        private readonly GraphMonitor _graphLoad = new GraphMonitor(GraphLength);
        private readonly GraphMonitor _graphUpdate = new GraphMonitor(GraphLength);
#if SHOW_LATENCY
        private readonly GraphMonitor _graphLatency = new GraphMonitor(GraphLength);
        //private readonly GraphMonitor _copyGraph = new GraphMonitor(GraphLength);
#endif
        private int[] _lastBuffer = new int[0];    // noflick
        private long _lastBlankStamp;              // WaitVBlank
        private int _debugFrameStart = 0;
        private float m_surfaceHeightScale = 1F;
        private bool _isRunning;
        private bool _isDebugInfo;
        private bool _isCancelWait;

        #endregion Fields


        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MimicTv { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AntiAlias { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScaleMode ScaleMode { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DebugInfo 
        {
            get { return _isDebugInfo; }
            set
            {
                if (_isDebugInfo == value)
                {
                    return;
                }
                _isDebugInfo = value;
                _graphRender.Clear();
                _graphLoad.Clear();
                _graphUpdate.Clear();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public VideoFilter VideoFilter
        {
            get
            {
                unsafe
                {
                    return _videoFilter == DrawFrame_None ? VideoFilter.None :
                        _videoFilter == DrawFrame_NoFlick ? VideoFilter.NoFlick :
                        VideoFilter.None;
                }
            }
            set
            {
                unsafe
                {
                    switch (value)
                    {
                        case VideoFilter.NoFlick:
                            _videoFilter = DrawFrame_NoFlick;
                            break;
                        case VideoFilter.None:
                        default:
                            _videoFilter = DrawFrame_None;
                            break;
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayIcon { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSyncSupported { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Size FrameSize { get; private set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                if (_isRunning == value)
                {
                    return;
                }
                _isRunning = value;
                _graphUpdate.ResetPeriod();
                _graphRender.ResetPeriod();
                //_renderGraph.Clear();
                //_loadGraph.Clear();
            }
        }

        #endregion Properties

        
        public unsafe RenderVideo()
        {
            _videoFilter = DrawFrame_None;
            DisplayIcon = true;
            ScaleMode = ScaleMode.FixedPixelSize;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CancelWait();
                _waitEvent.Dispose();
                
                var list = _iconTextures.Values;
                _iconTextures.Clear();
                foreach (var icon in list)
                {
                    icon.Dispose();
                }
            }
            base.Dispose(disposing);
        }


        #region IHostVideo

        public bool IsSynchronized { get; set; }

        private void WaitFrame()
        {
            if (_device == null)
            {
                return;
            }
            WaitFrameInt();
        }

        private void WaitFrameInt()
        {
            _waitEvent.Reset();
            try
            {
                if (_isCancelWait || !IsSyncSupported)
                {
                    return;
                }
                var frameRate = _device.DisplayMode.RefreshRate;
                if (frameRate <= 50)
                {
                    frameRate = 50;
                }
                _frameResampler.SourceRate = frameRate;
                while (!_isCancelWait)
                {
                    WaitVBlank(frameRate);
                    if (_frameResampler.Next())
                    {
                        break;
                    }
                    RenderAsync();
                    //RequestPresentAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                _waitEvent.Set();
            }
        }

        public void CancelWait()
        {
            _isCancelWait = true;
            Thread.MemoryBarrier();
            _waitEvent.WaitOne();
            _isCancelWait = false;
            Thread.MemoryBarrier();
        }

        private void WaitVBlank(int refreshRate)
        {
            // check if VBlank has already occurred 
            var frequency = Stopwatch.Frequency;
            var timeStamp = Stopwatch.GetTimestamp();
            var timeFrame = frequency / refreshRate;
            var delta = timeStamp - _lastBlankStamp;
            if (delta >= timeFrame)
            {
                // some frames was missed, so try to catch up
                _lastBlankStamp += timeFrame;
                if (delta > timeFrame * 2)
                {
                    // too late, so resync
                    _lastBlankStamp = timeStamp;
                }
#if SHOW_LATENCY
                _graphLatency.PushValue(double.NaN);
#endif
                return;
            }

            // wait VBlank
            var vtimeFrame = _device.DisplayMode.Height;
            var vfrequency = vtimeFrame * refreshRate;
            var vtime = _device.RasterStatus.ScanLine;
            if (vtime < vtimeFrame)
            {
                var delay = ((vtimeFrame - vtime) * 1000) / vfrequency;
                if (delay > 4 && delay < 40)
                {
                    delay = delay - 1;
#if SHOW_LATENCY
                    timeStamp = Stopwatch.GetTimestamp();
#endif
                    Thread.Sleep(delay);
#if SHOW_LATENCY
                    var realTime = (Stopwatch.GetTimestamp() - timeStamp) * 1000D / frequency;
                    _graphLatency.PushValue(realTime - delta);
#endif
                }
            }
            var deadline = timeFrame + timeFrame / vtimeFrame;
            while (!_isCancelWait && !_device.RasterStatus.InVBlank)
            {
                if (Stopwatch.GetTimestamp() - _lastBlankStamp >= deadline)
                {
                    // our processor time was stolen by another process so we loss vblank
                    _lastBlankStamp += deadline;
                    return;
                }                
                Thread.SpinWait(1);
            }
            _lastBlankStamp = Stopwatch.GetTimestamp();
        }

        public void PushFrame(IVideoFrame frame)
        {
            if (frame == null)
            {
                throw new ArgumentNullException("frame");
            }
            if (IsSynchronized && !frame.IsRefresh)
            {
                WaitFrame();
            }
            if (!frame.IsRefresh)
            {
                if (DebugInfo)
                {
                    _graphUpdate.PushPeriod();
                    _graphLoad.PushValue(frame.InstantUpdateTime);
                }
                //RequestPresentAsync();
                RenderAsync();
            }
            _debugFrameStart = frame.StartTact;
            FrameSize = new Size(
                frame.VideoData.Size.Width,
                (int)(frame.VideoData.Size.Height * frame.VideoData.Ratio + 0.5F));
            lock (_syncRoot)
            {
                if (_device == null)
                {
                    return;
                }
                UpdateSurface(frame.VideoData);
                UpdateIconList(frame.Icons);
            }
            if (frame.IsRefresh)
            {
                //RequestPresentAsync();
                RenderAsync();
            }
        }

        #endregion IHostVideo

        
        #region Private

        protected override void OnLoadResources()
        {
            base.OnLoadResources();
            IsSyncSupported = _device.DeviceCaps.DriverCaps.ReadScanLine;
            _sprite = new D3dSprite(_device);
            _spriteTv = new D3dSprite(_device);
            _spriteIcon = new D3dSprite(_device);

            var gdiFont = new System.Drawing.Font(
                "Microsoft Sans Serif",
                10f/*8.25f*/,
                System.Drawing.FontStyle.Bold,
                GraphicsUnit.Pixel);
            _font = new D3dFont(_device, gdiFont);

            _device.SetRenderState(RenderStates.AlphaBlendEnable, true);
            _device.SetRenderState(RenderStates.SourceBlend, (int)Blend.SourceAlpha);
            _device.SetRenderState(RenderStates.DestinationBlend, (int)Blend.InvSourceAlpha);
            //_device.SetRenderState(RenderStates.BlendOperation, (int)BlendOperation.Add);
        }

        protected override void OnUnloadResources()
        {
            Dispose(ref _texture0);
            Dispose(ref _textureMaskTv);
            Dispose(ref _sprite);
            Dispose(ref _spriteTv);
            Dispose(ref _spriteIcon);
            Dispose(ref _font);
            foreach (var iconTexture in _iconTextures.Values)
            {
                iconTexture.UnloadResources();
            }
            base.OnUnloadResources();
        }

        private unsafe void UpdateSurface(IVideoData videoData)
        {
            try
            {
                m_surfaceHeightScale = videoData.Ratio;
                if (_surfaceSize != videoData.Size || _texture0 == null)
                {
                    Dispose(ref _texture0);
                    Dispose(ref _textureMaskTv);
                    CreateTextures(videoData.Size);
                }
                if (_texture0 != null)
                {
                    using (var gs = _texture0.LockRectangle(0, LockFlags.None))
                    {
                        fixed (int* srcPtr = videoData.Buffer)
                        {
                            //var startTick = Stopwatch.GetTimestamp();
                            _videoFilter((int*)gs.InternalData, srcPtr);
                            //var copyTime = Stopwatch.GetTimestamp() - startTick;
                            //PushGraphValue(m_copyGraph, ref m_copyGraphIndex, copyTime);
                        }
                    }
                    _texture0.UnlockRectangle(0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private unsafe void CreateTextures(Size surfaceSize)
        {
            var potSize = GetPotSize(surfaceSize);
            _texture0 = new D3dTexture(_device, potSize, potSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            _texturePitch = potSize;
            _surfaceSize = surfaceSize;

            var maskTvSize = new Size(surfaceSize.Width, surfaceSize.Height * MimicTvRatio);
            var maskTvPitch = GetPotSize(maskTvSize);
            _textureMaskTv = new D3dTexture(_device, maskTvPitch, maskTvPitch, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
            _maskTvSize = new Size(maskTvPitch, maskTvPitch);
            using (var gs = _textureMaskTv.LockRectangle(0, LockFlags.None))
            {
                var pixelColor = 0;
                var gapColor = MimicTvAlpha << 24;
                var pdst = (int*)gs.InternalData.ToPointer();
                for (var y = 0; y < maskTvSize.Height; y++)
                {
                    pdst += maskTvPitch;
                    var color = (y % MimicTvRatio) != (MimicTvRatio - 1) ? pixelColor : gapColor;
                    for (var x = 0; x < maskTvSize.Width; x++)
                    {
                        pdst[x] = color;
                    }
                }
            }
            _textureMaskTv.UnlockRectangle(0);
        }

        protected override void OnRenderScene()
        {
            _device.Clear(ClearFlags.Target, Color.Black, 1, 0);
            _device.BeginScene();
            try
            {
                if (_texture0 != null)
                {
                    if (DebugInfo)
                    {
                        _graphRender.PushPeriod();
                    }
                    var wndSize = GetDeviceSize();
                    var dstRect = GetDestinationRect(wndSize, GetSurfaceScaledSize());
                    RenderFrame(dstRect, _surfaceSize);
                    if (MimicTv)
                    {
                        RenderMaskTv(dstRect, _surfaceSize);
                    }
                    if (DebugInfo)
                    {
                        RenderDebugInfo(wndSize);
                    }
                    if (DisplayIcon)
                    {
                        RenderIcons();
                    }
                }
            }
            finally
            {
                _device.EndScene();
            }
            try
            {
                _device.Present();
            }
            catch (Exception ex)
            {
                // DeviceLostException
                // GraphicsException [-2005530510]
                Logger.Debug(ex);
            }
        }

        private void RenderFrame(RectangleF dstRect, Size size)
        {
            var srcRect = new Rectangle(0, 0, size.Width, size.Height);
            _sprite.Begin(SpriteFlags.None);
            try
            {
                if (!AntiAlias)
                {
                    _device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
                    _device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
                    _device.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Point);
                }
                _sprite.Draw2D(
                   _texture0,
                   srcRect,
                   dstRect.Size,
                   dstRect.Location,
                   -1);
            }
            finally
            {
                _sprite.End();
            }
        }

        private void RenderMaskTv(RectangleF dstRect, Size size)
        {
            var srcRect = new Rectangle(0, 0, size.Width, size.Height * MimicTvRatio);
            _spriteTv.Begin(SpriteFlags.AlphaBlend);
            try
            {
                _spriteTv.Draw2D(
                    _textureMaskTv,
                    srcRect,
                    dstRect.Size,
                    dstRect.Location,
                    -1);
            }
            finally
            {
                _spriteTv.End();
            }
        }

        private void RenderIcons()
        {
            var iconSize = new SizeF(32, 32);
            var iconNumber = 1;
            foreach (var iconTexture in _iconTextures.Values)
            {
                if (!iconTexture.Visible || iconTexture.Texture == null)
                {
                    continue;
                }
                var iconRect = new Rectangle(new Point(0, 0), iconTexture.Size);
                var iconPos = new PointF(_device.PresentationParameters.BackBufferWidth - iconSize.Width * iconNumber, 0);
                _spriteIcon.Begin(SpriteFlags.AlphaBlend);
                try
                {
                    _spriteIcon.Draw2D(
                       iconTexture.Texture,
                       iconRect,
                       iconSize,
                       iconPos,
                       -1);
                }
                finally
                {
                    _spriteIcon.End();
                }
                iconNumber++;
            }
        }

        private void RenderDebugInfo(SizeF wndSize)
        {
            var frameRate = _device.DisplayMode.RefreshRate;
            var graphRender = _graphRender.Get();
            var graphLoad = _graphLoad.Get();
#if SHOW_LATENCY
            var graphLatency = _graphLatency.Get();
            //var graphCopy = _copyGraph.Get();
#endif
            var graphUpdate = _graphUpdate.Get();
            var frequency = GraphMonitor.Frequency;
            var limitDisplay = frequency / frameRate;
            var limit50 = frequency / 50D;
            var limit1ms = frequency / 1000D;
            var maxRender = graphRender.Max();
            var maxLoad = graphLoad.Max();
            var minT = graphRender.Min() * 1000D / frequency;
            var avgT = graphRender.Average() * 1000D / frequency;
            var maxT = maxRender * 1000D / frequency;
#if SHOW_LATENCY
            var minL = _graphLatency.IsDataAvailable ? graphLatency.Min() * 1000D / frequency : 0D;
            var avgL = _graphLatency.IsDataAvailable ? graphLatency.Average() * 1000D / frequency : 0D;
            var maxL = _graphLatency.IsDataAvailable ? graphLatency.Max() * 1000D / frequency : 0D;
#endif
            var avgE = graphLoad.Average() * 1000D / frequency;
            var avgU = graphUpdate.Average() * 1000D / frequency;
            var maxScale = Math.Max(maxRender, maxLoad);
            maxScale = Math.Max(maxScale, limit50);
            maxScale = Math.Max(maxScale, limitDisplay);
            var fpsRender = 1000D / avgT;
            var fpsUpdate = 1000D / avgU;
            var textValue = string.Format(
                "Render FPS: {0:F3}\nUpdate FPS: {1:F3}\nDevice FPS: {2}\nBack: [{3}, {4}]\nClient: [{5}, {6}]\nSurface: [{7}, {8}]\nFrameStart: {9}T",
                fpsRender,
                IsRunning ? fpsUpdate : (double?)null,
                frameRate,
                wndSize.Width,
                wndSize.Height,
                ClientSize.Width,
                ClientSize.Height,
                _surfaceSize.Width,
                _surfaceSize.Height,
                _debugFrameStart);
            var textRect = _font.MeasureString(
                null,
                textValue,
                DrawTextFormat.NoClip,
                Color.Yellow);
            textRect = new Rectangle(
                textRect.Left,
                textRect.Top,
                Math.Max(textRect.Width + 10, GraphLength),
                textRect.Height);
            FillRect(textRect, Color.FromArgb(192, Color.Green));
            _font.DrawText(
                null,
                textValue,
                textRect,
                DrawTextFormat.NoClip,
                Color.Yellow);
            if (IsRunning)
            {
                // Draw graphs
                var graphRect = new Rectangle(
                    textRect.Left,
                    textRect.Top + textRect.Height,
                    GraphLength,
                    (int)(wndSize.Height - textRect.Top - textRect.Height));
                FillRect(graphRect, Color.FromArgb(192, Color.Black));
                RenderGraph(graphRender, maxScale, graphRect, Color.FromArgb(196, Color.Lime));
                RenderGraph(graphLoad, maxScale, graphRect, Color.FromArgb(196, Color.Red));
                //RenderGraph(graphCopy, maxTime, graphRect, Color.FromArgb(196, Color.Yellow));
                RenderLimit(limitDisplay, maxScale, graphRect, Color.FromArgb(196, Color.Yellow));
                RenderLimit(limit50, maxScale, graphRect, Color.FromArgb(196, Color.Magenta));
                DrawGraphGrid(maxScale, limit1ms, graphRect, _graphRender.GetIndex(), Color.FromArgb(64, Color.White));

                var msgTime = string.Format(
                    "MinT: {0:F3} [ms]\nAvgT: {1:F3} [ms]\nMaxT: {2:F3} [ms]\nAvgE: {3:F3} [ms]",
                    minT,
                    avgT,
                    maxT,
                    avgE);
#if SHOW_LATENCY
                if (_graphLatency.IsDataAvailable)
                {
                    msgTime = string.Format(
                        "{0}\nMinL: {1:F3} [ms]\nAvgL: {2:F3} [ms]\nMaxL: {3:F3} [ms]",
                        msgTime,
                        minL,
                        avgL,
                        maxL);
                }
#endif
                _font.DrawText(
                    null,
                    msgTime,
                    graphRect,
                    DrawTextFormat.NoClip,
                    Color.FromArgb(156, Color.Yellow));
            }
        }

        private void DrawGraphGrid(double maxValue, double step, Rectangle rect, int index, Color color)
        {
            var icolor = color.ToArgb();
            var list = new List<Point>();
            var lineCount = maxValue / step;
            if (lineCount > 40*40D)
            {
                step = maxValue / 20D;
                icolor = Color.FromArgb(color.A, Color.Violet).ToArgb();
            }
            else if (lineCount > 40D)
            {
                step *= 10D;
                icolor = Color.FromArgb(color.A, Color.Red).ToArgb();
            }
            for (var t = 0D; t < maxValue; t += step)
            {
                var value = (int)((1D - (t / maxValue)) * rect.Height);
                list.Add(new Point(rect.Left, rect.Top + value));
                list.Add(new Point(rect.Left + rect.Width, rect.Top + value));
            }
            for (var t = 0; t < GraphLength; t += 25)
            {
                var ts = GraphLength - (t + index) % GraphLength;
                list.Add(new Point(rect.Left + ts, rect.Top));
                list.Add(new Point(rect.Left + ts, rect.Top + rect.Height));
            }

            var vertices = list
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, icolor))
                .ToArray();
            _device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            _device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        private void RenderLimit(double limit, double maxValue, Rectangle rect, Color color)
        {
            if (limit < 0 || limit > maxValue)
            {
                return;
            }
            var icolor = color.ToArgb();
            var list = new List<Point>();
            var value = 1D - (limit / maxValue);
            if (value < 0D)
            {
                value = 0;
            }
            var hValue = (int)(value * rect.Height);
            list.Add(new Point(rect.Left, rect.Top + hValue));
            list.Add(new Point(rect.Left+rect.Width, rect.Top + hValue));
            var vertices = list
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, icolor))
                .ToArray();
            _device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            _device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
        }

        private void RenderGraph(double[] graph, double maxValue, Rectangle rect, Color color)
        {
            if (graph.Length < 1)
            {
                return;
            }
            var icolor = color.ToArgb();
            var list = new List<Point>();
            for (var x = 0; x < graph.Length && x < rect.Width; x++)
            {
                var value = 1D - (graph[x] / maxValue);
                if (value < 0D)
                {
                    value = 0;
                }
                var hValue = (int)(value * rect.Height);
                list.Add(new Point(rect.Left + x, rect.Top + rect.Height));
                list.Add(new Point(rect.Left + x, rect.Top + hValue));
            }
            var vertices = list
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, icolor))
                .ToArray();
            _device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            _device.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length/2, vertices);
        }

        private void FillRect(Rectangle rect, Color color)
        {
            var icolor = color.ToArgb();
            var rectv = new[]
            {
                new CustomVertex.TransformedColored(rect.Left, rect.Top+rect.Height+0.5F, 0, 1f, icolor),
                new CustomVertex.TransformedColored(rect.Left, rect.Top, 0, 1f, icolor),
                new CustomVertex.TransformedColored(rect.Left+rect.Width, rect.Top+rect.Height+0.5F, 0, 1f, icolor),
                new CustomVertex.TransformedColored(rect.Left+rect.Width, rect.Top, 0, 1f, icolor),
            };
            _device.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            _device.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rectv);
        }

        private RectangleF GetDestinationRect(SizeF wndSize, SizeF size)
        {
            var dstSize = GetDestinationSize(wndSize, size);
            var dstPos = GetDestinationPos(wndSize, dstSize);
            return new RectangleF(dstPos, dstSize);
        }

        private PointF GetDestinationPos(SizeF wndSize, SizeF dstSize)
        {
            return new PointF(
                (float)Math.Floor((wndSize.Width - dstSize.Width) / 2F),
                (float)Math.Floor((wndSize.Height - dstSize.Height) / 2F));
        }

        private SizeF GetDestinationSize(SizeF wndSize, SizeF srfScaledSize)
        {
            var dstSize = srfScaledSize;
            if (dstSize.Width > 0 &&
                dstSize.Height > 0)
            {
                var rx = wndSize.Width / dstSize.Width;
                var ry = wndSize.Height / dstSize.Height;
                if (ScaleMode == ScaleMode.SquarePixelSize)
                {
                    rx = (float)Math.Floor(rx);
                    ry = (float)Math.Floor(ry);
                    rx = ry = Math.Min(rx, ry);
                    rx = rx < 1F ? 1F : rx;
                    ry = ry < 1F ? 1F : ry;
                }
                if (ScaleMode == ScaleMode.FixedPixelSize)
                {
                    rx = (float)Math.Floor(rx);
                    ry = (float)Math.Floor(ry);
                    rx = rx < 1F ? 1F : rx;
                    ry = ry < 1F ? 1F : ry;
                }
                else if (ScaleMode == ScaleMode.KeepProportion)
                {
                    if (rx > ry)
                    {
                        rx = (wndSize.Width * ry / rx) / dstSize.Width;
                    }
                    else if (rx < ry)
                    {
                        ry = (wndSize.Height * rx / ry) / dstSize.Height;
                    }
                }
                dstSize = new SizeF(
                    dstSize.Width * rx,
                    dstSize.Height * ry);
            }
            return dstSize;
        }

        private SizeF GetDeviceSize()
        {
            var param = _device.PresentationParameters;
            return new SizeF(
                param.BackBufferWidth, 
                param.BackBufferHeight);
        }

        private SizeF GetSurfaceScaledSize()
        {
            var surfSize = _surfaceSize;
            return new SizeF(
                surfSize.Width,
                surfSize.Height * m_surfaceHeightScale);
        }

        private static int GetPotSize(Size surfaceSize)
        {
            // Create POT texture (e.g. 512x512) to render NPOT image (e.g. 320x240),
            // because NPOT textures is not supported on some videocards
            var size = surfaceSize.Width > surfaceSize.Height ?
                surfaceSize.Width :
                surfaceSize.Height;
            var potSize = 0;
            for (var power = 1; potSize < size; power++)
            {
                potSize = Pow(2, power);
            }
            return potSize;
        }

        private static int Pow(int value, int power)
        {
            var result = value;
            for (var i = 0; i < power; i++)
            {
                result *= value;
            }
            return result;
        }

        private void UpdateIconList(IIconDescriptor[] icons)
        {
            var nonUsed = _iconTextures.Keys.ToList();
            foreach (var id in icons)
            {
                var iconTexture = default(IconTextureWrapper);
                if (!_iconTextures.ContainsKey(id))
                {
                    iconTexture = new IconTextureWrapper(id);
                    _iconTextures.Add(id, iconTexture);
                }
                else
                {
                    iconTexture = _iconTextures[id];
                }
                iconTexture.Visible = id.Visible;
                nonUsed.Remove(id);
                if (_device != null)
                {
                    iconTexture.LoadResources(_device);
                }
            }
            foreach (var id in nonUsed)
            {
                _iconTextures[id].Dispose();
                _iconTextures.Remove(id);
            }
        }

        #endregion Private


        #region Video Filters

        private unsafe void DrawFrame_None(int* pDstBuffer, int* pSrcBuffer)
        {
            for (var y = 0; y < _surfaceSize.Height; y++)
            {
                var srcLine = pSrcBuffer + _surfaceSize.Width * y;
                var dstLine = pDstBuffer + _texturePitch * y;
                NativeMethods.CopyMemory(dstLine, srcLine, _surfaceSize.Width * 4);
            }
        }

        private unsafe void DrawFrame_NoFlick(int* pDstBuffer, int* pSrcBuffer)
        {
            var size = _surfaceSize.Height * _surfaceSize.Width;
            if (_lastBuffer.Length < size)
            {
                _lastBuffer = new int[size];
            }
            fixed (int* pSrcBuffer2 = _lastBuffer)
            {
                for (var y = 0; y < _surfaceSize.Height; y++)
                {
                    var surfaceOffset = _surfaceSize.Width * y;
                    var pSrcArray1 = pSrcBuffer + surfaceOffset;
                    var pSrcArray2 = pSrcBuffer2 + surfaceOffset;
                    var pDstArray = pDstBuffer + _texturePitch * y;
                    for (var i = 0; i < _surfaceSize.Width; i++)
                    {
                        var src1 = pSrcArray1[i];
                        var src2 = pSrcArray2[i];
                        var r1 = (((src1 >> 16) & 0xFF) + ((src2 >> 16) & 0xFF)) / 2;
                        var g1 = (((src1 >> 8) & 0xFF) + ((src2 >> 8) & 0xFF)) / 2;
                        var b1 = (((src1 >> 0) & 0xFF) + ((src2 >> 0) & 0xFF)) / 2;
                        pSrcArray2[i] = src1;
                        pDstArray[i] = -16777216 | (r1 << 16) | (g1 << 8) | b1;
                    }
                }
            }
        }

        #endregion Video Filters


        #region TextureWrapper

        private class IconTextureWrapper : IDisposable
        {
            private IIconDescriptor m_iconDesc;

            public D3dTexture Texture;
            public bool Visible;

            public IconTextureWrapper(IIconDescriptor iconDesc)
            {
                m_iconDesc = iconDesc;
                Visible = iconDesc.Visible;
            }

            public void Dispose()
            {
                UnloadResources();
            }

            public Size Size 
            { 
                get { return m_iconDesc.Size; } 
            }

            public void LoadResources(Device device)
            {
                if (device == null || Texture != null)
                {
                    return;
                }
                using (var stream = m_iconDesc.GetImageStream())
                {
                    Texture = D3dTextureLoader.FromStream(device, stream);
                }
            }

            public void UnloadResources()
            {
                var texture = Texture;
                Texture = null;
                if (texture != null)
                {
                    texture.Dispose();
                }
            }
        }
        
        #endregion TextureWrapper

        private unsafe delegate void VideoFilterDelegate(int* dstBuffer, int* srcBuffer);
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
