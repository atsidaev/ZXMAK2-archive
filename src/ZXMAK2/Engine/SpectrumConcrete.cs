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
using ZXMAK2.Controls.Debugger;

namespace ZXMAK2.Engine
{
	public unsafe class SpectrumConcrete : SpectrumBase
	{
		#region private data

		private Z80CPU _cpu;
		private BusManager _bus;
		private LoadManager _loader;

		private List<ushort> _breakpoints = null;

        //conditional breakpoints
        private DictionarySafe<byte, breakpointInfo> _breakpointsExt = null;

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

        #region 2.) Extended breakpoints(conditional on memory change, write, registry change, ...)
        public override void AddExtBreakpoint(List<string> newBreakpointDesc)
        {
            if (_breakpointsExt == null)
                _breakpointsExt = new DictionarySafe<byte, breakpointInfo>();

            breakpointInfo breakpointInfo = new breakpointInfo();

            //1.LEFT condition
            bool leftIsMemoryReference = false;

            string left = newBreakpointDesc[1];
            if (FormCpu.isMemoryReference(left))
            {
                breakpointInfo.leftCondition = left.ToUpper();

                // it can be memory reference by registry value, e.g.: (PC), (DE), ...
                if (FormCpu.isRegistryMemoryReference(left))
                    breakpointInfo.leftValue = FormCpu.getRegistryValueByName(_cpu.regs, FormCpu.getRegistryFromReference(left));
                else
                    breakpointInfo.leftValue = FormCpu.getReferencedMemoryPointer(left);

                leftIsMemoryReference = true;
            }
            else
            {
                //must be a registry
                if (!FormCpu.isRegistry(left))
                    throw new Exception("incorrect breakpoint(left condition)");

                breakpointInfo.leftCondition = left.ToUpper();
            }

            //2.CONDITION type
            breakpointInfo.conditionTypeSign = newBreakpointDesc[2]; // ==, !=, <, >, ...

            //3.RIGHT condition
            byte rightType = 0xFF; // 0 - memory reference, 1 - registry value, 2 - common value

            string right = newBreakpointDesc[3];
            if (FormCpu.isMemoryReference(right))
            {
                breakpointInfo.rightCondition = right.ToUpper(); // because of breakpoint panel
                breakpointInfo.rightValue = ReadMemory(FormCpu.getReferencedMemoryPointer(right));

                rightType = 0;
            }
            else
            {
                if (FormCpu.isRegistry(right))
                {
                    breakpointInfo.rightCondition = right;

                    rightType = 1;
                }
                else
                {
                    //it has to be a common value, e.g.: #4000, %111010101, ...
                    breakpointInfo.rightCondition = right.ToUpper(); // because of breakpoint panel
                    breakpointInfo.rightValue = FormCpu.convertNumberWithPrefix(right); // last chance

                    rightType = 2;
                }
            }

            if (rightType == 0xFF)
                throw new Exception("incorrect right condition");

            //4. finish
            if (leftIsMemoryReference)
            {
                if (FormCpu.isRegistryMemoryReference(breakpointInfo.leftCondition)) // left condition is e.g.: (PC), (HL), (DE), ...
                {
                    if (rightType == 2) // right is number
                        breakpointInfo.accessType = BreakPointConditionType.registryMemoryReferenceVsValue;
                }
            }
            else
            {
                if (rightType == 2)
                    breakpointInfo.accessType = BreakPointConditionType.registryVsValue;
            }

            breakpointInfo.isOn = true; // activate the breakpoint

            //save breakpoint command line string
            breakpointInfo.breakpointString = String.Empty;
            for (byte counter = 0; counter < newBreakpointDesc.Count; counter++)
            {
                breakpointInfo.breakpointString += newBreakpointDesc[counter];
                if (counter+1 < newBreakpointDesc.Count)
                    breakpointInfo.breakpointString += " ";
            }

            // ADD breakpoint into list
            // Here will be the breakpoint key assigned by searching keys starting with key 0
            // Maximum 255 breakpoints is allowed
            for (byte counter = 0; counter < 0xFF; counter++)
            {
                if (!_breakpointsExt.ContainsKey(counter))
                {
                    _breakpointsExt.Add(counter, breakpointInfo);
                    return;
                }
            }

            throw new Exception("Maximum breakpoints count(255) exceeded...");
        }
        public override void RemoveExtBreakpoint(byte breakpointNrToRemove)
        {
            _breakpointsExt.Remove(breakpointNrToRemove);
        }
        public override DictionarySafe<byte, breakpointInfo> GetExtBreakpointsList()
        {
            if (_breakpointsExt != null)
                return _breakpointsExt;

            _breakpointsExt = new DictionarySafe<byte, breakpointInfo>();

            return _breakpointsExt;
        }
        public override bool CheckExtBreakpoints()
        {
            if (_breakpointsExt == null || _breakpointsExt.Count == 0)
                return false;

            lock (_breakpointsExt)
            {
                foreach (KeyValuePair<byte, breakpointInfo> breakpoint in _breakpointsExt)
                {
                    if (!breakpoint.Value.isOn)
                        continue;

                    ushort leftValue = 0;
                    ushort rightValue = 0;

                    switch (breakpoint.Value.accessType)
                    {
                        // e.g.: PC == #9C40
                        case BreakPointConditionType.registryVsValue:
                            leftValue = FormCpu.getRegistryValueByName(_cpu.regs, breakpoint.Value.leftCondition);
                            rightValue = breakpoint.Value.rightValue;
                            break;
                        // e.g.: (#9C40) != #2222
                        case BreakPointConditionType.memoryVsValue:
                            leftValue = ReadMemory(breakpoint.Value.leftValue);
                            rightValue = breakpoint.Value.rightValue;
                            break;
                        // e.g.: (PC) == #D1 - instruction breakpoint
                        case BreakPointConditionType.registryMemoryReferenceVsValue:
                            leftValue = ReadMemory(FormCpu.getRegistryValueByName(_cpu.regs, FormCpu.getRegistryFromReference(breakpoint.Value.leftCondition)));
                            rightValue = breakpoint.Value.rightValue;
                            if (rightValue > 0xFF) //check on 2 bytes right condition, e.g.: (PC) == #5EED
                            {
                                int hiByte = FormCpu.getRegistryValueByName(_cpu.regs, FormCpu.getRegistryFromReference(breakpoint.Value.leftCondition)) + 1;
                                if (hiByte > 0xFFFF)
                                    hiByte = 0;
                                leftValue += Convert.ToUInt16(ReadMemory(Convert.ToUInt16(hiByte)) * 256);
                            }
                            break;
                        default:
                            break;
                    }

                    //condition
                    if (breakpoint.Value.conditionTypeSign == "==") // is equal
                    {
                        if (leftValue == rightValue)
                            return true;
                    }
                    else if (breakpoint.Value.conditionTypeSign == "!=") // is not equal
                    {
                        if (leftValue != rightValue)
                            return true;
                    };
                }
            }

            return false;
        }

        public override void EnableOrDisableBreakpointStatus(byte whichBpToEnableOrDisable, bool setOn) //enables/disables breakpoint, command "on" or "off"
        {
            if (_breakpointsExt == null || _breakpointsExt.Count == 0)
                return;

            if( !_breakpointsExt.ContainsKey(whichBpToEnableOrDisable) )
                return;

            breakpointInfo tempbreakpointInfo = (breakpointInfo)_breakpointsExt[whichBpToEnableOrDisable];
            tempbreakpointInfo.isOn = setOn;
            _breakpointsExt[whichBpToEnableOrDisable] = tempbreakpointInfo;

            return;
        }

        // clears all conditional breakpoints
        public override void ClearExtBreakpoints()
        {
            lock (_breakpointsExt)
                _breakpointsExt.Clear();
        }

        public override void LoadBreakpointsListFromFile(string fileName)
        {
            System.IO.StreamReader file = null;

            try
            {
                if (!File.Exists(fileName))
                    throw new Exception("file " + fileName + " does not exists...");

                string dbgCommandFromFile = String.Empty;
                file = new System.IO.StreamReader(fileName);
                while ((dbgCommandFromFile = file.ReadLine()) != null)
                {
                    if (dbgCommandFromFile.Trim() == String.Empty || dbgCommandFromFile[0] == ';')
                        continue;

                    List<string> parsedCommand = FormCpu.ParseCommand(dbgCommandFromFile);
                    if (parsedCommand == null)
                        throw new Exception("unknown debugger command");

                    AddExtBreakpoint(parsedCommand);
                }
            }
            finally
            {
                file.Close();
            }
        }
        public override void SaveBreakpointsListToFile(string fileName)
        {
            DictionarySafe<byte, breakpointInfo> localBreakpointsList = GetExtBreakpointsList();
            if (localBreakpointsList.Count == 0)
                return;

            System.IO.StreamWriter file = null;
            try
            {
                file = new System.IO.StreamWriter(fileName);

                foreach (KeyValuePair<byte, breakpointInfo> breakpoint in localBreakpointsList)
                {
                    file.WriteLine(breakpoint.Value.breakpointString);
                }
            }
            finally
            {
                file.Close();
            }
        }
        #endregion

		public unsafe override void ExecuteFrame()
		{
			//System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			//stopwatch.Start();

			int frameTact = _bus.GetFrameTact();
			long t = _cpu.Tact - frameTact + _bus.FrameTactCount;

			while (t > _cpu.Tact/* && IsRunning*/)
			{
				_bus.ExecCycle();
				if (
                    ( CheckBreakpoint(_cpu.regs.PC) || CheckExtBreakpoints() )  &&
                    !_cpu.HALTED
                   )
				{
					int delta1 = (int)(_cpu.Tact - t);
					if (delta1 >= 0)
						m_frameStartTact = delta1;
					IsRunning = false;
					OnUpdateFrame();
					OnBreakpoint();
					return;
				}
			}
			int delta = (int)(_cpu.Tact - t);
			if (delta >= 0)
				m_frameStartTact = delta;

			//stopwatch.Stop();
			//LogAgent.Info("{0}", stopwatch.ElapsedTicks);
		}

		protected override void OnExecCycle()
		{
			int frameTact = _bus.GetFrameTact();
			long t = _cpu.Tact - frameTact + _bus.FrameTactCount;
			_bus.ExecCycle();
			int delta = (int)(_cpu.Tact - t);
			if (delta >= 0)
				m_frameStartTact = delta;
			if (_breakpoints != null && CheckBreakpoint(_cpu.regs.PC) && !_cpu.HALTED)
			{
				IsRunning = false;
				OnUpdateFrame();
				OnBreakpoint();
			}
		}
	}

}
