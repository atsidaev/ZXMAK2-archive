using System;
using System.Collections.Generic;
using System.Text;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHostVideo
    {
        void WaitFrame();
        void CancelWait();
        void PushFrame(IVideoFrame frame);
    }
}
