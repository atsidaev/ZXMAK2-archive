/// Description: Z80 CPU Emulator [registers part]
/// Author: Alex Makeev
/// Date: 18.03.2007
using System;
using System.Runtime.InteropServices;


namespace ZXMAK2.Engine.Z80
{
    [Flags]
    public enum ZFLAGS : byte
    {
        S = 0x80,
        Z = 0x40,
        F5 = 0x20,
        H = 0x10,
        F3 = 0x08,
        PV = 0x04,
        N = 0x02,
        C = 0x01
    }

    [StructLayout(LayoutKind.Explicit)]
    public class REGS
    {
        public const int ZR_BC = 0, ZR_DE = 1, ZR_HL = 2, ZR_SP = 3;
        public const int ZR_B = 0, ZR_C = 1, ZR_D = 2, ZR_E = 3, ZR_H = 4, ZR_L = 5, ZR_F = 6, ZR_A = 7;

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
                    case ZR_B: return B;
                    case ZR_C: return C;
                    case ZR_D: return D;
                    case ZR_E: return E;
                    case ZR_H: return H;
                    case ZR_L: return L;
                    case ZR_A: return A;
                    case ZR_F: return F;
                    default:
                        throw new Exception("RegistersZ80 indexer wrong!");
                }
            }
            set
            {
                switch (index)
                {
                    case ZR_B: B = value; break;
                    case ZR_C: C = value; break;
                    case ZR_D: D = value; break;
                    case ZR_E: E = value; break;
                    case ZR_H: H = value; break;
                    case ZR_L: L = value; break;
                    case ZR_A: A = value; break;
                    case ZR_F: F = value; break;
                    default:
                        throw new Exception("RegistersZ80 indexer wrong!");
                }
            }
        }

        internal void SetPair(int RR, ushort value)
        {
            switch (RR)
            {
                case ZR_BC: BC = value; return;
                case ZR_DE: DE = value; return;
                case ZR_HL: HL = value; return;
                case ZR_SP: SP = value; return;
            }
            throw new Exception("RegistersZ80 SetPair index wrong!");
        }

        internal ushort GetPair(int RR)
        {
            switch (RR)
            {
                case ZR_BC: return BC;
                case ZR_DE: return DE;
                case ZR_HL: return HL;
                case ZR_SP: return SP;
            }
            throw new Exception("RegistersZ80 GetPair index wrong!");
        }

        #endregion
    }
}
