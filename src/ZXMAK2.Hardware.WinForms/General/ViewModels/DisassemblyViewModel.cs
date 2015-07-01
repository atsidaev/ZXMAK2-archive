using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Mvvm;
using ZXMAK2.Engine.Cpu.Tools;

namespace ZXMAK2.Hardware.WinForms.General.ViewModels
{
    public class DisassemblyViewModel : BaseDebuggerViewModel
    {
        private readonly DasmTool _dasmTool;
        private readonly TimingTool _timingTool;


        public DisassemblyViewModel(IDebuggable target, ISynchronizeInvoke synchronizeInvoke)
            : base(target, synchronizeInvoke)
        {
            _dasmTool = new DasmTool(target.ReadMemory);
            _timingTool = new TimingTool(target.CPU, target.ReadMemory);
        }
        

        #region Properties

        private ushort? _activeAddress;
        
        public ushort? ActiveAddress
        {
            get { return _activeAddress; }
            set { PropertyChangeNul("ActiveAddress", ref _activeAddress, value); }
        }

        #endregion Properties


        #region Private

        protected override void OnTargetStateChanged()
        {
            base.OnTargetStateChanged();
            ActiveAddress = IsRunning ? (ushort?)null : Target.CPU.regs.PC;
        }

        public byte[] GetData(ushort addr, int len)
        {
            var data = new byte[len];
            Target.ReadMemory(addr, data, 0, len);
            return data;
        }

        public void GetDisassembly(ushort addr, out string dasm, out int len)
        {
            var mnemonic = _dasmTool.GetMnemonic(addr, out len);
            var timing = _timingTool.GetTimingString(addr);
            dasm = string.Format("{0,-24} ; {1}", mnemonic, timing);
        }

        #endregion Private
    }
}
