using System;
using System.IO;
using System.Drawing;

namespace ZXMAK2.Interfaces
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
		bool CheckInt(int frameTact);

		byte PortFE { get; set; }
	}
}
