using System;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHostVideo : IDisposable
    {
        bool IsSyncSupported { get; }
        
        void WaitFrame();
        void CancelWait();
        void PushFrame(IVideoFrame frame);
    }
}
