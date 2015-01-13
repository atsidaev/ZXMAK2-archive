/// Description: Z80 CPU Emulator [direct opcode part]
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;

namespace ZXMAK2.Engine.Cpu.Processor
{
    public partial class Z80CPU
    {
        #region direct/DD/FD

        private void INA_NN_(byte cmd)      // IN A,(N) [11T] 
        {
            // 11T (4, 3, 4)

            regs.MW = RDMEM(regs.PC++); Tact += 3;
            regs.MW += (ushort)(regs.A << 8);

            regs.A = RDPORT(regs.MW); Tact += 4;
            regs.MW++;
        }

        private void OUT_NN_A(byte cmd)     // OUT (N),A [11T]+ 
        {
            // 11T (4, 3, 4)

            regs.MW = RDMEM(regs.PC++); Tact += 3;
            regs.MW += (ushort)(regs.A << 8);

            WRPORT(regs.MW, regs.A); Tact += 4;
            regs.ML++;
        }

        private void DI(byte cmd)
        {
            IFF1 = false;
            IFF2 = false;
        }

        private void EI(byte cmd)
        {
            IFF1 = true;
            IFF2 = true;
            BINT = true;
        }

        private void LDSPHL(byte cmd)       // LD SP,HL 
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            regs.SP = regs.HL;
        }

        private void EX_SP_HL(byte cmd)     // EX (SP),HL
        {
            // 19T (4, 3, 4, 3, 5)

            ushort tmpsp = regs.SP;
            regs.MW = RDMEM(tmpsp); Tact += 3;
            tmpsp++;

            regs.MW += (ushort)(RDMEM(tmpsp) * 0x100); Tact += 3;
            RDNOMREQ(tmpsp); Tact += 1;

            WRMEM(tmpsp, regs.H); Tact += 3;
            tmpsp--;

            WRMEM(tmpsp, regs.L); Tact += 3;
            WRNOMREQ(tmpsp); Tact++;
            WRNOMREQ(tmpsp); Tact++;
            regs.HL = regs.MW;
        }

        private void JP_HL_(byte cmd)       // JP (HL) 
        {
            regs.PC = regs.HL;
        }

        private void EXDEHL(byte cmd)       // EX DE,HL 
        {
            ushort tmp;
            tmp = regs.HL;      // ix префикс не действует!
            regs.HL = regs.DE;
            regs.DE = tmp;
        }

        private void EXAFAF(byte cmd)       // EX AF,AF' 
        {
            regs.EXAF();
        }

        private void EXX(byte cmd)          // EXX 
        {
            regs.EXX();
        }

        #endregion

        #region logical

        private void RLCA(byte cmd)
        {
            regs.F = (byte)(rlcaf[regs.A] | (regs.F & (byte)(CpuFlags.S | CpuFlags.Z | CpuFlags.Pv)));
            int x = regs.A;
            x <<= 1;
            if ((x & 0x100) != 0) x = (x | 0x01) & 0xFF;
            regs.A = (byte)x;
        }

        private void RRCA(byte cmd)
        {
            regs.F = (byte)(rrcaf[regs.A] | (regs.F & (byte)(CpuFlags.S | CpuFlags.Z | CpuFlags.Pv)));
            int x = regs.A;
            if ((x & 0x01) != 0) x = (x >> 1) | 0x80;
            else x >>= 1;
            regs.A = (byte)x;
        }

        private void RLA(byte cmd)
        {
            bool carry = (regs.F & (byte)CpuFlags.C) != 0;
            regs.F = (byte)(rlcaf[regs.A] | (regs.F & (byte)(CpuFlags.S | CpuFlags.Z | CpuFlags.Pv))); // use same table with rlca
            regs.A = (byte)((regs.A << 1) & 0xFF);
            if (carry) regs.A |= (byte)0x01;
        }

        private void RRA(byte cmd)
        {
            bool carry = (regs.F & (byte)CpuFlags.C) != 0;
            regs.F = (byte)(rrcaf[regs.A] | (regs.F & (byte)(CpuFlags.S | CpuFlags.Z | CpuFlags.Pv))); // use same table with rrca
            regs.A = (byte)(regs.A >> 1);
            if (carry) regs.A |= (byte)0x80;
        }

        private void DAA(byte cmd)
        {
            regs.AF = daatab[regs.A + 0x100 * ((regs.F & 3) + ((regs.F >> 2) & 4))];
        }

        private void CPL(byte cmd)
        {
            regs.A ^= (byte)0xFF;
            regs.F = (byte)((regs.F & (int)~(CpuFlags.F3 | CpuFlags.F5)) | (int)(CpuFlags.N | CpuFlags.H) | (regs.A & (int)(CpuFlags.F3 | CpuFlags.F5)));
        }

        private void SCF(byte cmd)
        {
            //regs.F = (byte)((regs.F & (int)~(ZFLAGS.H | ZFLAGS.N)) | (regs.A & (int)(ZFLAGS.F3 | ZFLAGS.F5)) | (int)ZFLAGS.C);
            regs.F = (byte)((regs.F & ((int)CpuFlags.Pv | (int)CpuFlags.Z | (int)CpuFlags.S)) |
                (regs.A & ((int)CpuFlags.F3 | (int)CpuFlags.F5)) |
                (int)CpuFlags.C);
        }

        private void CCF(byte cmd)
        {
            //regs.F = (byte)(((regs.F & (int)~(ZFLAGS.N | ZFLAGS.H)) | ((regs.F << 4) & (int)ZFLAGS.H) | (regs.A & (int)(ZFLAGS.F3 | ZFLAGS.F5))) ^ (int)ZFLAGS.C);
            regs.F = (byte)((regs.F & ((int)CpuFlags.Pv | (int)CpuFlags.Z | (int)CpuFlags.S)) |
                ((regs.F & (int)CpuFlags.C) != 0 ? (int)CpuFlags.H : (int)CpuFlags.C) | (regs.A & ((int)CpuFlags.F3 | (int)CpuFlags.F5)));
        }

        #endregion

        #region jmp/call/ret/jr

        static private byte[] conds = new byte[4] 
        { 
            (byte)CpuFlags.Z, 
            (byte)CpuFlags.C, 
            (byte)CpuFlags.Pv, 
            (byte)CpuFlags.S 
        };

        private void DJNZ(byte cmd)      // DJNZ nn
        {
            // B==0 => 8T (5, 3)
            // B!=0 => 13T (5, 3, 5)

            RDNOMREQ(regs.IR); Tact++;

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            if (--regs.B != 0)
            {
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                regs.MW = (ushort)(regs.PC + drel);
                regs.PC = regs.MW;
            }
        }

        private void JRNN(byte cmd)      // JR nn
        {
            // 12T (4, 3, 5)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            regs.MW = (ushort)(regs.PC + drel);
            regs.PC = regs.MW;
        }

        private void JRXNN(byte cmd)     // JR x,nn
        {
            // false => 7T (4, 3)
            // true  => 12 (4, 3, 5)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            int cond = (cmd & 0x18) >> 3;
            int TST = conds[cond >> 1];
            int F = regs.AF & TST;
            if ((cond & 1) != 0) F ^= TST;
            if (F == 0)
            {
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                RDNOMREQ(regs.PC); Tact++;
                regs.MW = (ushort)(regs.PC + drel);
                regs.PC = regs.MW;
            }
        }

        private void CALLNNNN(byte cmd)  // CALL
        {
            // 17T (4, 3, 4, 3, 3)

            regs.MW = RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            regs.MW += (ushort)(RDMEM(regs.PC) * 0x100); Tact += 3;
            RDNOMREQ(regs.PC); Tact++;
            regs.PC++;
            regs.SP--;

            WRMEM(regs.SP, (byte)(regs.PC >> 8)); Tact += 3;
            regs.SP--;

            WRMEM(regs.SP, (byte)(regs.PC & 0xFF)); Tact += 3;
            regs.PC = regs.MW;
        }

        private void CALLXNNNN(byte cmd) // CALL x,#nn
        {
            // false => 10T (4, 3, 3)
            // true  => 17T (4, 3, 4, 3, 3)

            int cond = (cmd & 0x38) >> 3;
            int TST = conds[cond >> 1];
            int F = regs.AF & TST;
            if ((cond & 1) != 0) F ^= TST;

            regs.MW = RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            regs.MW += (ushort)(RDMEM(regs.PC) * 0x100); Tact += 3;
            if (F == 0)
            {
                RDNOMREQ(regs.PC); Tact++;
            }
            regs.PC++;

            if (F == 0)
            {
                regs.SP--;

                WRMEM(regs.SP, (byte)(regs.PC >> 8)); Tact += 3;
                regs.SP--;

                WRMEM(regs.SP, (byte)regs.PC); Tact += 3;
                regs.PC = regs.MW;
            }
        }

        private void RET(byte cmd)       // RET
        {
            // 10T (4, 3, 3)

            regs.MW = RDMEM(regs.SP); Tact += 3;
            regs.SP++;

            regs.MW += (ushort)(RDMEM(regs.SP) * 0x100); Tact += 3;
            regs.SP++;
            regs.PC = regs.MW;
        }

        private void RETX(byte cmd)      // RET x
        {
            // false => 5T (5)
            // true  => 11T (5, 3, 3)

            int cond = (cmd & 0x38) >> 3;
            int TST = conds[cond >> 1];
            int F = regs.AF & TST;
            if ((cond & 1) != 0) F ^= TST;

            RDNOMREQ(regs.IR); Tact++;

            if (F == 0)
            {
                regs.MW = RDMEM(regs.SP); Tact += 3;
                regs.SP++;

                regs.MW += (ushort)(RDMEM(regs.SP) * 0x100); Tact += 3;
                regs.SP++;
                regs.PC = regs.MW;
            }
        }

        private void JPNNNN(byte cmd)    // JP nnnn
        {
            // 10T (4, 3, 3)

            regs.MW = RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            regs.MW += (ushort)(RDMEM(regs.PC) * 0x100); Tact += 3;
            regs.PC = regs.MW;
        }

        private void JPXNN(byte cmd)     // JP x,#nn ???
        {
            // 10T (4, 3, 3)

            int cond = (cmd & 0x38) >> 3;
            int TST = conds[cond >> 1];
            int F = regs.AF & TST;
            if ((cond & 1) != 0) F ^= TST;

            regs.MW = RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            regs.MW += (ushort)(RDMEM(regs.PC) * 0x100); Tact += 3;
            regs.PC++;

            if (F == 0)
                regs.PC = regs.MW;
        }


        private void RSTNN(byte cmd)     // RST #nn ?TIME?
        {
            // 11T (5, 3, 3)

            RDNOMREQ(regs.IR); Tact++;
            regs.SP--;

            WRMEM(regs.SP, (byte)(regs.PC >> 8)); Tact += 3;
            regs.SP--;

            WRMEM(regs.SP, (byte)regs.PC); Tact += 3;
            regs.MW = (ushort)(cmd & 0x38);
            regs.PC = regs.MW;
        }

        #endregion

        #region push/pop

        private void PUSHRR(byte cmd)    // PUSH RR ?TIME?
        {
            // 11T (5, 3, 3)

            RDNOMREQ(regs.IR); Tact += 1;
            int rr = (cmd & 0x30) >> 4;
            ushort val = rr == CpuRegId.Sp ? regs.AF : regs.GetPair(rr);

            regs.SP--;
            WRMEM(regs.SP, (byte)(val >> 8)); Tact += 3;

            regs.SP--;
            WRMEM(regs.SP, (byte)(val & 0xFF)); Tact += 3;
        }

        private void POPRR(byte cmd)     // POP RR
        {
            // 10T (4, 3, 3)

            ushort val = RDMEM(regs.SP);
            regs.SP++;
            Tact += 3;

            val |= (ushort)(RDMEM(regs.SP) << 8);
            regs.SP++;
            int rr = (cmd & 0x30) >> 4;
            if (rr == CpuRegId.Sp) regs.AF = val;
            else regs.SetPair(rr, val);
            Tact += 3;
        }

        #endregion

        #region ALU

        private delegate void ALUALGORITHM(byte src);
        private ALUALGORITHM[] alualg;
        private ALUALGORITHM[] alulogic;

        private void ALUAN(byte cmd)
        {
            // 7T (4, 3)

            byte val = RDMEM(regs.PC);
            regs.PC++;
            int op = (cmd & 0x38) >> 3;
            alualg[op](val);
            Tact += 3;
        }

        private void ALUAR(byte cmd)     // ADD/ADC/SUB/SBC/AND/XOR/OR/CP A,R
        {
            int r = cmd & 0x07;
            int op = (cmd & 0x38) >> 3;
            alualg[op](regs[r]);
        }

        private void ALUA_HL_(byte cmd)     // ADD/ADC/SUB/SBC/AND/XOR/OR/CP A,(HL)
        {
            // 7T (4, 3)

            int op = (cmd & 0x38) >> 3;
            byte val = RDMEM(regs.HL);
            alualg[op](val);
            Tact += 3;
        }

        private void ADDHLRR(byte cmd)   // ADD HL,RR
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            regs.MW = (ushort)(regs.HL + 1);
            regs.HL = ALU_ADDHLRR(regs.HL, regs.GetPair((cmd & 0x30) >> 4));
        }

        #endregion

        #region loads

        private void LDA_RR_(byte cmd)   // LD A,(RR)
        {
            // 7T (4, 3)

            ushort rr = regs.GetPair((cmd & 0x30) >> 4);
            regs.A = RDMEM(rr);
            regs.MW = (ushort)(rr + 1);
            Tact += 3;
        }

        private void LDA_NN_(byte cmd)   // LD A,(nnnn)
        {
            // 13T (4, 3, 3, 3)

            ushort adr = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;

            adr += (ushort)(RDMEM(regs.PC) * 0x100);
            regs.PC++;
            Tact += 3;

            regs.A = RDMEM(adr);
            regs.MW = (ushort)(adr + 1);
            Tact += 3;
        }

        private void LDHL_NN_(byte cmd)   // LD HL,(nnnn)
        {
            // 16T (4, 3, 3, 3, 3)

            ushort adr = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;

            adr += (ushort)(RDMEM(regs.PC) * 0x100);
            regs.PC++;
            Tact += 3;

            ushort val = RDMEM(adr);
            regs.MW = (ushort)(adr + 1);
            Tact += 3;

            val += (ushort)(RDMEM(regs.MW) * 0x100);
            regs.HL = val;
            Tact += 3;
        }

        private void LD_RR_A(byte cmd)   // LD (RR),A
        {
            // 7T (4, 3)

            ushort rr = regs.GetPair((cmd & 0x30) >> 4);
            WRMEM(rr, regs.A);
            regs.MH = regs.A;
            regs.ML = (byte)(rr + 1);
            Tact += 3;
        }

        private void LD_NN_A(byte cmd)   // LD (nnnn),A
        {
            // 13T (4, 3, 3, 3)

            ushort adr = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;

            adr += (ushort)(RDMEM(regs.PC) * 0x100);
            regs.PC++;
            Tact += 3;

            WRMEM(adr, regs.A);
            regs.MH = regs.A;
            regs.ML = (byte)(adr + 1);
            Tact += 3;
        }

        private void LD_NN_HL(byte cmd)   // LD (nnnn),HL
        {
            // 16T (4, 3, 3, 3, 3)

            ushort adr = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;

            adr += (ushort)(RDMEM(regs.PC) * 0x100);
            regs.PC++;
            Tact += 3;

            WRMEM(adr, regs.L);
            regs.MW = (ushort)(adr + 1);
            Tact += 3;

            WRMEM(regs.MW, regs.H);
            Tact += 3;
        }

        private void LDRRNNNN(byte cmd)  // LD RR,nnnn
        {
            // 10T (4, 3, 3)

            ushort val = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;

            val |= (ushort)(RDMEM(regs.PC) << 8);
            regs.PC++;
            int rr = (cmd & 0x30) >> 4;
            regs.SetPair(rr, val);
            Tact += 3;
        }

        private void LDRNN(byte cmd)     // LD R,nn
        {
            // 7T (4, 3)

            int r = (cmd & 0x38) >> 3;
            regs[r] = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;
        }

        private void LD_HL_NN(byte cmd)     // LD (HL),nn
        {
            // 10T (4, 3, 3)

            byte val = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;

            WRMEM(regs.HL, val);
            Tact += 3;
        }

        private void LDRdRs(byte cmd)     // LD R1,R2
        {
            int rsrc = cmd & 0x07;
            int rdst = (cmd & 0x38) >> 3;
            regs[rdst] = regs[rsrc];
        }

        private void LD_HL_R(byte cmd)    // LD (HL),R
        {
            // 7T (4, 3)

            int rsrc = cmd & 0x07;
            WRMEM(regs.HL, regs[rsrc]);
            Tact += 3;
        }

        private void LDR_HL_(byte cmd)    // LD R,(HL)
        {
            // 7T (4, 3)

            int rdst = (cmd & 0x38) >> 3;
            regs[rdst] = RDMEM(regs.HL);
            Tact += 3;
        }

        #endregion

        #region INC/DEC

        private void DECRR(byte cmd)     // DEC RR
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            int rr = (cmd & 0x30) >> 4;
            regs.SetPair(rr, (ushort)(regs.GetPair(rr) - 1));
        }

        private void INCRR(byte cmd)     // INC RR
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            int rr = (cmd & 0x30) >> 4;
            regs.SetPair(rr, (ushort)(regs.GetPair(rr) + 1));
        }

        private void DECR(byte cmd)      // DEC R
        {
            int r = (cmd & 0x38) >> 3;
            regs[r] = ALU_DECR(regs[r]);
        }

        private void INCR(byte cmd)      // INC R
        {
            int r = (cmd & 0x38) >> 3;
            regs[r] = ALU_INCR(regs[r]);
        }

        private void DEC_HL_(byte cmd)      // DEC (HL)
        {
            // 11T (4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_DECR(val);

            WRMEM(regs.HL, val); Tact += 3;
        }

        private void INC_HL_(byte cmd)      // INC (HL)
        {
            // 11T (4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_INCR(val);

            WRMEM(regs.HL, val); Tact += 3;
        }

        #endregion

        private void HALT(byte cmd)
        {
            HALTED = true;
            regs.PC--;      // workaround for Z80 snapshot halt issue + comfortable debugging
        }

        private void initExec()
        {
            opTABLE = new XFXOPDO[256]
            {
//              0        1         2         3         4          5        6         7          8       9        A         B        C          D         E         F
                null,    LDRRNNNN, LD_RR_A,  INCRR,    INCR,      DECR,    LDRNN,    RLCA,      EXAFAF, ADDHLRR, LDA_RR_,  DECRR,   INCR,      DECR,     LDRNN,    RRCA,   // 00..0F
                DJNZ,    LDRRNNNN, LD_RR_A,  INCRR,    INCR,      DECR,    LDRNN,    RLA,       JRNN,   ADDHLRR, LDA_RR_,  DECRR,   INCR,      DECR,     LDRNN,    RRA,    // 10..1F
                JRXNN,   LDRRNNNN, LD_NN_HL, INCRR,    INCR,      DECR,    LDRNN,    DAA,       JRXNN,  ADDHLRR, LDHL_NN_, DECRR,   INCR,      DECR,     LDRNN,    CPL,    // 20..2F
                JRXNN,   LDRRNNNN, LD_NN_A,  INCRR,    INC_HL_,   DEC_HL_, LD_HL_NN, SCF,       JRXNN,  ADDHLRR, LDA_NN_,  DECRR,   INCR,      DECR,     LDRNN,    CCF,    // 30..3F

                null,    LDRdRs,   LDRdRs,   LDRdRs,   LDRdRs,    LDRdRs,  LDR_HL_,  LDRdRs,    LDRdRs, null,    LDRdRs,   LDRdRs,  LDRdRs,    LDRdRs,   LDR_HL_,  LDRdRs, // 40..4F
                LDRdRs,  LDRdRs,   null,     LDRdRs,   LDRdRs,    LDRdRs,  LDR_HL_,  LDRdRs,    LDRdRs, LDRdRs,  LDRdRs,   null,    LDRdRs,    LDRdRs,   LDR_HL_,  LDRdRs, // 50..5F
                LDRdRs,  LDRdRs,   LDRdRs,   LDRdRs,   null,      LDRdRs,  LDR_HL_,  LDRdRs,    LDRdRs, LDRdRs,  LDRdRs,   LDRdRs,  LDRdRs,    null,     LDR_HL_,  LDRdRs, // 60..6F
                LD_HL_R, LD_HL_R,  LD_HL_R,  LD_HL_R,  LD_HL_R,   LD_HL_R, HALT,     LD_HL_R,   LDRdRs, LDRdRs,  LDRdRs,   LDRdRs,  LDRdRs,    LDRdRs,   LDR_HL_,  null,   // 70..7F
    
                ALUAR,   ALUAR,    ALUAR,    ALUAR,    ALUAR,     ALUAR,   ALUA_HL_, ALUAR,     ALUAR,  ALUAR,   ALUAR,    ALUAR,   ALUAR,     ALUAR,    ALUA_HL_, ALUAR,  // 80..8F
                ALUAR,   ALUAR,    ALUAR,    ALUAR,    ALUAR,     ALUAR,   ALUA_HL_, ALUAR,     ALUAR,  ALUAR,   ALUAR,    ALUAR,   ALUAR,     ALUAR,    ALUA_HL_, ALUAR,  // 90..9F
                ALUAR,   ALUAR,    ALUAR,    ALUAR,    ALUAR,     ALUAR,   ALUA_HL_, ALUAR,     ALUAR,  ALUAR,   ALUAR,    ALUAR,   ALUAR,     ALUAR,    ALUA_HL_, ALUAR,  // A0..AF
                ALUAR,   ALUAR,    ALUAR,    ALUAR,    ALUAR,     ALUAR,   ALUA_HL_, ALUAR,     ALUAR,  ALUAR,   ALUAR,    ALUAR,   ALUAR,     ALUAR,    ALUA_HL_, ALUAR,  // B0..BF

                RETX,    POPRR,    JPXNN,    JPNNNN,   CALLXNNNN, PUSHRR,  ALUAN,    RSTNN,     RETX,   RET,     JPXNN,    null,    CALLXNNNN, CALLNNNN, ALUAN,    RSTNN,  // C0..CF
                RETX,    POPRR,    JPXNN,    OUT_NN_A, CALLXNNNN, PUSHRR,  ALUAN,    RSTNN,     RETX,   EXX,     JPXNN,    INA_NN_, CALLXNNNN, null,     ALUAN,    RSTNN,  // D0..DF
                RETX,    POPRR,    JPXNN,    EX_SP_HL, CALLXNNNN, PUSHRR,  ALUAN,    RSTNN,     RETX,   JP_HL_,  JPXNN,    EXDEHL,  CALLXNNNN, null,     ALUAN,    RSTNN,  // E0..EF
                RETX,    POPRR,    JPXNN,    DI,       CALLXNNNN, PUSHRR,  ALUAN,    RSTNN,     RETX,   LDSPHL,  JPXNN,    EI,      CALLXNNNN, null,     ALUAN,    RSTNN,  // F0..FF
            };
        }
    }
}
