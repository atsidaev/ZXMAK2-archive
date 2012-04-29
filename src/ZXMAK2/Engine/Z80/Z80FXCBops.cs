/// Description: Z80 CPU Emulator [FXCB prefixed opcodes part]
/// Author: Alex Makeev
/// Date: 25.09.2011
using System;


namespace ZXMAK2.Engine.Z80
{
    public partial class Z80CPU
    {
        #region #FXCB

        private void FXCB_RLC(byte cmd, ushort adr)       // *RLC r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RLC(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RRC(byte cmd, ushort adr)       // *RRC r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RRC(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RL(byte cmd, ushort adr)        // *RL r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RL(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RR(byte cmd, ushort adr)        // *RR r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RR(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SLA(byte cmd, ushort adr)       // *SLA r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SLA(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SRA(byte cmd, ushort adr)       // *SRA r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SRA(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SLL(byte cmd, ushort adr)       // **SLL r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SLL(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SRL(byte cmd, ushort adr)       // *SRL r
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SRL(val);
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RES(byte cmd, ushort adr)       // *RES   b,r,(IX+drel)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val &= (byte)~(1 << ((cmd & 0x38) >> 3));
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SET(byte cmd, ushort adr)       // *SET   b,r,(IX+drel)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val |= (byte)(1 << ((cmd & 0x38) >> 3));
            regs[cmd & 7] = val;

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RLCIX(byte cmd, ushort adr)       // RLC (IX+) [23T]
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RLC(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RRCIX(byte cmd, ushort adr)       // RRC (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RRC(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RLIX(byte cmd, ushort adr)        // RL (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RL(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_RRIX(byte cmd, ushort adr)        // RR (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_RR(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SLAIX(byte cmd, ushort adr)       // SLA (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SLA(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SRAIX(byte cmd, ushort adr)       // SRA (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SRA(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SLLIX(byte cmd, ushort adr)       // *SLL (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SLL(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SRLIX(byte cmd, ushort adr)       // SRL (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val = ALU_SRL(val);

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_BITIX(byte cmd, ushort adr)       // BIT (IX+)
        {
            // 20T (4, 4, 3, 5, 4)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            ALU_BITMEM(val, (cmd & 0x38) >> 3);
        }

        private void FXCB_RESIX(byte cmd, ushort adr)       // RES (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val &= (byte)~(1 << ((cmd & 0x38) >> 3));

            WRMEM(adr, val); Tact += 3;
        }

        private void FXCB_SETIX(byte cmd, ushort adr)       // SET (IX+)
        {
            // 23T (4, 4, 3, 5, 4, 3)

            byte val = RDMEM(adr); Tact += 3;
            RDNOMREQ(adr); Tact++;
            val |= (byte)(1 << ((cmd & 0x38) >> 3));

            WRMEM(adr, val); Tact += 3;
        }

        #endregion

        private void initFXCB()
        {
            fxcbopTABLE = new FXCBOPDO[256]
            {
                FXCB_RLC,   FXCB_RLC,   FXCB_RLC,   FXCB_RLC,   FXCB_RLC,   FXCB_RLC,   FXCB_RLCIX, FXCB_RLC,    FXCB_RRC,   FXCB_RRC,   FXCB_RRC,   FXCB_RRC,   FXCB_RRC,   FXCB_RRC,   FXCB_RRCIX, FXCB_RRC,    // 00..0F
                FXCB_RL,    FXCB_RL,    FXCB_RL,    FXCB_RL,    FXCB_RL,    FXCB_RL,    FXCB_RLIX,  FXCB_RL,     FXCB_RR,    FXCB_RR,    FXCB_RR,    FXCB_RR,    FXCB_RR,    FXCB_RR,    FXCB_RRIX,  FXCB_RR,     // 10..1F
                FXCB_SLA,   FXCB_SLA,   FXCB_SLA,   FXCB_SLA,   FXCB_SLA,   FXCB_SLA,   FXCB_SLAIX, FXCB_SLA,    FXCB_SRA,   FXCB_SRA,   FXCB_SRA,   FXCB_SRA,   FXCB_SRA,   FXCB_SRA,   FXCB_SRAIX, FXCB_SRA,    // 20..2F
                FXCB_SLL,   FXCB_SLL,   FXCB_SLL,   FXCB_SLL,   FXCB_SLL,   FXCB_SLL,   FXCB_SLLIX, FXCB_SLL,    FXCB_SRL,   FXCB_SRL,   FXCB_SRL,   FXCB_SRL,   FXCB_SRL,   FXCB_SRL,   FXCB_SRLIX, FXCB_SRL,    // 30..3F
                FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  // 40..4F
                FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  // 50..5F
                FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  // 60..6F
                FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX, FXCB_BITIX,  // 70..7F
                FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    // 80..8F
                FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    // 90..9F
                FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    // A0..AF
                FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RES,   FXCB_RESIX, FXCB_RES,    // B0..BF
                FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    // C0..CF
                FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    // D0..DF
                FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    // E0..EF
                FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SET,   FXCB_SETIX, FXCB_SET,    // F0..FF
            };
        }
    }
}
