

namespace ZXMAK2.Host.Interfaces
{
    public interface IVideoFrame
    {
        IVideoData VideoData { get; }
        IIconDescriptor[] Icons { get; }
        
        int StartTact { get; }
        double InstantUpdateTime { get; }
        double InstantRenderTime { get; }
    }
}
