using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZXMAK2.Host.WinForms.Tools
{
    public class FrameResampler
    {
        private readonly int _targetRate;
        private int _sourceRate;
        private int _ratio;
        private int _index;
        

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
            var isSkipped = _ratio > 0 && ++_index > _ratio;
            if (isSkipped)
            {
                _index = 0;
            }
            return !isSkipped;
        }

        private void Update()
        {
            _index = 0;
            var frameRest = _sourceRate % _targetRate;
            _ratio = frameRest != 0 ? _targetRate / frameRest : 0;
        }
    }
}
