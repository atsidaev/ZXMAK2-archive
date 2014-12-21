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
using System.ComponentModel;


namespace ZXMAK2.WinForms
{
    public class RenderVideo : Render3D, IHostVideo
    {
        #region Fields

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
        
        public void PushFrame(VirtualMachine vm)
        {
            m_fpsUpdate.Frame();
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

        private const byte MimicRatio = 4;      // 1/x
        private const byte MimicAlpha = 0xFF;

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

                var maskTvSize = new Size(surfaceSize.Width, surfaceSize.Height * MimicRatio);
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
                            *(ptr + offset) = (y%MimicRatio)!=(MimicRatio-1) ? 0 : MimicAlpha<<24;
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
            lock (SyncRoot)
            {
                if (m_texture != null)
                {
                    m_fpsRender.Frame();
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
                            m_surfaceSize.Height*MimicRatio);
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
                            m_fpsUpdate.Value,
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
                            textRect.Width + 10,
                            textRect.Height);

                        FillRect(textRect, Color.FromArgb(64, Color.Green));

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

        private void FillRect(Rectangle textRect, Color color)
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
                new CustomVertex.TransformedColored(textRect.Left, textRect.Top+textRect.Height, 0, 0, colorInt),
                new CustomVertex.TransformedColored(textRect.Left, textRect.Top, 0, 0, colorInt),
                new CustomVertex.TransformedColored(textRect.Left+textRect.Width, textRect.Top+textRect.Height, 0, 0, colorInt),
                new CustomVertex.TransformedColored(textRect.Left+textRect.Width, 0, 0, 0, colorInt),
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
        SquarePixelSize,
    }
}
