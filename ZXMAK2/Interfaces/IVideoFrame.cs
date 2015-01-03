

namespace ZXMAK2.Interfaces
{
    public interface IVideoFrame
    {
        IVideoData VideoData { get; }
        IIconDescriptor[] Icons { get; }
        
        int StartTact { get; }
    }
}
