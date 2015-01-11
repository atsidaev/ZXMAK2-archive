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
using ZXMAK2.Entities;
using ZXMAK2.Interfaces;
using ZXMAK2.Host.Interfaces;


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

        private bool _isInitialized;
        private Sprite m_sprite = null;
        private Texture m_texture = null;
        private Texture m_textureMaskTv = null;
        private Size m_surfaceSize = new Size(0, 0);
        private Size m_textureSize = new Size(0, 0);
        private Size m_textureMaskTvSize = new Size(0, 0);
        private float m_surfaceHeightScale = 1F;

        private Sprite m_iconSprite = null;
        private Microsoft.DirectX.Direct3D.Font m_font = null;

        private unsafe DrawFilterDelegate m_drawFilter;
        private unsafe delegate void DrawFilterDelegate(int* dstBuffer, int* srcBuffer);
        private readonly FpsMonitor m_fpsUpdate = new FpsMonitor();
        private readonly FpsMonitor m_fpsRender = new FpsMonitor();
        //private List<double> m_renderGraph = new List<double>();
        //private List<double> m_loadGraph = new List<double>();
        private readonly double[] m_renderGraph = new double[GraphLength];
        private readonly double[] m_loadGraph = new double[GraphLength];
        private int m_renderGraphIndex;
        private int m_loadGraphIndex;


        #endregion Fields


        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool MimicTv { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Smoothing { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ScaleMode ScaleMode { get; set; }
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DebugInfo { get; set; }

        public unsafe bool NoFlic
        {
            get { return m_drawFilter == drawFrame_noflic; }
            set
            {
                m_drawFilter = value ?
                    (DrawFilterDelegate)drawFrame_noflic :
                    (DrawFilterDelegate)drawFrame;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IconDisk { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool DisplayIcon { get; set; }

        public bool IsReadScanlineSupported { get; private set; }
        
        #endregion Properties

        
        public unsafe RenderVideo()
        {
            m_drawFilter = drawFrame;
            DisplayIcon = true;
            ScaleMode = ScaleMode.FixedPixelSize;
        }

        #region IHostVideo

        private bool _isCancel;
        private int _syncFrame;
        private bool _vblankValue;

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
            if (!IsReadScanlineSupported)
            {
                Thread.Sleep(1);
                return;
            }
            // FIXME: stupid synchronization, 
            // because there is no event from Direct3D
            var frameRest = D3D.DisplayMode.RefreshRate % 50;
            var frameRatio = frameRest != 0 ? 50 / frameRest : 0;
            _isCancel = false;
            var priority = Thread.CurrentThread.Priority;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            try
            {
                while (!_isCancel)
                {
                    // wait VBlank
                    while (!_isCancel)
                    {
                        var state = D3D.RasterStatus.InVBlank;
                        var change = state != _vblankValue;
                        _vblankValue = state;
                        if (change && _vblankValue)
                        {
                            break;
                        }
                    }
                    if (frameRatio > 0 && ++_syncFrame > frameRatio)
                    {
                        _syncFrame = 0;
                        continue;
                    }
                    return;
                }
            }
            finally
            {
                Thread.CurrentThread.Priority = priority;
            }
        }

        public void CancelWait()
        {
            _isCancel = true;
        }
        
        public void PushFrame(IVideoFrame frame, bool isRequested)
        {
            if (!isRequested)
            {
                m_fpsUpdate.Frame();
                if (DebugInfo)
                {
                    PushGraphValue(m_loadGraph, ref m_loadGraphIndex, frame.InstantTime);
                }
            }
            m_debugFrameStart = frame.StartTact;
            FrameSize = new Size(
                frame.VideoData.Size.Width,
                (int)(frame.VideoData.Size.Height * frame.VideoData.Ratio + 0.5F));
            if (!_isInitialized)
            {
                return;
            }
            UpdateIcons(frame.Icons);
            UpdateSurface(frame.VideoData);
        }

        #endregion IHostVideo

        public Size FrameSize { get; private set; }

        private bool _isRunning;

        public bool IsRunning 
        {
            get { return _isRunning; }
            set 
            { 
                _isRunning = value; 
                m_fpsUpdate.Reset();
                ClearGraph(m_renderGraph, ref m_renderGraphIndex);
                ClearGraph(m_loadGraph, ref m_loadGraphIndex);
            }
        }

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
            m_sprite = new Sprite(D3D);
            m_iconSprite = new Sprite(D3D);
        }

        protected override void OnDestroyDevice()
        {
            if (m_texture != null)
            {
                m_texture.Dispose();
                m_texture = null;
            }
            if (m_textureMaskTv != null)
            {
                m_textureMaskTv.Dispose();
                m_textureMaskTv = null;
            }
            foreach (var textureWrapper in m_iconWrapperDict.Values)
            {
                textureWrapper.Dispose();
            }
            m_iconWrapperDict.Clear();
            if (m_sprite != null)
            {
                m_sprite.Dispose();
                m_sprite = null;
            }
            if (m_iconSprite != null)
            {
                m_iconSprite.Dispose();
                m_iconSprite = null;
            }
            if (m_font != null)
            {
                m_font.Dispose();
                m_font = null;
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
                    if (m_surfaceSize != videoData.Size)
                    {
                        initTextures(videoData.Size);
                    }
                    if (m_texture != null)
                    {
                        using (GraphicsStream gs = m_texture.LockRectangle(0, LockFlags.None))
                            fixed (int* srcPtr = videoData.Buffer)
                                m_drawFilter((int*)gs.InternalData, srcPtr);
                        m_texture.UnlockRectangle(0);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
            Invalidate();
        }

        private unsafe void initTextures(Size surfaceSize)
        {
            lock (SyncRoot)
            {
                if (D3D == null)
                {
                    return;
                }
                //base.ResizeContext(surfaceSize);
                int potSize = getPotSize(surfaceSize);
                if (m_texture != null)
                {
                    m_texture.Dispose();
                    m_texture = null;
                }
                if (m_textureMaskTv != null)
                {
                    m_textureMaskTv.Dispose();
                    m_textureMaskTv = null;
                }
                m_texture = new Texture(D3D, potSize, potSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                m_textureSize = new System.Drawing.Size(potSize, potSize);
                m_surfaceSize = surfaceSize;

                var maskTvSize = new Size(surfaceSize.Width, surfaceSize.Height * MimicTvRatio);
                var maskTvPotSize = getPotSize(maskTvSize);
                m_textureMaskTv = new Texture(D3D, maskTvPotSize, maskTvPotSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                m_textureMaskTvSize = new Size(maskTvPotSize, maskTvPotSize);
                using (GraphicsStream gs = m_textureMaskTv.LockRectangle(0, LockFlags.None))
                {                
                    for (var x=0; x < maskTvSize.Width; x++)
                        for (var y=0; y < maskTvSize.Height; y++)
                        {
                            var ptr = (int*)gs.InternalData;
                            var offset = y * maskTvPotSize + x;
                            *(ptr + offset) = (y%MimicTvRatio)!=(MimicTvRatio-1) ? 0 : MimicTvAlpha<<24;
                        }
                }
                m_textureMaskTv.UnlockRectangle(0);


                initIconTextures();
            }
        }

        private void initIconTextures()
        {
            foreach (var textureWrapper in m_iconWrapperDict.Values)
            {
                textureWrapper.Load(D3D);
            }
            if (m_font != null)
            {
                m_font.Dispose();
                m_font = null;
            }
            var gdiFont = new System.Drawing.Font(
                "Microsoft Sans Serif",
                10f/*8.25f*/,
                System.Drawing.FontStyle.Bold,
                GraphicsUnit.Pixel);
            m_font = new Microsoft.DirectX.Direct3D.Font(D3D, gdiFont);
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
                if (m_texture != null)
                {
                    m_fpsRender.Frame();
                    if (DebugInfo)
                    {
                        PushGraphValue(m_renderGraph, ref m_renderGraphIndex, m_fpsRender.InstantTime);
                    }

                    m_sprite.Begin(SpriteFlags.None);

                    ////if (d3d.DeviceCaps.TextureFilterCaps.SupportsMinifyAnisotropic)
                    ////if (device.DeviceCaps.TextureFilterCaps.SupportsMagnifyAnisotropic)
                    ////d3d.SamplerState[0].MipFilter = TextureFilter.Point;
                    //TextureFilter min = D3D.SamplerState[0].MinFilter;
                    //TextureFilter mag = D3D.SamplerState[0].MagFilter;
                    //D3D.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
                    //D3D.SamplerState[0].MagFilter = TextureFilter.Linear;

                    if (!Smoothing)
                    {
                        D3D.SamplerState[0].MinFilter = TextureFilter.None;
                        D3D.SamplerState[0].MagFilter = TextureFilter.None;
                    }

                    var wndSize = GetDeviceSize();
                    var dstSize = GetDestinationSize(wndSize, GetSurfaceScaledSize());
                    var dstPos = GetDestinationPos(wndSize, dstSize);

                    var srcRect = new Rectangle(
                        0,
                        0,
                        m_surfaceSize.Width,
                        m_surfaceSize.Height);
                    m_sprite.Draw2D(
                       m_texture,
                       srcRect,
                       dstSize,
                       dstPos,
                       0x00FFFFFF);
                    m_sprite.End();
                    
                    if (MimicTv)
                    {
                        var srcRectTv = new Rectangle(
                            0,
                            0,
                            m_surfaceSize.Width,
                            m_surfaceSize.Height*MimicTvRatio);
                        m_sprite.Begin(SpriteFlags.AlphaBlend);
                        m_sprite.Draw2D(
                            m_textureMaskTv,
                            srcRectTv,
                            dstSize,
                            dstPos,
                            -1);        
                        m_sprite.End();
                    }


                    //D3D.SamplerState[0].MinFilter = min;
                    //D3D.SamplerState[0].MagFilter = mag;

                    if (DebugInfo)
                    {
                        var textValue = string.Format(
                            "Render FPS: {0:F3}\nUpdate FPS: {1:F3}\nDevice FPS: {2}\nBack: [{3}, {4}]\nClient: [{5}, {6}]\nSurface: [{7}, {8}]\nFrameStart: {9}T",
                            m_fpsRender.Value,
                            IsRunning ? m_fpsUpdate.Value : (double?)null,
                            D3D.DisplayMode.RefreshRate,
                            wndSize.Width,
                            wndSize.Height,
                            ClientSize.Width,
                            ClientSize.Height,
                            m_surfaceSize.Width,
                            m_surfaceSize.Height,
                            m_debugFrameStart);
                        var textRect = m_font.MeasureString(
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
                        m_font.DrawText(
                            null,
                            textValue,
                            textRect,
                            DrawTextFormat.NoClip,
                            Color.Yellow);
                        if (IsRunning)
                        {
                            // Draw graphs
                            var graphRender = GetGraph(m_renderGraph, ref m_renderGraphIndex);
                            var graphLoad = GetGraph(m_loadGraph, ref m_loadGraphIndex);
                            var maxTime = Math.Max(graphRender.Max(), graphLoad.Max());
                            var limitTime = (double)Stopwatch.Frequency / D3D.DisplayMode.RefreshRate;
                            var graphRect = new Rectangle(
                                textRect.Left,
                                textRect.Top + textRect.Height,
                                GraphLength,
                                (int)(wndSize.Height - textRect.Top - textRect.Height));
                            FillRect(graphRect, Color.FromArgb(192, Color.Black));
                            RenderGraph(graphRender, maxTime, graphRect, Color.FromArgb(196, Color.Yellow));
                            RenderGraph(graphLoad, maxTime, graphRect, Color.FromArgb(196, Color.Lime));
                            RenderLimit(limitTime, maxTime, graphRect, Color.FromArgb(196, Color.Red));
                        }
                    }

                    if (DisplayIcon)
                    {
                        var devIconSize = new SizeF(32, 32);
                            //D3D.PresentationParameters.BackBufferWidth / 20,
                            //D3D.PresentationParameters.BackBufferHeight / 15);
                        var iconNumber = 1;
                        foreach (var itw in m_iconWrapperDict.Values)
                        {
                            if (itw.Visible && itw.Texture != null)
                            {
                                var iconRect = new Rectangle(new Point(0, 0), itw.Size);
                                var devIconPos = new PointF(D3D.PresentationParameters.BackBufferWidth - devIconSize.Width * iconNumber, 0);
                                m_iconSprite.Begin(SpriteFlags.AlphaBlend);
                                m_iconSprite.Draw2D(
                                   itw.Texture,
                                   iconRect,
                                   devIconSize,
                                   devIconPos,
                                   Color.FromArgb(255, 255, 255, 255));
                                m_iconSprite.End();
                                iconNumber++;
                            }
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
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 0, colorInt))
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
                .Select(p => new CustomVertex.TransformedColored(p.X, p.Y, 0, 0, colorInt))
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
                new CustomVertex.TransformedColored(rect.Left+0.5F, rect.Top+rect.Height+0.5F, 0, 0, colorInt),
                new CustomVertex.TransformedColored(rect.Left+0.5F, rect.Top+0.5F, 0, 0, colorInt),
                new CustomVertex.TransformedColored(rect.Left+rect.Width+0.5F, rect.Top+rect.Height+0.5F, 0, 0, colorInt),
                new CustomVertex.TransformedColored(rect.Left+rect.Width+0.5F, rect.Top+0.5F, 0, 0, colorInt),
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
            var surfSize = m_surfaceSize;
            return new SizeF(
                surfSize.Width,
                surfSize.Height * m_surfaceHeightScale);
        }

        private unsafe void drawFrame(int* pDstBuffer, int* pSrcBuffer)
        {
            for (var y = 0; y < m_surfaceSize.Height; y++)
            {
                var srcLine = pSrcBuffer + m_surfaceSize.Width * y;
                var dstLine = pDstBuffer + m_textureSize.Width * y;
                for (var i = 0; i < m_surfaceSize.Width; i++)
                {
                    dstLine[i] = srcLine[i];
                }
            }
        }

        private static void PushGraphValue(double[] graph, ref int index, double value)
        {
            graph[index] = value;
            index = (index + 1) % graph.Length;
        }

        private static void ClearGraph(double[] graph, ref int index)
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

        private int[] m_lastBuffer = new int[0];

        private unsafe void drawFrame_noflic(int* pDstBuffer, int* pSrcBuffer)
        {
            var size = m_surfaceSize.Height * m_surfaceSize.Width;
            if (m_lastBuffer.Length < size)
            {
                m_lastBuffer = new int[size];
            }
            fixed (int* pSrcBuffer2 = m_lastBuffer)
            {
                for (var y = 0; y < m_surfaceSize.Height; y++)
                {
                    var surfaceOffset = m_surfaceSize.Width * y;
                    var pSrcArray1 = pSrcBuffer + surfaceOffset;
                    var pSrcArray2 = pSrcBuffer2 + surfaceOffset;
                    var pDstArray = pDstBuffer + m_textureSize.Width * y;
                    for (var i = 0; i < m_surfaceSize.Width; i++)
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

        private static int getPotSize(Size surfaceSize)
        {
            // Create POT texture (e.g. 512x512) to render NPOT image (e.g. 320x240),
            // because NPOT textures is not supported on some videocards
            var size = surfaceSize.Width > surfaceSize.Height ?
                surfaceSize.Width :
                surfaceSize.Height;
            var potSize = 0;
            for (var power = 1; potSize < size; power++)
            {
                potSize = pow(2, power);
            }
            return potSize;
        }

        private static int pow(int value, int power)
        {
            var result = value;
            for (var i = 0; i < power; i++)
            {
                result *= value;
            }
            return result;
        }

        private int m_debugFrameStart = 0;

        private Dictionary<IIconDescriptor, IconTextureWrapper> m_iconWrapperDict = new Dictionary<IIconDescriptor, IconTextureWrapper>();

        private void UpdateIcons(IIconDescriptor[] iconDescArray)
        {
            lock (SyncRoot)
            {
                foreach (var id in iconDescArray)
                {
                    if (!m_iconWrapperDict.ContainsKey(id))
                    {
                        m_iconWrapperDict.Add(id, new IconTextureWrapper(id));
                    }
                    var itw = m_iconWrapperDict[id];
                    itw.Visible = id.Visible;
                    if (itw.Texture == null && D3D != null)
                    {
                        itw.Load(D3D);
                    }
                }
                var iconDescList = new List<IIconDescriptor>(iconDescArray);
                var deleteList = new List<IIconDescriptor>();
                foreach (var id in m_iconWrapperDict.Keys)
                {
                    if (!iconDescList.Contains(id))
                    {
                        deleteList.Add(id);
                    }
                }
                foreach (var id in deleteList)
                {
                    m_iconWrapperDict[id].Dispose();
                    m_iconWrapperDict.Remove(id);
                }
            }
        }

        #endregion Private


        #region TextureWrapper

        private class IconTextureWrapper : IDisposable
        {
            private IIconDescriptor m_iconDesc;

            public Texture Texture;
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

            public Size Size { get { return m_iconDesc.Size; } }

            public void Load(Device D3D)
            {
                if (Texture != null)
                    Texture.Dispose();
                Texture = null;
                Texture = TextureLoader.FromStream(
                    D3D,
                    m_iconDesc.GetImageStream());
            }
        }
        
        #endregion TextureWrapper
    }

    public enum ScaleMode
    {
        Stretch = 0,
        KeepProportion,
        FixedPixelSize,
        SquarePixelSize,
    }
}
