using System;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHostVideo : IDisposable
    {
        void WaitFrame();
        void CancelWait();
        void PushFrame(IVideoFrame frame, bool isRequested);
    }
}
