using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine.Bus;
using ZXMAK2.Engine.Serializers;


namespace ZXMAK2.Engine
{
    public unsafe class SpectrumConcrete : SpectrumBase
    {
        #region private data

        private Z80CPU _cpu;
        private BusManager _bus;
        private LoadManager _loader;

        private List<ushort> _breakpoints = null;

        #endregion

        public override BusManager BusManager { get { return _bus; } }


        public override Z80CPU CPU { get { return _cpu; } }
        public override LoadManager Loader { get { return _loader; } }

        public override void Init()
        {
            base.Init();
            _loader = new LoadManager(this);
            _cpu = new Z80CPU();
            _bus = new BusManager();
            _bus.Init(_cpu, _loader, false);
            _bus.FrameReady += OnUpdateFrame;
            //default devices...
			_bus.Add(new ZXMAK2.Engine.Devices.Memory.MemoryPentagon128());
			_bus.Add(new ZXMAK2.Engine.Devices.Ula.UlaPentagon());
            _bus.Add(new ZXMAK2.Engine.Devices.Disk.BetaDiskInterface());
            _bus.Add(new ZXMAK2.Engine.Devices.AY8910());
            _bus.Add(new ZXMAK2.Engine.Devices.BeeperDevice());
            _bus.Add(new ZXMAK2.Engine.Devices.TapeDevice());
            _bus.Add(new ZXMAK2.Engine.Devices.KeyboardDevice());
            _bus.Add(new ZXMAK2.Engine.Devices.KempstonMouseDevice());
            _bus.Add(new ZXMAK2.Engine.Devices.AyMouseDevice());
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
            if (!IsRunning)
                return;

			//System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            //stopwatch.Start();

            int frameTact = _bus.GetFrameTact();
            long t = _cpu.Tact - frameTact + _bus.FrameTactCount;

            while (IsRunning && (t - _cpu.Tact) > 0)
            {
                OnExecCycle();
			}

            //stopwatch.Stop();
            //LogAgent.Info("{0}", stopwatch.ElapsedTicks);
        }

		protected override void OnExecCycle()
		{
            _bus.ExecCycle();
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
