/// Description: Video renderer control
/// Author: Alex Makeev
/// Date: 27.03.2008
using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using ZXMAK2.Engine;
using System.Collections.Generic;


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
			foreach (IconTextureWrapper itw in m_iconWrapperDict.Values)
			{
				itw.Dispose();
			}
			m_iconWrapperDict.Clear();
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
			foreach (IconTextureWrapper itw in m_iconWrapperDict.Values)
				itw.Load(D3D);
			if (_font != null)
			{
				_font.Dispose();
				_font = null;
			}
			_font = new Microsoft.DirectX.Direct3D.Font(D3D, new System.Drawing.Font("Microsoft Sans Serif", 10f/*8.25f*/, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel));
		}

		private long lastTick = 0;
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


					if (KeepProportion && srcRect.Width > 0 && srcRect.Height > 0)
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
						dstRect = new RectangleF((dstRect.Width - width) / 2, (dstRect.Height - height) / 2, width, height);
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
						float fps = 0;
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

					if (DisplayIcon)
					{
						SizeF devIconSize = new SizeF(
							D3D.PresentationParameters.BackBufferWidth / 20,
							D3D.PresentationParameters.BackBufferHeight / 15);
						int iconNumber = 1;
						foreach (IconTextureWrapper itw in m_iconWrapperDict.Values)
						{
							if (itw.Visible && itw.Texture != null)
							{
								Rectangle iconRect = new Rectangle(new Point(0, 0), itw.Size);
								PointF devIconPos = new PointF(D3D.PresentationParameters.BackBufferWidth - devIconSize.Width * iconNumber, 0);
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

		private Dictionary<IconDescriptor, IconTextureWrapper> m_iconWrapperDict = new Dictionary<IconDescriptor, IconTextureWrapper>();

		public void UpdateIcons(IconDescriptor[] iconDescArray)
		{
			lock (SyncRoot)
			{
				foreach (IconDescriptor id in iconDescArray)
				{
					if (!m_iconWrapperDict.ContainsKey(id))
					{
						m_iconWrapperDict.Add(id, new IconTextureWrapper(id));
					}
					IconTextureWrapper itw = m_iconWrapperDict[id];
					itw.Visible = id.Visible;
					if (itw.Texture == null && D3D != null)
						itw.Load(D3D);
				}
				List<IconDescriptor> iconDescList = new List<IconDescriptor>(iconDescArray);
				List<IconDescriptor> deleteList = new List<IconDescriptor>();
				foreach (IconDescriptor id in m_iconWrapperDict.Keys)
				{
					if (!iconDescList.Contains(id))
						deleteList.Add(id);
				}
				foreach (IconDescriptor id in deleteList)
				{
					m_iconWrapperDict[id].Dispose();
					m_iconWrapperDict.Remove(id);
				}
			}
		}
	}

	internal class IconTextureWrapper : IDisposable
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
}
