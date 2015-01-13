/// Description: Generic ZX Spectrum emulator
/// Author: Alex Makeev
/// Date: 18.03.2008
using System;
using System.Linq;
using System.Collections.Generic;

using ZXMAK2.Serializers;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine.Cpu.Tools;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Engine
{
    public class Spectrum : IMachineState, IDisposable
    {
        #region Fields

        private readonly BusManager _bus = new BusManager();
        private readonly List<Breakpoint> _breakpoints = new List<Breakpoint>();

        private long _tactLimitStepOver = 71680 * 5;
        private bool _isRunning = false;
        private int _frameStartTact;

        #endregion Fields


        #region .ctor

        public Spectrum()
        {
            _bus.FrameReady += OnUpdateFrame;
        }

        public void Dispose()
        {
        }

        #endregion .ctor


        #region Properties

        public event EventHandler Breakpoint;
        public event EventHandler UpdateState;
        public event EventHandler UpdateFrame;

        public CpuUnit CPU
        {
            get { return _bus.Cpu; }
        }

        public BusManager BusManager
        {
            get { return _bus; }
        }

        public int FrameStartTact
        {
            get { return _frameStartTact; }
        }

        public bool IsRunning
        {
            get { return _isRunning; }
            set { _isRunning = value; OnUpdateState(); }
        }

        #endregion Properties


        #region Public

        public void Init()
        {
            _tactLimitStepOver = 71680 * 50 * 5;
            _bus.Init(this, false);
            _bus.Cpu.RST = true;
            _bus.Cpu.ExecCycle();
            _bus.Cpu.RST = false;
            _bus.Cpu.Tact = 0;
        }

        public void RaiseUpdateState()
        {
            OnUpdateState();
        }

        public void ExecuteFrame()
        {
            var frameTact = _bus.GetFrameTact();
            var cpu = _bus.Cpu;
            var t = cpu.Tact - frameTact + _bus.FrameTactCount;

            while (t > cpu.Tact/* && IsRunning*/)
            {
                // Alex: performance critical block, do not modify!
                _bus.ExecCycle();
                if (_breakpoints.Count == 0 || cpu.HALTED)
                {
                    continue;
                }
                // Alex: end of performance critical block

                if (CheckBreakpoint())
                {
                    int delta1 = (int)(cpu.Tact - t);
                    if (delta1 >= 0)
                        _frameStartTact = delta1;
                    IsRunning = false;
                    OnUpdateFrame();
                    OnBreakpoint();
                    return;
                }
            }
            var delta = (int)(cpu.Tact - t);
            if (delta >= 0)
            {
                _frameStartTact = delta;
            }
        }

        #endregion Public


        #region Debugger

        public void DoReset()
        {
            CPU.RST = true;
            OnExecCycle();
            CPU.RST = false;
            OnUpdateState();
        }

        public void DoNmi()
        {
            BusManager.RequestNmi(BusManager.FrameTactCount * 50);
            OnUpdateState();
        }

        public void DoStepInto()
        {
            if (IsRunning)
            {
                return;
            }
            StepInto();
            OnUpdateState();
        }

        public void DoStepOver()
        {
            if (IsRunning)
            {
                return;
            }
            StepOver();
            OnUpdateState();
        }

        public byte ReadMemory(ushort addr)
        {
            var memory = _bus.FindDevice<IMemoryDevice>();
            if (memory == null)
            {
                return 0xFF;
            }
            return memory.RDMEM_DBG(addr);
        }

        public void WriteMemory(ushort addr, byte value)
        {
            var memory = _bus.FindDevice<IMemoryDevice>();
            if (memory == null)
            {
                return;
            }
            memory.WRMEM_DBG(addr, value);
            OnUpdateState();
        }

        public void AddBreakpoint(Breakpoint bp)
        {
            _breakpoints.Add(bp);
        }

        public void RemoveBreakpoint(Breakpoint bp)
        {
            _breakpoints.Remove(bp);
        }

        public Breakpoint[] GetBreakpointList()
        {
            return _breakpoints.ToArray();
        }

        public void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }

        #endregion Debugger


        #region Private

        private void OnBreakpoint()
        {
            if (Breakpoint != null)
                Breakpoint(this, new EventArgs());
        }

        private void OnUpdateState()
        {
            if (UpdateState != null)
                UpdateState(this, new EventArgs());
        }

        private void OnUpdateFrame()
        {
            if (UpdateFrame != null)
                UpdateFrame(this, new EventArgs());
        }

        private bool OnMaxTactExceed(long tactLimit)
        {
            var service = Locator.Resolve<IUserQuery>();
            if (service == null)
            {
                return true;
            }
            var msg = string.Format(
                "{0} tacts executed,\nbut operation not complete!\n\nAre you sure to continue?",
                tactLimit);
            return service.Show(
                msg,
                "Warning",
                DlgButtonSet.YesNo,
                DlgIcon.Question) != DlgResult.Yes;
        }

        private void OnExecCycle()
        {
            int frameTact = _bus.GetFrameTact();
            var cpu = _bus.Cpu;
            long t = cpu.Tact - frameTact + _bus.FrameTactCount;
            _bus.ExecCycle();
            int delta = (int)(cpu.Tact - t);
            if (delta >= 0)
                _frameStartTact = delta;
            if (CheckBreakpoint())
            {
                IsRunning = false;
                OnUpdateFrame();
                OnBreakpoint();
            }
        }

        private void StepInto()
        {
            do
            {
                OnExecCycle();
            } while (CPU.FX != OPFX.NONE || CPU.XFX != OPXFX.NONE);
        }

        private void StepOver()
        {
            var tactLimit = _tactLimitStepOver;
            var t = CPU.Tact;
            var dasmTool = new DasmTool(ReadMemory);
            int len;
            var opCodeStr = dasmTool.GetMnemonic(CPU.regs.PC, out len);
            var nextAddr = (ushort)((CPU.regs.PC + len) & 0xFFFF);

            var donotStepOver = opCodeStr.IndexOf("J") >= 0 ||
                opCodeStr.IndexOf("RET") >= 0;

            if (donotStepOver)
            {
                StepInto();
            }
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

        private bool CheckBreakpoint()
        {
            return _breakpoints
                .Any(bp => bp.Check(this));
        }

        #endregion Private
    }
}
