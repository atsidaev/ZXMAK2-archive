using System;
using System.Diagnostics;


namespace ZXMAK2.Engine
{
    public class FpsMonitor
    {
        private int m_frameCounter;
        private long m_lastTime;
        private long m_lastTimeFrame;

        public double Value { get; private set; }
        public double InstantTime { get; private set; }

        public FpsMonitor()
        {
            Reset();
        }

        public void Frame()
        {
            var stamp = Stopwatch.GetTimestamp();
            InstantTime = stamp - m_lastTime;
            m_lastTime = stamp;
            m_frameCounter++;
            if (m_frameCounter >= 50)
            {
                var time = stamp - m_lastTimeFrame;
                Value = CalcAverage(time, m_frameCounter);
                m_frameCounter = 0;
                m_lastTimeFrame = stamp;
            }
        }

        private double CalcAverage(long time, int frameCount)
        {
            return frameCount * (double)Stopwatch.Frequency / time;
        }
        
        public void Reset()
        {
            Value = 0;
            InstantTime = 0;
            m_frameCounter = 0;
            m_lastTimeFrame = m_lastTime = Stopwatch.GetTimestamp();
        }
    }
}
