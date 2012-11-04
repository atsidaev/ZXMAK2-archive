using System;
using System.Xml;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Serializers;
using ZXMAK2.Engine;

namespace ZXMAK2.Interfaces
{
	public interface IBusManager
	{
		void SubscribeRDMEM_M1(int addrMask, int maskedValue, BusReadProc proc);
		void SubscribeRDMEM(int addrMask, int maskedValue, BusReadProc proc);
		void SubscribeWRMEM(int addrMask, int maskedValue, BusWriteProc proc);
		void SubscribeRDIO(int addrMask, int maskedValue, BusReadIoProc proc);
		void SubscribeWRIO(int addrMask, int maskedValue, BusWriteIoProc proc);
		void SubscribeRDNOMREQ(int addrMask, int maskedValue, BusNoMreqProc proc);
		void SubscribeWRNOMREQ(int addrMask, int maskedValue, BusNoMreqProc proc);
		void SubscribeRESET(BusSignalProc proc);
		void SubscribeNMIACK(BusSignalProc proc);
		void SubscribeINTACK(BusSignalProc proc);

		void SubscribePreCycle(BusCycleProc proc);
		void SubscribeBeginFrame(BusFrameEventHandler handler);
		void SubscribeEndFrame(BusFrameEventHandler handler);

		void AddSerializer(FormatSerializer serializer);
		void RegisterIcon(IconDescriptor iconDesc);

		Z80CPU CPU { get; }
		bool IsSandbox { get; }
		String GetSatelliteFileName(string extension);

		BusDeviceBase FindDevice(Type type);

		RzxHandler RzxHandler { get; set; }
	}

	public enum BusCategory
	{
		Memory,
		ULA,
		Disk,
		Sound,
		Music,
		Tape,
		Keyboard,
		Mouse,
		Other,
	}

	public delegate void BusReadProc(ushort addr, ref byte value);
	public delegate void BusWriteProc(ushort addr, byte value);
	public delegate void BusReadIoProc(ushort addr, ref byte value, ref bool iorqge);
	public delegate void BusWriteIoProc(ushort addr, byte value, ref bool iorqge);
	public delegate void BusNoMreqProc(ushort addr);
	public delegate void BusCycleProc(int frameTact);
	public delegate void BusSignalProc();
	public delegate void BusFrameEventHandler();
}
