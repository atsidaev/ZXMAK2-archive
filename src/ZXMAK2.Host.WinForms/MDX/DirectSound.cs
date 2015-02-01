/// Description: Sound player
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Host.WinForms.Tools;



namespace ZXMAK2.Host.WinForms.Mdx
{
    public sealed unsafe class DirectSound : IHostSound
    {
        private readonly Device _device;
        private readonly SecondaryBuffer _soundBuffer;
        private readonly Notify _notify;
        private readonly int _sampleRate;

        private uint _zeroValue;
        private int _bufferSize;
        private int _bufferCount;

        private Thread _waveFillThread = null;
        private readonly AutoResetEvent _fillEvent = new AutoResetEvent(true);
        private bool _isFinished;


        public DirectSound(
            Form form,
            int sampleRate,
            int bufferCount)
        {
            if ((sampleRate % 50) != 0)
            {
                throw new ArgumentOutOfRangeException("sampleRate", "Sample rate must be a multiple of 50!");
            }
            _sampleRate = sampleRate;
            _fillQueue = new Queue<byte[]>(bufferCount);
            _playQueue = new Queue<byte[]>(bufferCount);
            var channels = (short)2;
            var bitsPerSample = (short)16;
            var bufferSize = sampleRate * (bitsPerSample / 8) * channels / 50;
            for (int i = 0; i < bufferCount; i++)
            {
                _fillQueue.Enqueue(new byte[bufferSize]);
            }
            _bufferSize = bufferSize;
            _bufferCount = bufferCount;
            _zeroValue = 0;

            _device = new Device();
            _device.SetCooperativeLevel(form, CooperativeLevel.Priority);

            var wf = new WaveFormat();
            wf.FormatTag = WaveFormatTag.Pcm;
            wf.SamplesPerSecond = sampleRate;
            wf.BitsPerSample = bitsPerSample;
            wf.Channels = channels;
            wf.BlockAlign = (short)(wf.Channels * (wf.BitsPerSample / 8));
            wf.AverageBytesPerSecond = wf.SamplesPerSecond * wf.BlockAlign;

            // Create a buffer
            using (var bufferDesc = new BufferDescription(wf))
            {
                bufferDesc.BufferBytes = _bufferSize * _bufferCount;
                bufferDesc.ControlPositionNotify = true;
                bufferDesc.GlobalFocus = true;

                _soundBuffer = new SecondaryBuffer(bufferDesc, _device);
            }

            _notify = new Notify(_soundBuffer);
            var posNotify = new BufferPositionNotify[_bufferCount];
            for (int i = 0; i < posNotify.Length; i++)
            {
                posNotify[i] = new BufferPositionNotify();
                posNotify[i].Offset = i * _bufferSize;
                posNotify[i].EventNotifyHandle = _fillEvent.SafeWaitHandle.DangerousGetHandle();
            }
            _notify.SetNotificationPositions(posNotify);

            _waveFillThread = new Thread(new ThreadStart(WaveFillThreadProc));
            _waveFillThread.IsBackground = true;
            _waveFillThread.Name = "DirectSound.WaveFillThreadProc";
            _waveFillThread.Priority = ThreadPriority.Highest;
            _waveFillThread.Start();
        }

        public void Dispose()
        {
            if (_waveFillThread == null)
            {
                return;
            }
            try
            {
                _isFinished = true;
                if (_soundBuffer != null && _soundBuffer.Status.Playing)
                {
                    _soundBuffer.Stop();
                }
                _fillEvent.Set();
                _cancelEvent.Set();

                _waveFillThread.Join();

                if (_soundBuffer != null)
                {
                    _soundBuffer.Dispose();
                }
                if (_notify != null)
                {
                    _notify.Dispose();
                }
                if (_device != null)
                {
                    _device.Dispose();
                }
                if (_fillEvent != null)
                {
                    _fillEvent.Dispose();
                }
                if (_frameEvent != null)
                {
                    _frameEvent.Dispose();
                }
                if (_cancelEvent != null)
                {
                    _cancelEvent.Dispose();
                }
            }
            finally
            {
                _waveFillThread = null;
            }
        }

        private void WaveFillThreadProc()
        {
            var lastWrittenBuffer = -1;
            var sampleData = new byte[_bufferSize];
            fixed (byte* lpSampleData = sampleData)
            {
                try
                {
                    _soundBuffer.Play(0, BufferPlayFlags.Looping);
                    while (!_isFinished)
                    {
                        _fillEvent.WaitOne();
                        var stIndex = (lastWrittenBuffer + 1) % _bufferCount;
                        var playingIndex = (_soundBuffer.PlayPosition / _bufferSize);
                        for (var i = stIndex; i != playingIndex; i = ++i % _bufferCount)
                        {
                            OnBufferFill(lpSampleData, sampleData.Length);
                            _soundBuffer.Write(_bufferSize * i, sampleData, LockFlag.None);
                            lastWrittenBuffer = i;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }
        }

        private void OnBufferFill(byte* buffer, int length)
        {
            byte[] buf = null;
            lock (_playQueue)
            {
                if (_playQueue.Count > 0)
                {
                    buf = _playQueue.Dequeue();
                }
            }
            uint* dst = (uint*)buffer;
            if (buf == null)
            {
                for (var i = 0; i < length / 4; i++)
                {
                    dst[i] = _lastSample == uint.MaxValue ? _zeroValue : _lastSample;
                }
                return;
            }
            fixed (byte* srcb = buf)
            {
                uint* src = (uint*)srcb;
                NativeMethods.CopyMemory(dst, src, length);
                //for (var i = 0; i < length / 4; i++)
                //{
                //    dst[i] = src[i];
                //}
                _lastSample = dst[length / 4 - 1];
            }
            lock (_fillQueue)
            {
                _fillQueue.Enqueue(buf);
                _frameEvent.Set();
            }
        }

        private readonly Queue<byte[]> _fillQueue;
        private readonly Queue<byte[]> _playQueue;
        private uint _lastSample = uint.MaxValue;


        private byte[] LockBuffer()
        {
            lock (_fillQueue)
            {
                if (_fillQueue.Count > 0)
                {
                    return _fillQueue.Dequeue();
                }
                return null;
            }
        }

        private void UnlockBuffer(byte[] sndbuf)
        {
            lock (_playQueue)
            {
                _playQueue.Enqueue(sndbuf);
            }
        }

        public int QueueLoadState
        {
            get
            {
                lock (_playQueue)
                {
                    return (int)(_playQueue.Count * 100.0 / _fillQueue.Count);
                }
            }
        }


        #region IHostSound

        private readonly AutoResetEvent _frameEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _cancelEvent = new AutoResetEvent(false);
        private readonly Queue<byte[]> _frames = new Queue<byte[]>();

        public int SampleRate
        {
            get { return _sampleRate; }
        }

        public void WaitFrame()
        {
            lock (_fillQueue)
            {
                _frameEvent.Reset();
                _cancelEvent.Reset();
                if (_fillQueue.Count > 0)
                {
                    return;
                }
            }
            WaitHandle.WaitAny(new[] { _frameEvent, _cancelEvent });
        }

        public void CancelWait()
        {
            _cancelEvent.Set();
        }

        public void PushFrame(uint[][] frameBuffers)
        {
            var buffer = LockBuffer();
            if (buffer == null)
            {
                return;
            }
            Mix(buffer, frameBuffers);
            UnlockBuffer(buffer);
        }

        #endregion IHostSound

        private void Mix(byte[] dst, uint[][] bufferArray)
        {
            fixed (byte* pbdst = dst)
            {
                var pdst = (short*)pbdst;
                for (var i = 0; i < dst.Length / 4; i++)
                {
                    var index = i * 2;
                    var left = 0;
                    var right = 0;
                    foreach (var src in bufferArray)
                    {
                        fixed (uint* puisrc = src)
                        {
                            var psrc = (short*)puisrc;
                            left += psrc[index];
                            right += psrc[index + 1];
                        }
                    }
                    if (bufferArray.Length > 0)
                    {
                        left /= bufferArray.Length;
                        right /= bufferArray.Length;
                    }
                    pdst[index] = (short)left;
                    pdst[index + 1] = (short)right;
                }
            }
        }
    }
}
