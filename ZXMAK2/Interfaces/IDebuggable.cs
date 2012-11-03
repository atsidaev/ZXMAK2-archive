using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Engine.Interfaces
{
    public interface IDebuggable
    {
        void DoReset();
        void DoStepInto();
        void DoStepOver();
        void DoRun();
        void DoStop();

        byte ReadMemory(ushort addr);
        void WriteMemory(ushort addr, byte value);
        void ReadMemory(ushort addr, byte[] data, int offset, int length);
        void WriteMemory(ushort addr, byte[] data, int offset, int length);

        void AddBreakpoint(ushort addr);
        void RemoveBreakpoint(ushort addr);
        ushort[] GetBreakpointList();
        bool CheckBreakpoint(ushort addr);
        void ClearBreakpoints();

        event EventHandler UpdateState;
        event EventHandler Breakpoint;
        bool IsRunning { get; }
        Z80CPU CPU { get; }
        int GetFrameTact();
		int FrameTactCount { get; }
	}
}
