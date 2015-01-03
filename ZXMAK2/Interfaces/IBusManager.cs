using System;
using System.Xml;
using ZXMAK2.Serializers;
using ZXMAK2.Engine;
using ZXMAK2.Entities;
using ZXMAK2.Engine.Cpu;

namespace ZXMAK2.Interfaces
{
	public interface IBusManager
	{
		void SubscribeRdMemM1(int addrMask, int maskedValue, BusReadProc proc);
		void SubscribeRdMem(int addrMask, int maskedValue, BusReadProc proc);
		void SubscribeWrMem(int addrMask, int maskedValue, BusWriteProc proc);
		void SubscribeRdIo(int addrMask, int maskedValue, BusReadIoProc proc);
		void SubscribeWrIo(int addrMask, int maskedValue, BusWriteIoProc proc);
		void SubscribeRdNoMreq(int addrMask, int maskedValue, BusNoMreqProc proc);
		void SubscribeWrNoMreq(int addrMask, int maskedValue, BusNoMreqProc proc);
		void SubscribeReset(BusSignalProc proc);
        void SubscribeNmiRq(BusRqProc proc);
        void SubscribeNmiAck(BusSignalProc proc);
		void SubscribeIntAck(BusSignalProc proc);

		void SubscribePreCycle(BusCycleProc proc);
		void SubscribeBeginFrame(BusFrameEventHandler handler);
		void SubscribeEndFrame(BusFrameEventHandler handler);

		void AddSerializer(FormatSerializer serializer);
		void RegisterIcon(IconDescriptor iconDesc);

        void AddCommandUi(ICommand command);

		CpuUnit CPU { get; }
		bool IsSandbox { get; }
		String GetSatelliteFileName(string extension);

        T FindDevice<T>() where T : class;

		RzxHandler RzxHandler { get; set; }

        void RequestNmi(int timeOut);
	}

	public delegate void BusReadProc(ushort addr, ref byte value);
	public delegate void BusWriteProc(ushort addr, byte value);
	public delegate void BusReadIoProc(ushort addr, ref byte value, ref bool iorqge);
	public delegate void BusWriteIoProc(ushort addr, byte value, ref bool iorqge);
	public delegate void BusNoMreqProc(ushort addr);
    public delegate void BusRqProc(BusCancelArgs e);
	public delegate void BusCycleProc(int frameTact);
	public delegate void BusSignalProc();
	public delegate void BusFrameEventHandler();

    public class BusCancelArgs
    {
        private bool m_cancel;

        public BusCancelArgs()
        {
            m_cancel = false;
        }

        public bool Cancel
        {
            get { return m_cancel; }
            set { m_cancel = m_cancel | value; }
        }
    }
}
