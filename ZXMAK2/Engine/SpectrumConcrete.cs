using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Engine;
using ZXMAK2.Serializers;
using ZXMAK2.Entities;

namespace ZXMAK2.Engine
{
    public unsafe class SpectrumConcrete : SpectrumBase
    {
        #region private data

        private BusManager _bus;

        private List<Breakpoint> _breakpoints = new List<Breakpoint>();

        private int m_frameStartTact;

        #endregion

        public override BusManager BusManager 
        { 
            get { return _bus; } 
        }

        public override CpuUnit CPU 
        { 
            get { return _bus.Cpu; } 
        }
        
        public override int FrameStartTact 
        { 
            get { return m_frameStartTact; } 
        }

        public SpectrumConcrete()
        {
            _bus = new BusManager();
        }

        public override void Init()
        {
            base.Init();
            _bus.Init(this, false);
            _bus.FrameReady += OnUpdateFrame;
            
            //default devices...
            _bus.Add(new ZXMAK2.Hardware.Pentagon.MemoryPentagon128());
            _bus.Add(new ZXMAK2.Hardware.Pentagon.UlaPentagon());
            _bus.Add(new ZXMAK2.Hardware.General.BetaDiskInterface());
            _bus.Add(new ZXMAK2.Hardware.General.AY8910());
            _bus.Add(new ZXMAK2.Hardware.General.BeeperDevice());
            _bus.Add(new ZXMAK2.Hardware.General.TapeDevice());
            _bus.Add(new ZXMAK2.Hardware.General.KeyboardDevice());
            _bus.Add(new ZXMAK2.Hardware.General.KempstonMouseDevice());
            _bus.Add(new ZXMAK2.Hardware.General.AyMouseDevice());
            _bus.Add(new ZXMAK2.Hardware.General.Debugger());
            _bus.Connect();

            _bus.Cpu.RST = true;
            _bus.Cpu.ExecCycle();
            _bus.Cpu.RST = false;
            _bus.Cpu.Tact = 0;
        }

        #region debugger methods

        public override byte ReadMemory(ushort addr)
        {
            var memory = _bus.FindDevice<IMemoryDevice>();
            return memory.RDMEM_DBG(addr);
        }

        public override void WriteMemory(ushort addr, byte value)
        {
            var memory = _bus.FindDevice<IMemoryDevice>();
            memory.WRMEM_DBG(addr, value);
            OnUpdateState();
        }

        public override void AddBreakpoint(Breakpoint bp)
        {
            _breakpoints.Add(bp);
        }

        public override void RemoveBreakpoint(Breakpoint bp)
        {
            _breakpoints.Remove(bp);
        }

        public override Breakpoint[] GetBreakpointList()
        {
            return _breakpoints.ToArray();
        }

        public override void ClearBreakpoints()
        {
            _breakpoints.Clear();
        }

        protected override bool CheckBreakpoint()
        {
            foreach (Breakpoint bp in _breakpoints)
                if (bp.Check(this))
                    return true;
            return false;
        }

        #endregion

        public unsafe override void ExecuteFrame()
        {
            //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            int frameTact = _bus.GetFrameTact();
            var cpu = _bus.Cpu;
            long t = cpu.Tact - frameTact + _bus.FrameTactCount;

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
                        m_frameStartTact = delta1;
                    IsRunning = false;
                    OnUpdateFrame();
                    OnBreakpoint();
                    return;
                }
            }
            int delta = (int)(cpu.Tact - t);
            if (delta >= 0)
                m_frameStartTact = delta;

            //stopwatch.Stop();
            //LogAgent.Info("{0}", stopwatch.ElapsedTicks);
        }

        protected override void OnExecCycle()
        {
            int frameTact = _bus.GetFrameTact();
            var cpu = _bus.Cpu;
            long t = cpu.Tact - frameTact + _bus.FrameTactCount;
            _bus.ExecCycle();
            int delta = (int)(cpu.Tact - t);
            if (delta >= 0)
                m_frameStartTact = delta;
            if (CheckBreakpoint())
            {
                IsRunning = false;
                OnUpdateFrame();
                OnBreakpoint();
            }
        }
    }

}
