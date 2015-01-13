/// Description: Z80 CPU Emulator [DD/FD prefixed opcode part]
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;

namespace ZXMAK2.Engine.Cpu.Processor
{
    public partial class Z80CPU
    {
        #region FXxx ops...

        private void FX_LDSPHL(byte cmd)       // LD SP,IX 
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            if (FX == CpuModeIndex.Ix)
                regs.SP = regs.IX;
            else
                regs.SP = regs.IY;
        }

        private void FX_EX_SP_HL(byte cmd)     // EX (SP),IX
        {
            // 23T (4, 4, 3, 4, 3, 5)
            
            ushort tmpsp = regs.SP;
            regs.MW = RDMEM(tmpsp); Tact += 3;
            tmpsp++;

            regs.MW += (ushort)(RDMEM(tmpsp) * 0x100); Tact += 3;
            RDNOMREQ(tmpsp); Tact++;

            if (FX == CpuModeIndex.Ix)
            {
                WRMEM(tmpsp, regs.XH); Tact += 3;
                tmpsp--;

                WRMEM(tmpsp, regs.XL); Tact += 3;
                WRNOMREQ(tmpsp); Tact++;
                WRNOMREQ(tmpsp); Tact++;
                regs.IX = regs.MW;
            }
            else
            {
                WRMEM(tmpsp, regs.YH); Tact += 3;
                tmpsp--;

                WRMEM(tmpsp, regs.YL); Tact += 3;
                WRNOMREQ(tmpsp); Tact++;
                WRNOMREQ(tmpsp); Tact++;
                regs.IY = regs.MW;
            }
        }

        private void FX_JP_HL_(byte cmd)       // JP (IX) 
        {
            if (FX == CpuModeIndex.Ix)
                regs.PC = regs.IX;
            else
                regs.PC = regs.IY;
        }

        private void FX_PUSHIX(byte cmd)       // PUSH IX
        {
            // 15 (4, 5, 3, 3)

            RDNOMREQ(regs.IR); Tact++;
            ushort val = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.SP--;

            WRMEM(regs.SP, (byte)(val >> 8)); Tact += 3;
            regs.SP--;

            WRMEM(regs.SP, (byte)(val & 0xFF)); Tact += 3;
        }

        private void FX_POPIX(byte cmd)        // POP IX
        {
            // 14T (4, 4, 3, 3)
            
            ushort val = RDMEM(regs.SP);
            regs.SP++;
            Tact += 3;
            
            val |= (ushort)(RDMEM(regs.SP) << 8);
            regs.SP++;
            if (FX == CpuModeIndex.Ix)
                regs.IX = val;
            else
                regs.IY = val;
            Tact += 3;
        }

        private void FX_ALUAXH(byte cmd)       // ADD/ADC/SUB/SBC/AND/XOR/OR/CP A,XH
        {
            byte val;
            if (FX == CpuModeIndex.Ix)
                val = (byte)(regs.IX >> 8);
            else
                val = (byte)(regs.IY >> 8);
            alualg[(cmd & 0x38) >> 3](val);
        }

        private void FX_ALUAXL(byte cmd)       // ADD/ADC/SUB/SBC/AND/XOR/OR/CP A,XL
        {
            byte val;
            if (FX == CpuModeIndex.Ix)
                val = (byte)(regs.IX & 0xFF);
            else
                val = (byte)(regs.IY & 0xFF);
            alualg[(cmd & 0x38) >> 3](val);
        }

        private void FX_ALUA_IX_(byte cmd)     // ADD/ADC/SUB/SBC/AND/XOR/OR/CP A,(IX)
        {
            // 19T (4, 4, 3, 5, 3)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;

            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            
            regs.PC++;
            regs.MW = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.MW = (ushort)(regs.MW + drel);

            byte val = RDMEM(regs.MW); Tact += 3;
            int op = (cmd & 0x38) >> 3;
            alualg[op](val);
        }

        private void FX_ADDIXRR(byte cmd)      // ADD IX,RR
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            
            ushort rde;
            switch ((cmd & 0x30) >> 4)
            {
                case 0: rde = regs.BC; break;
                case 1: rde = regs.DE; break;
                case 2:
                    if (FX == CpuModeIndex.Ix) rde = regs.IX;
                    else rde = regs.IY;
                    break;
                case 3: rde = regs.SP; break;
                default: throw new Exception();
            }
            if (FX == CpuModeIndex.Ix)
            {
                regs.MW = (ushort)(regs.IX + 1);
                regs.IX = ALU_ADDHLRR(regs.IX, rde);
            }
            else
            {
                regs.MW = (ushort)(regs.IY + 1);
                regs.IY = ALU_ADDHLRR(regs.IY, rde);
            }
        }

        private void FX_DECIX(byte cmd)        // DEC IX
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            if (FX == CpuModeIndex.Ix)
                regs.IX--;
            else
                regs.IY--;
        }

        private void FX_INCIX(byte cmd)        // INC IX
        {
            RDNOMREQ(regs.IR); Tact++;
            RDNOMREQ(regs.IR); Tact++;
            if (FX == CpuModeIndex.Ix)
                regs.IX++;
            else
                regs.IY++;
        }

        private void FX_LDIX_N_(byte cmd)      // LD IX,(nnnn)
        {
            // 20 (4, 4, 3, 3, 3, 3)
            
            ushort adr = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;
            
            adr += (ushort)(RDMEM(regs.PC) * 0x100);
            regs.PC++;
            regs.MW = (ushort)(adr + 1);
            Tact += 3;
            
            ushort val = RDMEM(adr);
            Tact += 3;
            
            val += (ushort)(RDMEM(regs.MW) * 0x100);
            if (FX == CpuModeIndex.Ix)
                regs.IX = val;
            else
                regs.IY = val;
            Tact += 3;
        }

        private void FX_LD_NN_IX(byte cmd)     // LD (nnnn),IX
        {
            // 20 (4, 4, 3, 3, 3, 3)
            
            ushort hl = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            ushort adr = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;
            
            adr += (ushort)(RDMEM(regs.PC) * 0x100);
            regs.PC++;
            regs.MW = (ushort)(adr + 1);
            Tact += 3;

            WRMEM(adr, (byte)hl);
            Tact += 3;

            WRMEM(regs.MW, (byte)(hl >> 8));
            Tact += 3;
        }

        private void FX_LDIXNNNN(byte cmd)     // LD IX,nnnn
        {
            // 14 (4, 4, 3, 3)
            
            ushort val = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;
            
            val |= (ushort)(RDMEM(regs.PC) << 8);
            regs.PC++;
            if (FX == CpuModeIndex.Ix)
                regs.IX = val;
            else
                regs.IY = val;
            Tact += 3;
        }

        private void FX_DEC_IX_(byte cmd)      // DEC (IX)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;

            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            
            regs.PC++;
            regs.MW = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.MW = (ushort)(regs.MW + drel);

            byte val = RDMEM(regs.MW); Tact += 3;
            RDNOMREQ(regs.MW); Tact++;
            val = ALU_DECR(val);
            
            WRMEM(regs.MW, val); Tact += 3;
        }

        private void FX_INC_IX_(byte cmd)      // INC (IX)
        {
            //23T (4, 4, 3, 5, 4, 3)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;
            
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;

            regs.PC++;
            regs.MW = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.MW = (ushort)(regs.MW + drel);

            byte val = RDMEM(regs.MW); Tact += 3;
            RDNOMREQ(regs.MW); Tact++;
            val = ALU_INCR(val);

            WRMEM(regs.MW, val); Tact += 3;
        }

        private void FX_LD_IX_NN(byte cmd)     // LD (IX),nn
        {
            // 19 (4, 4, 3, 5, 3)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;
            regs.PC++;

            byte val = RDMEM(regs.PC); Tact += 3;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;

            regs.PC++;
            regs.MW = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.MW = (ushort)(regs.MW + drel);
            
            WRMEM(regs.MW, val); Tact += 3;
        }

        private void FX_LD_IX_R(byte cmd)      // LD (IX),R
        {
            // 19T (4, 4, 3, 5, 3)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;

            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;

            regs.PC++;
            regs.MW = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.MW = (ushort)(regs.MW + drel);

            int rsrc = cmd & 0x07;
            WRMEM(regs.MW, regs[rsrc]); Tact += 3;
        }

        private void FX_LDR_IX_(byte cmd)      // LD R,(IX)
        {
            // 19T (4, 4, 3, 5, 3)

            int drel = (sbyte)RDMEM(regs.PC); Tact += 3;
            
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            RDNOMREQ(regs.PC); Tact++;
            
            regs.PC++;
            regs.MW = FX == CpuModeIndex.Ix ? regs.IX : regs.IY;
            regs.MW = (ushort)(regs.MW + drel);

            int rdst = (cmd & 0x38) >> 3;
            regs[rdst] = RDMEM(regs.MW); Tact += 3;
        }

        #endregion

        #region RdRs...

        private void FX_LDHL(byte cmd)
        {
            if (FX == CpuModeIndex.Ix)
                regs.XH = regs.XL;
            else
                regs.YH = regs.YL;
        }

        private void FX_LDLH(byte cmd)
        {
            if (FX == CpuModeIndex.Ix)
                regs.XL = regs.XH;
            else
                regs.YL = regs.YH;
        }

        private void FX_LDRL(byte cmd)
        {
            int rdst = (cmd & 0x38) >> 3;
            if (FX == CpuModeIndex.Ix)
                regs[rdst] = regs.XL;
            else
                regs[rdst] = regs.YL;
        }

        private void FX_LDRH(byte cmd)
        {
            int rdst = (cmd & 0x38) >> 3;
            if (FX == CpuModeIndex.Ix)
                regs[rdst] = regs.XH;
            else
                regs[rdst] = regs.YH;
        }

        private void FX_LDLR(byte cmd)
        {
            int rsrc = cmd & 0x07;
            if (FX == CpuModeIndex.Ix)
                regs.XL = regs[rsrc];
            else
                regs.YL = regs[rsrc];
        }

        private void FX_LDHR(byte cmd)
        {
            int rsrc = cmd & 0x07;
            if (FX == CpuModeIndex.Ix)
                regs.XH = regs[rsrc];
            else
                regs.YH = regs[rsrc];
        }

        private void FX_LDLNN(byte cmd)     // LD XL,nn
        {
            // 11T (4, 4, 3)
            
            if (FX == CpuModeIndex.Ix)
                regs.XL = RDMEM(regs.PC);
            else
                regs.YL = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;
        }

        private void FX_LDHNN(byte cmd)     // LD XH,nn
        {
            // 11T (4, 4, 3)
            
            if (FX == CpuModeIndex.Ix)
                regs.XH = RDMEM(regs.PC);
            else
                regs.YH = RDMEM(regs.PC);
            regs.PC++;
            Tact += 3;
        }

        private void FX_INCL(byte cmd)      // INC XL
        {
            if (FX == CpuModeIndex.Ix)
                regs.XL = ALU_INCR(regs.XL);
            else
                regs.YL = ALU_INCR(regs.YL);
        }

        private void FX_INCH(byte cmd)      // INC XH
        {
            if (FX == CpuModeIndex.Ix)
                regs.XH = ALU_INCR(regs.XH);
            else
                regs.YH = ALU_INCR(regs.YH);
        }

        private void FX_DECL(byte cmd)      // DEC XL
        {
            if (FX == CpuModeIndex.Ix)
                regs.XL = ALU_DECR(regs.XL);
            else
                regs.YL = ALU_DECR(regs.YL);
        }

        private void FX_DECH(byte cmd)      // DEC XH
        {
            if (FX == CpuModeIndex.Ix)
                regs.XH = ALU_DECR(regs.XH);
            else
                regs.YH = ALU_DECR(regs.YH);
        }

        #endregion


        private void initExecFX()
        {
            fxopTABLE = new XFXOPDO[256]
            {
//              0           1            2            3            4           5           6             7           8        9           A           B         C          D          E             F
                null,       LDRRNNNN,    LD_RR_A,     INCRR,       INCR,       DECR,       LDRNN,        RLCA,       EXAFAF,  FX_ADDIXRR, LDA_RR_,    DECRR,    INCR,      DECR,      LDRNN,        RRCA,   // 00..0F
                DJNZ,       LDRRNNNN,    LD_RR_A,     INCRR,       INCR,       DECR,       LDRNN,        RLA,        JRNN,    FX_ADDIXRR, LDA_RR_,    DECRR,    INCR,      DECR,      LDRNN,        RRA,    // 10..1F
                JRXNN,      FX_LDIXNNNN, FX_LD_NN_IX, FX_INCIX,    FX_INCH,    FX_DECH,    FX_LDHNN,     DAA,        JRXNN,   FX_ADDIXRR, FX_LDIX_N_, FX_DECIX, FX_INCL,   FX_DECL,   FX_LDLNN,     CPL,    // 20..2F
                JRXNN,      LDRRNNNN,    LD_NN_A,     INCRR,       FX_INC_IX_, FX_DEC_IX_, FX_LD_IX_NN,  SCF,        JRXNN,   FX_ADDIXRR, LDA_NN_,    DECRR,    INCR,      DECR,      LDRNN,        CCF,    // 30..3F

                null,       LDRdRs,      LDRdRs,      LDRdRs,      FX_LDRH,    FX_LDRL,    FX_LDR_IX_,   LDRdRs,     LDRdRs,  null,       LDRdRs,     LDRdRs,   FX_LDRH,   FX_LDRL,   FX_LDR_IX_,   LDRdRs, // 40..4F
                LDRdRs,     LDRdRs,      null,        LDRdRs,      FX_LDRH,    FX_LDRL,    FX_LDR_IX_,   LDRdRs,     LDRdRs,  LDRdRs,     LDRdRs,     null,     FX_LDRH,   FX_LDRL,   FX_LDR_IX_,   LDRdRs, // 50..5F
                FX_LDHR,    FX_LDHR,     FX_LDHR,     FX_LDHR,     null,       FX_LDHL,    FX_LDR_IX_,   FX_LDHR,    FX_LDLR, FX_LDLR,    FX_LDLR,    FX_LDLR,  FX_LDLH,   null,      FX_LDR_IX_,   FX_LDLR,// 60..6F
                FX_LD_IX_R, FX_LD_IX_R,  FX_LD_IX_R,  FX_LD_IX_R,  FX_LD_IX_R, FX_LD_IX_R, HALT,         FX_LD_IX_R, LDRdRs,  LDRdRs,     LDRdRs,     LDRdRs,   FX_LDRH,   FX_LDRL,   FX_LDR_IX_,   null,   // 70..7F

                ALUAR,      ALUAR,       ALUAR,       ALUAR,       FX_ALUAXH,  FX_ALUAXL,  FX_ALUA_IX_,  ALUAR,      ALUAR,   ALUAR,      ALUAR,      ALUAR,    FX_ALUAXH, FX_ALUAXL, FX_ALUA_IX_,  ALUAR,  // 80..8F
                ALUAR,      ALUAR,       ALUAR,       ALUAR,       FX_ALUAXH,  FX_ALUAXL,  FX_ALUA_IX_,  ALUAR,      ALUAR,   ALUAR,      ALUAR,      ALUAR,    FX_ALUAXH, FX_ALUAXL, FX_ALUA_IX_,  ALUAR,  // 90..9F
                ALUAR,      ALUAR,       ALUAR,       ALUAR,       FX_ALUAXH,  FX_ALUAXL,  FX_ALUA_IX_,  ALUAR,      ALUAR,   ALUAR,      ALUAR,      ALUAR,    FX_ALUAXH, FX_ALUAXL, FX_ALUA_IX_,  ALUAR,  // A0..AF
                ALUAR,      ALUAR,       ALUAR,       ALUAR,       FX_ALUAXH,  FX_ALUAXL,  FX_ALUA_IX_,  ALUAR,      ALUAR,   ALUAR,      ALUAR,      ALUAR,    FX_ALUAXH, FX_ALUAXL, FX_ALUA_IX_,  ALUAR,  // B0..BF

                RETX,       POPRR,       JPXNN,       JPNNNN,      CALLXNNNN,  PUSHRR,     ALUAN,        RSTNN,      RETX,    RET,        JPXNN,      null,     CALLXNNNN, CALLNNNN,  ALUAN,        RSTNN,  // C0..CF
                RETX,       POPRR,       JPXNN,       OUT_NN_A,    CALLXNNNN,  PUSHRR,     ALUAN,        RSTNN,      RETX,    EXX,        JPXNN,      INA_NN_,  CALLXNNNN, null,      ALUAN,        RSTNN,  // D0..DF
                RETX,       FX_POPIX,    JPXNN,       FX_EX_SP_HL, CALLXNNNN,  FX_PUSHIX,  ALUAN,        RSTNN,      RETX,    FX_JP_HL_,  JPXNN,      EXDEHL,   CALLXNNNN, null,      ALUAN,        RSTNN,  // E0..EF
                RETX,       POPRR,       JPXNN,       DI,          CALLXNNNN,  PUSHRR,     ALUAN,        RSTNN,      RETX,    FX_LDSPHL,  JPXNN,      EI,       CALLXNNNN, null,      ALUAN,        RSTNN,  // F0..FF
            };
        }
    }
}
