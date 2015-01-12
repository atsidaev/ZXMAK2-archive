using ZXMAK2.Engine.Cpu;


namespace ZXMAK2.Engine.Interfaces
{
	public interface IMachineState
	{
		CpuUnit CPU { get; }
		BusManager BusManager { get; }
		byte ReadMemory(ushort addr);
		void WriteMemory(ushort addr, byte value);
	}
}
