

namespace ZXMAK2.Host.Interfaces
{
    public interface ISoundFrame
    {
        int SampleRate { get; }
        void Refresh();
        uint[] GetBuffer();
    }
}
