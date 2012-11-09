/// Description: Z80 CPU Emulator
/// Author: Alex Makeev
/// Date: 13.04.2007
using System;
using System.Threading;
using System.Collections.Generic;

namespace ZXMAK2.Engine.Z80
{
	public delegate void OnNOMREQ(ushort addr);
	public delegate void OnBUSAK();
	public delegate byte OnRDBUS(ushort ADDR);
	public delegate void OnWRBUS(ushort ADDR, byte value);
	public enum OPFX { NONE, IX, IY }
	public enum OPXFX { NONE, CB, ED }
	public enum CpuType { Z80 = 0, Z84 }


	public partial class Z80CPU
	{
		public CpuType CpuType = CpuType.Z80;
		public int RzxCounter = 0;
		public long Tact = 0;
		public REGS regs = new REGS();
		public bool HALTED;
		public bool IFF1;
		public bool IFF2;
		public byte IM;
		public bool BINT;       // last opcode was EI or DD/FD prefix (to prevent INT handling)
		public OPFX FX;
		public OPXFX XFX;

		public bool INT = false;
		public bool NMI = false;
		public bool RST = false;
		public byte BUS = 0xFF;     // state of free data bus

		public OnBUSAK RESET = null;
		public OnBUSAK NMIACK_M1 = null;
		public OnBUSAK INTACK_M1 = null;
		public OnRDBUS RDMEM_M1 = null;
		public OnRDBUS RDMEM = null;
		public OnWRBUS WRMEM = null;
		public OnRDBUS RDPORT = null;
		public OnWRBUS WRPORT = null;
		public OnNOMREQ RDNOMREQ = null;
		public OnNOMREQ WRNOMREQ = null;

		public Z80CPU()
		{
			ALU_INIT();
			initExec();
			initExecFX();
			initExecED();
			initExecCB();
			initFXCB();
		}


		public void ExecCycle()
		{
			byte cmd = 0;
			if (XFX == OPXFX.NONE && FX == OPFX.NONE)
			{
				if (checkSignals())
					return;
				cmd = RDMEM_M1(regs.PC);
			}
			else
			{
				if (checkSignals())
					return;
				cmd = RDMEM(regs.PC);
			}
			Tact += 3;
			regs.PC++;
			if (XFX == OPXFX.CB)
			{
				BINT = false;
				ExecCB(cmd);
				XFX = OPXFX.NONE;
				FX = OPFX.NONE;
			}
			else if (XFX == OPXFX.ED)
			{
				refresh();
				BINT = false;
				ExecED(cmd);
				XFX = OPXFX.NONE;
				FX = OPFX.NONE;
			}
			else if (cmd == 0xDD)
			{
				refresh();
				FX = OPFX.IX;
				BINT = true;
			}
			else if (cmd == 0xFD)
			{
				refresh();
				FX = OPFX.IY;
				BINT = true;
			}
			else if (cmd == 0xCB)
			{
				refresh();
				XFX = OPXFX.CB;
				BINT = true;
			}
			else if (cmd == 0xED)
			{
				refresh();
				XFX = OPXFX.ED;
				BINT = true;
			}
			else
			{
				refresh();
				BINT = false;
				ExecDirect(cmd);
				FX = OPFX.NONE;
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
			if (FX != OPFX.NONE)
			{
				// elapsed T: 4, 4, 3
				// will be T: 4, 4, 3, 5

				int drel = (sbyte)cmd;

				regs.MW = FX == OPFX.IX ? (ushort)(regs.IX + drel) : (ushort)(regs.IY + drel);
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
			XFXOPDO opdo = FX == OPFX.NONE ? opTABLE[cmd] : fxopTABLE[cmd];
			if (opdo != null)
				opdo(cmd);
		}

		private void refresh()
		{
			regs.R = (byte)(((regs.R + 1) & 0x7F) | (regs.R & 0x80));
			Tact += 1;
			RzxCounter++;
		}

		private bool checkSignals()
		{
			if (RST)    // RESET
			{
				RESET();
				refresh();      //+1T
				FX = OPFX.NONE;
				XFX = OPXFX.NONE;
				HALTED = false;
				IFF1 = false;
				IFF2 = false;
				IM = 0;         // ?
				regs.PC = 0;
				regs.IR = 0;

				//regs.SP = 0xFFFF; //?
				//regs.AF = 0xFFFF; //?

				Tact += 4 - 1;      // total should be 3T?
				return true;
			}
			else if (NMI)
			{
				// 10T (4, 3, 3)

				if (HALTED) // workaround for Z80 snapshot halt issue + comfortable debugging
					regs.PC++;

				NMIACK_M1();
				Tact += 3;
				refresh();

				IFF1 = false;
				HALTED = false;
				regs.SP--;
				WRMEM(regs.SP, (byte)(regs.PC >> 8));
				Tact += 3;

				regs.SP--;
				WRMEM(regs.SP, (byte)(regs.PC & 0xFF));
				regs.PC = 0x0066;
				Tact += 3;

				return true;
			}
			else if (INT && (!BINT) && IFF1)
			{
				// IM0: ?13T (?,?,?)
				// IM1: 13T (?,?,?)
				// IM2: ?19T (?,?,?,?)

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
				IFF2 = false;
				HALTED = false;

				WRMEM(regs.SP, (byte)(regs.PC >> 8));   // M2: 3T write PCH; SP--
				regs.SP--;
				Tact += 3;

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
					ushort adr = (ushort)((regs.IR & 0xFF00) | BUS);
					regs.MW = RDMEM(adr);               // M4: 3T read VL
					Tact += 3;

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