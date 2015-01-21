

using System;
namespace ZXMAK2.Host.WinForms.Tools
{
    public class FrameResampler
    {
        private readonly int _targetRate;
        private int _sourceRate;
        private double _ratio;
        private double _time;
        

        public FrameResampler(int targetRate)
        {
            _targetRate = targetRate;
        }

        public int SourceRate
        {
            get { return _sourceRate; }
            set 
            {
                if (_sourceRate == value)
                {
                    return;
                }
                _sourceRate = value;
                Update();
            }
        }

        /// <summary>
        /// Switch to the next source frame, returns true when target frame should be updated
        /// </summary>
        public bool Next()
        {
            _time += _ratio;
            var isSkipped = _time < 1D;
            if (_time >= 1D)
            {
                _time -= Math.Floor(_time);
            }
            return !isSkipped;
        }

        private void Update()
        {
            _time = 0;
            _ratio = (double)_targetRate / (double)_sourceRate;
        }
    }
}
