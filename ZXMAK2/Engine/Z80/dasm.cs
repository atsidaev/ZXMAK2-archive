/// Description: Z80 Disassembler
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;


namespace ZXMAK2.Engine.Z80
{
    public partial class Z80CPU
    {
        public static string GetMnemonic(OnRDBUS MemReader, int Addr, bool Hex, out int MnemLength)
        {
            string sbuf;
            int pos;

            int CurPtr = 0x0000;

            byte selcode = MemReader((ushort)(Addr + CurPtr));
            string Mnem;
            MnemLength = 1;
            string PrefixReg = "*prefix*";

            int PlusMinusOffsetIndex = 0;

            if (selcode == 0xCB)
            {
                CurPtr++;
                MnemLength++;
                Mnem = CBhZ80Code[MemReader((ushort)(Addr + CurPtr))];
            }
            else if (selcode == 0xED)
            {
                CurPtr++;
                MnemLength++;
                Mnem = EDhZ80Code[MemReader((ushort)(Addr + CurPtr))];
                if (Mnem.Length == 0) Mnem = "*NOP";
            }
            else if ((selcode == 0xDD) || (selcode == 0xFD))
            {
            REPREFIX_DDFD:
                selcode = MemReader((ushort)(Addr + CurPtr));
                if (selcode == 0xDD) PrefixReg = "IX";
                else PrefixReg = "IY";
                CurPtr++;
                PlusMinusOffsetIndex = 1;
                MnemLength++;
                if (MemReader((ushort)(Addr + CurPtr)) == 0xCB)
                {
                    CurPtr++;
                    MnemLength++;
                    PlusMinusOffsetIndex = 0;
                    Mnem = DDFDCBhZ80Code[MemReader((ushort)(Addr + CurPtr + 1))];

                    if (Mnem.Length == 0) MnemLength++;  // ??? for CBh
                }
                else if (MemReader((ushort)(Addr + CurPtr)) == 0xED)
                {
                    CurPtr++;
                    MnemLength++;
                    Mnem = EDhZ80Code[MemReader((ushort)(Addr + CurPtr))];
                    if (Mnem.Length == 0) Mnem = "*NOP";
                    if (Mnem[0] != '*') Mnem = "*" + Mnem; // mark undocumented as "*"...
                }
                else if ((MemReader((ushort)(Addr + CurPtr)) == 0xDD) || (MemReader((ushort)(Addr + CurPtr)) == 0xFD))     // DD/FD, DD/FD
                {
                    goto REPREFIX_DDFD;
                }
                else Mnem = DDFDhZ80Code[MemReader((ushort)(Addr + CurPtr))];

                if (Mnem.Length == 0) Mnem = "*" + DirectZ80Code[MemReader((ushort)(Addr + CurPtr))];
            }
            else Mnem = DirectZ80Code[MemReader((ushort)(Addr + CurPtr))];

            if (Mnem.IndexOf("$") < 0) return Mnem;

            do
            {
                if (Mnem.IndexOf("$R") >= 0)  // Prefix register
                {
                    pos = Mnem.IndexOf("$R");
                    if (Mnem.Length <= (pos + 1 + 1))               // ixl/ixh -> xl/xh  !!!!!
                    {
                        Mnem = Mnem.Remove(pos, 2);
                        Mnem = Mnem.Insert(pos, PrefixReg);
                    }
                    else if ((Mnem[pos + 2] == 'L') || (Mnem[pos + 2] == 'H'))
                    {
                        Mnem = Mnem.Remove(pos, 2);
                        Mnem = Mnem.Insert(pos, "" + PrefixReg[1]);
                    }
                    else
                    {
                        Mnem = Mnem.Remove(pos, 2);
                        Mnem = Mnem.Insert(pos, PrefixReg);
                    }
                }
                if (Mnem.IndexOf("$PLUS") >= 0)  // PrefixReg+-offset
                {
                    sbyte val = (sbyte)MemReader((ushort)(Addr + (CurPtr + PlusMinusOffsetIndex)));
                    int uval = val;
                    if (val < 0) uval = -uval;

                    if (val < 0)
                    {
                        if (Hex) sbuf = "-#" + uval.ToString("X2"); //sprintf(buf, "-"Z80ASMHEX"%02X", int(uval));
                        else sbuf = "-" + uval.ToString();          //sprintf(buf, "-%i", int(uval));
                    }
                    else
                    {
                        if (Hex) sbuf = "+#" + uval.ToString("X2");  //sprintf(buf, "+"Z80ASMHEX"%02X", int(uval));
                        else sbuf = "+" + uval.ToString();          //sprintf(buf, "+%i", int(uval));
                    }
                    pos = Mnem.IndexOf("$PLUS");
                    Mnem = Mnem.Remove(pos, 5);
                    Mnem = Mnem.Insert(pos, sbuf);
                    MnemLength++;
                    CurPtr++;
                }


                if (Mnem.IndexOf("$S") >= 0)  // Internal bits value
                {
                    byte code = MemReader((ushort)(Addr + CurPtr));
                    int bitadr = (code & 0x38) >> 3;

                    sbuf = bitadr.ToString();                    //sprintf(buf, "%i", int(bitadr));

                    pos = Mnem.IndexOf("$S");
                    Mnem = Mnem.Remove(pos, 2);
                    Mnem = Mnem.Insert(pos, sbuf);
                }

                if (Mnem.IndexOf("$W") >= 0)  // 2byte value
                {
                    ushort val = (ushort)(MemReader((ushort)(Addr + CurPtr + 1)) + 256 * MemReader((ushort)(Addr + CurPtr + 2)));
                    if (Hex) sbuf = "#" + val.ToString("X4");       // sprintf(buf, Z80ASMHEX"%04X", int(val));
                    else sbuf = val.ToString();                  // sprintf(buf, "%i", int(val));

                    pos = Mnem.IndexOf("$W");
                    Mnem = Mnem.Remove(pos, 2);
                    Mnem = Mnem.Insert(pos, sbuf);
                    MnemLength += 2;
                }

                if (Mnem.IndexOf("$N") >= 0)  // 1byte value
                {
                    byte val = MemReader((ushort)(Addr + CurPtr + 1));
                    if (Hex) sbuf = "#" + val.ToString("X2");       //sprintf(buf, Z80ASMHEX"%02X", int(val));
                    else sbuf = val.ToString();                  //sprintf(buf, "%i", int(val));

                    pos = Mnem.IndexOf("$N");
                    Mnem = Mnem.Remove(pos, 2);
                    Mnem = Mnem.Insert(pos, sbuf);
                    MnemLength++;
                }

                if (Mnem.IndexOf("$T") >= 0)  // Internal bits value ($S)*8
                {
                    byte code = MemReader((ushort)(Addr + CurPtr));
                    int rstadr = ((code & 0x38) >> 3) * 8;

                    if (Hex) sbuf = "#" + rstadr.ToString("X2");    //sprintf(buf, Z80ASMHEX"%02X", int(rstadr));
                    else sbuf = rstadr.ToString();               //sprintf(buf, "%i", int(rstadr));

                    pos = Mnem.IndexOf("$T");
                    Mnem = Mnem.Remove(pos, 2);
                    Mnem = Mnem.Insert(pos, sbuf);
                }

                if (Mnem.IndexOf("$DIS") >= 0)  // 1byte offset value
                {
                    sbyte val = (sbyte)MemReader((ushort)(Addr + CurPtr + 1));
                    //         int adr = (Addr + 2) + val;
                    int adr = (Addr + 2 + CurPtr) + val;
                    adr = (ushort)adr;

                    if (Hex) sbuf = "#" + adr.ToString("X4");       //sprintf(buf, Z80ASMHEX"%04X", int(adr));
                    else sbuf = adr.ToString();                  //sprintf(buf, "%i", int(adr));

                    pos = Mnem.IndexOf("$DIS");
                    Mnem = Mnem.Remove(pos, 4);
                    Mnem = Mnem.Insert(pos, sbuf);
                    MnemLength++;
                }

            } while (Mnem.IndexOf("$") >= 0);

            return Mnem;
        }

        #region Tables

        private static readonly string[] DirectZ80Code = new string[256] 
		{
		    #region data
			"NOP",                  // 0x00
			"LD     BC,$W",
			"LD     (BC),A",
			"INC    BC",
			"INC    B",
			"DEC    B",
			"LD     B,$N",
			"RLCA",
			"EX     AF,AF'",
			"ADD    HL,BC",
			"LD     A,(BC)",
			"DEC    BC",
			"INC    C",
			"DEC    C",
			"LD     C,$N",
			"RRCA",
			"DJNZ   $DIS",            // 0x10
			"LD     DE,$W",
			"LD     (DE),A",
			"INC    DE",
			"INC    D",
			"DEC    D",
			"LD     D,$N",
			"RLA",
			"JR     $DIS",
			"ADD    HL,DE",
			"LD     A,(DE)",
			"DEC    DE",
			"INC    E",
			"DEC    E",
			"LD     E,$N",
			"RRA",
			"JR     NZ,$DIS",           // 0x20
			"LD     HL,$W",
			"LD     ($W),HL",
			"INC    HL",
			"INC    H",
			"DEC    H",
			"LD     H,$N",
			"DAA",
			"JR     Z,$DIS",
			"ADD    HL,HL",
			"LD     HL,($W)",
			"DEC    HL",
			"INC    L",
			"DEC    L",
			"LD     L,$N",
			"CPL",
			"JR     NC,$DIS",           // 0x30
			"LD     SP,$W",
			"LD     ($W),A",
			"INC    SP",
			"INC    (HL)",
			"DEC    (HL)",
			"LD     (HL),$N",
			"SCF",
			"JR     C,$DIS",            // 0x38
			"ADD    HL,SP",
			"LD     A,($W)",
			"DEC    SP",
			"INC    A",
			"DEC    A",
			"LD     A,$N",
			"CCF",                   // 0x3F
			"LD     B,B",            // 0x40
			"LD     B,C",
			"LD     B,D",
			"LD     B,E",
			"LD     B,H",
			"LD     B,L",
			"LD     B,(HL)",
			"LD     B,A",
			"LD     C,B",
			"LD     C,C",
			"LD     C,D",
			"LD     C,E",
			"LD     C,H",
			"LD     C,L",
			"LD     C,(HL)",
			"LD     C,A",
	
			"LD     D,B",            // 0x50
			"LD     D,C",
			"LD     D,D",
			"LD     D,E",
			"LD     D,H",
			"LD     D,L",
			"LD     D,(HL)",
			"LD     D,A",
	
			"LD     E,B",
			"LD     E,C",
			"LD     E,D",
			"LD     E,E",
			"LD     E,H",
			"LD     E,L",
			"LD     E,(HL)",
			"LD     E,A",
	
			"LD     H,B",            // 0x60
			"LD     H,C",
			"LD     H,D",
			"LD     H,E",
			"LD     H,H",
			"LD     H,L",
			"LD     H,(HL)",
			"LD     H,A",
	
			"LD     L,B",
			"LD     L,C",
			"LD     L,D",
			"LD     L,E",
			"LD     L,H",
			"LD     L,L",
			"LD     L,(HL)",
			"LD     L,A",

			"LD     (HL),B",         // 0x70
			"LD     (HL),C",
			"LD     (HL),D",
			"LD     (HL),E",
			"LD     (HL),H",
			"LD     (HL),L",
			"HALT",
			"LD     (HL),A",
	
			"LD     A,B",
			"LD     A,C",
			"LD     A,D",
			"LD     A,E",
			"LD     A,H",
			"LD     A,L",
			"LD     A,(HL)",
			"LD     A,A",

			"ADD    A,B",            // 0x80
			"ADD    A,C",
			"ADD    A,D",
			"ADD    A,E",
			"ADD    A,H",
			"ADD    A,L",
			"ADD    A,(HL)",
			"ADD    A,A",

			"ADC    A,B",
			"ADC    A,C",
			"ADC    A,D",
			"ADC    A,E",
			"ADC    A,H",
			"ADC    A,L",
			"ADC    A,(HL)",
			"ADC    A,A",

			"SUB    B",              // 0x90
			"SUB    C",
			"SUB    D",
			"SUB    E",
			"SUB    H",
			"SUB    L",
			"SUB    (HL)",
			"SUB    A",

			"SBC    A,B",
			"SBC    A,C",
			"SBC    A,D",
			"SBC    A,E",
			"SBC    A,H",
			"SBC    A,L",
			"SBC    A,(HL)",
			"SBC    A,A",

			"AND    B",              // 0xA0
			"AND    C",
			"AND    D",
			"AND    E",
			"AND    H",
			"AND    L",
			"AND    (HL)",
			"AND    A",

			"XOR    B",
			"XOR    C",
			"XOR    D",
			"XOR    E",
			"XOR    H",
			"XOR    L",
			"XOR    (HL)",
			"XOR    A",

			"OR     B",              // 0xB0
			"OR     C",
			"OR     D",
			"OR     E",
			"OR     H",
			"OR     L",
			"OR     (HL)",
			"OR     A",

			"CP     B",
			"CP     C",
			"CP     D",
			"CP     E",
			"CP     H",
			"CP     L",
			"CP     (HL)",
			"CP     A",         // 0xBF

			"RET    NZ",             // 0xC0
			"POP    BC",
			"JP     NZ,$W",
			"JP     $W",
			"CALL   NZ,$W",
			"PUSH   BC",
			"ADD    A,$N",
			"RST    $T",
			"RET    Z",
			"RET",
			"JP     Z,$W",
			"*CB",          // 0xCB
			"CALL   Z,$W",
			"CALL   $W",
			"ADC    A,$N",
			"RST    $T",
        
			"RET    NC",             // 0xD0
			"POP    DE",
			"JP     NC,$W",
			"OUT    ($N),A",
			"CALL   NC,$W",
			"PUSH   DE",
			"SUB    $N",
			"RST    $T",
			"RET    C",
			"EXX",
			"JP     C,$W",
			"IN     A,($N)",
			"CALL   C,$W",
			"*IX",          // 0xDD
			"SBC    A,$N",
			"RST    $T",
	        
			"RET    PO",             // 0xE0
			"POP    HL",
			"JP     PO,$W",
			"EX     (SP),HL",
			"CALL   PO,$W",
			"PUSH   HL",
			"AND    $N",
			"RST    $T",
			"RET    PE",
			"JP     (HL)",
			"JP     PE,$W",
			"EX     DE,HL",
			"CALL   PE,$W",
			"*ED",          // 0xED
			"XOR    $N",
			"RST    $T",
        
			"RET    P",              // 0xF0
			"POP    AF",
			"JP     P,$W",
			"DI",
			"CALL   P,$W",
			"PUSH   AF",
			"OR     $N",
			"RST    $T",
			"RET    M",
			"LD     SP,HL",
			"JP     M,$W",
			"EI",
			"CALL   M,$W",
			"*IY",          // 0xFD
			"CP     $N",
			"RST    $T"
			#endregion
		};

        private static readonly string[] CBhZ80Code = new string[256] 
		{
            #region data
        "RLC    B",
        "RLC    C",
        "RLC    D",
        "RLC    E",
        "RLC    H",
        "RLC    L",
        "RLC    (HL)",
        "RLC    A",

        "RRC    B",
        "RRC    C",
        "RRC    D",
        "RRC    E",
        "RRC    H",
        "RRC    L",
        "RRC    (HL)",
        "RRC    A",

        "RL     B",
        "RL     C",
        "RL     D",
        "RL     E",
        "RL     H",
        "RL     L",
        "RL     (HL)",
        "RL     A",

        "RR     B",
        "RR     C",
        "RR     D",
        "RR     E",
        "RR     H",
        "RR     L",
        "RR     (HL)",
        "RR     A",

        "SLA    B",
        "SLA    C",
        "SLA    D",
        "SLA    E",
        "SLA    H",
        "SLA    L",
        "SLA    (HL)",
        "SLA    A",

        "SRA    B",
        "SRA    C",
        "SRA    D",
        "SRA    E",
        "SRA    H",
        "SRA    L",
        "SRA    (HL)",
        "SRA    A",

        "*SLL   B",               // SLL or SLI (UNDOCUMENTED)
        "*SLL   C",
        "*SLL   D",
        "*SLL   E",
        "*SLL   H",
        "*SLL   L",
        "*SLL   (HL)",
        "*SLL   A",

        "SRL    B",
        "SRL    C",
        "SRL    D",
        "SRL    E",
        "SRL    H",
        "SRL    L",
        "SRL    (HL)",
        "SRL    A",


        "BIT    $S,B",             // Bit 0
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 1
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 2
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 3
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 4
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 5
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 6
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",

        "BIT    $S,B",             // Bit 7
        "BIT    $S,C",
        "BIT    $S,D",
        "BIT    $S,E",
        "BIT    $S,H",
        "BIT    $S,L",
        "BIT    $S,(HL)",
        "BIT    $S,A",


        "RES    $S,B",             // Bit 0
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 1
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 2
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 3
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 4
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 5
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 6
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "RES    $S,B",             // Bit 7
        "RES    $S,C",
        "RES    $S,D",
        "RES    $S,E",
        "RES    $S,H",
        "RES    $S,L",
        "RES    $S,(HL)",
        "RES    $S,A",

        "SET    $S,B",             // Bit 0
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 1
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 2
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 3
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 4
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 5
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 6
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",

        "SET    $S,B",             // Bit 7
        "SET    $S,C",
        "SET    $S,D",
        "SET    $S,E",
        "SET    $S,H",
        "SET    $S,L",
        "SET    $S,(HL)",
        "SET    $S,A",
            #endregion
		};

        private static readonly string[] EDhZ80Code = new string[256] 
		{
            #region data
        "",             // 0x00
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x08
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x10
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x18
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x20
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x28
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x30
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x38
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "IN     B,(C)",       // 0x40
        "OUT    (C),B",       // 0x41
        "SBC    HL,BC",       // 0x42
        "LD     ($W),BC",     // 0x43
        "NEG",                // 0x44
        "RETN",               // 0x45
        "IM     0",           // 0x46
        "LD     I,A",         // 0x47

        "IN     C,(C)",       // 0x48
        "OUT    (C),C",       // 0x49
        "ADC    HL,BC",       // 0x4A
        "LD     BC,($W)",     // 0x4B
        "*NEG",               // 0x4C  undocumented
        "RETI",               // 0x4D
        "*IM    0",           // 0x4E
        "LD     R,A",         // 0x4F

        "IN     D,(C)",       // 0x50
        "OUT    (C),D",       // 0x51
        "SBC    HL,DE",       // 0x52
        "LD     ($W),DE",     // 0x53
        "*NEG",               // 0x54
        "*RETN",              // 0x55
        "IM     1",           // 0x56
        "LD     A,I",         // 0x57

        "IN     E,(C)",       // 0x58
        "OUT    (C),E",       // 0x59
        "ADC    HL,DE",       // 0x5A
        "LD     DE,($W)",     // 0x5B
        "*NEG",               // 0x5C
        "*RETN",              // 0x5D
        "IM     2",           // 0x5E
        "LD     A,R",         // 0x5F

        "IN     H,(C)",       // 0x60
        "OUT    (C),H",       // 0x61
        "SBC    HL,HL",       // 0x62
        "LD     ($W),HL",     // 0x63
        "*NEG",               // 0x64
        "*RETN",              // 0x65
        "*IM    0",           // 0x66
        "RRD",                // 0x67

        "IN     L,(C)",       // 0x68
        "OUT    (C),L",       // 0x69
        "ADC    HL,HL",       // 0x6A
        "LD     HL,($W)",     // 0x6B
        "*NEG",               // 0x6C
        "*RETN",              // 0x6D
        "*IM    0",           // 0x6E
        "RLD",                // 0x6F

        "*IN    (C)",         // 0x70   NON STORED INPUT PORT! (Only flags)
        "*OUT   (C),0",       // 0x71
        "SBC    HL,SP",       // 0x72
        "LD     ($W),SP",     // 0x73
        "*NEG",               // 0x74
        "*RETN",              // 0x75
        "*IM    1",           // 0x76
        "*NOP",               // 0x77

        "IN     A,(C)",       // 0x78
        "OUT    (C),A",       // 0x79
        "ADC    HL,SP",       // 0x7A
        "LD     SP,($W)",     // 0x7B
        "*NEG",               // 0x7C
        "*RETN",              // 0x7D
        "*IM    2",           // 0x7E
        "*NOP",               // 0x7F

        "",             // 0x80
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x88
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x90
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x98
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "LDI",          // 0xA0
        "CPI",          // 0xA1
        "INI",          // 0xA2
        "OUTI",         // 0xA3
        "",
        "",
        "",
        "",

        "LDD",          // 0xA8
        "CPD",          // 0xA9
        "IND",          // 0xAA
        "OUTD",         // 0xAB
        "",
        "",
        "",
        "",

        "LDIR",         // 0xB0
        "CPIR",         // 0xB1
        "INIR",         // 0xB2
        "OTIR",         // 0xB3
        "",
        "",
        "",
        "",

        "LDDR",         // 0xB8
        "CPDR",         // 0xB9
        "INDR",         // 0xBA
        "OTDR",         // 0xBB
        "",
        "",
        "",
        "",

        "",             // 0xC0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xC8
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xD0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xD8
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xE0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xE8
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xF0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xF8
        "",
        "",
        "",
        "",
        "",
        "",
        ""
            #endregion
		};

        private static readonly string[] DDFDhZ80Code = new string[256] 
		{
            #region data
        "",                     // 0x00
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x08
        "ADD    $R,BC",
        "",
        "",
        "",
        "",
        "",
        "",

        "",            // 0x10
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x18
        "ADD    $R,DE",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x20
        "LD     $R,$W",
        "LD     ($W),$R",
        "INC    $R",
        "*INC   $RH",
        "*DEC   $RH",
        "*LD    $RH,$N",
        "",

        "",             // 0x28
        "ADD    $R,$R",
        "LD     $R,($W)",
        "DEC    $R",
        "*INC   $RL",
        "*DEC   $RL",
        "*LD    $RL,$N",
        "",

        "",             // 0x30
        "",
        "",
        "",
        "INC    ($R$PLUS)",
        "DEC    ($R$PLUS)",
        "LD     ($R$PLUS),$N",
        "",

        "",             // 0x38
        "ADD    $R,SP",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0x40
        "",
        "",
        "",
        "*LD    B,$RH",
        "*LD    B,$RL",
        "LD     B,($R$PLUS)",
        "",

        "",             // 0x48
        "",
        "",
        "",
        "*LD    C,$RH",
        "*LD    C,$RL",
        "LD     C,($R$PLUS)",
        "",

        "",             // 0x50
        "",
        "",
        "",
        "*LD    D,$RH",
        "*LD    D,$RL",
        "LD     D,($R$PLUS)",
        "",

        "",             // 0x58
        "",
        "",
        "",
        "*LD    E,$RH",
        "*LD    E,$RL",
        "LD     E,($R$PLUS)",
        "",

        "*LD    $RH,B",    // 0x60
        "*LD    $RH,C",
        "*LD    $RH,D",
        "*LD    $RH,E",
        "*LD    $RH,$RH",
        "*LD    $RH,$RL",
        "LD     H,($R$PLUS)",
        "*LD    $RH,A",

        "*LD    $RL,B",    // 0x68
        "*LD    $RL,C",
        "*LD    $RL,D",
        "*LD    $RL,E",
        "*LD    $RL,$RH",
        "*LD    $RL,$RL",
        "LD     L,($R$PLUS)",
        "*LD    $RL,A",

        "LD     ($R$PLUS),B", // 0x70
        "LD     ($R$PLUS),C",
        "LD     ($R$PLUS),D",
        "LD     ($R$PLUS),E",
        "LD     ($R$PLUS),H",
        "LD     ($R$PLUS),L",
        "",
        "LD     ($R$PLUS),A",

        "",             // 0x78
        "",
        "",
        "",
        "*LD    A,$RH",
        "*LD    A,$RL",
        "LD     A,($R$PLUS)",
        "",

        "",             // 0x80
        "",
        "",
        "",
        "*ADD   A,$RH",
        "*ADD   A,$RL",
        "ADD    A,($R$PLUS)",
        "",

        "",             // 0x88
        "",
        "",
        "",
        "*ADC   A,$RH",
        "*ADC   A,$RL",
        "ADC    A,($R$PLUS)",
        "",

        "",             // 0x90
        "",
        "",
        "",
        "*SUB   $RH",
        "*SUB   $RL",
        "SUB    ($R$PLUS)",
        "",

        "",             // 0x98
        "",
        "",
        "",
        "*SBC   A,$RH",
        "*SBC   A,$RL",
        "SBC    A,($R$PLUS)",
        "",

        "",             // 0xA0
        "",
        "",
        "",
        "*AND   $RH",
        "*AND   $RL",
        "AND    ($R$PLUS)",
        "",

        "",             // 0xA8
        "",
        "",
        "",
        "*XOR   $RH",
        "*XOR   $RL",
        "XOR    ($R$PLUS)",
        "",

        "",             // 0xB0
        "",
        "",
        "",
        "*OR    $RH",
        "*OR    $RL",
        "OR     ($R$PLUS)",
        "",

        "",             // 0xB8
        "",
        "",
        "",
        "*CP    $RH",
        "*CP    $RL",
        "CP     ($R$PLUS)",
        "",

        "",             // 0xC0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xC8
        "",
        "",
        "*DD/FD,CB",
        "",
        "",
        "",
        "",

        "",             // 0xD0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xD8
        "",
        "",
        "",
        "",
        "*DD/FD,DD",
        "",
        "",

        "",             // 0xE0
        "POP    $R",
        "",
        "EX     (SP),$R",
        "",
        "PUSH   $R",
        "",
        "",

        "",             // 0xE8
        "JP     ($R)",
        "",
        "",     // EX DE,HL???
        "",
        "*DD/FD,ED",
        "",
        "",

        "",             // 0xF0
        "",
        "",
        "",
        "",
        "",
        "",
        "",

        "",             // 0xF8
        "LD     SP,$R",
        "",
        "",
        "",
        "*DD/FD,FD",
        "",
        ""
            #endregion
		};

        private static readonly string[] DDFDCBhZ80Code = new string[256] 
		{
            #region data
        "*RLC   B,($R$PLUS)",      // 0x00
        "*RLC   C,($R$PLUS)",
        "*RLC   D,($R$PLUS)",
        "*RLC   E,($R$PLUS)",
        "*RLC   H,($R$PLUS)",
        "*RLC   L,($R$PLUS)",
        "RLC    ($R$PLUS)",
        "*RLC   A,($R$PLUS)",

        "*RRC   B,($R$PLUS)",      // 0x08
        "*RRC   C,($R$PLUS)",
        "*RRC   D,($R$PLUS)",
        "*RRC   E,($R$PLUS)",
        "*RRC   H,($R$PLUS)",
        "*RRC   L,($R$PLUS)",
        "RRC    ($R$PLUS)",
        "*RRC   A,($R$PLUS)",

        "*RL    B,($R$PLUS)",       // 0x10
        "*RL    C,($R$PLUS)",
        "*RL    D,($R$PLUS)",
        "*RL    E,($R$PLUS)",
        "*RL    H,($R$PLUS)",
        "*RL    L,($R$PLUS)",
        "RL     ($R$PLUS)",
        "*RL    A,($R$PLUS)",

        "*RR    B,($R$PLUS)",       // 0x18
        "*RR    C,($R$PLUS)",
        "*RR    D,($R$PLUS)",
        "*RR    E,($R$PLUS)",
        "*RR    H,($R$PLUS)",
        "*RR    L,($R$PLUS)",
        "RR     ($R$PLUS)",
        "*RR    A,($R$PLUS)",

        "*SLA   B,($R$PLUS)",      // 0x20
        "*SLA   C,($R$PLUS)",
        "*SLA   D,($R$PLUS)",
        "*SLA   E,($R$PLUS)",
        "*SLA   H,($R$PLUS)",
        "*SLA   L,($R$PLUS)",
        "SLA    ($R$PLUS)",
        "*SLA   A,($R$PLUS)",

        "*SRA   B,($R$PLUS)",      // 0x28
        "*SRA   C,($R$PLUS)",
        "*SRA   D,($R$PLUS)",
        "*SRA   E,($R$PLUS)",
        "*SRA   H,($R$PLUS)",
        "*SRA   L,($R$PLUS)",
        "SRA    ($R$PLUS)",
        "*SRA   A,($R$PLUS)",

        "*SLL   B,($R$PLUS)",      // 0x30
        "*SLL   C,($R$PLUS)",
        "*SLL   D,($R$PLUS)",
        "*SLL   E,($R$PLUS)",
        "*SLL   H,($R$PLUS)",
        "*SLL   L,($R$PLUS)",
        "*SLL   ($R$PLUS)",         // SLI or SLL
        "*SLL   A,($R$PLUS)",

        "*SRL   B,($R$PLUS)",      // 0x38
        "*SRL   C,($R$PLUS)",
        "*SRL   D,($R$PLUS)",
        "*SRL   E,($R$PLUS)",
        "*SRL   H,($R$PLUS)",
        "*SRL   L,($R$PLUS)",
        "SRL    ($R$PLUS)",
        "*SRL   A,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x40
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 0
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x48
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 1
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x50
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 2
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x58
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 3
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x60
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 4
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x68
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 5
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x70
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 6
        "*BIT   $S,($R$PLUS)",

        "*BIT   $S,($R$PLUS)",    // 0x78
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "*BIT   $S,($R$PLUS)",
        "BIT    $S,($R$PLUS)",             // BIT 7
        "*BIT   $S,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0x80
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 0
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0x88
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 1
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0x90
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 2
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0x98
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 3
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0xA0
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 4
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0xA8
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 5
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0xB0
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 6
        "*RES   $S,A,($R$PLUS)",

        "*RES   $S,B,($R$PLUS)",   // 0xB8
        "*RES   $S,C,($R$PLUS)",
        "*RES   $S,D,($R$PLUS)",
        "*RES   $S,E,($R$PLUS)",
        "*RES   $S,H,($R$PLUS)",
        "*RES   $S,L,($R$PLUS)",
        "RES    $S,($R$PLUS)",             // RES 7
        "*RES   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xC0
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 0
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xC8
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 1
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xD0
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 2
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xD8
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 3
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xE0
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 4
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xE8
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 5
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xF0
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 6
        "*SET   $S,A,($R$PLUS)",

        "*SET   $S,B,($R$PLUS)",   // 0xF8
        "*SET   $S,C,($R$PLUS)",
        "*SET   $S,D,($R$PLUS)",
        "*SET   $S,E,($R$PLUS)",
        "*SET   $S,H,($R$PLUS)",
        "*SET   $S,L,($R$PLUS)",
        "SET    $S,($R$PLUS)",             // SET 7
        "*SET   $S,A,($R$PLUS)",
            #endregion
		};

        #endregion
    }
}