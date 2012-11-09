/// Description: Z80 CPU Emulator [ED prefixed opcode part]
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;

namespace ZXMAK2.Engine.Z80
{
	public partial class Z80CPU
	{
		#region #ED

		private void ED_LDI(byte cmd)
		{
			// 16T (4, 4, 3, 5)

			byte val = RDMEM(regs.HL); Tact += 3;

			WRMEM(regs.DE, val); Tact += 3;
			WRNOMREQ(regs.DE); Tact++;
			WRNOMREQ(regs.DE); Tact++;

			regs.HL++;
			regs.DE++;
			regs.BC--;
			val += regs.A;

			regs.F = (byte)((regs.F & (int)~(ZFLAGS.N | ZFLAGS.H | ZFLAGS.PV | ZFLAGS.F3 | ZFLAGS.F5)) |
				(val & (int)ZFLAGS.F3) | ((val << 4) & (int)ZFLAGS.F5));
			if (regs.BC != 0) regs.F |= (byte)ZFLAGS.PV;
		}

		private void ED_CPI(byte cmd)
		{
			// 16T (4, 4, 3, 5)

			byte cf = (byte)(regs.F & (int)ZFLAGS.C);
			byte val = RDMEM(regs.HL); Tact += 3;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;

			regs.HL++;
			regs.F = (byte)(cpf8b[regs.A * 0x100 + val] + cf);
			if (--regs.BC != 0) regs.F |= (byte)ZFLAGS.PV;
			regs.MW++;
		}

		private void ED_INI(byte cmd)   // INI [16T]
		{
			// 16T (4, 5, 3, 4)

			RDNOMREQ(regs.IR); Tact++;

			byte val = RDPORT(regs.BC); Tact += 3;

			WRMEM(regs.HL, val); Tact += 3;

			regs.MW = (ushort)(regs.BC + 1);
			regs.HL++;
			regs.B--;

			//FUSE
			byte flgtmp = (byte)(val + regs.C + 1);
			regs.F = (byte)(log_f[regs.B] & (int)~ZFLAGS.PV);
			if ((log_f[(flgtmp & 0x07) ^ regs.B] & (int)ZFLAGS.PV) != 0) regs.F |= (byte)ZFLAGS.PV;
			if (flgtmp < val) regs.F |= (byte)(ZFLAGS.H | ZFLAGS.C);
			if ((val & 0x80) != 0) regs.F |= (byte)ZFLAGS.N;

			Tact++; //?? really?
		}

		private void ED_OUTI(byte cmd)  // OUTI [16T]
		{
			// 16 (4, 5, 3, 4)

			RDNOMREQ(regs.IR); Tact++;
			regs.B--;

			byte val = RDMEM(regs.HL); Tact += 3;

			WRPORT(regs.BC, val); Tact += 3;

			regs.MW = (ushort)(regs.BC + 1);
			regs.HL++;

			//FUSE
			byte flgtmp = (byte)(val + regs.L);
			regs.F = (byte)(log_f[regs.B] & (int)~ZFLAGS.PV);
			if ((log_f[(flgtmp & 0x07) ^ regs.B] & (int)ZFLAGS.PV) != 0) regs.F |= (byte)ZFLAGS.PV;
			if (flgtmp < val) regs.F |= (byte)(ZFLAGS.H | ZFLAGS.C);
			if ((val & 0x80) != 0) regs.F |= (byte)ZFLAGS.N;

			Tact++; //?? really?
		}

		private void ED_LDD(byte cmd)
		{
			// 16T (4, 4, 3, 5)

			byte val = RDMEM(regs.HL); Tact += 3;

			WRMEM(regs.DE, val); Tact += 3;
			WRNOMREQ(regs.DE); Tact++;
			WRNOMREQ(regs.DE); Tact++;

			regs.HL--;
			regs.DE--;
			regs.BC--;
			val += regs.A;

			regs.F = (byte)((regs.F & (int)~(ZFLAGS.N | ZFLAGS.H | ZFLAGS.PV | ZFLAGS.F3 | ZFLAGS.F5)) |
				(val & (int)ZFLAGS.F3) | ((val << 4) & (int)ZFLAGS.F5));
			if (regs.BC != 0) regs.F |= (byte)ZFLAGS.PV;
		}

		private void ED_CPD(byte cmd)
		{
			// 16T (4, 4, 3, 5)

			byte cf = (byte)(regs.F & (int)ZFLAGS.C);
			byte val = RDMEM(regs.HL); Tact += 3;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;

			regs.HL--;
			regs.BC--;
			regs.MW--;
			regs.F = (byte)(cpf8b[regs.A * 0x100 + val] + cf);
			if (regs.BC != 0) regs.F |= (byte)ZFLAGS.PV;
		}

		private void ED_IND(byte cmd)   // IND [16T]
		{
			// 16T (4, 5, 3, 4)

			RDNOMREQ(regs.IR); Tact++;

			byte val = RDPORT(regs.BC); Tact += 3;

			WRMEM(regs.HL, val); Tact += 3;

			regs.MW = (ushort)(regs.BC - 1);
			regs.HL--;
			regs.B--;

			//FUSE
			byte flgtmp = (byte)(val + regs.C - 1);
			regs.F = (byte)(log_f[regs.B] & (int)~ZFLAGS.PV);
			if ((log_f[(flgtmp & 0x07) ^ regs.B] & (int)ZFLAGS.PV) != 0) regs.F |= (byte)ZFLAGS.PV;
			if (flgtmp < val) regs.F |= (byte)(ZFLAGS.H | ZFLAGS.C);
			if ((val & 0x80) != 0) regs.F |= (byte)ZFLAGS.N;

			Tact++; // ?? really?
		}

		private void ED_OUTD(byte cmd)  // OUTD [16T]
		{
			// 16T (4, 5, 3, 4)

			RDNOMREQ(regs.IR); Tact++;

			regs.B--;
			byte val = RDMEM(regs.HL); Tact += 3;

			WRPORT(regs.BC, val); Tact += 3;

			regs.MW = (ushort)(regs.BC - 1);
			regs.HL--;

			//FUSE
			byte flgtmp = (byte)(val + regs.L);
			regs.F = (byte)(log_f[regs.B] & (int)~ZFLAGS.PV);
			if ((log_f[(flgtmp & 0x07) ^ regs.B] & (int)ZFLAGS.PV) != 0) regs.F |= (byte)ZFLAGS.PV;
			if (flgtmp < val) regs.F |= (byte)(ZFLAGS.H | ZFLAGS.C);
			if ((val & 0x80) != 0) regs.F |= (byte)ZFLAGS.N;

			Tact++; // ?? really?
		}

		private void ED_LDIR(byte cmd)
		{
			//BC==0 => 16T (4, 4, 3, 5)
			//BC!=0 => 21T (4, 4, 3, 5, 5)

			byte val = RDMEM(regs.HL); Tact += 3;

			WRMEM(regs.DE, val); Tact += 3;
			WRNOMREQ(regs.DE); Tact++;
			WRNOMREQ(regs.DE); Tact++;

			regs.BC--;
			val += regs.A;

			regs.F = (byte)((regs.F & (int)~(ZFLAGS.N | ZFLAGS.H | ZFLAGS.PV | ZFLAGS.F3 | ZFLAGS.F5)) |
				(val & (int)ZFLAGS.F3) | ((val << 4) & (int)ZFLAGS.F5));
			if (regs.BC != 0)
			{
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				regs.PC--;
				regs.MW = regs.PC;
				regs.PC--;
				regs.F |= (byte)ZFLAGS.PV;
			}
			regs.HL++;
			regs.DE++;
		}

		private void ED_CPIR(byte cmd)
		{
			//BC==0 => 16T (4, 4, 3, 5)
			//BC!=0 => 21T (4, 4, 3, 5, 5)

			regs.MW++;
			byte cf = (byte)(regs.F & (int)ZFLAGS.C);
			byte val = RDMEM(regs.HL); Tact += 3;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;

			regs.BC--;
			regs.F = (byte)(cpf8b[regs.A * 0x100 + val] + cf);

			if (regs.BC != 0)
			{
				regs.F |= (byte)ZFLAGS.PV;
				if ((regs.F & (int)ZFLAGS.Z) == 0)
				{
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					regs.PC--;
					regs.MW = regs.PC;
					regs.PC--;
				}
			}
			regs.HL++;
		}

		private void ED_INIR(byte cmd)      // INIR [16T/21T]
		{
			// B==0 => 16T (4, 5, 3, 4)
			// B!=0 => 21T (4, 5, 3, 4, 5)

			RDNOMREQ(regs.IR); Tact++;

			regs.MW = (ushort)(regs.BC + 1);
			byte val = RDPORT(regs.BC); Tact += 3;

			WRMEM(regs.HL, val); Tact += 3;
			regs.B = ALU_DECR(regs.B);
			Tact++; // ?? really?

			if (regs.B != 0)
			{
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				regs.PC -= 2;
				regs.F |= (byte)ZFLAGS.PV;
			}
			else regs.F &= (byte)~ZFLAGS.PV;
			regs.HL++;
		}

		private void ED_OTIR(byte cmd)  // OTIR [16T/21T]
		{
			// B==0 => 16T (4, 5, 3, 4)
			// B!=0 => 21T (4, 5, 3, 4, 5)

			RDNOMREQ(regs.IR); Tact++;

			regs.B = ALU_DECR(regs.B);
			byte val = RDMEM(regs.HL); Tact += 3;

			WRPORT(regs.BC, val); Tact += 3;
			Tact++; //?? really?

			regs.HL++;
			if (regs.B != 0)
			{
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				regs.PC -= 2;
				regs.F |= (byte)ZFLAGS.PV;
			}
			else regs.F &= (byte)~ZFLAGS.PV;
			regs.F &= (byte)~ZFLAGS.C;
			if (regs.L == 0) regs.F |= (byte)ZFLAGS.C;
			regs.MW = (ushort)(regs.BC + 1);
		}

		private void ED_LDDR(byte cmd)
		{
			//BC==0 => 16T (4, 4, 3, 5)
			//BC!=0 => 21T (4, 4, 3, 5, 5)

			byte val = RDMEM(regs.HL); Tact += 3;

			WRMEM(regs.DE, val); Tact += 3;
			WRNOMREQ(regs.DE); Tact++;
			WRNOMREQ(regs.DE); Tact++;

			regs.BC--;
			val += regs.A;

			regs.F = (byte)((regs.F & (int)~(ZFLAGS.N | ZFLAGS.H | ZFLAGS.PV | ZFLAGS.F3 | ZFLAGS.F5)) |
				(val & (int)ZFLAGS.F3) | ((val << 4) & (int)ZFLAGS.F5));
			if (regs.BC != 0)
			{
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				WRNOMREQ(regs.DE); Tact++;
				regs.PC--;
				regs.MW = regs.PC;
				regs.PC--;
				regs.F |= (byte)ZFLAGS.PV;
			}
			regs.HL--;
			regs.DE--;
		}

		private void ED_CPDR(byte cmd)
		{
			// BC==0 => 16T (4, 4, 3, 5)
			// BC!=0 => 21T (4, 4, 3, 5, 5)

			regs.MW--;
			byte cf = (byte)(regs.F & (int)ZFLAGS.C);
			byte val = RDMEM(regs.HL); Tact += 3;

			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			regs.BC--;
			regs.F = (byte)(cpf8b[regs.A * 0x100 + val] + cf);

			if (regs.BC != 0)
			{
				regs.F |= (byte)ZFLAGS.PV;
				if ((regs.F & (int)ZFLAGS.Z) == 0)
				{
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					RDNOMREQ(regs.HL); Tact++;
					regs.PC--;
					regs.MW = regs.PC;
					regs.PC--;
				}
			}
			regs.HL--;
		}

		private void ED_INDR(byte cmd)      // INDR [16T/21T]
		{
			// B==0 => 16 (4, 5, 3, 4)
			// B!=0 => 21 (4, 5, 3, 4, 5)

			RDNOMREQ(regs.IR); Tact++;

			regs.MW = (ushort)(regs.BC - 1);
			byte val = RDPORT(regs.BC); Tact += 3;

			WRMEM(regs.HL, val); Tact += 3;

			regs.B = ALU_DECR(regs.B);
			Tact++; //?? really?

			if (regs.B != 0)
			{
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				WRNOMREQ(regs.HL); Tact++;
				regs.PC -= 2;
				regs.F |= (byte)ZFLAGS.PV;
			}
			else regs.F &= (byte)~ZFLAGS.PV;
			regs.HL--;
		}

		private void ED_OTDR(byte cmd)  //OTDR [16T/21T]
		{
			// B==0 => 16T (4, 5, 3, 4)
			// B!=0 => 21T (4, 5, 3, 4, 5)

			RDNOMREQ(regs.IR); Tact++;

			byte val = RDMEM(regs.HL); Tact += 3;
			regs.B = ALU_DECR(regs.B);

			WRPORT(regs.BC, val); Tact += 3;
			Tact++; //?? really?

			if (regs.B != 0)
			{
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				RDNOMREQ(regs.BC); Tact++;
				regs.PC -= 2;
				regs.F |= (byte)ZFLAGS.PV;
			}
			else regs.F &= (byte)~ZFLAGS.PV;
			regs.F &= (byte)~ZFLAGS.C;
			if (regs.L == 0xFF) regs.F |= (byte)ZFLAGS.C;
			regs.MW = (ushort)(regs.BC - 1);
			regs.HL--;
		}

		private void ED_INRC(byte cmd)      // in R,(c)  [12T] 
		{
			// 12T (4, 4, 4)

			regs.MW = regs.BC;
			byte pval = RDPORT(regs.BC);
			regs.MW++;
			int reg = (cmd & 0x38) >> 3;
			if (reg != REGS.ZR_F)
				regs[reg] = pval;
			regs.F = (byte)(log_f[pval] | (regs.F & (int)ZFLAGS.C));
			Tact += 4;
		}

		private void ED_OUTCR(byte cmd)     // out (c),R [12T]
		{
			// 12T (4, 4, 4)

			regs.MW = regs.BC;
			int reg = (cmd & 0x38) >> 3;

			if (reg != REGS.ZR_F)
				WRPORT(regs.BC, regs[reg]);
			else
				WRPORT(regs.BC, (byte)(CpuType == Z80.CpuType.Z80 ? 0x00 : 0xFF));	// 0 for Z80 and 0xFF for Z84
			regs.MW++;
			Tact += 4;
		}

		private void ED_ADCHLRR(byte cmd)   // adc hl,RR
		{
			RDNOMREQ(regs.IR); Tact++;
			RDNOMREQ(regs.IR); Tact++;
			RDNOMREQ(regs.IR); Tact++;
			RDNOMREQ(regs.IR); Tact++;
			RDNOMREQ(regs.IR); Tact++;
			RDNOMREQ(regs.IR); Tact++;
			RDNOMREQ(regs.IR); Tact++;

			regs.MW = (ushort)(regs.HL + 1);
			int reg = (cmd & 0x30) >> 4;
			byte fl = (byte)((((regs.HL & 0x0FFF) + (regs.GetPair(reg) & 0x0FFF) + (regs.F & (int)ZFLAGS.C)) >> 8) & (int)ZFLAGS.H);
			uint tmp = (uint)((regs.HL & 0xFFFF) + (regs.GetPair(reg) & 0xFFFF) + (regs.F & (int)ZFLAGS.C));  // AF???
			if ((tmp & 0x10000) != 0) fl |= (byte)ZFLAGS.C;
			if ((tmp & 0xFFFF) == 0) fl |= (byte)ZFLAGS.Z;
			int ri = (int)(short)regs.HL + (int)(short)regs.GetPair(reg) + (int)(regs.F & (int)ZFLAGS.C);
			if (ri < -0x8000 || ri >= 0x8000) fl |= (byte)ZFLAGS.PV;
			regs.HL = (ushort)tmp;
			regs.F = (byte)(fl | (regs.H & (int)(ZFLAGS.F3 | ZFLAGS.F5 | ZFLAGS.S)));
		}

		private void ED_SBCHLRR(byte cmd)   // sbc hl,RR
		{
			RDNOMREQ(regs.IR); Tact += 1;
			RDNOMREQ(regs.IR); Tact += 1;
			RDNOMREQ(regs.IR); Tact += 1;
			RDNOMREQ(regs.IR); Tact += 1;
			RDNOMREQ(regs.IR); Tact += 1;
			RDNOMREQ(regs.IR); Tact += 1;
			RDNOMREQ(regs.IR); Tact += 1;

			regs.MW = (ushort)(regs.HL + 1);
			int reg = (cmd & 0x30) >> 4;
			byte fl = (byte)ZFLAGS.N;
			fl |= (byte)((((regs.HL & 0x0FFF) - (regs.GetPair(reg) & 0x0FFF) - (regs.F & (int)ZFLAGS.C)) >> 8) & (int)ZFLAGS.H);
			uint tmp = (uint)((regs.HL & 0xFFFF) - (regs.GetPair(reg) & 0xFFFF) - (regs.F & (int)ZFLAGS.C));  // AF???
			if ((tmp & 0x10000) != 0) fl |= (byte)ZFLAGS.C;
			if ((tmp & 0xFFFF) == 0) fl |= (byte)ZFLAGS.Z;
			int ri = (int)(short)regs.HL - (int)(short)regs.GetPair(reg) - (int)(regs.F & (int)ZFLAGS.C);
			if (ri < -0x8000 || ri >= 0x8000) fl |= (byte)ZFLAGS.PV;
			regs.HL = (ushort)tmp;
			regs.F = (byte)(fl | (regs.H & (int)(ZFLAGS.F3 | ZFLAGS.F5 | ZFLAGS.S)));
		}

		private void ED_LDRR_NN_(byte cmd)  // ld RR,(NN)
		{
			// 20T (4, 4, 3, 3, 3, 3)

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
			regs.SetPair((cmd & 0x30) >> 4, val);
			Tact += 3;
		}

		private void ED_LD_NN_RR(byte cmd)  // ld (NN),RR
		{
			// 20 (4, 4, 3, 3, 3, 3)

			ushort adr = RDMEM(regs.PC);
			regs.PC++;
			Tact += 3;

			adr += (ushort)(RDMEM(regs.PC) * 0x100);
			regs.PC++;
			regs.MW = (ushort)(adr + 1);
			ushort val = regs.GetPair((cmd & 0x30) >> 4);
			Tact += 3;

			WRMEM(adr, (byte)(val & 0xFF));
			Tact += 3;

			WRMEM(regs.MW, (byte)(val >> 8));
			Tact += 3;
		}

		private void ED_RETN(byte cmd)      // reti/retn
		{
			// 14T (4, 4, 3, 3)

			IFF1 = IFF2;
			ushort adr = RDMEM(regs.SP);
			Tact += 3;

			adr += (ushort)(RDMEM(++regs.SP) * 0x100);
			++regs.SP;
			regs.PC = adr;
			regs.MW = adr;
			Tact += 3;
		}

		private void ED_IM(byte cmd)        // im X
		{
			byte mode = (byte)((cmd & 0x18) >> 3);
			if (mode < 2) mode = 1;
			mode--;
			IM = mode;
		}

		private void ED_LDXRA(byte cmd)     // ld I/R,a
		{
			RDNOMREQ(regs.IR); Tact++;

			if ((cmd & 0x08) == 0)   // I
				regs.I = regs.A;
			else
				regs.R = regs.A;
		}

		private void ED_LDAXR(byte cmd)     // ld a,I/R
		{
			RDNOMREQ(regs.IR); Tact++;

			bool rI = (cmd & 0x08) == 0;
			if (rI)   // I
				regs.A = regs.I;
			else
				regs.A = regs.R;

			regs.F = (byte)(((regs.F & (byte)ZFLAGS.C) | log_f[regs.A]) & ~(byte)ZFLAGS.PV);
			if (!INT && (rI ? IFF1 : IFF2)) regs.F |= (byte)ZFLAGS.PV;
		}

		private void ED_RRD(byte cmd)       // RRD
		{
			// 18T (4, 4, 3, 4, 3)

			byte tmp = RDMEM(regs.HL); Tact += 3;

			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;

			regs.MW = (ushort)(regs.HL + 1);
			byte val = (byte)((regs.A << 4) | (tmp >> 4));

			WRMEM(regs.HL, val); Tact += 3;
			regs.A = (byte)((regs.A & 0xF0) | (tmp & 0x0F));
			regs.F = (byte)(log_f[regs.A] | (regs.F & (int)ZFLAGS.C));
		}

		private void ED_RLD(byte cmd)       // RLD
		{
			// 18T (4, 4, 3, 4, 3)

			byte tmp = RDMEM(regs.HL); Tact += 3;

			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;
			RDNOMREQ(regs.HL); Tact++;

			regs.MW = (ushort)(regs.HL + 1);
			byte val = (byte)((regs.A & 0x0F) | (tmp << 4));

			WRMEM(regs.HL, val); Tact += 3;
			regs.A = (byte)((regs.A & 0xF0) | (tmp >> 4));
			regs.F = (byte)(log_f[regs.A] | (regs.F & (int)ZFLAGS.C));
		}

		private void ED_NEG(byte cmd)       // NEG
		{
			regs.F = sbcf[regs.A];
			regs.A = (byte)-regs.A;
		}

		#endregion

		private void initExecED()
		{
			edopTABLE = new XFXOPDO[256] 
			{
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // 00..0F
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // 10..1F
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // 20..2F
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // 30..3F
				ED_INRC, ED_OUTCR, ED_SBCHLRR, ED_LD_NN_RR, ED_NEG, ED_RETN, ED_IM, ED_LDXRA,  ED_INRC, ED_OUTCR, ED_ADCHLRR, ED_LDRR_NN_, ED_NEG, ED_RETN, ED_IM, ED_LDXRA, // 40..4F
				ED_INRC, ED_OUTCR, ED_SBCHLRR, ED_LD_NN_RR, ED_NEG, ED_RETN, ED_IM, ED_LDAXR,  ED_INRC, ED_OUTCR, ED_ADCHLRR, ED_LDRR_NN_, ED_NEG, ED_RETN, ED_IM, ED_LDAXR, // 50..5F
				ED_INRC, ED_OUTCR, ED_SBCHLRR, ED_LD_NN_RR, ED_NEG, ED_RETN, ED_IM, ED_RRD,    ED_INRC, ED_OUTCR, ED_ADCHLRR, ED_LDRR_NN_, ED_NEG, ED_RETN, ED_IM, ED_RLD,   // 60..6F
				ED_INRC, ED_OUTCR, ED_SBCHLRR, ED_LD_NN_RR, ED_NEG, ED_RETN, ED_IM, null,   ED_INRC, ED_OUTCR, ED_ADCHLRR, ED_LDRR_NN_, ED_NEG, ED_RETN, ED_IM, null,  // 70..7F
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // 80..8F
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // 90..9F
				ED_LDI,  ED_CPI,  ED_INI,  ED_OUTI, null, null,  null, null,  ED_LDD,  ED_CPD,  ED_IND,  ED_OUTD, null, null, null, null,             // A0..AF
				ED_LDIR, ED_CPIR, ED_INIR, ED_OTIR, null, null,  null, null,  ED_LDDR, ED_CPDR, ED_INDR, ED_OTDR, null, null, null, null,             // B0..BF
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // C0..CF
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // D0..DF
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null,             // E0..EF
				null, null, null, null, null, null, null, null,  null, null, null, null, null, null, null, null              // F0..FF
			};
		}
	}
}
