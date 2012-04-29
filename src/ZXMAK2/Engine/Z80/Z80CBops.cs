/// Description: Z80 CPU Emulator [CB prefixed opcodes part]
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;

namespace ZXMAK2.Engine.Z80
{
    public partial class Z80CPU
    {
        #region #CB

        private void CB_RLC(byte cmd)       // RLC r
        {
            int r = cmd & 7;
            regs[r] = ALU_RLC(regs[r]);
        }
        
        private void CB_RRC(byte cmd)       // RRC r
        {
            int r = cmd & 7;
            regs[r] = ALU_RRC(regs[r]);
        }
        
        private void CB_RL(byte cmd)        // RL r
        {
            int r = cmd & 7;
            regs[r] = ALU_RL(regs[r]);
        }
        
        private void CB_RR(byte cmd)        // RR r
        {
            int r = cmd & 7;
            regs[r] = ALU_RR(regs[r]);
        }
        
        private void CB_SLA(byte cmd)       // SLA r
        {
            int r = cmd & 7;
            regs[r] = ALU_SLA(regs[r]);
        }
        
        private void CB_SRA(byte cmd)       // SRA r
        {
            int r = cmd & 7;
            regs[r] = ALU_SRA(regs[r]);
        }
        
        private void CB_SLL(byte cmd)       // *SLL r
        {
            int r = cmd & 7;
            regs[r] = ALU_SLL(regs[r]);
        }
        
        private void CB_SRL(byte cmd)       // SRL r
        {
            int r = cmd & 7;
            regs[r] = ALU_SRL(regs[r]);
        }
        
        private void CB_BIT(byte cmd)       // BIT r
        {
            ALU_BIT(regs[cmd & 7], (cmd & 0x38) >> 3);
        }
        
        private void CB_RES(byte cmd)       // RES r
        {
            regs[cmd & 7] &= (byte)~(1 << ((cmd & 0x38) >> 3));
        }
        
        private void CB_SET(byte cmd)       // SET r
        {
            regs[cmd & 7] |= (byte)(1 << ((cmd & 0x38) >> 3));
        }

        private void CB_RLCHL(byte cmd)       // RLC (HL)
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_RLC(val);

            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_RRCHL(byte cmd)       // RRC (HL)
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_RRC(val);

            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_RLHL(byte cmd)        // RL (HL)
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_RL(val);

            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_RRHL(byte cmd)        // RR (HL) [15T]
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_RR(val);
            
            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_SLAHL(byte cmd)       // SLA (HL) [15T]
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact++;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_SLA(val);
            
            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_SRAHL(byte cmd)       // SRA (HL) [15T]
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_SRA(val);
            
            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_SLLHL(byte cmd)       // *SLL (HL) [15T]
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_SLL(val);
            
            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_SRLHL(byte cmd)       // SRL (HL)
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val = ALU_SRL(val);
            
            WRMEM(regs.HL, val); Tact += 3;
        }
        
        private void CB_BITHL(byte cmd)       // BIT (HL) [12T]
        {
            // 12T (4, 4, 4)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            ALU_BITMEM(val, (cmd & 0x38) >> 3);
        }

        private void CB_RESHL(byte cmd)       // RES (HL) [15T]
        {
            // 15 (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val &= (byte)~(1 << ((cmd & 0x38) >> 3));

            WRMEM(regs.HL, val); Tact += 3;
        }

        private void CB_SETHL(byte cmd)       // SET (HL) [15T]
        {
            // 15T (4, 4, 4, 3)

            byte val = RDMEM(regs.HL); Tact += 3;
            RDNOMREQ(regs.HL); Tact++;
            val |= (byte)(1 << ((cmd & 0x38) >> 3));
            
            WRMEM(regs.HL, val); Tact += 3;
        }

        #endregion

        private void initExecCB()
        {
            cbopTABLE = new XFXOPDO[256]
            {
//              0       1       2       3       4       5       6         7        8       9       A       B       C       D       E         F
                CB_RLC, CB_RLC, CB_RLC, CB_RLC, CB_RLC, CB_RLC, CB_RLCHL, CB_RLC,  CB_RRC, CB_RRC, CB_RRC, CB_RRC, CB_RRC, CB_RRC, CB_RRCHL, CB_RRC,  // 00..0F
                CB_RL,  CB_RL,  CB_RL,  CB_RL,  CB_RL,  CB_RL,  CB_RLHL,  CB_RL,   CB_RR,  CB_RR,  CB_RR,  CB_RR,  CB_RR,  CB_RR,  CB_RRHL,  CB_RR,   // 10..1F
                CB_SLA, CB_SLA, CB_SLA, CB_SLA, CB_SLA, CB_SLA, CB_SLAHL, CB_SLA,  CB_SRA, CB_SRA, CB_SRA, CB_SRA, CB_SRA, CB_SRA, CB_SRAHL, CB_SRA,  // 20..2F
                CB_SLL, CB_SLL, CB_SLL, CB_SLL, CB_SLL, CB_SLL, CB_SLLHL, CB_SLL,  CB_SRL, CB_SRL, CB_SRL, CB_SRL, CB_SRL, CB_SRL, CB_SRLHL, CB_SRL,  // 30..3F
                CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  // 40..4F
                CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  // 50..5F
                CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  // 60..6F
                CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BIT, CB_BITHL, CB_BIT,  // 70..7F
                CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  // 80..8F
                CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  // 90..9F
                CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  // A0..AF
                CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RES, CB_RESHL, CB_RES,  // B0..BF
                CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  // C0..CF
                CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  // D0..DF
                CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  // E0..EF
                CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SET, CB_SETHL, CB_SET,  // F0..FF
            };
        }
    }
}
