

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostSound
    {
        void WaitFrame();
        void CancelWait();
        void PushFrame(uint[][] frameBuffers);
    }
}
