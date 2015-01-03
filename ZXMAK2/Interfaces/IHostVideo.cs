using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine;

namespace ZXMAK2.Interfaces
{
    public interface IHostVideo
    {
        void WaitFrame();
        void CancelWait();
        void PushFrame(IVideoFrame frame);
    }
}
