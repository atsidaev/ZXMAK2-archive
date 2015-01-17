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
        public double InstantTime { get; private set; }


        public VideoFrame(
            IVideoData data, 
            IIconDescriptor[] icons, 
            int startTact,
            double instantTime)
        {
            VideoData = data;
            Icons = icons;
            StartTact = startTact;
            InstantTime = instantTime;
        }
    }
}
