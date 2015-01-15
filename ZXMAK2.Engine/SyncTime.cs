using System;
using System.Threading;
using System.Diagnostics;


namespace ZXMAK2.Engine
{
    public class SyncTime : IDisposable
    {
        private readonly object _syncRoot = new object();
        private readonly ManualResetEvent _waitEvent = new ManualResetEvent(true);
        private long _lastTimeStamp;
        private bool _isCancel;


        public SyncTime()
        {
            _lastTimeStamp = Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            Cancel();
            _waitEvent.Dispose();
        }

        public void WaitFrame()
        {
            _waitEvent.Reset();
            try
            {
                if (_isCancel)
                {
                    return;
                }
                var frequency = Stopwatch.Frequency;
                var time50 = frequency / 50;
                var stamp = Stopwatch.GetTimestamp();
                var time = stamp - _lastTimeStamp;
                if (time < time50)
                {
                    var delay = (int)(((time50 - time) * 1000) / frequency);
                    if (delay > 5)
                    {
                        Thread.Sleep(delay - 1);
                    }
                }
                while (true)
                {
                    stamp = Stopwatch.GetTimestamp();
                    time = stamp - _lastTimeStamp;
                    if (time >= time50)
                    {
                        break;
                    }
                    Thread.SpinWait(1);
                }
                if (time > time50 * 2)
                {
                    // resync
                    _lastTimeStamp = stamp;
                }
                else
                {
                    _lastTimeStamp += time50;
                }
            }
            finally
            {
                _waitEvent.Set();
            }
        }

        public void Cancel()
        {
            _isCancel = true;
            Thread.MemoryBarrier();
            _waitEvent.WaitOne();
            _isCancel = false;
            Thread.MemoryBarrier();
        }
    }
}
