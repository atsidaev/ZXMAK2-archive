using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine;

namespace ZXMAK2.Interfaces
{
	public interface IMachineState
	{
		CpuUnit CPU { get; }
		BusManager BusManager { get; }
		byte ReadMemory(ushort addr);
		void WriteMemory(ushort addr, byte value);
	}
}
