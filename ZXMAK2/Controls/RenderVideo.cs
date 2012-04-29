/// Description: Video renderer control
/// Author: Alex Makeev
/// Date: 27.03.2008
using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;


namespace ZXMAK2.Controls
{
	public class RenderVideo : Render3D
	{
        private Sprite _sprite = null;
		private Texture _texture = null;
        private Size m_surfaceSize = new Size(0, 0);
        private Size m_textureSize = new Size(0, 0);
        private float m_surfaceHeightScale = 1F;

        private Sprite _iconSprite = null;
        private Texture _iconDiskTexture = null;
        private Size _iconDiskTextureSize = new Size(0,0);
        private Microsoft.DirectX.Direct3D.Font _font = null;


		public bool Smoothing = false;
        public bool KeepProportion = false;
        public bool DebugInfo = false;

        public bool IconDisk = false;
        public bool DisplayIcon = true;

		protected override void OnCreateDevice()
		{
            base.OnCreateDevice();
            _sprite = new Sprite(D3D);
            _iconSprite = new Sprite(D3D);
        }

		protected override void OnDestroyDevice()
		{
            if (_texture != null)
            {
                _texture.Dispose();
                _texture = null;
            }
            if (_iconDiskTexture != null)
            {
                _iconDiskTexture.Dispose();
                _iconDiskTexture = null;
            }
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

        public unsafe void UpdateSurface(int[] surfaceBuffer, Size surfaceSize, float surfaceHeightScale)
		{
            lock (SyncRoot)
            {
                if (D3D == null)
                    return;
                try
                {
                    m_surfaceHeightScale = surfaceHeightScale;
                    if (m_surfaceSize != surfaceSize)
                    {
                        initTextures(surfaceSize);
                    }
                    if (_texture != null)
                    {
                        using (GraphicsStream gs = _texture.LockRectangle(0, LockFlags.None))
                            fixed (int* srcPtr = surfaceBuffer)
                                drawFrame((int*)gs.InternalData, srcPtr);
                        _texture.UnlockRectangle(0);
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
                    return;
                //base.ResizeContext(surfaceSize);
                int potSize = getPotSize(surfaceSize);
                if (_texture != null)
                {
                    _texture.Dispose();
                    _texture = null;
                }
                _texture = new Texture(D3D, potSize, potSize, 1, Usage.None, Format.A8R8G8B8, Pool.Managed);
                m_textureSize = new System.Drawing.Size(potSize, potSize);
                m_surfaceSize = surfaceSize;
                initIconTextures();
            }
        }

        private void initIconTextures()
        {
            if (_iconDiskTexture != null)
            {
                _iconDiskTexture.Dispose();
                _iconDiskTexture = null;
            }
            _iconDiskTexture = TextureLoader.FromStream(
                D3D,
                Utils.GetIconStream("Fdd.png"));
            _iconDiskTextureSize = new Size(128, 128);
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
            _font = new Microsoft.DirectX.Direct3D.Font(D3D, new System.Drawing.Font("Microsoft Sans Serif", 10f/*8.25f*/, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel));
        }

        private long lastTick =0;
        private float lastFps = 0;

		protected override void OnRenderScene()
		{
            lock (SyncRoot)
            {
                if (_texture != null)
                {
                    _sprite.Begin(SpriteFlags.None);

                    ////if (d3d.DeviceCaps.TextureFilterCaps.SupportsMinifyAnisotropic)
                    ////if (device.DeviceCaps.TextureFilterCaps.SupportsMagnifyAnisotropic)
                    ////d3d.SamplerState[0].MipFilter = TextureFilter.Point;
                    //TextureFilter min = D3D.SamplerState[0].MinFilter;
                    //TextureFilter mag = D3D.SamplerState[0].MagFilter;
                    //D3D.SamplerState[0].MinFilter = TextureFilter.Anisotropic;
                    //D3D.SamplerState[0].MagFilter = TextureFilter.Linear;

                    //TODO: Add video output option "Enable filter"
                    if (!Smoothing)
                    {
                        D3D.SamplerState[0].MinFilter = TextureFilter.None;
                        D3D.SamplerState[0].MagFilter = TextureFilter.None;
                    }

                    Rectangle srcRect = new Rectangle(0, 0, m_surfaceSize.Width, m_surfaceSize.Height);
                    RectangleF dstRect = new RectangleF(
                        0,
                        0,
                        D3D.PresentationParameters.BackBufferWidth,
                        D3D.PresentationParameters.BackBufferHeight);


                    if (KeepProportion && srcRect.Width > 0 && srcRect.Height>0)
                    {
                        float srcWidth = (float)srcRect.Width;
                        float srcHeight = (float)srcRect.Height * m_surfaceHeightScale;
                        
                        float rx = (float)dstRect.Width / srcWidth;
                        float ry = (float)dstRect.Height / srcHeight;
                        float width = dstRect.Width;
                        float height = dstRect.Height;
                        if (rx > ry)
                            width = width * ry / rx;
                        else if (rx < ry)
                            height = height * rx / ry;
                        dstRect = new RectangleF((dstRect.Width-width)/2, (dstRect.Height-height)/2, width, height);
                    }

                    _sprite.Draw2D(
                       _texture,
                       srcRect,
                       dstRect.Size,
                       dstRect.Location,
                       0x00FFFFFF);
                    
                    //D3D.SamplerState[0].MinFilter = min;
                    //D3D.SamplerState[0].MagFilter = mag;

                    _sprite.End();

                    if (DebugInfo)
                    {
                        float fps=0;
                        long tick = System.Diagnostics.Stopwatch.GetTimestamp();
                        long dt = tick - lastTick;
                        if (lastTick != 0 && dt > 0)
                            fps = System.Diagnostics.Stopwatch.Frequency / dt;
                        if (fps > 0)
                            lastFps += 0.3F * (fps - lastFps);
                        lastTick = tick;
                        string textValue = string.Format(
                            "Render FPS: {0:F1}\nDevice FPS: {1}\nBack: [{2}, {3}]\nClient: [{4}, {5}]\nSurface: [{6}, {7}]\nFrameStart: {8}T",
                            lastFps,
                            D3D.DisplayMode.RefreshRate,
                            D3D.PresentationParameters.BackBufferWidth,
                            D3D.PresentationParameters.BackBufferHeight,
                            ClientSize.Width,
                            ClientSize.Height,
                            m_surfaceSize.Width,
                            m_surfaceSize.Height,
                            m_debugFrameStart);
                        Rectangle textRect = _font.MeasureString(null, textValue, DrawTextFormat.NoClip, Color.Yellow);
                        _font.DrawText(null, textValue, textRect, DrawTextFormat.NoClip, Color.Yellow);
                    }

                    if (DisplayIcon && IconDisk)
                    {
                        _iconSprite.Begin(SpriteFlags.AlphaBlend);
                        SizeF devIconSize = new SizeF(
                            D3D.PresentationParameters.BackBufferWidth / 20,
                            D3D.PresentationParameters.BackBufferHeight / 15);
                        Rectangle iconRect = new Rectangle(new Point(0, 0), _iconDiskTextureSize);
                        PointF devIconPos = new PointF(
                            D3D.PresentationParameters.BackBufferWidth - devIconSize.Width,
                            0);
                        _iconSprite.Draw2D(
                           _iconDiskTexture,
                           iconRect,
                           devIconSize,
                           devIconPos,
                           Color.FromArgb(255, 255, 255, 255));
                        _iconSprite.End();
                    }
                }
            }
		}

		private unsafe void drawFrame(int* dstBuffer, int* srcBuffer)
		{
            for (int y = 0; y < m_surfaceSize.Height; y++)
            {
                int* srcLine = srcBuffer + m_surfaceSize.Width * y;
                int* dstLine = dstBuffer + m_textureSize.Width * y;
                for (int i = 0; i < m_surfaceSize.Width; i++)
                    dstLine[i] = srcLine[i];
            }
		}

        private static int getPotSize(Size surfaceSize)
        {
            // Create POT texture (e.g. 512x512) to render NPOT image (e.g. 320x240),
            // because NPOT textures is not supported on some videocards
            int size = surfaceSize.Width > surfaceSize.Height ? surfaceSize.Width : surfaceSize.Height;
            int potSize;
            for (int power = 1; (potSize = pow(2, power)) < size; power++) ;
            return potSize;
        }

		private static int pow(int value, int power)
		{
			int result = value;
			for (int i = 0; i < power; i++)
				result *= value;
			return result;// 65535;
		}

        private int m_debugFrameStart = 0;
        public int DebugStartTact 
        {
            get { return m_debugFrameStart; }
            set { m_debugFrameStart = value; }
        }
    }
}
