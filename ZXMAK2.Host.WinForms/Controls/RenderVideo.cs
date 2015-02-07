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
        private readonly double[] _renderGraph = new double[GraphLength];
        private readonly double[] _loadGraph = new double[GraphLength];
        private readonly double[] _updateGraph = new double[GraphLength];
#if SHOW_LATENCY
        private readonly double[] _latencyGraph = new double[GraphLength];
        private int _latencyGraphIndex;
        //private readonly double[] m_copyGraph = new double[GraphLength];
        //private int m_copyGraphIndex;
        private int _lastLatencyIndex;
#endif
        private int _updateGraphIndex;
        private int _renderGraphIndex;
        private int _loadGraphIndex;
        private int _graphDelayCounter;
        private int[] _lastBuffer = new int[0];    // noflick
        private long _lastBlankStamp;              // WaitVBlank
        private int _debugFrameStart = 0;
        private float m_surfaceHeightScale = 1F;
        private bool _isInitialized;
        private bool _isRunning;
        private bool _isDebugInfo;
        private bool _isCancelWait;

        private long _lastUpdateStamp;
        private long _lastRenderStamp;

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
                _graphDelayCounter = 0;
                _lastRenderStamp = Stopwatch.GetTimestamp();
                _lastUpdateStamp = Stopwatch.GetTimestamp();
                ClearGraph(_renderGraph, ref _renderGraphIndex);
                ClearGraph(_loadGraph, ref _loadGraphIndex);
                ClearGraph(_updateGraph, ref _updateGraphIndex);
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
                _lastUpdateStamp = Stopwatch.GetTimestamp();
                _lastRenderStamp = Stopwatch.GetTimestamp();
                _graphDelayCounter = 0;
                //ClearGraph(m_renderGraph, ref m_renderGraphIndex);
                //ClearGraph(m_loadGraph, ref m_loadGraphIndex);
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

        public void WaitFrame()
        {
            if (!_isInitialized)
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
                    RequestPresentAsync();
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
                PushGraphValue(_latencyGraph, ref _latencyGraphIndex, double.NaN);
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
                    PushGraphValue(_latencyGraph, ref _latencyGraphIndex, realTime-delta);
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

        public void PushFrame(IVideoFrame frame, bool isRequested)
        {
            if (!isRequested)
            {
                if (DebugInfo)
                {
                    var updateStamp = Stopwatch.GetTimestamp();
                    var updateTime = updateStamp - _lastUpdateStamp;
                    _lastUpdateStamp = updateStamp;
                    PushGraphValue(_updateGraph, ref _updateGraphIndex, updateTime);
                    PushGraphValue(_loadGraph, ref _loadGraphIndex, frame.InstantUpdateTime);
                }
                RequestPresentAsync();
            }
            _debugFrameStart = frame.StartTact;
            FrameSize = new Size(
                frame.VideoData.Size.Width,
                (int)(frame.VideoData.Size.Height * frame.VideoData.Ratio + 0.5F));
            if (!_isInitialized || _device == null)
            {
                return;
            }
            UpdateSurface(frame.VideoData);
            UpdateIconList(frame.Icons);
            if (isRequested)
            {
                RequestPresentAsync();
            }
        }

        #endregion IHostVideo

        
        #region Private

        protected override void OnLoadResources()
        {
            base.OnLoadResources();
            _isInitialized = false;
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

            _isInitialized = true;
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

        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            try
            {
                if (_isInitialized)
                {
                    base.OnPaint(e);
                }
                else
                {
                    RenderError(e, "Direct3DX initialization failed!\nProbably DirectX 9 (June 2010) is not installed");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private bool _presentRequested;

        private void RequestPresentAsync()
        {
            if (!Created)
            {
                return;
            }
            _presentRequested = true;
            Invalidate();
        }

        private unsafe void UpdateSurface(IVideoData videoData)
        {
            lock (_syncRoot)
            {
                try
                {
                    m_surfaceHeightScale = videoData.Ratio;
                    if (_surfaceSize != videoData.Size || _texture0==null)
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
            if (!_isInitialized)
            {
                return;
            }
            lock (_syncRoot)
            {
                if (IsRunning && !_presentRequested)
                {
                    return;
                }
                _presentRequested = false;
                
                _device.Clear(ClearFlags.Target, Color.Black, 1, 0);
                _device.BeginScene();
                try
                {
                    if (_texture0 != null)
                    {
                        if (DebugInfo)
                        {
                            var renderStamp = Stopwatch.GetTimestamp();
                            var renderTime = renderStamp - _lastRenderStamp;
                            _lastRenderStamp = renderStamp;
                            PushGraphValue(_renderGraph, ref _renderGraphIndex, renderTime);
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
                catch (DeviceLostException ex)
                {
                    Logger.Debug(ex);
                }
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
            var graphRender = GetGraph(_renderGraph, ref _renderGraphIndex);
            var graphLoad = GetGraph(_loadGraph, ref _loadGraphIndex);
#if SHOW_LATENCY
            var isLatencyAvailable = _lastLatencyIndex != _latencyGraphIndex;
            _lastLatencyIndex = _latencyGraphIndex;
            var graphLatency = isLatencyAvailable ? GetGraph(_latencyGraph, ref _latencyGraphIndex) : default(double[]);
            //var graphCopy = GetGraph(m_copyGraph, ref m_copyGraphIndex);
#endif
            var graphUpdate = GetGraph(_updateGraph, ref _updateGraphIndex);
            var frequency = (double)Stopwatch.Frequency;
            var limitDisplay = frequency / frameRate;
            var limit50 = frequency / 50D;
            var limit1ms = frequency / 1000D;
            var maxRender = graphRender.Max();
            var maxLoad = graphLoad.Max();
            var minT = graphRender.Min() * 1000D / frequency;
            var avgT = graphRender.Average() * 1000D / frequency;
            var maxT = maxRender * 1000D / frequency;
#if SHOW_LATENCY
            var minL = isLatencyAvailable ? graphLatency.Min() * 1000D / frequency : 0D;
            var avgL = isLatencyAvailable ? graphLatency.Average() * 1000D / frequency : 0D;
            var maxL = isLatencyAvailable ? graphLatency.Max() * 1000D / frequency : 0D;
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
                DrawGraphGrid(maxScale, limit1ms, graphRect, _renderGraphIndex, Color.FromArgb(64, Color.White));

                var msgTime = string.Format(
                    "MinT: {0:F3} [ms]\nAvgT: {1:F3} [ms]\nMaxT: {2:F3} [ms]\nAvgE: {3:F3} [ms]",
                    minT,
                    avgT,
                    maxT,
                    avgE);
#if SHOW_LATENCY
                if (isLatencyAvailable)
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

        private void PushGraphValue(double[] graph, ref int index, double value)
        {
            if (_graphDelayCounter < 2)
            {
                // eliminate value of first 2 frames (startup delays)
                if (graph == _renderGraph)
                {
                    _graphDelayCounter++;
                }
                return;
            }
            graph[index] = value;
            index = (index + 1) % graph.Length;
        }

        private void ClearGraph(double[] graph, ref int index)
        {
            for (var i = 0; i < graph.Length; i++)
            {
                graph[i] = 0;
            }
            index = 0;
        }

        private static double[] GetGraph(double[] graph, ref int index)
        {
            var array = new double[graph.Length];
            var fixedIndex = index;
            for (var i = 0; i < graph.Length; i++)
            {
                array[i] = graph[(fixedIndex + i) % graph.Length];
            }
            return array;
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
            lock (_syncRoot)
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
