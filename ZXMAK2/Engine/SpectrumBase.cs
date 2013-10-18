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
using ZXMAK2.Entities;

namespace ZXMAK2.Engine
{
    public abstract class SpectrumBase : IMachineState, IDisposable
    {
        #region private

        private bool m_isRunning = false;

        private long m_tactLimitStepOver = 71680 * 5;

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
            BusManager.RequestNmi(BusManager.FrameTactCount * 50);
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

                    if (CheckBreakpoint())
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

        public virtual void Dispose()
        {
        }

        #region debugger specific

        public abstract ushort ReadMemory16bit(ushort addr);
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

        public abstract void AddBreakpoint(Breakpoint bp);
        public abstract void RemoveBreakpoint(Breakpoint bp);
        public abstract Breakpoint[] GetBreakpointList();
        public abstract void ClearBreakpoints();
        protected abstract bool CheckBreakpoint();

        #endregion

        public unsafe abstract void ExecuteFrame();

        #endregion

        public void RaiseUpdateState()
        {
            OnUpdateState();
        }
    }
}
