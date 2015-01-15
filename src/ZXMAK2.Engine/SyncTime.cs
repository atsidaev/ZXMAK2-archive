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
                var stamp = _lastTimeStamp;
                var time = 0L;
                do
                {
                    stamp = Stopwatch.GetTimestamp();
                    time = stamp - _lastTimeStamp;
                    if (time > 0)
                    {
                        var rest = frequency / (time * 1000);
                        if (rest > 1)
                        {
                            Thread.Sleep((int)(rest - 1));
                        }
                    }
                } while (!_isCancel && time < time50);
                if (time > time50 * 2)
                {
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
            Thread.MemoryBarrier();
            _isCancel = false;
        }
    }
}
