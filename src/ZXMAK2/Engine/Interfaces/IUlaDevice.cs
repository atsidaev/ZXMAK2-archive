using System;
using System.IO;
using System.Drawing;

namespace ZXMAK2.Engine.Interfaces
{
    public interface IUlaDevice //: BusDeviceBase
    {
        int[] VideoBuffer { get; set; }
        float VideoHeightScale { get; }
        Size VideoSize { get; }
        
        void LoadScreenData(Stream stream);
        void SaveScreenData(Stream stream);
        void ForceRedrawFrame();

        void Flush();

        int FrameTactCount { get; }

		byte PortFE { get; set; }
    }
}
