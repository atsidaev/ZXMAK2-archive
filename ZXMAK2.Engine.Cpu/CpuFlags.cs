using System;


namespace ZXMAK2.Engine.Cpu
{
    [Flags]
    public enum CpuFlags : byte
    {
        S = 0x80,
        Z = 0x40,
        F5 = 0x20,
        H = 0x10,
        F3 = 0x08,
        Pv = 0x04,
        N = 0x02,
        C = 0x01
    }
}
