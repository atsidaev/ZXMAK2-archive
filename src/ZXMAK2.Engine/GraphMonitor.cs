using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;


namespace ZXMAK2.Engine
{
    public class GraphMonitor
    {
        private readonly int _sampleCount;
        private readonly double[] _graph;
        private int _index;
        private long? _lastStamp;

        
        public GraphMonitor(int sampleCount)
        {
            _sampleCount = sampleCount;
            _graph = new double[sampleCount];
        }

        public static double Frequency
        {
            get { return Stopwatch.Frequency; }
        }

        public void Push(double value)
        {
            var index = _index;
            _index = (index + 1) % _sampleCount;
            _graph[index] = value;
        }

        public void PushPeriod()
        {
            var stamp = Stopwatch.GetTimestamp();
            if (_lastStamp.HasValue)
            {
                var delta = stamp - _lastStamp;
                Push((double)delta);
            }
            _lastStamp = stamp;
        }

        public IEnumerable<double> Get()
        {
            var array = new double[_sampleCount];
            var fixedIndex = _index;
            for (var i = 0; i < _sampleCount; i++)
            {
                array[i] = _graph[(fixedIndex + i) % _sampleCount];
            }
            return array;
        }

        public void Clear()
        {
            _lastStamp = null;
            _index = 0;
            Array.Clear(_graph, 0, _graph.Length);
        }
    }
}
