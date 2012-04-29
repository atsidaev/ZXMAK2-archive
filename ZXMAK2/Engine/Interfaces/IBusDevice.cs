using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Z80;
using System.Xml;
using ZXMAK2.Engine.Serializers;

namespace ZXMAK2.Engine.Interfaces
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

		Z80CPU CPU { get; }
		bool IsSandbox { get; }
        
        IBusDevice FindDevice(Type type);
    }

    public interface IBusDevice
    {
        string Name { get; }
        string Description { get; }
        BusCategory Category { get; }
		int BusOrder { get; set; }
        #region Comment
        /// <summary>
        /// Collect information about device. Add handlers & serializers here.
        /// </summary>
        #endregion
        void BusInit(IBusManager bmgr);
        #region Comment
        /// <summary>
        /// Called after Init, before device will be used. Add load files here
        /// </summary>
        #endregion
        void BusConnect();
        #region Comment
        /// <summary>
        /// Called when device using finished. Add flush & close files here
        /// </summary>
        #endregion
        void BusDisconnect();
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
