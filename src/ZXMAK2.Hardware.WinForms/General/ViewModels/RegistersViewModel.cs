using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Mvvm;

namespace ZXMAK2.Hardware.WinForms.General
{
    public class RegistersViewModel : BaseDebuggerViewModel
    {
        private static readonly List<string> _regNames = new[]
        {
            "Pc", "Sp", "Ir", "Im", "Wz", "Lpc", "Iff1", "Iff2", "Halted", "Bint",
            "Af", "Af_", "Hl", "Hl_", "De", "De_", "Bc", "Bc_", "Ix", "Iy",
            "FlagS", "FlagZ", "Flag5", "FlagH", "Flag3", "FlagV", "FlagN", "FlagC",
        }.ToList();

        
        public RegistersViewModel(IDebuggable target, ISynchronizeInvoke synchronizeInvoke)
            : base (target, synchronizeInvoke)
        {
        }

        public event EventHandler TargetStateChanged;

        
        #region Properties

        public ushort Pc
        {
            get { return Target.CPU.regs.PC; }
        }

        public ushort Sp
        {
            get { return Target.CPU.regs.SP; }
        }

        public ushort Ir
        {
            get { return Target.CPU.regs.IR; }
        }

        public int Im
        {
            get { return Target.CPU.IM; }
        }

        public ushort Wz
        {
            get { return Target.CPU.regs.MW; }
        }

        public ushort Lpc
        {
            get { return Target.CPU.LPC; }
        }

        public bool Iff1
        {
            get { return Target.CPU.IFF1; }
        }

        public bool Iff2
        {
            get { return Target.CPU.IFF2; }
        }

        public bool Halted
        {
            get { return Target.CPU.HALTED; }
        }

        public bool Bint
        {
            get { return Target.CPU.BINT; }
        }

        public ushort Af
        {
            get { return Target.CPU.regs.AF; }
        }

        public ushort Af_
        {
            get { return Target.CPU.regs._AF; }
        }

        public ushort Hl
        {
            get { return Target.CPU.regs.HL; }
        }

        public ushort Hl_
        {
            get { return Target.CPU.regs._HL; }
        }

        public ushort De
        {
            get { return Target.CPU.regs.DE; }
        }

        public ushort De_
        {
            get { return Target.CPU.regs._DE; }
        }

        public ushort Bc
        {
            get { return Target.CPU.regs.BC; }
        }

        public ushort Bc_
        {
            get { return Target.CPU.regs._BC; }
        }

        public ushort Ix
        {
            get { return Target.CPU.regs.IX; }
        }

        public ushort Iy
        {
            get { return Target.CPU.regs.IY; }
        }

        public bool FlagS
        {
            get { return (Af & 0x80) != 0; }
        }

        public bool FlagZ
        {
            get { return (Af & 0x40) != 0; }
        }

        public bool Flag5
        {
            get { return (Af & 0x20) != 0; }
        }

        public bool FlagH
        {
            get { return (Af & 0x10) != 0; }
        }

        public bool Flag3
        {
            get { return (Af & 0x08) != 0; }
        }

        public bool FlagV
        {
            get { return (Af & 0x04) != 0; }
        }

        public bool FlagN
        {
            get { return (Af & 0x02) != 0; }
        }

        public bool FlagC
        {
            get { return (Af & 0x01) != 0; }
        }

        #endregion Properties


        #region Private

        protected override void OnTargetStateChanged()
        {
            base.OnTargetStateChanged();
            _regNames.ForEach(OnPropertyChanged);
            
            var handler = TargetStateChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion Private
    }
}
