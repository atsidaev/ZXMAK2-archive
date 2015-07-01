using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Mvvm;
using ZXMAK2.Mvvm.Attributes;

namespace ZXMAK2.Hardware.WinForms.General.ViewModels
{
    public class RegistersViewModel : BaseDebuggerViewModel
    {
        private static readonly List<string> _regNames = new[]
        {
            "Pc", "Sp", "Ir", "Im", "Wz", "Lpc", 
            "Af", "Af_", "Hl", "Hl_", "De", "De_", "Bc", "Bc_", "Ix", "Iy",
            "Iff1", "Iff2", "Halted", "Bint",
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
            set { PropertyChangeVal("Pc", ref Target.CPU.regs.PC, value); }
        }

        public ushort Sp
        {
            get { return Target.CPU.regs.SP; }
            set { PropertyChangeVal("Sp", ref Target.CPU.regs.SP, value); }
        }

        public ushort Ir
        {
            get { return Target.CPU.regs.IR; }
            set { PropertyChangeVal("Ir", ref Target.CPU.regs.IR, value); }
        }

        public int Im
        {
            get { return Target.CPU.IM; }
            set { PropertyChangeVal("Im", ref Target.CPU.IM, (byte)(value % 3)); }
        }

        public ushort Wz
        {
            get { return Target.CPU.regs.MW; }
            set { PropertyChangeVal("Wz", ref Target.CPU.regs.MW, value); }
        }

        public ushort Lpc
        {
            get { return Target.CPU.LPC; }
        }

        public ushort Af
        {
            get { return Target.CPU.regs.AF; }
            set { PropertyChangeVal("Af", ref Target.CPU.regs.AF, value); }
        }

        public ushort Af_
        {
            get { return Target.CPU.regs._AF; }
            set { PropertyChangeVal("Af_", ref Target.CPU.regs._AF, value); }
        }

        public ushort Hl
        {
            get { return Target.CPU.regs.HL; }
            set { PropertyChangeVal("Hl", ref Target.CPU.regs.HL, value); }
        }

        public ushort Hl_
        {
            get { return Target.CPU.regs._HL; }
            set { PropertyChangeVal("Hl_", ref Target.CPU.regs._HL, value); }
        }

        public ushort De
        {
            get { return Target.CPU.regs.DE; }
            set { PropertyChangeVal("De", ref Target.CPU.regs.DE, value); }
        }

        public ushort De_
        {
            get { return Target.CPU.regs._DE; }
            set { PropertyChangeVal("De_", ref Target.CPU.regs._DE, value); }
        }

        public ushort Bc
        {
            get { return Target.CPU.regs.BC; }
            set { PropertyChangeVal("Bc", ref Target.CPU.regs.BC, value); }
        }

        public ushort Bc_
        {
            get { return Target.CPU.regs._BC; }
            set { PropertyChangeVal("Bc_", ref Target.CPU.regs._BC, value); }
        }

        public ushort Ix
        {
            get { return Target.CPU.regs.IX; }
            set { PropertyChangeVal("Ix", ref Target.CPU.regs.IX, value); }
        }

        public ushort Iy
        {
            get { return Target.CPU.regs.IY; }
            set { PropertyChangeVal("Iy", ref Target.CPU.regs.IY, value); }
        }

        public bool Iff1
        {
            get { return Target.CPU.IFF1; }
            set { PropertyChangeVal("Iff1", ref Target.CPU.IFF1, value); }
        }

        public bool Iff2
        {
            get { return Target.CPU.IFF2; }
            set { PropertyChangeVal("Iff2", ref Target.CPU.IFF2, value); }
        }

        public bool Halt
        {
            get { return Target.CPU.HALTED; }
            set { PropertyChangeVal("Halt", ref Target.CPU.HALTED, value); }
        }

        public bool Bint
        {
            get { return Target.CPU.BINT; }
            set { PropertyChangeVal("Bint", ref Target.CPU.BINT, value); }
        }

        #region Flags

        [DependsOnProperty("Af")]
        public bool FlagS
        {
            get { return (Af & 0x80) != 0; }
            set { Af = (ushort)((Af & ~0x80) | (value ? 0x80 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool FlagZ
        {
            get { return (Af & 0x40) != 0; }
            set { Af = (ushort)((Af & ~0x40) | (value ? 0x40 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool Flag5
        {
            get { return (Af & 0x20) != 0; }
            set { Af = (ushort)((Af & ~0x20) | (value ? 0x20 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool FlagH
        {
            get { return (Af & 0x10) != 0; }
            set { Af = (ushort)((Af & ~0x10) | (value ? 0x10 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool Flag3
        {
            get { return (Af & 0x08) != 0; }
            set { Af = (ushort)((Af & ~0x08) | (value ? 0x08 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool FlagV
        {
            get { return (Af & 0x04) != 0; }
            set { Af = (ushort)((Af & ~0x04) | (value ? 0x04 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool FlagN
        {
            get { return (Af & 0x02) != 0; }
            set { Af = (ushort)((Af & ~0x02) | (value ? 0x02 : 0)); }
        }

        [DependsOnProperty("Af")]
        public bool FlagC
        {
            get { return (Af & 0x01) != 0; }
            set { Af = (ushort)((Af & ~0x01) | (value ? 0x01 : 0)); }
        }

        #endregion Flags

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
