using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Interfaces
{
    public interface IHostSound
    {
        void WaitFrame();
        void CancelWait();
        void PushFrame(uint[][] frameBuffers);
    }
}
