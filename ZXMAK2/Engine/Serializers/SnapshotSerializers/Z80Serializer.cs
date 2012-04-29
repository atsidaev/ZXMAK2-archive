using System;
using System.IO;

using ZXMAK2.Engine.Z80;
using ZXMAK2.Engine.Interfaces;


namespace ZXMAK2.Engine.Serializers.SnapshotSerializers
{
    public class Z80Serializer : SnapshotSerializerBase
	{
		public Z80Serializer(SpectrumBase spec)
            : base(spec)
		{
		}

		#region FormatSerializer

		public override string FormatExtension { get { return "Z80"; } }

		public override bool CanDeserialize { get { return true; } }
		public override bool CanSerialize { get { return true; } }

		public override void Deserialize(Stream stream)
		{
			loadFromStream(stream);
            UpdateState();
        }

		public override void Serialize(Stream stream)
		{
			saveToStream(stream);
		}

		#endregion


		#region private

		private void loadFromStream(Stream stream)
		{
			byte[] hdr = new byte[30];
			byte[] hdr1 = new byte[25];
			int version = 1;

			stream.Read(hdr, 0, 30); // 30 bytes

			if (hdr[Z80HDR_FLAGS] == 0xFF)
				hdr[Z80HDR_FLAGS] = 0x01; // Because of compatibility, if byte 12 is 255, it has to be regarded as being 1.

			if (getUInt16(hdr, Z80HDR_PC) == 0)  // if Version >= 1.45 ( 2.01 or 3.0 )
			{
				version = 2;
				stream.Read(hdr1, 0, 25);

				if (hdr1[Z80HDR1_EXTSIZE] == 54)   // if Version is 3.0
				{
					version = 3;
					byte[] bhdr2 = new byte[31];
					stream.Read(bhdr2, 0, 31);
				}
				else if (hdr1[Z80HDR1_EXTSIZE] != 23)
				{
					string msg = string.Format(
						"Z80 format version not recognized!\n" +
						"(ExtensionSize = {0},\n" +
						"supported only ExtensionSize={{0(old format), 23, 54}})",
						hdr1[Z80HDR1_EXTSIZE]);
					LogAgent.Warn("{0}", msg);
					DialogProvider.Show(
                        msg, 
                        "Z80 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
					return;
				}
			}

            InitStd128K();

			IMemoryDevice memory = _spec.BusManager.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
			IUlaDevice ula = _spec.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;

			// Load registers:
			_spec.CPU.regs.A = hdr[Z80HDR_A];
			_spec.CPU.regs.F = hdr[Z80HDR_F];
			_spec.CPU.regs.HL = getUInt16(hdr, Z80HDR_HL);
			_spec.CPU.regs.DE = getUInt16(hdr, Z80HDR_DE);
			_spec.CPU.regs.BC = getUInt16(hdr, Z80HDR_BC);
			_spec.CPU.regs._AF = (ushort)(hdr[Z80HDR_A_] * 0x100 + hdr[Z80HDR_F_]);
			_spec.CPU.regs._HL = getUInt16(hdr, Z80HDR_HL_);
			_spec.CPU.regs._DE = getUInt16(hdr, Z80HDR_DE_);
			_spec.CPU.regs._BC = getUInt16(hdr, Z80HDR_BC_);
			_spec.CPU.regs.IX = getUInt16(hdr, Z80HDR_IX);
			_spec.CPU.regs.IY = getUInt16(hdr, Z80HDR_IY);
			_spec.CPU.regs.SP = getUInt16(hdr, Z80HDR_SP);
			_spec.CPU.regs.I = hdr[Z80HDR_I];
			_spec.CPU.regs.R = (byte)(hdr[Z80HDR_R7F] | (((hdr[Z80HDR_FLAGS] & 1) != 0) ? 0x80 : 0x00));
			_spec.CPU.regs.PC = (version == 1) ? getUInt16(hdr, Z80HDR_PC) : getUInt16(hdr1, Z80HDR1_PC);

			_spec.CPU.BINT = false;
			_spec.CPU.XFX = Z80.OPXFX.NONE;
			_spec.CPU.FX = Z80.OPFX.NONE;
			_spec.CPU.HALTED = false;

			// CPU.Status...
			_spec.CPU.IFF1 = hdr[Z80HDR_IFF1] != 0;
			_spec.CPU.IFF2 = hdr[Z80HDR_IFF2] != 0;
			switch (hdr[Z80HDR_CONFIG] & 3)
			{
				case 0: _spec.CPU.IM = 0; break;
				case 1: _spec.CPU.IM = 1; break;
				case 2: _spec.CPU.IM = 2; break;
				default: _spec.CPU.IM = 0; break;
			}

			// Others...
			ula.PortFE = (byte)((hdr[Z80HDR_FLAGS] >> 1) & 0x07);

			if (version > 1)
			{
				if (hdr1[Z80HDR1_IF1PAGED] == 0xFF)
					LogAgent.Warn("Z80Serializer.loadFromStream: Interface I not implemented, but Interface I ROM required!");

				// Load AY-3-8910 registers
				IAY8910Device aydev = _spec.BusManager.FindDevice(typeof(IAY8910Device)) as IAY8910Device;
				if (aydev != null)
				{
					for (int i = 0; i < 16; i++)
					{
						aydev.ADDR_REG = (byte)i;
						aydev.DATA_REG = hdr1[Z80HDR1_AYSTATE + i];
					}
				}
			}

			bool dataCompressed = (hdr[Z80HDR_FLAGS] & 0x20) != 0;
			bool mode128 = false;

			if (version == 2)
			{
				switch (hdr1[Z80HDR1_HWMODE])
				{
					case 0:  // 48k
					case 1:  // 48k + If.1
						break;
					case 2:  // SamRam
						LogAgent.Warn("Z80Serializer.loadFromStream: SamRam not implemented!");
						break;
					case 3:  // 128k
					case 4:  // 128k + If.1
					case 9:  // [ext] Pentagon (128K)
					case 10: // [ext] Scorpion (256K)
						mode128 = true;
						break;
					default:
						LogAgent.Warn(
							"Z80Serializer.loadFromStream: Unrecognized ZX Spectrum config (Z80HDR1_HWMODE=0x{0:2X})!", 
                            hdr1[Z80HDR1_HWMODE]);
						break;
				}
			}
			if (version == 3)
			{
				switch (hdr1[Z80HDR1_HWMODE])
				{
					case 0:  // 48k
					case 1:  // 48k + If.1
					case 2:  // SamRam
						LogAgent.Warn("Z80Serializer.loadFromStream: SamRam not implemented!");
						break;
					case 3:  // 48k + M.G.T.
						break;
					case 4:  // 128k
					case 5:  // 128k + If.1
					case 6:  // 128k + M.G.T.
					case 9:  // [ext] Pentagon (128K)
					case 10: // [ext] Scorpion (256K)
						mode128 = true;
						break;
					default:
						LogAgent.Warn(
							"Z80Serializer.loadFromStream: Unrecognized ZX Spectrum config (Z80HDR1_HWMODE=0x{0:2X})!", 
                            hdr1[Z80HDR1_HWMODE]);
						break;
				}
			}

			// Set 7FFD...
			byte p7FFD = 0x30;	// Lock 48K page 0
			if (version == 1)
			{
				p7FFD = 0x30;	// Lock 48K page 0
			}
			else
			{
				if (mode128)
					p7FFD = hdr1[Z80HDR1_SR7FFD];
				else
					p7FFD = 0x30; // Lock 48K page 0
			}
			memory.CMR0 = p7FFD;


			// load rampages
			if (version == 1)
			{
				int comprSize = (int)(stream.Length - stream.Position);
				if (comprSize < 0) comprSize = 0;

				byte[] buf = new byte[comprSize + 1024];
				stream.Read(buf, 0, comprSize);

				byte[] memdump = new byte[0x1FFFF];

				if (dataCompressed) 
				{	
					DecompressZ80(memdump, buf, 0xC000);
				}
				else
				{	
					for (int i = 0; i < 0xC000; i++)
						memdump[i] = buf[i];
				}

				int currPage = p7FFD & 0x07;
				for (int i = 0; i < 0x4000; i++)
					memory.RamPages[5][i] = memdump[i];
				for (int i = 0; i < 0x4000; i++)
					memory.RamPages[2][i] = memdump[i + 0x4000];
				for (int i = 0; i < 0x4000; i++)
					memory.RamPages[currPage][i] = memdump[i + 0x8000];
			}
			else
			{
				byte[] bitbuf = new byte[4];

				int blockSize = 0;   //WORD
				int blockNumber = 0; //byte
				byte[] block = new byte[129000];
				byte[] rawdata = new byte[0x4000];

				while (stream.Position < stream.Length)
				{
					stream.Read(bitbuf, 0, 2);
					blockSize = getUInt16(bitbuf, 0);

					stream.Read(bitbuf, 0, 1);
					blockNumber = bitbuf[0];

					stream.Read(block, 0, blockSize);
					DecompressZ80(rawdata, block, 0x4000);

					if (blockNumber >= 3 && blockNumber <= 10 && mode128)
					{
						for (int i = 0; i < 0x4000; i++)
							memory.RamPages[(blockNumber-3)&7][i] = rawdata[i];
					}
					else if (((blockNumber == 4) || (blockNumber == 5) || (blockNumber == 8)) && (!mode128))
					{
						int page48 = p7FFD & 0x07;
						if (blockNumber == 8) page48 = 5;
						if (blockNumber == 4) page48 = 2;

						for (int i = 0; i < 0x4000; i++)
							memory.RamPages[page48][i] = rawdata[i];
					}
					else if (blockNumber == 0)
					{
                        int rom48index = memory.GetRomIndex(RomName.ROM_SOS);
                        for (int i = 0; i < 0x4000; i++)
                            memory.RomPages[rom48index][i] = rawdata[i];
						LogAgent.Warn("Z80Serializer.loadFromStream: ROM 48K loaded from snapshot!");
						DialogProvider.Show(
                            "ROM 48K loaded from snapshot!", 
                            "Z80 loader",
                            DlgButtonSet.OK,
                            DlgIcon.Warning);
					}
					else if (blockNumber == 2)
					{
                        int rom128index = memory.GetRomIndex(RomName.ROM_128);
                        for (int i = 0; i < 0x4000; i++)
                            memory.RomPages[rom128index][i] = rawdata[i];
						LogAgent.Warn("Z80Serializer.loadFromStream: ROM 128K loaded from snapshot!");
						DialogProvider.Show(
                            "ROM 128K loaded from snapshot!", 
                            "Z80 loader",
                            DlgButtonSet.OK,
                            DlgIcon.Warning);
					}
				}
			}
		}

		private void saveToStream(Stream stream)
		{
			IMemoryDevice memory = _spec.BusManager.FindDevice(typeof(IMemoryDevice)) as IMemoryDevice;
			IUlaDevice ula = _spec.BusManager.FindDevice(typeof(IUlaDevice)) as IUlaDevice;

			// TODO: align to nonprefix+halt!!!
			//while (_spec.CPU.FX != OPFX.NONE || _spec.CPU.XFX != OPXFX.NONE)
			//    _spec.CPU.ExecCycle();

			byte[] hdr = new byte[30];
			byte[] hdr1 = new byte[25];

			setUint16(hdr, Z80HDR_PC, 0);          // Extended header present... (only if 128K)
			setUint16(hdr1, Z80HDR1_EXTSIZE, 23);  // Z80 V2.01
            
			// Save regs:
			hdr[Z80HDR_A] = _spec.CPU.regs.A;
			hdr[Z80HDR_F] = _spec.CPU.regs.F;
			setUint16(hdr, Z80HDR_HL, _spec.CPU.regs.HL);
			setUint16(hdr, Z80HDR_DE, _spec.CPU.regs.DE);
			setUint16(hdr, Z80HDR_BC, _spec.CPU.regs.BC);

			hdr[Z80HDR_A_] = (byte)(_spec.CPU.regs._AF >> 8);
			hdr[Z80HDR_F_] = (byte)_spec.CPU.regs._AF;
			setUint16(hdr, Z80HDR_HL_, _spec.CPU.regs._HL);
			setUint16(hdr, Z80HDR_DE_, _spec.CPU.regs._DE);
			setUint16(hdr, Z80HDR_BC_, _spec.CPU.regs._BC);

			setUint16(hdr, Z80HDR_IX, _spec.CPU.regs.IX);
			setUint16(hdr, Z80HDR_IY, _spec.CPU.regs.IY);
			setUint16(hdr, Z80HDR_SP, _spec.CPU.regs.SP);

            hdr[Z80HDR_FLAGS] = 0; //clear

            hdr[Z80HDR_I] = _spec.CPU.regs.I;
			hdr[Z80HDR_R7F] = (byte)(_spec.CPU.regs.R & 0x7F);
			if ((_spec.CPU.regs.R & 0x80) != 0) 
				hdr[Z80HDR_FLAGS] |= 0x01;

			hdr[Z80HDR_FLAGS] |= (byte)((ula.PortFE & 7) << 1);
			hdr[Z80HDR_FLAGS] |= 0x20; // compression

			setUint16(hdr1, Z80HDR1_PC, _spec.CPU.regs.PC);

			// CPU.Status...
			if (_spec.CPU.IFF1) hdr[Z80HDR_IFF1] = 0xFF;
			else hdr[Z80HDR_IFF1] = 0x00;
			if (_spec.CPU.IFF2) hdr[Z80HDR_IFF2] = 0xFF;
			else hdr[Z80HDR_IFF2] = 0x00;

			hdr[Z80HDR_CONFIG] = _spec.CPU.IM;
			if (_spec.CPU.IM > 2) 
				hdr[Z80HDR_CONFIG] = 0x00;
			//   hdr[Z80HDR_CONFIG] |= 0x04; // ?? эмуляция 2-го выпуска спектрума

			bool mode128 = !memory.IsMap48;		// 48K / 128K ?

			if (!mode128)
				setUint16(hdr, Z80HDR_PC, _spec.CPU.regs.PC);   // if 48K use V1.45

			hdr1[Z80HDR1_HWMODE] = 0x03;				// 0-48K, 3-128K , but use V1.45 for 48K

			byte p7FFD = memory.CMR0;//0x30;
			hdr1[Z80HDR1_SR7FFD] = p7FFD;
			hdr1[Z80HDR1_IF1PAGED] = 0x00;
			hdr1[Z80HDR1_STUFF] = 0x03;                  // R & LDIR emulation enable

			hdr1[Z80HDR1_7FFD] = 0x0E;       // ?? dont know what is it, but other emuls doing that (128K only)

			// Save AY registers
			IAY8910Device aydev = _spec.BusManager.FindDevice(typeof(IAY8910Device)) as IAY8910Device;
			if (aydev != null)
			{
				byte ayaddr = aydev.ADDR_REG;
				for (int i = 0; i < 16; i++)
				{
					aydev.ADDR_REG = (byte)i;
					hdr1[Z80HDR1_AYSTATE + i] = aydev.DATA_REG;
				}
				aydev.ADDR_REG = ayaddr;
			}
			else
			{
				for (int i = 0; i < 16; i++)
					hdr1[Z80HDR1_AYSTATE + i] = 0xFF;
			}

			// Save headers and memory dumps...
			int blockSize = 0;
			int blockNumber = 0;
			byte[] block = new byte[200000];

			if (!mode128)         // For 48K
			{
				byte[] ram48 = new byte[0xFFFF];
				
				for (int i = 0; i < 0x4000; i++)
					ram48[i+0x0000] = memory.RamPages[memory.Map48[1]][i];
				for (int i = 0; i < 0x4000; i++)
					ram48[i+0x4000] = memory.RamPages[memory.Map48[2]][i];
				for (int i = 0; i < 0x4000; i++)
					ram48[i + 0x8000] = memory.RamPages[memory.Map48[3]][i];
				
				blockSize = CompressZ80(block, ram48, 0x4000 * 3);
				if ((blockSize + 4) >= (0x4000 * 3))       // Disable compression in case when compression is not effective
				{
					hdr[Z80HDR_FLAGS] &= unchecked((byte)~0x20);
					blockSize = 0x4000 * 3;
					for (int i = 0; i < blockSize; i++)
						block[i] = ram48[i];
				}
				// Save header V1.45...
				stream.Write(hdr, 0, 30);

				// Save 48K block...
				stream.Write(block, 0, blockSize);

				if ((hdr[Z80HDR_FLAGS] & 0x20) != 0) // in case when block is compressed write end-marker
				{
					byte[] endmark = new byte[4] { 0x00, 0xED, 0xED, 0x00 };
					stream.Write(endmark, 0, 4);
				}
			}
			else                 // for 128K
			{
				// Save headers V2.01...
				stream.Write(hdr, 0, 30);
				stream.Write(hdr1, 0, 25);    // V2.01 if 128K

				for (int i = 0; i < 8; i++)
				{
					byte[] blockData = memory.RamPages[i];
					blockSize = CompressZ80(block, blockData, 0x4000);
					blockNumber = (i & 0x07) + 3;
					stream.Write(getBytes(blockSize), 0, 2);
					stream.Write(getBytes(blockNumber), 0, 1);
					stream.Write(block, 0, blockSize);
				}
			}
		}

		#endregion

		#region struct offsets
		private const int Z80HDR_A = 0;
		private const int Z80HDR_F = 1;
		private const int Z80HDR_BC = 2;
		private const int Z80HDR_HL = 4;
		private const int Z80HDR_PC = 6;   // for New Format == 0 !
		private const int Z80HDR_SP = 8;
		private const int Z80HDR_I = 10;
		private const int Z80HDR_R7F = 11;
		private const int Z80HDR_FLAGS = 12;
		// if(Flags==0xFF) Flags = 0x01
		//                        Bit 0  : Равен 7-му биту регистра R
		//                        Bit 1-3: Цвет бордюра
		//                        Bit 4  : 1- впечатан БЕЙСИК SamRam
		//                        Bit 5  : 1- блок данных компрессирован
		//                        Bit 6-7: Не значащие
		private const int Z80HDR_DE = 13;
		private const int Z80HDR_BC_ = 15;
		private const int Z80HDR_DE_ = 17;
		private const int Z80HDR_HL_ = 19;
		private const int Z80HDR_A_ = 21;
		private const int Z80HDR_F_ = 22;
		private const int Z80HDR_IY = 23;
		private const int Z80HDR_IX = 25;
		private const int Z80HDR_IFF1 = 27;  // if(0) DI
		private const int Z80HDR_IFF2 = 28;
		private const int Z80HDR_CONFIG = 29;
		//                        Bit 0-1: Режим прерываний (0, 1 или 2)
		//                        Bit 2  : 1=эмуляция 2-го выпуска Спектрума
		//                        Bit 3  : 1=Двойная частота прерываний
		//                        Bit 4-5: 1=Высокая видеосинхронизация
		//                                 3=Низкая видеосинхронизация
		//                                 0,2=Нормальная
		//                        Bit 6-7: 0=Курсор/Protek/AGF- джойстик
		//                                 1=Кемпстон-джойстик
		//                                 2=Sinclair-левый джойстик
		//                                 3=Sinclair-правый джойстик

		private const int Z80HDR1_EXTSIZE = 0;  // Длина дополнительного блока (23 for V2.01; 54 for V3.0)
		private const int Z80HDR1_PC = 2;
		private const int Z80HDR1_HWMODE = 4;   // Hardware mode
		//      Число         Значение в версии 2.01    Значение в версии 3.0
		//
		//        0               48k                     48k
		//        1               48k + If.1              48k + If.1
		//        2               SamRam                  48k + M.G.T.
		//        3               128k                    SamRam
		//        4               128k + If.1             128k
		//        5               -                       128k + If.1
		//        6               -                       128k + M.G.T.
		private const int Z80HDR1_SR7FFD = 5;
		private const int Z80HDR1_IF1PAGED = 6;
		private const int Z80HDR1_STUFF = 7;   //D0: R emulation on; D1: LDIR emulation on
		private const int Z80HDR1_7FFD = 8;    //???
		private const int Z80HDR1_AYSTATE = 9;
		#endregion

		#region compression
		private int CompressZ80(byte[] dest, byte[] src, int SrcSize)
		{
			int DestSize = dest.Length;
			// code was taken from SPCONV
			uint NO = 0;
			uint YES = 1;

			uint i, j;
			uint num;
			byte c, n;
			uint ed;

			i = 0;
			j = 0;
			/* ensure 'ed' is not set */
			ed = NO;
			while (i < SrcSize)
			{
				c = src[i];
				i++;
				if (i < SrcSize)
				{
					n = src[i];
				}
				else
				{
					/* force 'n' to be unequal to 'c' */
					n = c;
					n++;
				}

				if (c != n)
				{
					dest[j] = c;
					j++;
					if (c == 0xed) ed = YES;
					else ed = NO;
				}
				else
				{
					if (c == 0xed)
					{
						/* two times 0xed - special care */
						dest[j] = 0xed;
						j++;
						dest[j] = 0xed;
						j++;
						dest[j] = 0x02;
						j++;
						dest[j] = 0xed;
						j++;
						i++; /* skip second ED */

						/* because 0xed is valid compressed we don't
						   have to watch it! */
						ed = NO;
					}
					else if (ed == YES)
					{
						/* can't compress now, skip this double pair */
						dest[j] = c;
						j++;
						ed = NO;  /* 'c' can't be 0xed */
					}
					else
					{
						num = 1;
						while (i < SrcSize)
						{
							if (c != src[i]) break;
							num++;
							i++;
							if (num == 255) break;
						}
						if (num <= 4)
						{
							/* no use to compress */
							while (num != 0)
							{
								dest[j] = c;
								j++;
								num--;
							}
						}
						else
						{
							dest[j] = 0xed;
							j++;
							dest[j] = 0xed;
							j++;
							dest[j] = (byte)num;
							j++;
							dest[j] = c;
							j++;
						}
					}
				}

				if (j >= DestSize)
				{
					DialogProvider.Show(
                        "Compression error: buffer overflow,\nfile can contain invalid data!", 
                        "Z80 loader",
                        DlgButtonSet.OK,
                        DlgIcon.Warning);

					/* compressed image bigger or same than dest buffer */
					for (int k = 0; k < SrcSize; k++)
						dest[k] = src[k];
					return SrcSize;
				}
			}
			return (int)j;
		}
		private int DecompressZ80(byte[] dest, byte[] src, int size)
		{
			uint c, j;
			uint k;
			byte l;
			byte im;

			uint i = 0;

			j = 0;
			while (j < size)
			{
				c = src[i++];
				//      if(c == -1) return;
				im = (byte)c;

				if (im != 0xed)
				{
					dest[j++] = im;
				}
				else
				{
					c = src[i++];
					//         if(c == -1) return;
					im = (byte)c;
					if (im != 0xed)
					{
						dest[j++] = 0xed;
						i--;
					}
					else
					{
						/* fetch count */
						k = src[i++];
						//            if(k == -1) return;

						/* fetch character */
						c = src[i++];
						//            if(c == -1) return;
						l = (byte)c;
						while (k != 0)
						{
							dest[j++] = l;
							k--;
						}
					}
				}
			}

			if (j != size)
			{
                string msg = string.Format(
                    "Decompression error - file corrupt? (j={0}, size={1})", 
                    j, 
                    size);
                LogAgent.Error("Z80Serializer: {0}", msg);
                DialogProvider.Show(
                    msg,
                    "Z80 loader",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
				return 1;
			}
			return size;
		}
		#endregion
	}
}
