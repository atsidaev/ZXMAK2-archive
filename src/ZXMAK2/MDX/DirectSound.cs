/// Description: Sound player
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;
using ZXMAK2.Interfaces;
using System.Collections.Generic;



namespace ZXMAK2.MDX
{
	public unsafe class DirectSound : IHostSound, IDisposable
	{
		private Device _device = null;
		private SecondaryBuffer _soundBuffer = null;
		private Notify _notify = null;

		private byte _zeroValue;
		private int _bufferSize;
		private int _bufferCount;

		private Thread _waveFillThread = null;
		private AutoResetEvent _fillEvent = new AutoResetEvent(true);
		private bool _isFinished;


        public DirectSound(Control mainForm, int device,
			int samplesPerSecond, short bitsPerSample, short channels,
			int bufferSize, int bufferCount)
		{
			_fillQueue = new Queue<byte[]>(bufferCount);
			_playQueue = new Queue<byte[]>(bufferCount);
            for (int i = 0; i < bufferCount; i++)
            {
                _fillQueue.Enqueue(new byte[bufferSize]);
            }
			_bufferSize = bufferSize;
			_bufferCount = bufferCount;
			_zeroValue = bitsPerSample == 8 ? (byte)128 : (byte)0;

			_device = new Device();
			_device.SetCooperativeLevel(mainForm, CooperativeLevel.Priority);

			var wf = new WaveFormat();
			wf.FormatTag = WaveFormatTag.Pcm;
			wf.SamplesPerSecond = samplesPerSecond;
			wf.BitsPerSample = bitsPerSample;
			wf.Channels = channels;
			wf.BlockAlign = (short)(wf.Channels * (wf.BitsPerSample / 8));
			wf.AverageBytesPerSecond = (int)wf.SamplesPerSecond * (int)wf.BlockAlign;

			// Create a buffer
			var bufferDesc = new BufferDescription(wf);
			bufferDesc.BufferBytes = _bufferSize * _bufferCount;
			bufferDesc.ControlPositionNotify = true;
			bufferDesc.GlobalFocus = true;

			_soundBuffer = new SecondaryBuffer(bufferDesc, _device);

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
			if (_waveFillThread != null)
			{
				try
				{
					_isFinished = true;
					if (_soundBuffer != null)
						if (_soundBuffer.Status.Playing)
							_soundBuffer.Stop();
					_fillEvent.Set();

					_waveFillThread.Join();

					if (_soundBuffer != null)
						_soundBuffer.Dispose();
					if (_notify != null)
						_notify.Dispose();

					if (_device != null)
						_device.Dispose();
				}
				finally
				{
					_waveFillThread = null;
					_soundBuffer = null;
					_notify = null;
					_device = null;
				}
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
					LogAgent.Error(ex);
				}
			}
		}

        protected void OnBufferFill(byte* buffer, int length)
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
                    dst[i] = lastSample;
                }
                return;
            }
            fixed (byte* srcb = buf)
            {
                uint* src = (uint*)srcb;
                for (var i = 0; i < length / 4; i++)
                {
                    dst[i] = src[i];
                }
                lastSample = dst[length / 4 - 1];
            }
            lock (_fillQueue)
            {
                _fillQueue.Enqueue(buf);
            }
            m_frameEvent.Set();
        }

		private readonly Queue<byte[]> _fillQueue;
		private readonly Queue<byte[]> _playQueue;
		private uint lastSample;
		
		
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

        private readonly AutoResetEvent m_frameEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent m_cancelEvent = new AutoResetEvent(false);
        private readonly Queue<byte[]> m_frames = new Queue<byte[]>();

        public void WaitFrame()
        {
            lock (_playQueue)
            {
                if (_fillQueue.Count > 0)
                {
                    return;
                }
                m_frameEvent.Reset();
                m_cancelEvent.Reset();
            }
            WaitHandle.WaitAny(new[] { m_frameEvent, m_cancelEvent });
        }

        public void CancelWait()
        {
            m_cancelEvent.Set();
        }

        public void PushFrame(uint[][] frameBuffers)
        {
            if (frameBuffers == null)
            {
                return;
            }
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
            fixed (byte* bptr = dst)
            {
                var uiptr = (uint*)bptr;
                for (var i = 0; i < dst.Length / 4; i++)    // clean buffer
                {
                    uint value1 = 0;
                    uint value2 = 0;
                    if (bufferArray.Length > 0)
                    {
                        for (int j = 0; j < bufferArray.Length; j++)
                        {
                            value1 += bufferArray[j][i] >> 16;
                            value2 += bufferArray[j][i] & 0xFFFF;
                        }
                        value1 /= (uint)bufferArray.Length;
                        value2 /= (uint)bufferArray.Length;
                    }
                    uiptr[i] = (value1 << 16) | value2;
                }

                //for (int i = 0; i < dst.Length / 4; i++)    // clean buffer
                //    uiptr[i] = 0;
                //foreach (uint[] buffer in bufferArray)       // mix sound sources
                //    fixed (uint* uibuffer = buffer)
                //        for (int i = 0; i < dst.Length/4; i++)
                //        {
                //            uint s1 = uiptr[i];
                //            uint s2 = uibuffer[i];
                //            uiptr[i] = ((((s1 >> 16) + (s2 >> 16)) / 2) << 16) | (((s1 & 0xFFFF) + (s2 & 0xFFFF)) / 2);
                //        }
            }
        }
    }
}
