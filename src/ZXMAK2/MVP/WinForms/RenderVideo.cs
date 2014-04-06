/// Description: Video renderer control
/// Author: Alex Makeev
/// Date: 27.03.2008
using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ZXMAK2.Engine;
using System.Collections.Generic;
using ZXMAK2.Entities;
using System.Diagnostics;
using ZXMAK2.Interfaces;
using System.Threading;


namespace ZXMAK2.MVP.WinForms
{
    public class RenderVideo : Render3D, IHostVideo
    {
        #region Fields

        private Sprite m_sprite = null;
        private Texture m_texture = null;
        private Size m_surfaceSize = new Size(0, 0);
        private Size m_textureSize = new Size(0, 0);
        private float m_surfaceHeightScale = 1F;

        private Sprite m_iconSprite = null;
        private Microsoft.DirectX.Direct3D.Font m_font = null;

        private unsafe DrawFilterDelegate m_drawFilter;
        private unsafe delegate void DrawFilterDelegate(int* dstBuffer, int* srcBuffer);
        //private Thread _threadVBlankScan;

        #endregion Fields


        #region Properties

        public bool Smoothing { get; set; }
        public ScaleMode ScaleMode { get; set; }
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

        public bool IconDisk { get; set; }
        public bool DisplayIcon { get; set; }

        #endregion Properties

        
        public unsafe RenderVideo()
        {
            m_drawFilter = drawFrame;
            DisplayIcon = true;
            ScaleMode = ScaleMode.FixedPixelSize;
        }

        //private void ThreadVBlankScanProc()
        //{
        //    var frame = 0;
        //    var vblank = false;
        //    while (_threadVBlankScan != null)
        //    {
        //        var value = D3D.RasterStatus.InVBlank;
        //        var change = vblank != value;
        //        vblank = value;
        //        if (change && value)
        //        {
        //            frame++;
        //            if (frame < 3)
        //            {
        //                m_vblankEvent.Set();
        //                Thread.Sleep(10);
        //            }
        //            else
        //            {
        //                frame = 0;
        //            }
        //        }
        //        //Thread.Sleep(0);
        //    }
        //}


        #region IHostVideo

        //private readonly AutoResetEvent m_cancelEvent = new AutoResetEvent(false);
        //private readonly AutoResetEvent m_vblankEvent = new AutoResetEvent(false);
        private bool _isCancel;
        private int _syncFrame;
        private bool _vblankValue;

        public void WaitFrame()
        {
            //m_cancelEvent.Reset();
            //WaitHandle.WaitAny(new[] { m_vblankEvent, m_cancelEvent });
            
            // FIXME: stupid synchronization, 
            // because there is no event from Direct3D
            var frameRest = D3D.DisplayMode.RefreshRate % 50;
            var frameRatio = frameRest != 0 ? 50 / frameRest : 0;
            _isCancel = false;
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

        public void CancelWait()
        {
            //m_cancelEvent.Set();
            _isCancel = true;
        }
        
        public void PushFrame(VirtualMachine vm)
        {
            UpdateIcons(vm.Spectrum.BusManager.IconDescriptorArray);
            m_debugFrameStart = vm.DebugFrameStartTact;
            UpdateSurface(vm.VideoData);
        }

        #endregion IHostVideo


        #region Private

        protected override void OnCreateDevice()
        {
            base.OnCreateDevice();
            m_sprite = new Sprite(D3D);
            m_iconSprite = new Sprite(D3D);
            //_threadVBlankScan = new Thread(ThreadVBlankScanProc);
            //_threadVBlankScan.Priority = ThreadPriority.Highest;
            //_threadVBlankScan.IsBackground = false;
            //_threadVBlankScan.Name = "RenderVideo.ThreadVBlankScanProc";
            //_threadVBlankScan.Start();
        }

        protected override void OnDestroyDevice()
        {
            //if (_threadVBlankScan != null)
            //{
            //    var thread = _threadVBlankScan;
            //    _threadVBlankScan = null;
            //    thread.Join();
            //}
            if (m_texture != null)
            {
                m_texture.Dispose();
                m_texture = null;
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
                    LogAgent.Error(ex);
                }
            }
            Invalidate();
        }

        private void initTextures(Size surfaceSize)
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
                m_texture = new Texture(D3D, potSize, potSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                m_textureSize = new System.Drawing.Size(potSize, potSize);
                m_surfaceSize = surfaceSize;
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

        private long m_lastTick = 0;
        private long m_lastFrameTime = Stopwatch.GetTimestamp();
        private Queue<double> m_frameTimes = new Queue<double>(); 

        protected override void OnRenderScene()
        {
            lock (SyncRoot)
            {
                if (m_texture != null)
                {
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

                    var wndSize = new SizeF(
                        D3D.PresentationParameters.BackBufferWidth,
                        D3D.PresentationParameters.BackBufferHeight);
                    var dstSize = new SizeF(
                        m_surfaceSize.Width,
                        m_surfaceSize.Height * m_surfaceHeightScale);

                    if (dstSize.Width > 0 &&
                        dstSize.Height > 0)
                    {
                        var rx = wndSize.Width / dstSize.Width;
                        var ry = wndSize.Height / dstSize.Height;
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

                    var srcRect = new Rectangle(
                        0,
                        0,
                        m_surfaceSize.Width,
                        m_surfaceSize.Height);
                    var dstLocation = new PointF(
                        (float)Math.Floor((wndSize.Width - dstSize.Width) / 2F),
                        (float)Math.Floor((wndSize.Height - dstSize.Height) / 2F));
                    m_sprite.Draw2D(
                       m_texture,
                       srcRect,
                       dstSize,
                       dstLocation,
                       0x00FFFFFF);

                    //D3D.SamplerState[0].MinFilter = min;
                    //D3D.SamplerState[0].MagFilter = mag;

                    m_sprite.End();

                    if (DebugInfo)
                    {
                        var tick = Stopwatch.GetTimestamp();
                        var frameTime = (tick - m_lastTick) / (double)Stopwatch.Frequency;
                        m_lastTick = tick;
                        var fps = MeasureFramePerSecond(frameTime);
                        var textValue = string.Format(
                            "Render FPS: {0:F2}\nDevice FPS: {1}\nBack: [{2}, {3}]\nClient: [{4}, {5}]\nSurface: [{6}, {7}]\nFrameStart: {8}T",
                            fps,
                            D3D.DisplayMode.RefreshRate,
                            D3D.PresentationParameters.BackBufferWidth,
                            D3D.PresentationParameters.BackBufferHeight,
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
                        m_font.DrawText(
                            null,
                            textValue,
                            textRect,
                            DrawTextFormat.NoClip,
                            Color.Yellow);
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

        //private double m_msrCounter = 0D;
        private double m_lastFps = 0D;

        private double MeasureFramePerSecond(double frameTime)
        {
            m_frameTimes.Enqueue(frameTime);
            const int msrInterval = 50;
            while (m_frameTimes.Count > msrInterval)
            {
                m_frameTimes.Dequeue();
            }
            if (m_frameTimes.Count == msrInterval)
            {
                //m_msrCounter += frameTime;
                //if (m_msrCounter >= 1D)   // one update per second
                {
                    //m_msrCounter = 0;
                    var totalTime = 0D;
                    foreach (var time in m_frameTimes)
                    {
                        totalTime += time;
                    }
                    m_lastFps = msrInterval / totalTime;
                }
            }
            else
            {
                m_lastFps = 0D;
            }
            return m_lastFps;
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

        private Dictionary<IconDescriptor, IconTextureWrapper> m_iconWrapperDict = new Dictionary<IconDescriptor, IconTextureWrapper>();

        private void UpdateIcons(IconDescriptor[] iconDescArray)
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
                var iconDescList = new List<IconDescriptor>(iconDescArray);
                var deleteList = new List<IconDescriptor>();
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
            private IconDescriptor m_iconDesc;

            public Texture Texture;
            public bool Visible;

            public IconTextureWrapper(IconDescriptor iconDesc)
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
                    m_iconDesc.GetIconStream());
            }
        }
        
        #endregion TextureWrapper
    }

    public enum ScaleMode
    {
        Stretch = 0,
        KeepProportion,
        FixedPixelSize,
    }
}
