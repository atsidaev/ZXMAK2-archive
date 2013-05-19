/// Description: Sound player
/// Author: Alex Makeev
/// Date: 26.03.2008
using System;
using System.Collections;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;
using ZXMAK2.Interfaces;



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
			_fillQueue = new Queue(bufferCount);
			_playQueue = new Queue(bufferCount);
			for (int i = 0; i < bufferCount; i++)
				_fillQueue.Enqueue(new byte[bufferSize]);
			
			
			_bufferSize = bufferSize;
			_bufferCount = bufferCount;
			_zeroValue = bitsPerSample == 8 ? (byte)128 : (byte)0;

			_device = new Device();
			_device.SetCooperativeLevel(mainForm, CooperativeLevel.Priority);

			WaveFormat wf = new WaveFormat();
			wf.FormatTag = WaveFormatTag.Pcm;
			wf.SamplesPerSecond = samplesPerSecond;
			wf.BitsPerSample = bitsPerSample;
			wf.Channels = channels;
			wf.BlockAlign = (short)(wf.Channels * (wf.BitsPerSample / 8));
			wf.AverageBytesPerSecond = (int)wf.SamplesPerSecond * (int)wf.BlockAlign;

			// Create a buffer
			BufferDescription bufferDesc = new BufferDescription(wf);
			bufferDesc.BufferBytes = _bufferSize * _bufferCount;
			bufferDesc.ControlPositionNotify = true;
			bufferDesc.GlobalFocus = true;

			_soundBuffer = new SecondaryBuffer(bufferDesc, _device);

			_notify = new Notify(_soundBuffer);
			BufferPositionNotify[] posNotify = new BufferPositionNotify[_bufferCount];
			for (int i = 0; i < posNotify.Length; i++)
			{
				posNotify[i] = new BufferPositionNotify();
				posNotify[i].Offset = i * _bufferSize;
				posNotify[i].EventNotifyHandle = _fillEvent.SafeWaitHandle.DangerousGetHandle();
			}
			_notify.SetNotificationPositions(posNotify);

			_waveFillThread = new Thread(new ThreadStart(waveFillThreadProc));
			_waveFillThread.IsBackground = true;
			_waveFillThread.Name = "Wave fill thread";
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

		private unsafe void waveFillThreadProc()
		{
			int lastWrittenBuffer = -1;
			byte[] sampleData = new byte[_bufferSize];


			fixed (byte* lpSampleData = sampleData)
			{
				try
				{
					_soundBuffer.Play(0, BufferPlayFlags.Looping);
					while (!_isFinished)
					{
						_fillEvent.WaitOne();

						for (int i = (lastWrittenBuffer + 1) % _bufferCount; i != (_soundBuffer.PlayPosition / _bufferSize); i = ++i % _bufferCount)
						{
							OnBufferFill((IntPtr)lpSampleData, sampleData.Length);
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

		protected void OnBufferFill(IntPtr buffer, int length)
		{
			byte[] buf = null;
			lock (_playQueue.SyncRoot)
				if (_playQueue.Count > 0)
					buf = _playQueue.Dequeue() as byte[];
			if (buf != null)
			{
				//Marshal.Copy(buf, 0, buffer, length);
				//fixed(uint* lsp = &lastSample)
				//   Marshal.Copy(buf, length - 4, (IntPtr)lsp, 4);
				uint* dst = (uint*)buffer;
				fixed (byte* srcb = buf)
				{
					uint* src = (uint*)srcb;
					for (int i = 0; i < length / 4; i++)
						dst[i] = src[i];
					lastSample = dst[length / 4 - 1];
				}
				lock (_fillQueue.SyncRoot)
					_fillQueue.Enqueue(buf);
			}
			else
			{
				uint* dst = (uint*)buffer;
				for (int i = 0; i < length / 4; i++)
					dst[i] = lastSample;
			}
		}

		private Queue _fillQueue = null;
		private Queue _playQueue = null;
		private uint lastSample = 0;
		
		
		public byte[] LockBuffer()
		{
			byte[] sndbuf = null;
			lock (_fillQueue.SyncRoot)
				if (_fillQueue.Count > 0)
					sndbuf = _fillQueue.Dequeue() as byte[];
			return sndbuf;
		}

		public void UnlockBuffer(byte[] sndbuf)
		{
			lock (_playQueue.SyncRoot)
				_playQueue.Enqueue(sndbuf);
		}

		public int QueueLoadState { get { return (int)(_playQueue.Count * 100.0 / _fillQueue.Count); } }
	}
}
