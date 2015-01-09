using System;

using ZXMAK2.Entities;
using ZXMAK2.Serializers;


namespace ZXMAK2.Interfaces
{
	public interface IBetaDiskDevice
	{
        bool DOSEN { get; set; }
        DiskImage[] FDD { get; }
        bool NoDelay { get; set; }
        bool LogIo { get; set; }

        DiskLoadManager LoadManager { get; }
	}
}
