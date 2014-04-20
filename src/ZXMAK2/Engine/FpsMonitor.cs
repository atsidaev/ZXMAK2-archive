using System;
using System.Diagnostics;


namespace ZXMAK2.Engine
{
    public class FpsMonitor
    {
        private readonly Stopwatch m_watch;
        private int m_frameCounter;

        public double Value { get; private set; }


        public FpsMonitor()
        {
            Value = 0;
            m_frameCounter = 0;
            m_watch = Stopwatch.StartNew();
        }

        public void Frame()
        {
            var time = m_watch.ElapsedTicks;
            m_frameCounter++;
            if (time >= Stopwatch.Frequency/3)
            {
                m_watch.Stop();
                m_watch.Reset();
                m_watch.Start();
                Value = m_frameCounter * (double)Stopwatch.Frequency / time;
                m_frameCounter = 0;
            }
        }
    }
}
