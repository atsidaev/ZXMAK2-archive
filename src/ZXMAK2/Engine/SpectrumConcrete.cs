using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine;
using ZXMAK2.Serializers;


namespace ZXMAK2.Engine
{
    public unsafe class SpectrumConcrete : SpectrumBase
    {
        #region private data

        private Z80CPU _cpu;
        private BusManager _bus;
        private LoadManager _loader;

        private List<ushort> _breakpoints = null;

        private int m_frameStartTact;

        #endregion

        public override BusManager BusManager { get { return _bus; } }

        public override Z80CPU CPU { get { return _cpu; } }
        public override LoadManager Loader { get { return _loader; } }

        public override int FrameStartTact { get { return m_frameStartTact; } }

        public SpectrumConcrete()
        {
            _loader = new LoadManager(this);
            _cpu = new Z80CPU();
            _bus = new BusManager();
        }

        public override void Init()
        {
            base.Init();
            _bus.Init(_cpu, _loader, false);
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
            _cpu.RST = true;
            _cpu.ExecCycle();
            _cpu.RST = false;
        }
        
        public override void Load(XmlNode busNode)
        {
            _bus.LoadConfig(busNode);
            _cpu.RST = true;
            _cpu.ExecCycle();
            _cpu.RST = false;
        }

        public override void Save(XmlNode busNode)
        {
            _bus.SaveConfig(busNode);
        }

        #region debugger methods

        public override byte ReadMemory(ushort addr)
        {
            IMemoryDevice memory = _bus.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
            return memory.RDMEM_DBG(addr);
        }

        public override void WriteMemory(ushort addr, byte value)
        {
            IMemoryDevice memory = _bus.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
            memory.WRMEM_DBG(addr, value);
            OnUpdateState();
        }

        public override void AddBreakpoint(ushort addr)
        {
            if (_breakpoints == null)
                _breakpoints = new List<ushort>();
            if (!_breakpoints.Contains(addr))
                _breakpoints.Add(addr);
        }

        public override void RemoveBreakpoint(ushort addr)
        {
            if (_breakpoints != null)
            {
                if (_breakpoints.Contains(addr))
                    _breakpoints.Remove(addr);
                if (_breakpoints.Count < 1)
                    _breakpoints = null;
            }
        }

        public override ushort[] GetBreakpointList()
        {
            if (_breakpoints == null)
                return new ushort[0];
            return _breakpoints.ToArray();
        }

        public override bool CheckBreakpoint(ushort addr)
        {
            if (_breakpoints != null)
                return _breakpoints.Contains(addr);
            return false;
        }

        public override void ClearBreakpoints()
        {
            if (_breakpoints != null)
                _breakpoints.Clear();
            _breakpoints = null;
        }

        #endregion


        public unsafe override void ExecuteFrame()
        {
			//System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            int frameTact = _bus.GetFrameTact();
            long t = _cpu.Tact - frameTact + _bus.FrameTactCount;

            while ((t - _cpu.Tact) > 0)
            {
                _bus.ExecCycle();
                if (_breakpoints != null && CheckBreakpoint(_cpu.regs.PC) && !_cpu.HALTED)
                {
                    long delta = t - _cpu.Tact;
                    if (delta <= 0)
                    {
                        m_frameStartTact = -(int)delta;
                    }
                    IsRunning = false;
                    OnUpdateFrame();
                    OnBreakpoint();
                    return;
                }
			}
            m_frameStartTact = (int)(_cpu.Tact - t);

            //stopwatch.Stop();
            //LogAgent.Info("{0}", stopwatch.ElapsedTicks);
        }

		protected override void OnExecCycle()
		{
            int frameTact = _bus.GetFrameTact();
            long t = _cpu.Tact - frameTact + _bus.FrameTactCount;
            _bus.ExecCycle();
            long delta = t - _cpu.Tact;
            if (delta <= 0)
            {
                m_frameStartTact = -(int)delta;
            }
            if (_breakpoints != null && CheckBreakpoint(_cpu.regs.PC) && !_cpu.HALTED)
            {
                IsRunning = false;
                OnUpdateFrame();
                OnBreakpoint();
                //break;
            }
		}
    }

}
