using System;
using System.Linq;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.WinForms.Controls;
using ZXMAK2.Host.WinForms.Tools;
using System.Collections.Concurrent;


namespace ZXMAK2.Host.WinForms.Mdx
{
    public class RenderVideoObject : RenderObject
    {
        #region Constants

        private const byte MimicTvRatio = 4;      // mask size 1/x of pixel
        private const byte MimicTvAlpha = 0x90;   // mask alpha

        #endregion Constants


        private readonly object _syncRoot = new object();
        private readonly ConcurrentQueue<IFrameVideo> _queue = new ConcurrentQueue<IFrameVideo>();
        private IFrameVideo _lastVideoData;
        private int[] _lastBuffer = new int[0];    // noflick


        private Size _frameSize;
        private SizeF _frameSizeNormalized;
        private int _textureStride;
        private int _textureMaskTvStride;

        private Sprite _sprite;
        private Sprite _spriteTv;
        private Texture _texture0;
        private Texture _textureMaskTv;



        #region IRenderObject

        public override void ReleaseResources()
        {
            lock (_syncRoot)
            {
                Dispose(ref _texture0);
                Dispose(ref _textureMaskTv);
                Dispose(ref _sprite);
                Dispose(ref _spriteTv);
            }
        }

        public override void Render(Device device, Size size)
        {
            lock (_syncRoot)
            {
                IFrameVideo videoData;
                if (!_queue.TryDequeue(out videoData))
                {
                    videoData = _lastVideoData;
                }
                while (_queue.Count > 0)    // cleanup queue
                {
                    IFrameVideo tmp;
                    if (_queue.TryDequeue(out tmp))
                    {
                        videoData = tmp;
                    }
                }
                if (videoData == null)
                {
                    //Logger.Debug("Frame skip");
                    return;
                }
                _lastVideoData = videoData;
                if (_sprite == null)
                {
                    _sprite = new Sprite(device);
                }
                if (_spriteTv == null)
                {
                    _spriteTv = new Sprite(device);
                }
                if (_frameSize != videoData.Size || _texture0 == null)
                {
                    Dispose(ref _texture0);
                    Dispose(ref _textureMaskTv);
                    CreateTextures(device, videoData.Size);
                }
                _frameSizeNormalized = new SizeF(_frameSize.Width, _frameSize.Height * videoData.Ratio);
                UpdateTexture(videoData);

                var dstRect = GetDestinationRect(size);
                RenderFrame(device, dstRect);
                if (MimicTv)
                {
                    RenderMaskTv(device, dstRect);
                }
            }
        }

        private void RenderFrame(Device device, RectangleF dstRect)
        {
            var srcRect = new Rectangle(0, 0, _frameSize.Width, _frameSize.Height);
            _sprite.Begin(SpriteFlags.None);
            try
            {
                if (!AntiAlias)
                {
                    device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
                    device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
                    device.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Point);
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

        private void RenderMaskTv(Device device, RectangleF dstRect)
        {
            var srcRect = new Rectangle(0, 0, _frameSize.Width, _frameSize.Height * MimicTvRatio);
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

        private unsafe void UpdateTexture(IFrameVideo videoData)
        {
            if (_texture0 == null)
            {
                return;
            }
            using (var gs = _texture0.LockRectangle(0, LockFlags.None))
            {
                fixed (int* srcPtr = videoData.Buffer)
                {
                    CopyStride(
                        (int*)gs.InternalData,
                        srcPtr,
                        _frameSize.Width,
                        _frameSize.Height,
                        _textureStride);
                }
            }
            _texture0.UnlockRectangle(0);
        }

        #endregion IRenderObject


        #region Public

        public bool AntiAlias { get; set; }
        public bool MimicTv { get; set; }
        public ScaleMode ScaleMode { get; set; }

        public VideoFilter VideoFilter { get; set; }

        public void Update(IFrameVideo videoData)
        {
            if (_queue.Count > 1)
            {
                return;
            }
            var clone = new FrameVideo(videoData.Size, videoData.Ratio);
            Array.Copy(videoData.Buffer, clone.Buffer, clone.Buffer.Length);
            if (VideoFilter == VideoFilter.NoFlick)
            {
                unsafe
                {
                    fixed (int* srcPtr = clone.Buffer)
                    {
                        FilterNoFlick(srcPtr, clone.Size.Width, clone.Size.Height);
                    }
                }
            }
            _queue.Enqueue(clone);
        }

        #endregion Public


        #region Private

        private unsafe void CreateTextures(Device device, Size size)
        {
            _frameSize = size;

            var maxSize = Math.Max(size.Width, size.Height);
            var potSize = GetPotSize(maxSize);
            _texture0 = new Texture(
                device, 
                potSize, 
                potSize, 
                1, 
                Usage.None, 
                Format.X8R8G8B8, 
                Pool.Managed);
            _textureStride = potSize;

            var maskSizeTv = new Size(size.Width, size.Height * MimicTvRatio);
            var maxSizeTv = Math.Max(maskSizeTv.Width, maskSizeTv.Height);
            var potSizeTv = GetPotSize(maxSizeTv);
            _textureMaskTv = new Texture(
                device, 
                potSizeTv, 
                potSizeTv, 
                1, 
                Usage.None, Format.A8R8G8B8, Pool.Managed);
            _textureMaskTvStride = potSizeTv;
            using (var gs = _textureMaskTv.LockRectangle(0, LockFlags.None))
            {
                var pixelColor = 0;
                var gapColor = MimicTvAlpha << 24;
                var pdst = (int*)gs.InternalData.ToPointer();
                for (var y = 0; y < maskSizeTv.Height; y++)
                {
                    pdst += potSizeTv;
                    var color = (y % MimicTvRatio) != (MimicTvRatio - 1) ? pixelColor : gapColor;
                    for (var x = 0; x < maskSizeTv.Width; x++)
                    {
                        pdst[x] = color;
                    }
                }
            }
            _textureMaskTv.UnlockRectangle(0);
        }

        private RectangleF GetDestinationRect(SizeF wndSize)
        {
            var dstSize = _frameSizeNormalized;
            if (dstSize.Width <= 0 || dstSize.Height <= 0)
            {
                return new RectangleF(new PointF(0,0), dstSize);
            }
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
            var dstPos = new PointF(
                (float)Math.Floor((wndSize.Width - dstSize.Width) / 2F),
                (float)Math.Floor((wndSize.Height - dstSize.Height) / 2F));
            return new RectangleF(dstPos, dstSize);
        }

        #endregion Private


        #region Video Filters

        private static unsafe void CopyStride(
            int* pDstBuffer,
            int* pSrcBuffer,
            int width,
            int height,
            int dstStride)
        {
            var lineSize = width << 2;
            var srcLine = pSrcBuffer;
            var dstLine = pDstBuffer;
            for (var y = 0; y < height; y++)
            {
                NativeMethods.CopyMemory(dstLine, srcLine, lineSize);
                srcLine += width;
                dstLine += dstStride;
            }
        }

        private unsafe void FilterNoFlick(
            int* pSrcBuffer,
            int width,
            int height)
        {
            var size = height * width;
            if (_lastBuffer.Length < size)
            {
                _lastBuffer = new int[size];
            }
            fixed (int* pSrcBuffer2 = _lastBuffer)
            {
                var pSrcArray1 = pSrcBuffer;
                var pSrcArray2 = pSrcBuffer2;
                for (var y = 0; y < height; y++)
                {
                    for (var i = 0; i < width; i++)
                    {
                        var src1 = pSrcArray1[i];
                        var src2 = pSrcArray2[i];
                        var r1 = (((src1 >> 16) & 0xFF) + ((src2 >> 16) & 0xFF)) / 2;
                        var g1 = (((src1 >> 8) & 0xFF) + ((src2 >> 8) & 0xFF)) / 2;
                        var b1 = (((src1 >> 0) & 0xFF) + ((src2 >> 0) & 0xFF)) / 2;
                        pSrcArray2[i] = src1;
                        pSrcArray1[i] = -16777216 | (r1 << 16) | (g1 << 8) | b1;
                    }
                    pSrcArray1 += width;
                    pSrcArray2 += width;
                }
            }
        }

        #endregion Video Filters
    }
}
