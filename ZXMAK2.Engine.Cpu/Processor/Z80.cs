/// Description: Z80 CPU Emulator
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;

namespace ZXMAK2.Engine.Cpu.Processor
{
    public partial class Z80CPU
    {
        public CpuType CpuType = CpuType.Z80;
        public int RzxCounter = 0;
        public long Tact = 0;
        public CpuRegs regs = new CpuRegs();
        public bool HALTED;
        public bool IFF1;
        public bool IFF2;
        public byte IM;
        public bool BINT;       // last opcode was EI or DD/FD prefix (to prevent INT handling)
        public CpuModeIndex FX;
        public CpuModeEx XFX;
        public ushort LPC;      // last opcode PC

        public bool INT = false;
        public bool NMI = false;
        public bool RST = false;
        public byte BUS = 0xFF;     // state of free data bus

        public Action RESET = null;
        public Action NMIACK_M1 = null;
        public Action INTACK_M1 = null;
        public Func<ushort, byte> RDMEM_M1 = null;
        public Func<ushort, byte> RDMEM = null;
        public Action<ushort, byte> WRMEM = null;
        public Func<ushort, byte> RDPORT = null;
        public Action<ushort, byte> WRPORT = null;
        public Action<ushort> RDNOMREQ = null;
        public Action<ushort> WRNOMREQ = null;

        public Z80CPU()
        {
            ALU_INIT();
            initExec();
            initExecFX();
            initExecED();
            initExecCB();
            initFXCB();

            regs.AF = 0xFF;
            regs.BC = 0xFF;
            regs.DE = 0xFF;
            regs.HL = 0xFF;
            regs._AF = 0xFF;
            regs._BC = 0xFF;
            regs._DE = 0xFF;
            regs._HL = 0xFF;
            regs.IX = 0xFF;
            regs.IY = 0xFF;
            regs.IR = 0xFF;
            regs.PC = 0xFF;
            regs.SP = 0xFF;
            regs.MW = 0xFF;
        }


        public void ExecCycle()
        {
            byte cmd = 0;
            if (XFX == CpuModeEx.None && FX == CpuModeIndex.None)
            {
                if (ProcessSignals())
                    return;
                LPC = regs.PC;
                cmd = RDMEM_M1(LPC);
            }
            else
            {
                if (ProcessSignals())
                    return;
                cmd = RDMEM(regs.PC);
            }
            Tact += 3;
            regs.PC++;
            if (XFX == CpuModeEx.Cb)
            {
                BINT = false;
                ExecCB(cmd);
                XFX = CpuModeEx.None;
                FX = CpuModeIndex.None;
            }
            else if (XFX == CpuModeEx.Ed)
            {
                refresh();
                BINT = false;
                ExecED(cmd);
                XFX = CpuModeEx.None;
                FX = CpuModeIndex.None;
            }
            else if (cmd == 0xDD)
            {
                refresh();
                FX = CpuModeIndex.Ix;
                BINT = true;
            }
            else if (cmd == 0xFD)
            {
                refresh();
                FX = CpuModeIndex.Iy;
                BINT = true;
            }
            else if (cmd == 0xCB)
            {
                refresh();
                XFX = CpuModeEx.Cb;
                BINT = true;
            }
            else if (cmd == 0xED)
            {
                refresh();
                XFX = CpuModeEx.Ed;
                BINT = true;
            }
            else
            {
                refresh();
                BINT = false;
                ExecDirect(cmd);
                FX = CpuModeIndex.None;
            }
        }

        private void ExecED(byte cmd)
        {
            XFXOPDO edop = edopTABLE[cmd];
            if (edop != null)
                edop(cmd);
        }

        private void ExecCB(byte cmd)
        {
            if (FX != CpuModeIndex.None)
            {
                // elapsed T: 4, 4, 3
                // will be T: 4, 4, 3, 5

                int drel = (sbyte)cmd;

                regs.MW = FX == CpuModeIndex.Ix ? (ushort)(regs.IX + drel) : (ushort)(regs.IY + drel);
                cmd = RDMEM(regs.PC); Tact += 3;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;

                regs.PC++;
                fxcbopTABLE[cmd](cmd, regs.MW);
            }
            else
            {
                refresh();
                cbopTABLE[cmd](cmd);
            }
        }

        private void ExecDirect(byte cmd)
        {
            XFXOPDO opdo = FX == CpuModeIndex.None ? opTABLE[cmd] : fxopTABLE[cmd];
            if (opdo != null)
                opdo(cmd);
        }

        private void refresh()
        {
            regs.R = (byte)(((regs.R + 1) & 0x7F) | (regs.R & 0x80));
            Tact += 1;
            RzxCounter++;
        }

        private bool ProcessSignals()
        {
            if (RST)    // RESET
            {
                // 3T
                RESET();
                refresh();      //+1T

                FX = CpuModeIndex.None;
                XFX = CpuModeEx.None;
                HALTED = false;

                IFF1 = false;
                IFF2 = false;
                regs.PC = 0;
                regs.IR = 0;
                IM = 0;
                //regs.SP = 0xFFFF;
                //regs.AF = 0xFFFF;

                Tact += 2;      // total should be 3T?
                return true;
            }
            else if (NMI)
            {
                // 11T (5, 3, 3)

                if (HALTED) // workaround for Z80 snapshot halt issue + comfortable debugging
                    regs.PC++;

                // M1
                NMIACK_M1();
                Tact += 4;
                refresh();

                IFF2 = IFF1;
                IFF1 = false;
                HALTED = false;
                regs.SP--;

                // M2
                WRMEM(regs.SP, (byte)(regs.PC >> 8));
                Tact += 3;
                regs.SP--;

                // M3
                WRMEM(regs.SP, (byte)(regs.PC & 0xFF));
                regs.PC = 0x0066;
                Tact += 3;

                return true;
            }
            else if (INT && (!BINT) && IFF1)
            {
                // http://www.z80.info/interrup.htm
                // IM0: 13T (7,3,3) [RST]
                // IM1: 13T (7,3,3)
                // IM2: 19T (7,3,3,3,3)

                if (HALTED) // workaround for Z80 snapshot halt issue + comfortable debugging
                    regs.PC++;


                INTACK_M1();
                // M1: 7T = interrupt acknowledgement; SP--
                regs.SP--;
                //if (HALTED) ??
                //    Tact += 2;
                Tact += 4 + 2;
                refresh();
                RzxCounter--;	// fix because INTAK should not be calculated

                IFF1 = false;
                IFF2 = false; // proof?
                HALTED = false;

                // M2
                WRMEM(regs.SP, (byte)(regs.PC >> 8));   // M2: 3T write PCH; SP--
                regs.SP--;
                Tact += 3;

                // M3
                WRMEM(regs.SP, (byte)(regs.PC & 0xFF)); // M3: 3T write PCL
                Tact += 3;

                if (IM == 0)        // IM 0: execute instruction taken from BUS with timing T+2???
                {
                    regs.MW = 0x0038; // workaround: just execute #FF
                }
                else if (IM == 1)   // IM 1: execute #FF with timing T+2 (11+2=13T)
                {
                    regs.MW = 0x0038;
                }
                else                // IM 2: VH=reg.I; VL=BUS; PC=[V]
                {
                    // M4
                    ushort adr = (ushort)((regs.IR & 0xFF00) | BUS);
                    regs.MW = RDMEM(adr);               // M4: 3T read VL
                    Tact += 3;

                    // M5
                    regs.MW += (ushort)(RDMEM(++adr) * 0x100);   // M5: 3T read VH, PC=V
                    Tact += 3;
                }
                regs.PC = regs.MW;

                return true;
            }
            return false;
        }

        #region Tables

        private delegate void XFXOPDO(byte cmd);
        private delegate void FXCBOPDO(byte cmd, ushort adr);
        private XFXOPDO[] opTABLE;
        private XFXOPDO[] fxopTABLE;
        private XFXOPDO[] edopTABLE;
        private XFXOPDO[] cbopTABLE;
        private FXCBOPDO[] fxcbopTABLE;

        #endregion
    }
}