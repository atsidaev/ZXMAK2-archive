/// Description: Generic ZX Spectrum emulator
/// Author: Alex Makeev
/// Date: 18.03.2008
using System;
using System.Xml;

using ZXMAK2.Engine.Z80;
using ZXMAK2.Serializers;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Controls.Debugger;
using System.Collections.Generic;

namespace ZXMAK2.Engine
{
	public abstract class SpectrumBase : IDisposable
	{
		#region private

		private bool m_isRunning = false;

		private long m_tactLimitStepOver = 71680*5;

		#endregion

		#region protected fields

		protected void OnBreakpoint()
		{
			if (Breakpoint != null)
				Breakpoint(this, new EventArgs());
		}

		protected void OnUpdateState()
		{
			if (UpdateState != null)
				UpdateState(this, new EventArgs());
		}

		protected void OnUpdateFrame()
		{
			if (UpdateFrame != null)
				UpdateFrame(this, new EventArgs());
		}

		protected bool OnMaxTactExceed(long tactLimit)
		{
            string msg = string.Format(
                "{0} tacts executed,\nbut operation not complete!\n\nAre you sure to continue?", 
                tactLimit);
            return DialogProvider.Show(
                msg, 
                "Warning", 
                DlgButtonSet.YesNo, 
                DlgIcon.Question) != DlgResult.Yes;
		}

		protected abstract void OnExecCycle();

		protected virtual void Reset()
		{
            CPU.RST = true;
            OnExecCycle();
            CPU.RST = false;
		}

        protected void Nmi()
        {
            CPU.NMI = true;
            OnExecCycle();
            CPU.NMI = false;
        }


		protected virtual void StepInto()
		{
			do
			{
                OnExecCycle();
            } while (CPU.FX != OPFX.NONE || CPU.XFX != OPXFX.NONE);
		}

		protected virtual void StepOver()
		{
			long tactLimit = m_tactLimitStepOver;
			long t = CPU.Tact;
			int len;
			string op = Z80CPU.GetMnemonic(ReadMemory, CPU.regs.PC, true, out len);
			ushort nextAddr = (ushort)((CPU.regs.PC + len) & 0xFFFF);

			bool donotStepOver = (op.IndexOf("J") >= 0) || (op.IndexOf("RET") >= 0);

			if (donotStepOver)
				StepInto();
			else
			{
				while (true)
				{
					if (CPU.Tact - t >= tactLimit)
					{
						OnUpdateFrame();
						if (OnMaxTactExceed(tactLimit))
							break;
						else
						{
							t = CPU.Tact;
							tactLimit *= 2;
						}
					}

					StepInto();
					if (CPU.regs.PC == nextAddr)
						break;

					if (CheckBreakpoint(CPU.regs.PC))
					{
						OnUpdateFrame();
						OnBreakpoint();
						break;
					}
				}
			}
		}

		#endregion

		#region public fields

		public abstract Z80CPU CPU { get; }
        public abstract BusManager BusManager { get; }
        public abstract LoadManager Loader { get; }

        public abstract int FrameStartTact { get; }
		
		public virtual bool IsRunning 
		{
			get { return m_isRunning; }
			set { m_isRunning = value; OnUpdateState(); }
		}

        public event EventHandler Breakpoint;
		public event EventHandler UpdateState;
		public event EventHandler UpdateFrame;

		public virtual void Init()
		{
			m_tactLimitStepOver = 71680 * 50 * 5;			
		}

        public abstract void Load(XmlNode busNode);
        public abstract void Save(XmlNode busNode);


		public virtual void Dispose() 
		{ 
		}

		#region debugger specific

        public abstract byte ReadMemory(ushort addr);
        public abstract void WriteMemory(ushort addr, byte value);

		public void DoReset()
		{
			Reset(); 
			OnUpdateState();
		}

        public void DoNmi()
        {
            Nmi();
            OnUpdateState();
        }

		public void DoStepInto()
		{
			if (IsRunning)
				return;
			StepInto();
			OnUpdateState();
		}

		public void DoStepOver()
		{
			if (IsRunning)
				return;
			StepOver();
			OnUpdateState();
		}

		public abstract void AddBreakpoint(ushort addr);
		public abstract void RemoveBreakpoint(ushort addr);
		public abstract ushort[] GetBreakpointList();
		public abstract bool CheckBreakpoint(ushort addr);
		public abstract void ClearBreakpoints();

        //conditional breakpoints
        public abstract void AddExtBreakpoint(List<string> breakListDesc);
        public abstract void RemoveExtBreakpoint(byte addr);
        public abstract Dictionary<byte, breakpointInfo> GetExtBreakpointsList();
        public abstract bool CheckExtBreakpoints();
        public abstract void EnableOrDisableBreakpointStatus(byte whichBpToEnableOrDisable, bool setOn); //enables/disables breakpoint, command "on" or "off"
        public abstract void ClearExtBreakpoints(int whichBpToClear); // if -1 => all breakpoints clear

		#endregion

        public unsafe abstract void ExecuteFrame();

        #endregion

        public void RaiseUpdateState()
        {
            OnUpdateState();
        }
    }
}
