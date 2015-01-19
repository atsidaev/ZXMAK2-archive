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
        private readonly Dictionary<IIconDescriptor, IconTextureWrapper> _iconWrapperDict = new Dictionary<IIconDescriptor, IconTextureWrapper>();
        private readonly FrameResampler _frameResampler = new FrameResampler(50);

        private D3dSprite _sprite = null;
        private D3dTexture _texture = null;
        private D3dTexture _textureMaskTv = null;
        private D3dSprite _iconSprite = null;
        private D3dFont _font = null;
        private Size _surfaceSize = new Size(0, 0);
        private Size _textureSize = new Size(0, 0);
        private Size _textureMaskTvSize = new Size(0, 0);

        private VideoFilterDelegate _videoFilter;
        private readonly FpsMonitor _fpsUpdate = new FpsMonitor();
        private readonly FpsMonitor _fpsRender = new FpsMonitor();
        private readonly double[] _renderGraph = new double[GraphLength];
        private readonly double[] _loadGraph = new double[GraphLength];
        //private readonly double[] m_copyGraph = new double[GraphLength];
        //private int m_copyGraphIndex;
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


        #endregion Fields


        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MimicTv { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Smoothing { get; set; }
        
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
                ClearGraph(_renderGraph, ref _renderGraphIndex);
                ClearGraph(_loadGraph, ref _loadGraphIndex);
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
        public bool IsReadScanlineSupported { get; private set; }

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
                _fpsUpdate.Reset();
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
                if (_isCancelWait || !IsReadScanlineSupported)
                {
                    return;
                }
                var frameRate = D3D.DisplayMode.RefreshRate;
                if (frameRate <= 0)
                {
                    frameRate = 75;
                }
                _frameResampler.SourceRate = frameRate;
                while (!_isCancelWait)
                {
                    WaitVBlank(frameRate);
                    if (_frameResampler.Next())
                    {
                        break;
                    }
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
            var timeStamp = Stopwatch.GetTimestamp();
            if ((timeStamp - _lastBlankStamp) > (Stopwatch.Frequency / refreshRate))
            {
                // some frames was missed, so try to catch up
                _lastBlankStamp = timeStamp;
                return;
            }

            // wait VBlank
            var timeFrame = D3D.DisplayMode.Height;
            var frequency = timeFrame * refreshRate;
            var time = D3D.RasterStatus.ScanLine;
            if (time < timeFrame)
            {
                var delay = ((timeFrame - time) * 1000) / frequency;
                if (delay > 5 && delay < 40)
                {
                    Thread.Sleep(delay - 1);
                }
            }
            while (!_isCancelWait && !D3D.RasterStatus.InVBlank)
            {
                Thread.SpinWait(1);
            }
            _lastBlankStamp = Stopwatch.GetTimestamp();
        }
        
        public void PushFrame(IVideoFrame frame, bool isRequested)
        {
            if (!isRequested)
            {
                _fpsUpdate.Frame();
                if (DebugInfo)
                {
                    PushGraphValue(_loadGraph, ref _loadGraphIndex, frame.InstantTime);
                }
            }
            _debugFrameStart = frame.StartTact;
            FrameSize = new Size(
                frame.VideoData.Size.Width,
                (int)(frame.VideoData.Size.Height * frame.VideoData.Ratio + 0.5F));
            if (!_isInitialized)
            {
                return;
            }
            UpdateIcons(frame.Icons);
            UpdateSurface(frame.VideoData);
            RequestPresentAsync();
        }

        #endregion IHostVideo

        
        #region Private

        protected override void OnCreateDevice()
        {
            base.OnCreateDevice();
            _isInitialized = false;
            OnCreateDeviceInt();
            _isInitialized = true;
        }

        private void OnCreateDeviceInt()
        {
            IsReadScanlineSupported = D3D.DeviceCaps.DriverCaps.ReadScanLine;
            _sprite = new D3dSprite(D3D);
            _iconSprite = new D3dSprite(D3D);
        }

        protected override void OnDestroyDevice()
        {
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
            if (_textureMaskTv != null)
            {
                _textureMaskTv.Dispose();
                _textureMaskTv = null;
            }
            foreach (var textureWrapper in _iconWrapperDict.Values)
            {
                textureWrapper.Dispose();
            }
            _iconWrapperDict.Clear();
            if (_sprite != null)
            {
                _sprite.Dispose();
                _sprite = null;
            }
            if (_iconSprite != null)
            {
                _iconSprite.Dispose();
                _iconSprite = null;
            }
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
            base.OnDestroyDevice();
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

        private void RequestPresentAsync()
        {
            if (!Created)
            {
                return;
            }
            if (InvokeRequired)
            {
                BeginInvoke(new Action(RequestPresentAsync));
                return;
            }
            //Invalidate();
            try
            {
                if (D3D == null)
                {
                    return;
                }
                RenderScene();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private unsafe void UpdateSurface(IVideoData videoData)
        {
            lock (SyncRoot)
            {
                if (D3D == null)
                {
                    return;
                }
                try
                {
                    m_surfaceHeightScale = videoData.Ratio;
                    if (_surfaceSize != videoData.Size)
                    {
                        InitVideoTextures(videoData.Size);
                    }
                    if (_texture != null)
                    {
                        using (GraphicsStream gs = _texture.LockRectangle(0, LockFlags.None))
                        {
                            fixed (int* srcPtr = videoData.Buffer)
                            {
                                //var startTick = Stopwatch.GetTimestamp();
                                _videoFilter((int*)gs.InternalData, srcPtr);
                                //var copyTime = Stopwatch.GetTimestamp() - startTick;
                                //PushGraphValue(m_copyGraph, ref m_copyGraphIndex, copyTime);
                            }
                        }
                        _texture.UnlockRectangle(0);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private unsafe void InitVideoTextures(Size surfaceSize)
        {
            lock (SyncRoot)
            {
                if (D3D == null)
                {
                    return;
                }
                //base.ResizeContext(surfaceSize);
                int potSize = GetPotSize(surfaceSize);
                if (_texture != null)
                {
                    _texture.Dispose();
                    _texture = null;
                }
                if (_textureMaskTv != null)
                {
                    _textureMaskTv.Dispose();
                    _textureMaskTv = null;
                }
                _texture = new D3dTexture(D3D, potSize, potSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                _textureSize = new Size(potSize, potSize);
                _surfaceSize = surfaceSize;

                var maskTvSize = new Size(surfaceSize.Width, surfaceSize.Height * MimicTvRatio);
                var maskTvPotSize = GetPotSize(maskTvSize);
                _textureMaskTv = new D3dTexture(D3D, maskTvPotSize, maskTvPotSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                _textureMaskTvSize = new Size(maskTvPotSize, maskTvPotSize);
                using (GraphicsStream gs = _textureMaskTv.LockRectangle(0, LockFlags.None))
                {
                    for (var x = 0; x < maskTvSize.Width; x++)
                    {
                        for (var y = 0; y < maskTvSize.Height; y++)
                        {
                            var ptr = (int*)gs.InternalData;
                            var offset = y * maskTvPotSize + x;
                            *(ptr + offset) = (y % MimicTvRatio) != (MimicTvRatio - 1) ? 0 : MimicTvAlpha << 24;
                        }
                    }
                }
                _textureMaskTv.UnlockRectangle(0);


                InitIconTextures();
            }
        }

        private void InitIconTextures()
        {
            foreach (var textureWrapper in _iconWrapperDict.Values)
            {
                textureWrapper.Load(D3D);
            }
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
            var gdiFont = new System.Drawing.Font(
                "Microsoft Sans Serif",
                10f/*8.25f*/,
                System.Drawing.FontStyle.Bold,
                GraphicsUnit.Pixel);
            _font = new Microsoft.DirectX.Direct3D.Font(D3D, gdiFont);
        }

        protected override void OnRenderScene()
        {
            if (!_isInitialized)
            {
                return;
            }
            OnRenderSceneInt();
        }

        private void OnRenderSceneInt()
        {
            lock (SyncRoot)
            {
                if (_texture != null)
                {
                    _fpsRender.Frame();
                    if (DebugInfo)
                    {
                        PushGraphValue(_renderGraph, ref _renderGraphIndex, _fpsRender.InstantTime);
                    }

                    _sprite.Begin(SpriteFlags.None);

                    D3D.SamplerState[0].MinFilter = Smoothing ? TextureFilter.Anisotropic : TextureFilter.None;
                    D3D.SamplerState[0].MagFilter = Smoothing ? TextureFilter.Linear : TextureFilter.None;

                    var wndSize = GetDeviceSize();
                    var dstSize = GetDestinationSize(wndSize, GetSurfaceScaledSize());
                    var dstPos = GetDestinationPos(wndSize, dstSize);

                    var srcRect = new Rectangle(
                        0,
                        0,
                        _surfaceSize.Width,
                        _surfaceSize.Height);
                    _sprite.Draw2D(
                       _texture,
                       srcRect,
                       dstSize,
                       dstPos,
                       0x00FFFFFF);
                    _sprite.End();
                    
                    if (MimicTv)
                    {
                        var srcRectTv = new Rectangle(
                            0,
                            0,
                            _surfaceSize.Width,
                            _surfaceSize.Height*MimicTvRatio);
                        _sprite.Begin(SpriteFlags.AlphaBlend);
                        _sprite.Draw2D(
                            _textureMaskTv,
                            srcRectTv,
                            dstSize,
                            dstPos,
                            -1);        
                        _sprite.End();
                    }


                    //D3D.SamplerState[0].MinFilter = min;
                    //D3D.SamplerState[0].MagFilter = mag;

                    if (DebugInfo)
                    {
                        var textValue = string.Format(
                            "Render FPS: {0:F3}\nUpdate FPS: {1:F3}\nDevice FPS: {2}\nBack: [{3}, {4}]\nClient: [{5}, {6}]\nSurface: [{7}, {8}]\nFrameStart: {9}T",
                            _fpsRender.Value,
                            IsRunning ? _fpsUpdate.Value : (double?)null,
                            D3D.DisplayMode.RefreshRate,
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
                            var graphRender = GetGraph(_renderGraph, ref _renderGraphIndex);
                            var graphLoad = GetGraph(_loadGraph, ref _loadGraphIndex);
                            //var graphCopy = GetGraph(m_copyGraph, ref m_copyGraphIndex);
                            var maxTime = Math.Max(graphRender.Max(), graphLoad.Max());
                            var limitDisplay = (double)Stopwatch.Frequency / D3D.DisplayMode.RefreshRate;
                            var limit50 = (double)Stopwatch.Frequency / 50F;
                            var graphRect = new Rectangle(
                                textRect.Left,
                                textRect.Top + textRect.Height,
                                GraphLength,
                                (int)(wndSize.Height - textRect.Top - textRect.Height));
                            FillRect(graphRect, Color.FromArgb(192, Color.Black));
                            RenderGraph(graphRender, maxTime, graphRect, Color.FromArgb(196, Color.Lime));
                            RenderGraph(graphLoad, maxTime, graphRect, Color.FromArgb(196, Color.Red));
                            //RenderGraph(graphCopy, maxTime, graphRect, Color.FromArgb(196, Color.Yellow));
                            RenderLimit(limitDisplay, maxTime, graphRect, Color.FromArgb(196, Color.Yellow));
                            RenderLimit(limit50, maxTime, graphRect, Color.FromArgb(196, Color.Magenta));
                        }
                    }

                    if (DisplayIcon)
                    {
                        var devIconSize = new SizeF(32, 32);
                        var iconNumber = 1;
                        foreach (var itw in _iconWrapperDict.Values)
                        {
                            if (!itw.Visible || itw.Texture == null)
                            {
                                continue;
                            }
                            var iconRect = new Rectangle(new Point(0, 0), itw.Size);
                            var devIconPos = new PointF(D3D.PresentationParameters.BackBufferWidth - devIconSize.Width * iconNumber, 0);
                            _iconSprite.Begin(SpriteFlags.AlphaBlend);
                            _iconSprite.Draw2D(
                               itw.Texture,
                               iconRect,
                               devIconSize,
                               devIconPos,
                               Color.FromArgb(255, 255, 255, 255));
                            _iconSprite.End();
                            iconNumber++;
                        }
                    }
                }
            }
        }

        private void RenderLimit(double limit, double maxValue, Rectangle rect, Color color)
        {
            if (limit < 0 || limit > maxValue)
            {
                return;
            }
            var alphaBlendEnabled = D3D.RenderState.AlphaBlendEnable;
            D3D.RenderState.AlphaBlendEnable = true;
            D3D.RenderState.SourceBlend = Blend.SourceAlpha;
            D3D.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            D3D.RenderState.BlendOperation = BlendOperation.Add;
            var colorInt = color.ToArgb();
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
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, colorInt))
                .ToArray();
            D3D.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            D3D.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length / 2, vertices);
            D3D.RenderState.AlphaBlendEnable = alphaBlendEnabled;
        }

        private void RenderGraph(double[] graph, double maxValue, Rectangle rect, Color color)
        {
            if (graph.Length < 1)
            {
                return;
            }
            var alphaBlendEnabled = D3D.RenderState.AlphaBlendEnable;
            D3D.RenderState.AlphaBlendEnable = true;
            D3D.RenderState.SourceBlend = Blend.SourceAlpha;
            D3D.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            D3D.RenderState.BlendOperation = BlendOperation.Add;
            var colorInt = color.ToArgb();
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
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 1f, colorInt))
                .ToArray();
            D3D.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            D3D.DrawUserPrimitives(PrimitiveType.LineList, vertices.Length/2, vertices);
            D3D.RenderState.AlphaBlendEnable = alphaBlendEnabled;
        }

        private void FillRect(Rectangle rect, Color color)
        {
            var alphaBlendEnabled = D3D.RenderState.AlphaBlendEnable;
            //D3D.RenderState.ScissorTestEnable = false;
            D3D.RenderState.AlphaBlendEnable = true;
            D3D.RenderState.SourceBlend = Blend.SourceAlpha;
            D3D.RenderState.DestinationBlend = Blend.InvSourceAlpha;
            D3D.RenderState.BlendOperation = BlendOperation.Add;
            var colorInt = color.ToArgb();
            var rectv = new[]
            {
                new CustomVertex.TransformedColored(rect.Left, rect.Top+rect.Height+0.5F, 0, 1f, colorInt),
                new CustomVertex.TransformedColored(rect.Left, rect.Top, 0, 1f, colorInt),
                new CustomVertex.TransformedColored(rect.Left+rect.Width, rect.Top+rect.Height+0.5F, 0, 1f, colorInt),
                new CustomVertex.TransformedColored(rect.Left+rect.Width, rect.Top, 0, 1f, colorInt),
            };
            D3D.VertexFormat = CustomVertex.TransformedColored.Format | VertexFormats.Diffuse;
            D3D.DrawUserPrimitives(PrimitiveType.TriangleStrip, 2, rectv);
            D3D.RenderState.AlphaBlendEnable = alphaBlendEnabled;
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
            var param = D3D.PresentationParameters;
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

        private void UpdateIcons(IIconDescriptor[] iconDescArray)
        {
            lock (SyncRoot)
            {
                foreach (var id in iconDescArray)
                {
                    if (!_iconWrapperDict.ContainsKey(id))
                    {
                        _iconWrapperDict.Add(id, new IconTextureWrapper(id));
                    }
                    var itw = _iconWrapperDict[id];
                    itw.Visible = id.Visible;
                    if (itw.Texture == null && D3D != null)
                    {
                        itw.Load(D3D);
                    }
                }
                var iconDescList = new List<IIconDescriptor>(iconDescArray);
                var deleteList = new List<IIconDescriptor>();
                foreach (var id in _iconWrapperDict.Keys)
                {
                    if (!iconDescList.Contains(id))
                    {
                        deleteList.Add(id);
                    }
                }
                foreach (var id in deleteList)
                {
                    _iconWrapperDict[id].Dispose();
                    _iconWrapperDict.Remove(id);
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
                var dstLine = pDstBuffer + _textureSize.Width * y;
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
                    var pDstArray = pDstBuffer + _textureSize.Width * y;
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
                if (Texture != null)
                    Texture.Dispose();
                Texture = null;
            }

            public Size Size 
            { 
                get { return m_iconDesc.Size; } 
            }

            public void Load(Device D3D)
            {
                if (Texture != null)
                    Texture.Dispose();
                Texture = null;
                Texture = D3dTextureLoader.FromStream(
                    D3D,
                    m_iconDesc.GetImageStream());
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
