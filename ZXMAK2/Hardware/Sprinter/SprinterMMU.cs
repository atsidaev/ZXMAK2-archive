//#define Debug
using System;
using System.IO;

using ZXMAK2.Engine.Attributes;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Hardware.Sprinter
{
    /*
        WingLion:
        А с распределением памяти ситуация такая. Порты страниц - переключают адреса 
        "виртуальных" страниц скорпиона независимо от того, что в этот момент 
        подключено в нулевую банку. С третьей банкой наоборот - запись в порт 
        страницы меняет адрес той страницы, какая установлена портами 7FFD,1FFD, 
        т.е. записав что-то в порт PAGE3 нужно помнить, какая страница стояла а 
        адреса #C000. Например, если нулевая, то поменяется страница и в адресе 
        #0000, если там было установлено ОЗУ.

        В неспектрумовских режимах Спринтера, спектрумовские порты отключаются, и 
        остается только спринтеровское управление страницами.
     */

    public enum ROMPages
    {
        rpROMExpansion = 0,
        rpROMTRDOS = 1,
        rpROMBasic128 = 2,
        rpROMBasic48 = 3,
        rpROMExpansionAlt = 4,
        rpROMTRDOSAlt = 5,
        rpROMBasic128Alt = 6,
        rpROMBasic48Alt = 7,
        rpRAM0 = 8,
        rpRAM1 = 9,
        rpRAM2 = 10,
        rpROMSystem = 11,
        rpRAMCache = 12,
        rpROMSystemAlt = 15
    }

    public enum DCPports
    {
        dcpNull = 0,
        dcpWG1F = 0x10,
        dcpWG3F = 0x11,
        dcpWG5F = 0x12,
        dcpWG7F = 0x13,
        dcpWGFF = 0x14,    //Порт на запись - состояние контроллера дисковода (FF)
        dcpJoystik = 0x15, //Порт на чтение - джойстик и IRQ/INTRQ контроллера
        dcpHDDData = 0x20,  //HDD - регистр данных
        dcpHDDStat = 0x21, //HDD - регистр состояния/ошибок
        dcpHDDSectCnt = 0x22,  //HDD - регистр количества секторов для операций R/W
        dcpHDDSector = 0x23,   //HDD - регистр сектора
        dcpHDDCylLow = 0x24,   //HDD - регистр дорожки low
        dcpHDDCylHigh = 0x25,   //HDD - регистр дорожки high
        dcpHDDHead = 0x26,   //HDD - регистр головок/выборка мастер-слэйв
        dcpHDDCommand = 0x27,   //HDD - регистр команд
        dcpHDD3F6 = 0x28,   //HDD - дополнительный регистр  управления 3F6
        dcpHDD3F7 = 0x29,   //HDD - дополнительный регистр  состояния 3F7
        dcpISA1MemoryRW = 0x30,     //ISA-SLOT #1 memory R/W
        dcpISA2MemoryRW = 0x31,     //ISA-SLOT #2 memory R/W
        dcpISA1PortsRW = 0x32,     //ISA-SLOT #1 ports R/W
        dcpISA2PortsRW = 0x33,     //ISA-SLOT #2 ports R/W
        dcpZXKeyboard = 0x40,
        dcpCovoxBlaster = 0x88,    //Covox/Covox-Blaster
        dcpAYBFFD = 0x90,
        dcpAYFFFD = 0x91,
        dcpScorp1FFD = 0xC0,          //Scorpion 1FFD port
        dcpPent7FFD = 0xC1,            //Pentagon 7FFD port
        dcpBorder = 0xc2,       //Border Write Only
        dcpRGADR = 0xC4,
        dcpRGMOD = 0xC5,
        dcpROMExpansion = 0xE0,
        dcpROMTRDOS = 0xE1,
        dcpROMB128 = 0xE2,
        dcpROMB48 = 0xE3,
        dcpROMExpansionAlt = 0xE4,
        dcpROMTRDOSAlt = 0xE5,
        dcpROMB128Alt = 0xE6,
        dcpROMB48Alt = 0xE7,
        dcpRAM0 = 0xE8,         //RAM Page (окно 0000-3fff)
        dcpRAM1 = 0xE9,         //RAM Page (окно 4000-7fff)
        dcpRAM2 = 0xEA,         //RAM Page (окно 8000-bfff)
        dcpROMSys = 0xEB,       //ROM page SYSTEM
        dcpRAMCache = 0xEC,     //RAM page CACHE
        dcpROMSysAlt = 0xEF, //ROM Page SYSTEM'
        dcpRAMPage0 = 0xF0,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage1 = 0xF1,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage2 = 0xF2,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage3 = 0xF3,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage4 = 0xF4,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage5 = 0xF5,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage6 = 0xF6,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage7 = 0xF7,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage8 = 0xF8,     //RAM Pages (окно C000-FFFF)
        dcpRAMPage9 = 0xF9,     //RAM Pages (окно C000-FFFF)
        dcpRAMPageA = 0xFA,     //RAM Pages (окно C000-FFFF)
        dcpRAMPageB = 0xFB,     //RAM Pages (окно C000-FFFF)
        dcpRAMPageC = 0xFC,     //RAM Pages (окно C000-FFFF)
        dcpRAMPageD = 0xFD,     //RAM Pages (окно C000-FFFF)
        dcpRAMPageE = 0xFE,     //RAM Pages (окно C000-FFFF)
        dcpRAMPageF = 0xFF      //RAM Pages (окно C000-FFFF)

    }

    public enum AccelCMD
    {
        Invalid = -1,
        Off = 1,     //ld b,b    выкл акселератор
        On = 2,      //ld d,d    вкл акселератор, указать размер блока
        Fill = 3,    //ld c,c    
        GrFill = 4,  //ld e,e
        Reserved = 5,  //ld h,h
        CopyBlok = 6, //ld l,l
        GrCopyBlok = 7 //ld a,a  копирование блока гр.экрана, вертикальные линии
    }

    public enum AccelSubCMD
    {
        None = 0,
        XORBlok = 1,
        ORBlok = 2,
        ANDBlok = 3
    }

    public class SprinterMMU : MemoryBase
    {
        private byte[][] m_cramPages = new byte[0x04][]; //кэш
        private byte[][] m_vramPages = new byte[0x10][]; //видео-рам
        private byte[] m_scorpion_pages = new byte[8];

        private byte[] m_pages = new byte[16];

        private byte m_page0;
        private byte m_page1;
        private byte m_page2;
        private byte m_page3;
        private bool m_cache;
        private byte m_sysport;
        //Video RAM
        private byte m_port_y;
        private byte m_port_videomode;
        private byte m_port_scr;
        private byte m_vblok4000;
        private byte m_vblokC000;
        private bool m_sys; //ROM Expansion
        private bool m_romA16; //ARAM16 (RA16)
        private byte m_romindex; //Номер страницы ПЗУ
        //private byte m_cacheindex; //Номер страницы Кэша
        private bool m_firstread = true;

        //Акселератор
        private bool m_acc_enable;
        private bool m_acc_on;

        //        private bool m_acc_wait_cmd;
        //      private bool m_acc_wait_data;

        private AccelCMD m_acc_mode;
        private AccelSubCMD m_acc_submode;

        private int m_acc_buf_size;
        private byte[] m_acc_buf = new byte[256];

#if Debug
        private ushort m_opaddr;
#endif

        //        System.Windows.Forms.ListBox lb;

        private SprinterULA m_ulaSprinter;
        private SprinterFdd m_SprinterBDI;

        public SprinterMMU()
            : base("Sprinter", 0x10, 0x0100)
        {
        }

        protected override void Init(int romPageCount, int ramPageCount)
        {
            base.Init(romPageCount, ramPageCount);
            for (int i = 0; i < this.m_vramPages.Length; i++)
            {
                this.m_vramPages[i] = new byte[0x4000];
            }
            for (int i = 0; i < this.m_cramPages.Length; i++)
            {
                this.m_cramPages[i] = new byte[0x4000];
            }
        }

        public override int GetRomIndex(RomId romId)
        {
            switch (romId)
            {
                // It seems like not used
                case RomId.ROM_128: return 0;
                case RomId.ROM_SOS: return 1;
                case RomId.ROM_DOS: return 2;
                case RomId.ROM_SYS: return 3;
            }
            Logger.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        #region  -- Bus IO Procs --
        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);

            this.m_ulaSprinter = bmgr.FindDevice<SprinterULA>();
            if (this.m_ulaSprinter != null)
            {
                this.m_ulaSprinter.VRAM = m_vramPages;
            }
            else
            {
                throw new ApplicationException("SprinterULA not found");
            }
            this.m_SprinterBDI = bmgr.FindDevice<SprinterFdd>();
            if (this.m_SprinterBDI == null)
            {
                throw new ApplicationException("SprinterBDI not found");
            }
            bmgr.SubscribeWrIo(0x0000, 0x0000, new BusWriteIoProc(this.WRDCP));  //write to DCP Port
            bmgr.SubscribeRdIo(0x0000, 0x0000, new BusReadIoProc(this.RDDCP));    //read from DCP port
            bmgr.SubscribeWrIo(0x00ff, 0x0082, new BusWriteIoProc(this.writePort82h));  //write PAGE0
            bmgr.SubscribeRdIo(0x00ff, 0x0082, new BusReadIoProc(this.readPort82h));    //read PAGE0
            bmgr.SubscribeWrIo(0x00ff, 0x00A2, new BusWriteIoProc(this.writePortA2h));  //write PAGE1
            bmgr.SubscribeRdIo(0x00ff, 0x00A2, new BusReadIoProc(this.readPortA2h));    //read PAGE1
            bmgr.SubscribeWrIo(0x00ff, 0x00C2, new BusWriteIoProc(this.writePortC2h));  //write PAGE2
            bmgr.SubscribeRdIo(0x00ff, 0x00C2, new BusReadIoProc(this.readPortC2h));    //read PAGE2
            bmgr.SubscribeWrIo(0x00ff, 0x00E2, new BusWriteIoProc(this.writePortE2h));  //write PAGE3
            bmgr.SubscribeRdIo(0x00ff, 0x00E2, new BusReadIoProc(this.readPortE2h));    //read PAGE3
            bmgr.SubscribeWrIo(0xd027, 0x5025, new BusWriteIoProc(this.writePort7FFDh));    //write 7FFDh
            bmgr.SubscribeWrIo(0xd027, 0x1025, new BusWriteIoProc(this.writePort1FFDh));  //write 1FFDh
            bmgr.SubscribeRdIo(0x00ff, 0x00fb, new BusReadIoProc(this.readPortFBh));  //read FBh  Open Cash
            bmgr.SubscribeRdIo(0x00ff, 0x007b, new BusReadIoProc(this.readPort7Bh));  //read 7Bh  Close Cash
            bmgr.SubscribeWrIo(0x00BD, 0x003c, new BusWriteIoProc(this.writePort7Ch));  //write 7Ch
            bmgr.SubscribeWrIo(0x00FF, 0x0089, new BusWriteIoProc(this.writePort89h));  //write PORTY
            bmgr.SubscribeWrIo(0x00FF, 0x00C9, new BusWriteIoProc(this.writePortC9h));  //write C9h
            bmgr.SubscribeWrIo(0x00FF, 0x00E9, new BusWriteIoProc(this.writePortE9h));  //write E9h
            bmgr.SubscribeRdIo(0x00ff, 0x0089, new BusReadIoProc(this.readPort89h));  //read PORTY
            bmgr.SubscribeRdIo(0x00ff, 0x00E9, new BusReadIoProc(this.readPortE9h));  //read E9h
            bmgr.SubscribeRdIo(0x00ff, 0x00C9, new BusReadIoProc(this.readPortC9h));  //read C9h
            bmgr.SubscribeWrMem(0xC000, 0x0000, new BusWriteProc(this.WriteMem0000));  //write 
            bmgr.SubscribeWrMem(0xC000, 0x4000, new BusWriteProc(this.WriteMem4000));  //write 
            bmgr.SubscribeWrMem(0xC000, 0x8000, new BusWriteProc(this.WriteMem8000));  //write 
            bmgr.SubscribeWrMem(0xC000, 0xC000, new BusWriteProc(this.WriteMemC000));  //write
            bmgr.SubscribeRdMem(0xC000, 0x0000, new BusReadProc(this.ReadMem0000));  //read
            bmgr.SubscribeRdMem(0xC000, 0x8000, new BusReadProc(this.ReadMem8000));  //read
            bmgr.SubscribeRdMem(0xC000, 0xC000, new BusReadProc(this.ReadMemC000));  //read
            bmgr.SubscribeRdMem(0xC000, 0x4000, new BusReadProc(this.ReadMem4000));  //read
            bmgr.SubscribeRdIo(0xFFFF, 0, new BusReadIoProc(this.readPort00h));  //read 0
            bmgr.SubscribeRdMemM1(0x0000, 0x0000, new BusReadProc(this.Accelerator));
            bmgr.SubscribeRdMem(0x0000, 0x0000, new BusReadProc(this.AccelRead));
            bmgr.SubscribeWrMem(0x0000, 0x0000, new BusWriteProc(this.AccelWrite));

#if Debug
            bmgr.SubscribeRDMEM_M1(0x0000, 0x0000, new BusReadProc(this.readRamM1));  //read operator from memory
#endif
            bmgr.SubscribeReset(new BusSignalProc(this.busReset));


        }

        #region Accelerator

        private void AccelRead(ushort addr, ref byte value)
        {
            if (m_acc_enable && m_acc_on && (m_acc_mode != AccelCMD.Off))
            {
                ;
                switch (m_acc_mode)
                {
                    case AccelCMD.On:           //LD D,D
                        {
                            if (value != 0) m_acc_buf_size = value;
                            else m_acc_buf_size = 256;
                        } break;

                    case AccelCMD.CopyBlok:     //LD L,L
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                            {
                                byte tmp = 0;
                                switch ((addr + i) & 0xc000)
                                {
                                    case 0x0000: this.ReadMem0000((ushort)(addr + i), ref tmp); break;
                                    case 0x4000: this.ReadMem4000((ushort)(addr + i), ref tmp); break;
                                    case 0x8000: this.ReadMem8000((ushort)(addr + i), ref tmp); break;
                                    case 0xc000: this.ReadMemC000((ushort)(addr + i), ref tmp); break;
                                }
                                switch (m_acc_submode)
                                {

                                    case AccelSubCMD.None:      //CopyBlok
                                        {
                                            m_acc_buf[i] = tmp;//RDMEM_DBG((ushort)(addr + i));
                                        } break;
                                    case AccelSubCMD.XORBlok:      //XOR (HL)
                                        {
                                            m_acc_buf[i] ^= tmp;

                                        } break;
                                    case AccelSubCMD.ORBlok:       //OR (HL)
                                        {
                                            m_acc_buf[i] |= tmp;

                                        } break;
                                    case AccelSubCMD.ANDBlok:      //AND (HL)
                                        {
                                            m_acc_buf[i] &= tmp;
                                        } break;
                                }

                            }
                            m_acc_submode = AccelSubCMD.None;
                        } break;

                    case AccelCMD.GrCopyBlok:   //LD A,A
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                            {
                                byte tmp = 0;
                                switch (addr & 0xc000)
                                {
                                    case 0x0000: this.ReadMem0000((ushort)(addr), ref tmp); break;
                                    case 0x4000: this.ReadMem4000((ushort)(addr), ref tmp); break;
                                    case 0x8000: this.ReadMem8000((ushort)(addr), ref tmp); break;
                                    case 0xc000: this.ReadMemC000((ushort)(addr), ref tmp); break;
                                }
                                switch (m_acc_submode)
                                {
                                    case AccelSubCMD.None: m_acc_buf[i] = tmp;//RDMEM_DBG((ushort)(addr + i));
                                        break;
                                    case AccelSubCMD.XORBlok: m_acc_buf[i] ^= tmp; break;
                                    case AccelSubCMD.ORBlok: m_acc_buf[i] |= tmp; break;
                                    case AccelSubCMD.ANDBlok: m_acc_buf[i] &= tmp; break;
                                }
                                //m_acc_buf[i] = VRamPages[(m_port_y & 0xf0) >> 4][(m_port_y & 0x0f) * 1024 + (addr & 0x3FF)]; //this.RDMEM_DBG(addr);
                                m_port_y++;
                            }
                            m_acc_submode = AccelSubCMD.None;
                        } break;
                    case AccelCMD.GrFill:   //LD E,E
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                                m_port_y++;
                        } break;

                }
            }
        }

        private void AccelWrite(ushort addr, byte value)
        {
            if (m_acc_enable && m_acc_on && (m_acc_mode != AccelCMD.Off))
            {
                ;
                switch (m_acc_mode)
                {
                    case AccelCMD.On:           //LD D,D
                        {
                            if (value != 0) m_acc_buf_size = value;
                            else m_acc_buf_size = 256;
                        } break;
                    case AccelCMD.GrCopyBlok:   //LD A,A
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                            {
                                switch ((addr) & 0xc000)
                                {
                                    case 0x0000: this.WriteMem0000((ushort)(addr), m_acc_buf[i]); break;
                                    case 0x4000: this.WriteMem4000((ushort)(addr), m_acc_buf[i]); break;
                                    case 0x8000: this.WriteMem8000((ushort)(addr), m_acc_buf[i]); break;
                                    case 0xc000: this.WriteMemC000((ushort)(addr), m_acc_buf[i]); break;
                                }
                                //                                VRamPages[(m_port_y & 0xf0) >> 4][(m_port_y & 0x0f) * 1024 + (addr & 0x3FF)] = m_acc_buf[i];
                                m_port_y++;
                            }
                        } break;
                    case AccelCMD.CopyBlok: //LD L,L
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                            {
                                switch ((addr + i) & 0xc000)
                                {
                                    case 0x0000: this.WriteMem0000((ushort)(addr + i), m_acc_buf[i]); break;
                                    case 0x4000: this.WriteMem4000((ushort)(addr + i), m_acc_buf[i]); break;
                                    case 0x8000: this.WriteMem8000((ushort)(addr + i), m_acc_buf[i]); break;
                                    case 0xc000: this.WriteMemC000((ushort)(addr + i), m_acc_buf[i]); break;

                                }
                                //WRMEM_DBG((ushort)(addr + i), m_acc_buf[i]);

                                //                                m_acc_buf[i] = RDMEM_DBG((ushort)(addr + i));
                            }
                        } break;

                    case AccelCMD.Fill:     //LD C,C
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                            {
                                switch ((addr + i) & 0xc000)
                                {
                                    case 0x0000: this.WriteMem0000((ushort)(addr + i), value); break;
                                    case 0x4000: this.WriteMem4000((ushort)(addr + i), value); break;
                                    case 0x8000: this.WriteMem8000((ushort)(addr + i), value); break;
                                    case 0xc000: this.WriteMemC000((ushort)(addr + i), value); break;
                                }
                                //this.WRMEM_DBG((ushort)(addr + i), value);
                            }
                        } break;
                    case AccelCMD.GrFill:   //LD E,E
                        {
                            for (int i = 0; i < m_acc_buf_size; i++)
                            {
                                switch ((addr) & 0xc000)
                                {
                                    case 0x0000: this.WriteMem0000((ushort)(addr), value); break;
                                    case 0x4000: this.WriteMem4000((ushort)(addr), value); break;
                                    case 0x8000: this.WriteMem8000((ushort)(addr), value); break;
                                    case 0xc000: this.WriteMemC000((ushort)(addr), value); break;
                                }
                                //VRamPages[(m_port_y & 0xf0) >> 4][(m_port_y & 0x0f) * 1024 + (addr & 0x03FF)] = value;
                                m_port_y++;
                            }
                        } break;
                }
            }
        }

        /*        private void GetAccelDATA(ushort addr, ref byte value)
                {
                    if (m_acc_enable && m_acc_on && m_acc_wait_data)
                    {
                        ;
                    }
                }*/

        private void Accelerator(ushort addr, ref byte value)
        {
            if (m_acc_enable)
            {
                switch (this.RDMEM_DBG(addr))
                {
                    //Accelerator off - ld b,b
                    case 0x40:
                        {
                            //                            m_acc_on = false;
                            //                            m_acc_wait_cmd = false;
                            m_acc_mode = AccelCMD.Off;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    //Accelerator on - ld d,d
                    case 0x52:
                        {
                            m_acc_on = true;
                            //                            m_acc_wait_cmd = true;
                            m_acc_mode = AccelCMD.On;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    case 0x49:
                        {
                            m_acc_on = true;
                            m_acc_mode = AccelCMD.Fill;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    case 0x5B:
                        {
                            m_acc_on = true;
                            m_acc_mode = AccelCMD.GrFill;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    case 0x64:
                        {
                            m_acc_on = true;
                            m_acc_mode = AccelCMD.Reserved;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    case 0x6D:
                        {
                            m_acc_on = true;
                            m_acc_mode = AccelCMD.CopyBlok;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    case 0x7F:
                        {
                            m_acc_on = true;
                            m_acc_mode = AccelCMD.GrCopyBlok;
                            m_acc_submode = AccelSubCMD.None;
                        }
                        break;
                    case 0xAE:
                        {
                            m_acc_on = true;
                            m_acc_submode = AccelSubCMD.XORBlok;
                        } break;
                    case 0xB6:
                        {
                            m_acc_on = true;
                            m_acc_submode = AccelSubCMD.ORBlok;
                        } break;
                    case 0xA6:
                        {
                            m_acc_on = true;
                            m_acc_submode = AccelSubCMD.ANDBlok;
                        } break;
                }
            }
        }
        #endregion

        //Обработчик записи в порт с расшифровкой по DCP-странице
        private void WRDCP(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                ushort dcpadr = (ushort)((this.DOSEN ? 2048 : 0) + (addr & 0x67) + (addr & 0x2000 >> 8) + (addr & 0xc000 >> 7));
#if Debug
                LogPort(addr, value);
#endif
                //            switch (m_ramPages[0x40][dcpadr])
                if ((RamPages[0x40][dcpadr] >= 0xF0) && (RamPages[0x40][dcpadr] <= 0xFF))
                {
                    m_scorpion_pages[(RamPages[0x40][dcpadr] & 0x0f)] = value;
                    iorqge = false;
                }
                else
                {
                    if ((RamPages[0x40][dcpadr] >= 0xE0) && (RamPages[0x40][dcpadr] <= 0xEF))
                    {
                        m_pages[RamPages[0x40][dcpadr] & 0x0f] = value;
                        iorqge = false;
                    }

                    /*                    switch (m_ramPages[0x40][dcpadr])
                                        {
                                            case DCPports.dcpROMSysAlt: 
                                        }*/
                }
#if Debug
                if (!iorqge)
                    LogPort(addr, value);
#endif
            }
        }

        //Обработчик чтения из порта с расшифровкой по DCP-странице
        private void RDDCP(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                ushort dcpadr = (ushort)((this.DOSEN ? 2048 : 0) + (addr & 0x67) + (addr & 0x2000 >> 8) + (addr & 0xc000 >> 7) + 1024);
#if Debug
                LogPort(addr, value);
#endif
                //            switch (m_ramPages[0x40][dcpadr])
                if ((RamPages[0x40][dcpadr] >= 0xF0) && (RamPages[0x40][dcpadr] <= 0xFF))
                {
                    value = m_scorpion_pages[(RamPages[0x40][dcpadr] & 0x0f)];
                    iorqge = false;
                }
                else
                {
                    if ((RamPages[0x40][dcpadr] >= 0xE0) && (RamPages[0x40][dcpadr] <= 0xEF))
                    {
                        value = m_pages[RamPages[0x40][dcpadr] & 0x0f];
                        iorqge = false;
                    }

                    /*                    switch (m_ramPages[0x40][dcpadr])
                                        {
                                            case DCPports.dcpROMSysAlt: 
                                        }*/
                }
#if Debug
                if (!iorqge)
                    LogPort(addr, value);
#endif
            }
        }

        private new void WriteMem0000(ushort addr, byte value)
        {
            if ((m_page0 >= 0x50) && (m_page0 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                if (((m_page0 & 8) == 0) || (((m_page0 & 8) != 0) && (value != 0xFF)))
                    m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr] = value;
                if ((m_page0 & 4) == 0)
                {
                    RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr] = value;
                    //   this.MapWrite8000[addr & 0x3fff] = value;
                }
            }
            else
            {
                this.MapWrite0000[addr & 0x3fff] = value;
            }
        }

        private new void ReadMem0000(ushort addr, ref byte value)
        {
            if ((m_page0 >= 0x50) && (m_page0 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                value = RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr];
                //m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr];
            }
            else
            {
                base.ReadMem0000(addr, ref value);
                //value = this.MapReadC000[addr & 0x3fff];
            }
        }

        private new void WriteMem4000(ushort addr, byte value)
        {
            if ((m_page1 >= 0x50) && (m_page1 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                if (((m_page1 & 8) == 0) || (((m_page1 & 8) != 0) && (value != 0xFF)))
                    m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr] = value;
                if ((m_page1 & 4) == 0)
                {
                    //   this.MapWrite4000[addr & 0x3fff] = value;
                    RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr] = value;
                }
            }
            else
            {
                this.MapWrite4000[addr & 0x3fff] = value;
                //видео-страница закрыта, значит выводим в спектрумовском режиме
                if ((m_port_y & 64) == 0)
                {
                    //бит 6==0, значит вывод в видео-озу в спектрумовском режиме разрешен
                    //необходимо транспонировать адрес спектрумовского экрана в адрес спринтеровского озу
                    byte vrampg = (byte)((addr & 0xf0) >> 4);
                    ushort vrampg_row = (ushort)((addr & 0x0f) * 1024);
                    ushort vrampg_col = (ushort)((addr & 0x1f00) >> 8);

                    if ((addr & 0x3fff) <= 0x1fff)
                    {
                        m_vramPages[vrampg][vrampg_row + vrampg_col + (m_vblok4000 * 32)] = value;
                    }
                    else
                    {
                        if ((m_port_y & 128) == 0)
                            m_vramPages[vrampg][vrampg_row + vrampg_col + (((m_vblok4000 + 1) & 31) * 32)] = value;
                    }

                }
            }
        }

        private new void WriteMem8000(ushort addr, byte value)
        {
            if ((m_page2 >= 0x50) && (m_page2 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                if (((m_page2 & 8) == 0) || (((m_page2 & 8) != 0) && (value != 0xFF)))
                    m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr] = value;
                if ((m_page2 & 4) == 0)
                {
                    RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr] = value;
                    //   this.MapWrite8000[addr & 0x3fff] = value;
                }
            }
            else
            {
                this.MapWrite8000[addr & 0x3fff] = value;
            }
        }

        private new void WriteMemC000(ushort addr, byte value)
        {
            if ((m_page3 >= 0x50) && (m_page3 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                if (((m_page3 & 8) == 0) || (((m_page3 & 8) != 0) && (value != 0xFF)))
                    m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr] = value;
                if ((m_page3 & 4) == 0)
                {
                    RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr] = value;
                    //= value;
                }
            }
            else
            {
                //Нет проверки на открытие спектрумовской графической страницы!!!


                this.MapWriteC000[addr & 0x3fff] = value;
                if (((this.CMR0 & 7) == 5) || ((this.CMR0 & 7) == 7))
                {
                    int blok = ((this.CMR0 & 7) == 5) ? m_vblok4000 : m_vblokC000;
                    //видео-страница закрыта, значит выводим в спектрумовском режиме если открыты стр 5 или 7
                    if ((m_port_y & 64) == 0)
                    {
                        //бит 6==0, значит вывод в видео-озу в спектрумовском режиме разрешен
                        //необходимо транспонировать адрес спектрумовского экрана в адрес спринтеровского озу
                        byte vrampg = (byte)((addr & 0xf0) >> 4);
                        ushort vrampg_row = (ushort)((addr & 0x0f) * 1024);
                        ushort vrampg_col = (ushort)((addr & 0x1f00) >> 8);

                        if ((addr & 0x3fff) <= 0x1fff)
                        {
                            m_vramPages[vrampg][vrampg_row + vrampg_col + (blok * 32)] = value;
                        }
                        else
                        {
                            if ((m_port_y & 128) == 0)
                                m_vramPages[vrampg][vrampg_row + vrampg_col + (((blok + 1) & 31) * 32)] = value;
                        }

                    }
                }
            }
        }

        private new void ReadMemC000(ushort addr, ref byte value)
        {
            if ((m_page3 >= 0x50) && (m_page3 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                value = RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr];
                //m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr];
            }
            else
            {
                base.ReadMemC000(addr, ref value);
                //value = this.MapReadC000[addr & 0x3fff];
            }
        }

        private new void ReadMem8000(ushort addr, ref byte value)
        {
            if ((m_page2 >= 0x50) && (m_page2 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                value = RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr];
                //m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr];
            }
            else
            {
                base.ReadMem8000(addr, ref value);
                //value = this.MapReadC000[addr & 0x3fff];
            }
        }

        private new void ReadMem4000(ushort addr, ref byte value)
        {
            if ((m_page1 >= 0x50) && (m_page1 <= 0x5f))
            {
                //открыта видеостраница, пишем в нее
                ushort vaddr = (ushort)(addr & 0x03ff);
                //00111100 00000000
                byte line = (byte)(m_port_y + ((addr & 0x3c00) >> 10));
                byte vpage = (byte)((line & 0xF0) >> 4);

                // Номер страницы ВидеоОЗУ:
                // 0x50 - нормальная запись
                // 3 бит номера - разрешение записи байта #FF в видео ОЗУ
                // 2 бит номера - разрешение записи в основное ОЗУ
                value = RamPages[0x50 + vpage][((line & 0x0f) * 1024) + vaddr];
                //m_vramPages[vpage][((line & 0x0f) * 1024) + vaddr];
            }
            else
            {
                base.ReadMem4000(addr, ref value);
                //value = this.MapReadC000[addr & 0x3fff];
            }
        }

#if Debug
        private void readRamM1(ushort addr, ref byte value)
        {
            m_opaddr = addr;
        }

        private void LogPort(ushort port, byte value)
        {
            Logger.GetLogger().LogMessage(String.Format("#{0:X4}: Write to port #{1:X4} value #{2:X2}", m_opaddr, port, value));
        }
#endif
        private void readPort00h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                value = 0;
                iorqge = false;
            }
        }

        private void writePort89h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                m_port_y = value;
                m_ulaSprinter.RGADR = value;

                //Перепроверить!!! тут в TASM происходит глюк непонятный
                /*                if ((this.CMR0 & 2) == 0)
                                {
                                    m_vblok4000 = (byte)(value & 31);
                                    m_vblokC000 = (byte)((m_vblok4000 + 1) & 31);//((value & 31) + 1)
                                }
                                else
                                {
                                    m_vblokC000 = (byte)(value & 31);
                                    m_vblok4000 = (byte)((m_vblokC000 + 1) & 31);//((value & 31) + 1);
                                }*/
                m_vblok4000 = (byte)(value & 31);
                m_vblokC000 = (byte)((value & 31) ^ 1);
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPort89h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                value = m_port_y;
                iorqge = false;
            }
        }

        private void writePortC9h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                m_port_videomode = value;
                m_ulaSprinter.RGMOD = value;

#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPortC9h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                value = m_port_videomode;
                iorqge = false;
            }
        }

        private void writePortE9h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                m_port_scr = value;
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPortE9h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                value = m_port_scr;
                iorqge = false;
            }
        }


        private void busReset()
        {
            this.CMR0 = 0;
            this.CMR1 = 0;
            this.PAGE1 = 5;
            this.PAGE0 = 0;
            this.PAGE2 = 2;
            this.PAGE3 = 0x40;
            this.m_cache = false;
            this.m_romindex = 0;
            this.m_sys = false;
            this.m_romA16 = false;
            m_firstread = true;
            //надо false и сделать обработку порта 204Eh
            m_acc_enable = true;
            m_acc_on = false;
            m_acc_mode = AccelCMD.Off;
            UpdateMapping();
        }

        private void writePort7Ch(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                m_sysport = value;
                //Если в порт 0x7C/0x3C записано значение 01, то включается ROM Expansion
                if ((addr & 64) == 64) this.m_romA16 = (value & 0x03) == 0x01;//) ? true : false;
                //                Logger.GetLogger().LogMessage(String.Format("Write to port #{0:X4} value #{1:X2}", addr, value));

                //                this.lb.Items.Add(String.Format("Write to port #{0:X4} value #{1:X2}", addr, value));
                this.m_sys = (addr & 64) == 0;//) ? true : false;
#if Debug
                LogPort(addr, value);
                Logger.GetLogger().LogMessage(String.Format("State: SYS - {0}, AROM16 - {1}", this.m_sys, this.m_romA16));
#endif

                //                this.lb.Items.Add(String.Format("State: SYS - {0}, AROM16 - {1}", this.m_sys, this.m_romA16));
                if ((value & 0x04) != 0)
                {
                    m_SprinterBDI.OpenPorts = ((value & 0x1C) == 0x1C ? true : false);
                }
                this.UpdateMapping();

                //                if ((value & 0x03) == 1) this.m_rom_exp = true;
                //                    else this.m_rom_exp = false;
                //                this.m_1ffd = value;
            }
        }

        private void readPortFBh(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                //Включение кеша
                this.m_cache = true;
                this.UpdateMapping();
            }
        }

        private void readPort7Bh(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                //Выключение кеша
                this.m_cache = false;
                this.UpdateMapping();
            }
        }

        private void writePort1FFDh(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                this.CMR1 = value;
                this.UpdateMapping();
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void writePort7FFDh(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                this.CMR0 = value;
                this.UpdateMapping();
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPort82h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                if (m_firstread)
                {
                    this.PAGE0 = 0x40;
                    this.UpdateMapping();
                    m_firstread = false;
                }
                value = this.PAGE0;
            }
        }

        private void writePort82h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                this.PAGE0 = value;
                this.UpdateMapping();
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPortA2h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                if (m_firstread)
                {
                    this.PAGE1 = 0x40;
                    this.UpdateMapping();
                    m_firstread = false;
                }
                value = this.PAGE1;
            }
        }

        private void writePortA2h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                this.PAGE1 = value;
                this.UpdateMapping();
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPortC2h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                if (m_firstread)
                {
                    this.PAGE2 = 0x40;
                    this.UpdateMapping();
                    m_firstread = false;
                }
                value = this.PAGE2;

            }
        }

        private void writePortC2h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                this.PAGE2 = value;
                this.UpdateMapping();
#if Debug
                LogPort(addr, value);
#endif
            }
        }

        private void readPortE2h(ushort addr, ref byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                if (m_firstread)
                {
                    this.PAGE3 = 0x40;
                    this.UpdateMapping();
                    m_firstread = false;
                }
                //  MessageBox.Show("Reading from PAGE3 Port: "+Convert.ToString(this.PAGE3));
                value = this.PAGE3;
            }
        }

        private void writePortE2h(ushort addr, byte value, ref bool iorqge)
        {
            if (iorqge)
            {
                iorqge = false;
                this.PAGE3 = value;
                this.UpdateMapping();
#if Debug
                LogPort(addr, value);
#endif

                //  MessageBox.Show("Write to PAGE3 Port: " + Convert.ToString(value));
            }
        }

        #endregion

        protected override void UpdateMapping()
        {
            if (((this.CMR1 & 0x01) == 1) && (this.m_sys))
            //            if ((this.CMR1 & 0x01) == 1)
            {
                //Вкл ОЗУ в 0й сектор адресного пространства (вместо ПЗУ)
                base.MapRead0000 = this.RamPages[this.PAGE0];
                base.MapWrite0000 = this.MapRead0000;
            }
            else
            {

                if (m_cache && this.m_sys)
                {
                    base.MapRead0000 = this.CachePages[0];
                    base.MapWrite0000 = base.MapRead0000;
                }
                else
                {
                    //Добавить выдачу страниц ПЗУ
                    //byte rom_exp = (byte)((this.m_rom_exp == true) ? 8 : 0);
                    //                    int index = (this.CMR0 & 0x10) >> 4;
                    //                    index = index | ((this.DOSEN) ? 0:1); // 1 bit (RA14)
                    //                    index = index | ( ((this.DOSEN) ? 1:0) & ((this.CMR1 & 1) & )

                    //int videoPage = ((this.CMR0 & 8) == 0) ? 5 : 7;
                    //                    if (((this.CMR1 & 0x01) == 0) && (!this.m_sys))
                    m_romindex = 0;
                    //                    if (((this.CMR1 & 0x01) == 1) || (!this.m_sys))
                    //                        m_romindex = 8;
                    m_romindex += (byte)((this.m_romA16) ? 0 : 8);

                    //m_romindex = (byte)((this.m_sys == false) ? 8 : 0);

                    /*                    if (((this.CMR1 & 1) == 0) || (!this.m_sys)) {
                                            if (this.m_romA16) m_romindex += 4;
                                        }*/

                    //                    if (this.m_romA16 && !(((this.CMR1 & 1) == 1) && this.m_sys)) m_romindex += 4;

                    base.MapRead0000 = this.RomPages[m_romindex];
                    base.MapWrite0000 = this.m_trashPage;
                }
            }

            base.MapRead4000 = this.RamPages[this.PAGE1];
            base.MapRead8000 = this.RamPages[this.PAGE2];
            base.MapReadC000 = this.RamPages[this.PAGE3];
            if ((PAGE1 >= 0x50) && (PAGE1 <= 0x5F))
            {
                base.MapWrite4000 = m_trashPage;
            }
            else base.MapWrite4000 = base.MapRead4000;
            if ((PAGE2 >= 0x50) && (PAGE2 <= 0x5F))
            {
                base.MapWrite8000 = m_trashPage;
            }
            else base.MapWrite8000 = base.MapRead8000;

            if ((PAGE3 >= 0x50) && (PAGE3 <= 0x5F))
            {
                base.MapWriteC000 = m_trashPage;
            }
            else base.MapWriteC000 = base.MapReadC000;
            base.Map48[0] = 0;
            base.Map48[1] = this.PAGE1;
            base.Map48[2] = this.PAGE2;
            base.Map48[3] = this.PAGE3;
        }

        #region -- RAM and ROM --

        public override bool IsMap48
        {
            get
            {
                return false;
            }
        }

        public override string Name
        {
            get
            {
                return "Sprinter RAM";
            }
        }

        public byte[][] VRamPages
        {
            get
            {
                return this.m_vramPages;
            }
        }

        public byte[][] CachePages
        {
            get
            {
                return this.m_cramPages;
            }
        }

        #endregion

        [HardwareValue("PAGE0", Description = "Port 82 (PAGE0)")]
        public virtual byte PAGE0
        {
            get
            {
                return this.m_page0;
            }
            set
            {
                if (this.m_page0 != value)
                {
                    this.m_page0 = value;
                    this.UpdateMapping();
                }
            }
        }

        [HardwareValue("PAGE1", Description = "Port A2(PAGE1)")]
        public virtual byte PAGE1
        {
            get
            {
                return this.m_page1;
            }
            set
            {
                if (this.m_page1 != value)
                {
                    this.m_page1 = value;
                    this.UpdateMapping();
                }
            }
        }

        [HardwareValue("PAGE2", Description = "Port C2 (PAGE2)")]
        public virtual byte PAGE2
        {
            get
            {
                return this.m_page2;
            }
            set
            {
                if (this.m_page2 != value)
                {
                    this.m_page2 = value;
                    this.UpdateMapping();
                }
            }
        }

        [HardwareValue("PAGE3", Description = "Port E2 (PAGE3)")]
        public virtual byte PAGE3
        {
            get
            {
                return this.m_page3;
            }
            set
            {
                if (this.m_page3 != value)
                {
                    this.m_page3 = value;
                    this.UpdateMapping();
                }
            }
        }

        [HardwareValue("SYSPORT", Description = "Port XX7C/XX3C")]
        public virtual byte SYSPORT
        {
            get { return this.m_sysport; }
        }

        [HardwareValue("RGADR", Description = "Port 89 (RGADR)")]
        public virtual byte RGADR
        {
            get { return this.m_port_y; }
        }

        [HardwareValue("RGMOD", Description = "Port C9 (RGMOD)")]
        public virtual byte RGMOD
        {
            get
            {
                return this.m_port_videomode;
            }
        }

        [HardwareValue("SYS", Description = "Variable SYS")]
        public virtual bool SYS
        {
            get
            {
                return this.m_sys;
            }
            set
            {
                this.m_sys = value;
                UpdateMapping();
            }
        }

        [HardwareValue("RA16", Description = "Variable RA16")]
        public virtual bool RA16
        {
            get
            {
                return this.m_romA16;
            }
            set
            {
                this.m_romA16 = value;
                UpdateMapping();
            }
        }

        public override string Description
        {
            get
            {
                return "Sprinter 4Mb RAM + 64Kb Cache + 256Kb VRAM Manager";
            }
        }

    }
}
