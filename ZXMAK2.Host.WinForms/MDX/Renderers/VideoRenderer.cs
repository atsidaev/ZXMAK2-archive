/* 
 *  Copyright 2008, 2015 Alex Makeev
 * 
 *  This file is part of ZXMAK2 (ZX Spectrum virtual machine).
 *
 *  ZXMAK2 is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  ZXMAK2 is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with ZXMAK2.  If not, see <http://www.gnu.org/licenses/>.
 *
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Microsoft.DirectX.Direct3D;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.WinForms.Controls;
using ZXMAK2.Host.WinForms.Tools;

namespace ZXMAK2.Host.WinForms.Mdx.Renderers
{
    public class VideoRenderer : RendererBase
    {
        #region Constants

        private const byte MimicTvRatio = 4;      // mask size 1/x of pixel
        private const byte MimicTvAlpha = 0x90;   // mask alpha

        #endregion Constants


        #region Fields

        private readonly ConcurrentQueue<IFrameVideo> _showQueue = new ConcurrentQueue<IFrameVideo>();
        private readonly ConcurrentQueue<IFrameVideo> _updateQueue = new ConcurrentQueue<IFrameVideo>();
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

        #endregion Fields


        #region .ctor

        public VideoRenderer(AllocatorPresenter allocator)
            : base(allocator)
        {
            _updateQueue.Enqueue(new FrameVideo(1, 1, 1));
            _updateQueue.Enqueue(new FrameVideo(1, 1, 1));
            _updateQueue.Enqueue(new FrameVideo(1, 1, 1));
        }

        #endregion .ctor


        #region RenderBase

        protected override void AttachSynchronized()
        {
            base.AttachSynchronized();
            _sprite = new Sprite(Allocator.Device);
            _spriteTv = new Sprite(Allocator.Device);
        }

        protected override void DetachSynchronized()
        {
            base.DetachSynchronized();
            if (_texture0 != null)
            {
                _texture0.Dispose();
                _texture0 = null;
            }
            if (_textureMaskTv != null)
            {
                _textureMaskTv.Dispose();
                _textureMaskTv = null;
            }
            if (_sprite != null)
            {
                _sprite.Dispose();
                _sprite = null;
            }
            if (_spriteTv != null)
            {
                _spriteTv.Dispose();
                _spriteTv = null;
            }
        }

        protected override void RenderSynchronized(int width, int height)
        {
            base.RenderSynchronized(width, height);
            IFrameVideo videoData;
            if (_showQueue.TryDequeue(out videoData))
            {
                if (_lastVideoData != null)
                {
                    _updateQueue.Enqueue(_lastVideoData);
                }
                _lastVideoData = videoData;
            }
            videoData = _lastVideoData;
            if (videoData == null)
            {
                Logger.Debug("Frame skip");
                return;
            }
            if (_frameSize != videoData.Size || _texture0 == null)
            {
                UpdateTextureSize(videoData.Size);
            }
            _frameSizeNormalized = new SizeF(_frameSize.Width, _frameSize.Height * videoData.Ratio);
            UpdateTextureData(videoData);

            var size = new Size(width, height);
            var dstRect = GetDestinationRect(size);
            RenderFrame(dstRect);
            if (MimicTv)
            {
                RenderMaskTv(dstRect);
            }
        }

        #endregion RenderBase


        #region Public

        public bool AntiAlias { get; set; }
        public bool MimicTv { get; set; }
        public ScaleMode ScaleMode { get; set; }
        public VideoFilter VideoFilter { get; set; }

        public void Update(IFrameVideo videoData)
        {
            IFrameVideo clone;
            if (!_updateQueue.TryDequeue(out clone))
            {
                return;
            }
            if (clone.Size != videoData.Size ||
                clone.Ratio != videoData.Ratio)
            {
                clone = new FrameVideo(videoData.Size, videoData.Ratio);
            }



            //if (_showQueue.Count > 0)
            //{
            //    return;
            //}
            //var clone = new FrameVideo(videoData.Size, videoData.Ratio);
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
            _showQueue.Enqueue(clone);
        }

        #endregion Public


        #region Private

        private void RenderFrame(RectangleF dstRect)
        {
            var srcRect = new Rectangle(0, 0, _frameSize.Width, _frameSize.Height);
            _sprite.Begin(SpriteFlags.None);
            try
            {
                if (!AntiAlias)
                {
                    Allocator.Device.SetSamplerState(0, SamplerStageStates.MinFilter, (int)TextureFilter.Point);
                    Allocator.Device.SetSamplerState(0, SamplerStageStates.MagFilter, (int)TextureFilter.Point);
                    Allocator.Device.SetSamplerState(0, SamplerStageStates.MipFilter, (int)TextureFilter.Point);
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

        private void RenderMaskTv(RectangleF dstRect)
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

        private RectangleF GetDestinationRect(SizeF wndSize)
        {
            var dstSize = _frameSizeNormalized;
            if (dstSize.Width <= 0 || dstSize.Height <= 0)
            {
                return new RectangleF(new PointF(0, 0), dstSize);
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

        private void UpdateTextureSize(Size size)
        {
            if (_texture0 != null)
            {
                _texture0.Dispose();
                _texture0 = null;
            }
            if (_textureMaskTv != null)
            {
                _textureMaskTv.Dispose();
                _textureMaskTv = null;
            }

            _frameSize = size;
            var maxSize = Math.Max(size.Width, size.Height);
            var potSize = AllocatorPresenter.GetPotSize(maxSize);
            _texture0 = new Texture(
                Allocator.Device,
                potSize,
                potSize,
                1,
                Usage.None,
                Format.X8R8G8B8,
                Pool.Managed);
            _textureStride = potSize;

            var maskSizeTv = new Size(size.Width, size.Height * MimicTvRatio);
            var maxSizeTv = Math.Max(maskSizeTv.Width, maskSizeTv.Height);
            var potSizeTv = AllocatorPresenter.GetPotSize(maxSizeTv);
            _textureMaskTv = new Texture(
                Allocator.Device,
                potSizeTv,
                potSizeTv,
                1,
                Usage.None, Format.A8R8G8B8, Pool.Managed);
            _textureMaskTvStride = potSizeTv;
            using (var gs = _textureMaskTv.LockRectangle(0, LockFlags.None))
            {
                var pixelColor = 0;
                var gapColor = MimicTvAlpha << 24;
                unsafe
                {
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
            }
            _textureMaskTv.UnlockRectangle(0);
        }

        private void UpdateTextureData(IFrameVideo videoData)
        {
            if (_texture0 == null)
            {
                return;
            }
            using (var gs = _texture0.LockRectangle(0, LockFlags.None))
            {
                unsafe
                {
                    fixed (int* srcPtr = videoData.Buffer)
                    {
                        CopyStride(
                            (int*)gs.InternalData.ToPointer(),
                            srcPtr,
                            _frameSize.Width,
                            _frameSize.Height,
                            _textureStride);
                    }
                }
            }
            _texture0.UnlockRectangle(0);
        }

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

        #endregion Private
    }
}
