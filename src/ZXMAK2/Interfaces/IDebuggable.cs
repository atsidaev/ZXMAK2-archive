using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Controls.Debugger;

namespace ZXMAK2.Interfaces
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

        void AddExtBreakpoint(List<string> breakListDesc);
        void RemoveExtBreakpoint(byte addr);
        DictionarySafe<byte, breakpointInfo> GetExtBreakpointsList();
        bool CheckExtBreakpoints();
        void EnableOrDisableBreakpointStatus(byte whichBpToEnableOrDisable, bool setOn); //enables/disables breakpoint, command "on" or "off"
        void ClearExtBreakpoints();
        void LoadBreakpointsListFromFile(string fileName);
        void SaveBreakpointsListToFile(string fileName);


		event EventHandler UpdateState;
		event EventHandler Breakpoint;
		bool IsRunning { get; }
		Z80CPU CPU { get; }
		int GetFrameTact();
		int FrameTactCount { get; }

		IRzxState RzxState { get; }
	}
}
