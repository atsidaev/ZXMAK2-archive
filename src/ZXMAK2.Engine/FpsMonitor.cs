using System;
using System.Diagnostics;


namespace ZXMAK2.Engine
{
    public class FpsMonitor
    {
        private readonly Stopwatch m_watch;
        private int m_frameCounter;
        private long m_lastTime;

        public double Value { get; private set; }
        public double InstantTime { get; private set; }

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
            if (m_frameCounter >= 50)// time >= Stopwatch.Frequency/3)
            {
                m_watch.Stop();
                m_watch.Reset();
                m_watch.Start();
                Value = CalcAverage(time, m_frameCounter);// m_frameCounter * (double)Stopwatch.Frequency / time;
                InstantTime = CalcInstant(time);
                m_frameCounter = 0;
                m_lastTime = 0;
            }
            else
            {
                InstantTime = CalcInstant(time);
            }
        }

        private double CalcAverage(long time, int frameCount)
        {
            return frameCount * (double)Stopwatch.Frequency / time;
        }

        private double CalcInstant(long time)
        {
            var delta = time - m_lastTime;
            m_lastTime = time;
            return delta;
        }

        public void Reset()
        {
            Frame();
            Value = 0;
        }
    }
}
