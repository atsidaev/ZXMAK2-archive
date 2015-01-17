using ZXMAK2.Model.Disk;
using ZXMAK2.Serializers;


namespace ZXMAK2.Engine.Interfaces
{
	public interface IBetaDiskDevice
	{
        bool DOSEN { get; set; }
        DiskImage[] FDD { get; }
        bool NoDelay { get; set; }
        bool LogIo { get; set; }

        ISerializeManager LoadManager { get; }
	}
}
