using System;
using System.Linq;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Entities
{
    public class VideoFrame : IVideoFrame
    {
        public IVideoData VideoData { get; private set; }
        public IIconDescriptor[] Icons { get; private set; }

        public int StartTact { get; private set; }
        public double InstantUpdateTime { get; private set; }
        public double InstantRenderTime { get; private set; }
        public bool IsRefresh { get; private set; }


        public VideoFrame(
            IVideoData data, 
            IIconDescriptor[] icons, 
            int startTact,
            double instantUpdateTime,
            double instantRenderTime,
            bool isRefresh)
        {
            VideoData = data;
            Icons = icons;
            StartTact = startTact;
            InstantUpdateTime = instantUpdateTime;
            InstantRenderTime = instantRenderTime;
            IsRefresh = isRefresh;
        }
    }
}
