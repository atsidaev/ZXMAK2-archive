using System;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHostSound : IDisposable
    {
        int SampleRate { get; }
        bool IsSyncSupported { get; }
        
        
        void WaitFrame();
        void CancelWait();
        void PushFrame(ISoundFrame soundFrame);
    }
}
