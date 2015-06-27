using System;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;


namespace ZXMAK2.Engine.Interfaces
{
	public interface IBusManager
	{
		void SubscribeRdMemM1(int addrMask, int maskedValue, BusReadProc proc);
		void SubscribeRdMem(int addrMask, int maskedValue, BusReadProc proc);
		void SubscribeWrMem(int addrMask, int maskedValue, BusWriteProc proc);
		void SubscribeRdIo(int addrMask, int maskedValue, BusReadIoProc proc);
		void SubscribeWrIo(int addrMask, int maskedValue, BusWriteIoProc proc);
        void SubscribeRdNoMreq(int addrMask, int maskedValue, Action<ushort> proc);
        void SubscribeWrNoMreq(int addrMask, int maskedValue, Action<ushort> proc);
		void SubscribeReset(Action proc);
        void SubscribeNmiRq(BusRqProc proc);
        void SubscribeNmiAck(Action proc);
		void SubscribeIntAck(Action proc);
        void SubscribeScanInt(Action<int> handler);

		void SubscribePreCycle(Action proc);
		void SubscribeBeginFrame(Action handler);
		void SubscribeEndFrame(Action handler);

		void AddSerializer(IFormatSerializer serializer);
		void RegisterIcon(IIconDescriptor iconDesc);

        void AddCommandUi(ICommand command);

		CpuUnit CPU { get; }
		bool IsSandbox { get; }
		string GetSatelliteFileName(string extension);

        T FindDevice<T>() where T : class;

		RzxHandler RzxHandler { get; }

        void RequestNmi(int timeOut);
	}

	public delegate void BusReadProc(ushort addr, ref byte value);
	public delegate void BusWriteProc(ushort addr, byte value);
	public delegate void BusReadIoProc(ushort addr, ref byte value, ref bool handled);
	public delegate void BusWriteIoProc(ushort addr, byte value, ref bool handled);
    public delegate void BusRqProc(BusCancelArgs e);

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
