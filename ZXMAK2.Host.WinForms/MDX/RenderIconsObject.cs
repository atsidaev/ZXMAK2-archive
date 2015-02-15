using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using ZXMAK2.Host.Interfaces;

namespace ZXMAK2.Host.WinForms.Mdx
{
    public class RenderIconsObject : RenderObject
    {
        #region Fields

        private readonly object _syncRoot = new object();
        private readonly Dictionary<IIconDescriptor, IconTextureWrapper> _iconTextures = new Dictionary<IIconDescriptor, IconTextureWrapper>();
        private Sprite _spriteIcon;

        #endregion Fields


        #region RenderObject

        public override void ReleaseResources()
        {
            lock (_syncRoot)
            {
                Dispose(ref _spriteIcon);
                foreach (var icon in _iconTextures.Values)
                {
                    icon.Dispose();
                }
            }
        }

        public override void Render(Device device, Size size)
        {
            lock (_syncRoot)
            {
                if (_spriteIcon == null)
                {
                    _spriteIcon = new Sprite(device);
                }
                var visibleIcons = _iconTextures.Values
                    .Where(icon => icon.Visible);
                foreach (var icon in visibleIcons)
                {
                    icon.LoadResources(device);
                }
                var potSize = GetPotSize(32);
                var iconSize = new SizeF(potSize, potSize);
                var iconNumber = 1;
                foreach (var iconTexture in visibleIcons)
                {
                    var iconRect = new Rectangle(new Point(0, 0), iconTexture.Size);
                    var iconPos = new PointF(
                        size.Width - iconSize.Width * iconNumber,
                        0);
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
        }

        #endregion RenderObject


        #region Public

        public void Update(IIconDescriptor[] icons)
        {
            var nonUsed = _iconTextures.Keys.ToList();
            foreach (var id in icons)
            {
                var iconTexture = default(IconTextureWrapper);
                if (!_iconTextures.ContainsKey(id))
                {
                    iconTexture = new IconTextureWrapper(id);
                    lock (_syncRoot)
                    {
                        _iconTextures.Add(id, iconTexture);
                    }
                }
                else
                {
                    iconTexture = _iconTextures[id];
                }
                iconTexture.Visible = id.Visible;
                nonUsed.Remove(id);
            }
            if (nonUsed.Count > 0)
            {
                lock (_syncRoot)
                {
                    foreach (var id in nonUsed)
                    {
                        _iconTextures[id].Dispose();
                        _iconTextures.Remove(id);
                    }
                }
            }
        }

        #endregion Public


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
                    Texture = TextureLoader.FromStream(device, stream);
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
    }
}
