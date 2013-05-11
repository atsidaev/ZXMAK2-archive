using System;

using ZXMAK2.Entities;


namespace ZXMAK2.Interfaces
{
	public interface IBetaDiskDevice
	{
        bool DOSEN { get; set; }
        DiskImage[] FDD { get; }
        bool NoDelay { get; set; }
        bool LogIo { get; set; }
	}
}
