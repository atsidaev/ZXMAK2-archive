/// Description: Z80 CPU Emulator [registers part]
/// Author: Alex Makeev
/// Date: 18.03.2007
using System;
using System.Runtime.InteropServices;
using ZXMAK2.Engine.Cpu.Processor;


namespace ZXMAK2.Engine.Cpu
{
    [StructLayout(LayoutKind.Explicit)]
    public class REGS
    {
        #region storage

        [FieldOffset(0)]
        public ushort AF = 0;
        [FieldOffset(2)]
        public ushort BC = 0;
        [FieldOffset(4)]
        public ushort DE = 0;
        [FieldOffset(6)]
        public ushort HL = 0;
        [FieldOffset(8)]
        public ushort _AF = 0;
        [FieldOffset(10)]
        public ushort _BC = 0;
        [FieldOffset(12)]
        public ushort _DE = 0;
        [FieldOffset(14)]
        public ushort _HL = 0;
        [FieldOffset(16)]
        public ushort IX = 0;
        [FieldOffset(18)]
        public ushort IY = 0;
        [FieldOffset(20)]
        public ushort IR = 0;
        [FieldOffset(22)]
        public ushort PC = 0;
        [FieldOffset(24)]
        public ushort SP = 0;
        [FieldOffset(26)]
        public ushort MW = 0;    // MEMPTR


        [FieldOffset(1)]
        public byte A;
        [FieldOffset(0)]
        public byte F;
        [FieldOffset(3)]
        public byte B;
        [FieldOffset(2)]
        public byte C;
        [FieldOffset(5)]
        public byte D;
        [FieldOffset(4)]
        public byte E;
        [FieldOffset(7)]
        public byte H;
        [FieldOffset(6)]
        public byte L;
        [FieldOffset(17)]
        public byte XH;
        [FieldOffset(16)]
        public byte XL;
        [FieldOffset(19)]
        public byte YH;
        [FieldOffset(18)]
        public byte YL;
        [FieldOffset(21)]
        public byte I;
        [FieldOffset(20)]
        public byte R;

        [FieldOffset(27)]
        public byte MH;
        [FieldOffset(26)]
        public byte ML;
        #endregion

        #region accessors

        internal void EXX()
        {
            ushort tmp = BC;
            BC = _BC;
            _BC = tmp;
            tmp = DE;
            DE = _DE;
            _DE = tmp;
            tmp = HL;
            HL = _HL;
            _HL = tmp;
        }
        
        internal void EXAF()
        {
            ushort tmp = AF;
            AF = _AF;
            _AF = tmp;
        }

        internal byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case CpuRegId.ZR_B: return B;
                    case CpuRegId.ZR_C: return C;
                    case CpuRegId.ZR_D: return D;
                    case CpuRegId.ZR_E: return E;
                    case CpuRegId.ZR_H: return H;
                    case CpuRegId.ZR_L: return L;
                    case CpuRegId.ZR_A: return A;
                    case CpuRegId.ZR_F: return F;
                    default:
                        throw new Exception("RegistersZ80 indexer wrong!");
                }
            }
            set
            {
                switch (index)
                {
                    case CpuRegId.ZR_B: B = value; break;
                    case CpuRegId.ZR_C: C = value; break;
                    case CpuRegId.ZR_D: D = value; break;
                    case CpuRegId.ZR_E: E = value; break;
                    case CpuRegId.ZR_H: H = value; break;
                    case CpuRegId.ZR_L: L = value; break;
                    case CpuRegId.ZR_A: A = value; break;
                    case CpuRegId.ZR_F: F = value; break;
                    default:
                        throw new Exception("RegistersZ80 indexer wrong!");
                }
            }
        }

        internal void SetPair(int RR, ushort value)
        {
            switch (RR)
            {
                case CpuRegId.ZR_BC: BC = value; return;
                case CpuRegId.ZR_DE: DE = value; return;
                case CpuRegId.ZR_HL: HL = value; return;
                case CpuRegId.ZR_SP: SP = value; return;
            }
            throw new Exception("RegistersZ80 SetPair index wrong!");
        }

        internal ushort GetPair(int RR)
        {
            switch (RR)
            {
                case CpuRegId.ZR_BC: return BC;
                case CpuRegId.ZR_DE: return DE;
                case CpuRegId.ZR_HL: return HL;
                case CpuRegId.ZR_SP: return SP;
            }
            throw new Exception("RegistersZ80 GetPair index wrong!");
        }

        #endregion
    }
}
