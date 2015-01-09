using System;
using System.IO;
using System.Xml;
using System.Text;


namespace ZXMAK2.Hardware.IC
{
    public enum AtaReg
    {
        /// <summary>
        /// Error information register when read. 
        /// The write precompensation register when written.
        /// [/CS0,/CS1]=01, [A2-A0]=001
        /// </summary>
        FeatureError = 1,
        /// <summary>
        /// Sector counter register.
        /// This register could be used to make multi-sector transfers. You'd have to write the number of sectors to transfer in this register.
        /// [/CS0,/CS1]=01, [A2-A0]=010
        /// </summary>
        SectorCount = 2,
        /// <summary>
        /// Start sector register. 
        /// This register holds the start sector of the current track to start reading/ writing to.
        /// [/CS0,/CS1]=01, [A2-A0]=011
        /// </summary>
        SectorNumber = 3,
        /// <summary>
        /// Low byte of the cylinder number. 
        /// This register holds low byte of the cylinder number for a disk transfer.
        /// [/CS0,/CS1]=01, [A2-A0]=100
        /// </summary>
        CylinderLow = 4,
        /// <summary>
        /// High two bits of the cylinder number. 
        /// The traditional IDE interface allows only cylinder numbers in the range 0..1023. 
        /// This register gets the two upper bits of this number.
        /// [/CS0,/CS1]=01, [A2-A0]=101
        /// </summary>
        CylinderHigh = 5,
        /// <summary>
        /// Head and device select register. 
        /// The bits 3..0 of this register hold the head number (0..15) for a transfer. 
        /// The bit 4 is to be written 0 for access to the IDE master device, 1 for access to the IDE slave device. 
        /// The bit 6 selects between CHS (0) and LBA (1) addressing mode.
        /// The bits 7 and 5 are fixed at 1x1B in the traditional interface.
        /// [/CS0,/CS1]=01, [A2-A0]=110
        /// </summary>
        HeadAndDrive = 6,
        /// <summary>
        /// Command/status register. 
        /// When written the IDE device regards the data you write to this register as a command. 
        /// When read you get the status of the IDE device. 
        /// Reading this register also clears any interrupts from the IDE device to the controller.
        /// [/CS0,/CS1]=01, [A2-A0]=111
        /// </summary>
        CommandStatus = 7,
        /// <summary>
        /// Second status register/interrupt and reset register. 
        /// When read this register gives you the same status byte as the primary ([/CS0,/CS1]=01, [A2..A0]=111) status register. 
        /// The only difference is that reading this register does not clear the interrupt from the IDE device when read. 
        /// When written you can enable/disable the interrupts the IDE device generates. 
        /// Also you can give a software reset to the IDE device.
        /// [/CS0,/CS1]=10, [A2-A0]=110
        /// </summary>
        ControlAltStatus = 8,
    }

    public class AtaPort
    {
        private AtaDevice[] dev;

        public AtaPort()
        {
            dev = new AtaDevice[2] {
				new AtaDevice(),
				new AtaDevice(),
			};
            dev[0].device_id = 0;
            dev[1].device_id = 0x10;
            Reset();
            dev[0].configure(new IdeDiskDescriptor());
            dev[1].configure(new IdeDiskDescriptor());
        }

        public void Reset()
        {
            //LogAgent.Debug("AtaPort.reset");
            dev[0].reset(RESET_TYPE.RESET_HARD);
            dev[1].reset(RESET_TYPE.RESET_HARD);
        }

        public byte Read(AtaReg n_reg)
        {
            byte result = (byte)(dev[0].read(n_reg) & dev[1].read(n_reg));
            //LogAgent.Debug("AtaPort.read({0}) = 0x{1:X2}", n_reg, result);
            return result;
        }

        public UInt16 ReadData()
        {
            UInt16 result = (UInt16)(dev[0].read_data() & dev[1].read_data());
            //LogAgent.Debug("AtaPort.read_data() = 0x{0:X4}", result);
            return result;
        }

        public void Write(AtaReg n_reg, byte data)
        {
            //LogAgent.Debug("AtaPort.write({0}, 0x{1:X2})", n_reg, data);
            dev[0].write(n_reg, data);
            dev[1].write(n_reg, data);
        }

        public void WriteData(UInt16 data)
        {
            //LogAgent.Debug("AtaPort.write_data(0x{0:X4})", data);
            dev[0].write_data(data);
            dev[1].write_data(data);
        }

        public byte read_intrq()
        {
            byte result = (byte)(dev[0].read_intrq() & dev[1].read_intrq());
            //LogAgent.Debug("AtaPort.read_intrq() = 0x{0:X2}", result);
            return result;
        }

        public void Load(string fileName)
        {
            try
            {
                var cfg0 = new IdeDiskDescriptor();
                var cfg1 = new IdeDiskDescriptor();
                if (fileName != null)
                {
                    if (File.Exists(fileName))
                        cfg0.Load(fileName);
                    else
                        cfg0.Save(fileName);
                }
                dev[0].configure(cfg0);
                dev[1].configure(cfg1);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool LedIo
        {
            get { return dev[0].LedIo || dev[1].LedIo; }
            set { dev[0].LedIo = dev[1].LedIo = value; }
        }

        public bool LogIo
        {
            get { return dev[0].LogIo || dev[1].LogIo; }
            set { dev[0].LogIo = dev[1].LogIo = value; }
        }
    }

    /// <summary>
    /// ATA device emulation 
    /// based on unrealspeccy source
    /// </summary>
    public class AtaDevice : IDisposable
    {
        private UInt32 c, h, s, lba;
        private byte[] regs { get { return reg.__regs; } }
        private AtaRegsUnion reg = new AtaRegsUnion();

        private bool intrq;
        public byte device_id;             // 0x00 - master, 0x10 - slave
        private bool atapi;                 // flag for CD-ROM device

        private HD_STATE state;
        private uint transptr, transcount;
        private int phys_dev;
        private byte[] transbf = new byte[0xFFFF]; // ATAPI is able to tranfer 0xFFFF bytes. passing more leads to error

        private AtaPasser ata_p = new AtaPasser();
        //ATAPI_PASSER atapi_p;
        
        public bool LedIo;
        public bool LogIo;


        public void Dispose()
        {
            ata_p.Dispose();
        }


        public bool loaded()
        {
            //was crashed at atapi_p.loaded() if no master or slave device!!! see fix in ATAPI_PASSER //Alone Coder
            return ata_p.IsLoaded();// || atapi_p.loaded(); 
        }

        public void configure(IdeDiskDescriptor cfg)
        {
            ata_p.Close();
            c = cfg.c;
            h = cfg.h;
            s = cfg.s;
            lba = cfg.lba;

            for (int i = 0; i < regs.Length; i++)	// clear registers
                regs[i] = 0;
            command_ok(); // reset state and transfer position

            phys_dev = -1;
            if (String.IsNullOrEmpty(cfg.image))
                return;

            var filedev = new PhysicalDeviceInfo();
            filedev.filename = cfg.image;
            filedev.type = cfg.cd ? DEVTYPE.ATA_FILECD : DEVTYPE.ATA_FILEHDD;

            bool success = false;
            if (filedev.type == DEVTYPE.ATA_FILEHDD)
            {
                filedev.usage = DEVUSAGE.ATA_OP_USE;
                success = ata_p.Open(filedev, cfg.readOnly | cfg.cd);
                atapi = false;
            }
            //if (filedev.type == DEVTYPE.ATA_FILECD)
            //{
            //    filedev.usage = DEVUSAGE.ATA_OP_USE;
            //    errCode = atapi_p.open(filedev);
            //    atapi = 1;
            //}
            if (success)
                return;
            cfg.image = string.Empty;
        }


        public byte read(AtaReg n_reg)
        {
            if (!loaded())
                return 0xFF;
            if (((reg.devhead ^ device_id) & 0x10) != 0)
            {
                return 0xFF;
            }

            if (n_reg == AtaReg.CommandStatus)
                intrq = false;
            if (n_reg == AtaReg.ControlAltStatus)
                n_reg = AtaReg.CommandStatus; // read alt.status -> read status
            if (n_reg == AtaReg.CommandStatus ||
                (reg.status & HD_STATUS.STATUS_BSY) != 0)
            {
                //	   printf("state=%d\n",state); //Alone Coder
                return (byte)reg.status;
            } // BSY=1 or read status
            // BSY = 0
            //// if (reg.status & STATUS_DRQ) return 0xFF;    // DRQ.  ATA-5: registers should not be queried while DRQ=1, but programs do this!
            // DRQ = 0
            return regs[(int)n_reg];
        }

        public void write(AtaReg n_reg, byte data)
        {
            //   printf("dev=%d, reg=%d, data=%02X\n", device_id, n_reg, data);
            if (!loaded())
                return;
            if (n_reg == AtaReg.FeatureError)
            {
                reg.feat = data;
                return;
            }

            if (n_reg != AtaReg.CommandStatus)
            {
                regs[(int)n_reg] = data;
                if ((reg.control & HD_CONTROL.CONTROL_SRST) != 0)
                {
                    //          printf("dev=%d, reset\n", device_id);
                    reset(RESET_TYPE.RESET_SRST);
                }
                return;
            }

            // execute command!
            if (((reg.devhead ^ device_id) & 0x10) != 0 &&
                data != 0x90)
            {
                return;
            }
            if ((reg.status & HD_STATUS.STATUS_DRDY) == 0 && !atapi)
            {
                Logger.Warn("hdd not ready cmd = #{0:X2} (ignored)", data);
                return;
            }

            reg.err = HD_ERROR.ERR_NONE;
            intrq = false;

            //{printf(" [");for (int q=1;q<9;q++) printf("-%02X",regs[q]);printf("]\n");}
            if (exec_atapi_cmd(data))
                return;
            if (exec_ata_cmd(data))
                return;
            reg.status = HD_STATUS.STATUS_DSC | HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_ERR;
            reg.err = HD_ERROR.ERR_ABRT;
            state = HD_STATE.S_IDLE;
            intrq = true;
        }

        public UInt16 read_data()
        {
            if (!loaded())
                return 0xFFFF;
            if (((reg.devhead ^ device_id) & 0x10) != 0)
                return 0xFFFF;
            if (/* (reg.status & (STATUS_DRQ | STATUS_BSY)) != STATUS_DRQ ||*/ transptr >= transcount)
                return 0xFFFF;

            LedIo = true;
            // DRQ=1, BSY=0, data present
            UInt16 result = (UInt16)(transbf[transptr * 2] | (transbf[transptr * 2 + 1] << 8));
            transptr++;
            //   printf(__FUNCTION__" data=0x%04X\n", result & 0xFFFF);

            if (transptr < transcount)
                return result;
            // look to state, prepare next block
            if (state == HD_STATE.S_READ_ID || state == HD_STATE.S_READ_ATAPI)
                command_ok();
            if (state == HD_STATE.S_READ_SECTORS)
            {
                //       __debugbreak();
                //       printf("dev=%d, cnt=%d\n", device_id, reg.count);
                if (--reg.count == 0)
                    command_ok();
                else
                {
                    next_sector();
                    read_sectors();
                }
            }

            return result;
        }

        public void write_data(UInt16 data)
        {
            if (!loaded())
                return;
            if (((reg.devhead ^ device_id) & 0x10) != 0)
                return;
            if (/* (reg.status & (STATUS_DRQ | STATUS_BSY)) != STATUS_DRQ ||*/ transptr >= transcount)
                return;

            LedIo = true;
            transbf[transptr * 2] = (byte)data;
            transbf[transptr * 2 + 1] = (byte)(data >> 8);
            transptr++;
            if (transptr < transcount)
                return;
            // look to state, prepare next block
            if (state == HD_STATE.S_WRITE_SECTORS)
            {
                write_sectors();
                return;
            }

            if (state == HD_STATE.S_FORMAT_TRACK)
            {
                format_track();
                return;
            }

            if (state == HD_STATE.S_RECV_PACKET)
            {
                handle_atapi_packet();
                return;
            }
            /*   if (state == S_MODE_SELECT) { exec_mode_select(); return; } */
        }

        public byte read_intrq()
        {
            if (!loaded() ||
                ((reg.devhead ^ device_id) & 0x10) != 0 ||
                (reg.control & HD_CONTROL.CONTROL_nIEN) != 0)
            {
                return 0xFF;
            }
            return intrq ? (byte)0xFF : (byte)0x00;
        }

        public bool exec_ata_cmd(byte cmd)
        {
            //   printf(__FUNCTION__" cmd=%02X\n", cmd);
            // EXECUTE DEVICE DIAGNOSTIC for both ATA and ATAPI
            if (cmd == 0x90)
            {
                reset_signature(RESET_TYPE.RESET_SOFT);
                return true;
            }

            if (atapi)
                return false;

            // [DEVICE RESET]
            if (cmd == 0x08)
            {
                reset(RESET_TYPE.RESET_SOFT);
                return true;
            }
            // INITIALIZE DEVICE PARAMETERS
            if (cmd == 0x91)
            {
                // pos = (reg.cyl * h + (reg.devhead & 0x0F)) * s + reg.sec - 1;
                h = (uint)((reg.devhead & 0xF) + 1);
                s = reg.count;
                if (s == 0)
                {
                    reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DF | HD_STATUS.STATUS_DSC | HD_STATUS.STATUS_ERR;
                    return true;
                }

                c = lba / s / h;

                reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DSC;
                return true;
            }

            if ((cmd & 0xFE) == 0x20) // ATA-3 (mandatory), read sectors
            { // cmd #21 obsolette, rqd for is-dos
                //       printf(__FUNCTION__" sec_cnt=%d\n", reg.count);
                read_sectors();
                return true;
            }

            if ((cmd & 0xFE) == 0x40) // ATA-3 (mandatory),  verify sectors
            { //rqd for is-dos
                verify_sectors();
                return true;
            }

            if ((cmd & 0xFE) == 0x30) // ATA-3 (mandatory), write sectors
            {
                if (seek())
                {
                    state = HD_STATE.S_WRITE_SECTORS;
                    reg.status = HD_STATUS.STATUS_DRQ | HD_STATUS.STATUS_DSC;
                    transptr = 0;
                    transcount = 0x100;
                }
                return true;
            }

            if (cmd == 0x50) // format track (данная реализация - ничего не делает)
            {
                reg.sec = 1;
                if (seek())
                {
                    state = HD_STATE.S_FORMAT_TRACK;
                    reg.status = HD_STATUS.STATUS_DRQ | HD_STATUS.STATUS_DSC;
                    transptr = 0;
                    transcount = 0x100;
                }
                return true;
            }

            if (cmd == 0xEC)
            {
                prepare_id();
                return true;
            }

            if (cmd == 0xE7)
            { // FLUSH CACHE
                if (ata_p.Flush())
                {
                    command_ok();
                    intrq = true;
                }
                else
                {
                    reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DF | HD_STATUS.STATUS_DSC | HD_STATUS.STATUS_ERR; // 0x71
                }
                return true;
            }

            if (cmd == 0x10)
            {
                recalibrate();
                command_ok();
                intrq = true;
                return true;
            }

            if (cmd == 0x70)
            { // seek
                if (!seek())
                    return true;
                command_ok();
                intrq = true;
                return true;
            }

            Logger.Error("*** unknown ATA cmd #{0:X2} ***", cmd);
            return false;
        }

        public bool exec_atapi_cmd(byte cmd)
        {
            if (!atapi)
                return false;

            // soft reset
            if (cmd == 0x08)
            {
                reset(RESET_TYPE.RESET_SOFT);
                return true;
            }
            if (cmd == 0xA1) // IDENTIFY PACKET DEVICE
            {
                prepare_id();
                return true;
            }

            if (cmd == 0xA0)
            { // packet
                state = HD_STATE.S_RECV_PACKET;
                reg.status = HD_STATUS.STATUS_DRQ;
                reg.intreason = ATAPI_INT_REASON.INT_COD;
                transptr = 0;
                transcount = 6;
                return true;
            }

            if (cmd == 0xEC)
            {
                reg.count = 1;
                reg.sec = 1;
                reg.cyl = 0xEB14;

                reg.status = HD_STATUS.STATUS_DSC | HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_ERR;
                reg.err = HD_ERROR.ERR_ABRT;
                state = HD_STATE.S_IDLE;
                intrq = true;
                return true;
            }

            Logger.Error("*** unknown ATAPI cmd #{0:X2} ***\n", cmd);
            // "command aborted" with ATAPI signature
            reg.count = 1;
            reg.sec = 1;
            reg.cyl = 0xEB14;
            return false;
        }

        public void reset_signature(RESET_TYPE mode = RESET_TYPE.RESET_SOFT)
        {
            reg.count = reg.sec = 1;
            reg.err = HD_ERROR.ERR_AMNF;	// = 1
            reg.cyl = atapi ? (ushort)0xEB14 : (ushort)0;
            reg.devhead &= (atapi && mode == RESET_TYPE.RESET_SOFT) ? (byte)0x10 : (byte)0;
            reg.status = (mode == RESET_TYPE.RESET_SOFT || !atapi) ?
                (HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DSC) : HD_STATUS.STATUS_NONE;
        }

        public void reset(RESET_TYPE mode)
        {
            reg.control = 0; // clear SRST
            intrq = false;

            command_ok();
            reset_signature(mode);
        }

        public bool seek()
        {
            uint pos;
            if ((reg.devhead & 0x40) != 0)
            {
                // LBA mode
                long tmp = regs[3] | (regs[4] << 8) | (regs[5] << 16) | (regs[6] << 24);
                pos = (uint)(tmp & 0x0FFFFFFF);
                if (pos >= lba)
                {
                    //          printf("seek error: lba %d:%d\n", lba, pos);
                    reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DF | HD_STATUS.STATUS_ERR;
                    reg.err = HD_ERROR.ERR_IDNF | HD_ERROR.ERR_ABRT;
                    intrq = true;
                    return false;
                }
                //      printf("lba %d:%d\n", lba, pos);
            }
            else
            {
                // CHS mode
                if (reg.cyl >= c || (uint)(reg.devhead & 0x0F) >= h || reg.sec > s || reg.sec == 0)
                {
                    //          printf("seek error: chs %4d/%02d/%02d\n", *(unsigned short*)(regs+4), (reg.devhead & 0x0F), reg.sec);
                    reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DF | HD_STATUS.STATUS_ERR;
                    reg.err = HD_ERROR.ERR_IDNF | HD_ERROR.ERR_ABRT;
                    intrq = true;
                    return false;
                }
                pos = (uint)((reg.cyl * h + (reg.devhead & 0x0F)) * s + reg.sec - 1);
                //      printf("chs %4d/%02d/%02d: %8d\n", *(unsigned short*)(regs+4), (reg.devhead & 0x0F), reg.sec, pos);
            }
            //printf("[seek %I64d]", ((__int64)pos) << 9);
            if (LogIo)
            {
                Logger.Info("IDE HDD SEEK lba={0} [fileOffset=#{1:X8}]", pos, ((long)pos) << 9);
            }
            if (!ata_p.Seek(pos))
            {
                reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DF | HD_STATUS.STATUS_ERR;
                reg.err = HD_ERROR.ERR_IDNF | HD_ERROR.ERR_ABRT;
                intrq = true;
                return false;
            }
            return true;
        }

        public void recalibrate()
        {
            reg.cyl = 0;
            reg.devhead &= 0xF0;

            if ((reg.devhead & 0x40) != 0) // LBA
            {
                reg.sec = 0;
                return;
            }

            reg.sec = 1;
        }

        public void prepare_id()
        {
            if (phys_dev == -1)
            {
                for (int i = 0; i < 512; i++)
                    transbf[i] = 0;
                string firmwareVersion = System.Windows.Forms.Application.ProductVersion;

                make_ata_string(transbf, 10 * 2, 10, "00000000001234567890");	// Serial number
                make_ata_string(transbf, 23 * 2, 4, firmwareVersion);			// Firmware revision
                make_ata_string(transbf, 27 * 2, 20, "ZXMAK2 HDD IMAGE");		// Model number

                setUInt16(transbf, 0 * 2, 0x045A);		// [General configuration]
                setUInt16(transbf, 1 * 2, (UInt16)c);
                setUInt16(transbf, 3 * 2, (UInt16)h);
                setUInt16(transbf, 6 * 2, (UInt16)s);
                setUInt16(transbf, 20 * 2, 3);			// a dual ported multi-sector buffer capable of simultaneous transfers with a read caching capability
                setUInt16(transbf, 21 * 2, 512);		// cache size=256k
                setUInt16(transbf, 22 * 2, 4);			// ECC bytes
                setUInt16(transbf, 49 * 2, 0x200);		// LBA supported
                setUInt32(transbf, 60 * 2, lba);		// [Total number of user addressable logical sectors]
                setUInt16(transbf, 80 * 2, 0x3E);		// support specifications up to ATA-5
                setUInt16(transbf, 81 * 2, 0x13);		// ATA/ATAPI-5 T13 1321D revision 3
                setUInt16(transbf, 82 * 2, 0x60);		// supported look-ahead and write cache

                // make checksum
                transbf[510] = 0xA5;
                byte cs = 0;
                for (int i = 0; i < 511; i++)
                    cs += transbf[i];
                transbf[511] = (byte)(0 - cs);
            }
            else
            { // copy as is...
                //for(int i=0; i < 512; i++)
                //	transbf[i] = phys[phys_dev].idsector[i];
            }

            state = HD_STATE.S_READ_ID;
            transptr = 0;
            transcount = 0x100;
            intrq = true;
            reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DRQ | HD_STATUS.STATUS_DSC;
            reg.err = HD_ERROR.ERR_NONE;
        }

        public void command_ok()
        {
            state = HD_STATE.S_IDLE;
            transptr = 0xFFFFFFFF;
            reg.err = 0;
            reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DSC;
        }

        public void next_sector()
        {
            if ((reg.devhead & 0x40) != 0)
            { // LBA
                // Original C++:
                //*(unsigned*)&reg.sec = (*(unsigned*)&reg.sec & 0xF0000000) + ((*(unsigned*)&reg.sec+1) & 0x0FFFFFFF);
                long tmp = regs[3] | (regs[4] << 8) | (regs[5] << 16) | (regs[6] << 24);
                tmp = (tmp & 0xF0000000) + ((tmp + 1) & 0x0FFFFFFF);
                regs[3] = (byte)tmp;
                regs[4] = (byte)(tmp >> 8);
                regs[5] = (byte)(tmp >> 16);
                regs[6] = (byte)(tmp >> 24);
                return;
            }
            // need to recalc CHS for every sector, coz ATA registers
            // should contain current position on failure
            if (reg.sec < s)
            {
                reg.sec++;
                return;
            }
            reg.sec = 1;
            byte head = (byte)((reg.devhead & 0x0F) + 1);
            if (head < h)
            {
                reg.devhead = (byte)((reg.devhead & 0xF0) | head);
                return;
            }
            reg.devhead &= 0xF0;
            reg.cyl++;
        }

        public void read_sectors()
        {
            //   __debugbreak();
            intrq = true;
            if (!seek())
                return;

            if (LogIo)
            {
                Logger.Info("IDE HDD READ SECTOR ****************************************************************");
            }
            if (!ata_p.ReadSector(transbf, 0))
            {
                reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DSC | HD_STATUS.STATUS_ERR;
                reg.err = HD_ERROR.ERR_UNC | HD_ERROR.ERR_IDNF;
                state = HD_STATE.S_IDLE;
                return;
            }
            transptr = 0;
            transcount = 0x100;
            state = HD_STATE.S_READ_SECTORS;
            reg.err = HD_ERROR.ERR_NONE;
            reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DRQ | HD_STATUS.STATUS_DSC;

            /*
               if(reg.devhead & 0x40)
                   printf("dev=%d lba=%d\n", device_id, *(unsigned*)(regs+3) & 0x0FFFFFFF);
               else
                   printf("dev=%d c/h/s=%d/%d/%d\n", device_id, reg.cyl, (reg.devhead & 0xF), reg.sec);
            */
        }

        public void verify_sectors()
        {
            intrq = true;
            //   __debugbreak();

            do
            {
                --reg.count;
                /*
                       if(reg.devhead & 0x40)
                           printf("lba=%d\n", *(unsigned*)(regs+3) & 0x0FFFFFFF);
                       else
                           printf("c/h/s=%d/%d/%d\n", reg.cyl, (reg.devhead & 0xF), reg.sec);
                */
                if (!seek())
                    return;
                /*
                       u8 Buf[512];
                       if (!ata_p.read_sector(Buf))
                       {
                          reg.status = STATUS_DRDY | STATUS_DF | STATUS_CORR | STATUS_DSC | STATUS_ERR;
                          reg.err = ERR_UNC | ERR_IDNF | ERR_ABRT | ERR_AMNF;
                          state = S_IDLE;
                          return;
                       }
                */
                if (reg.count != 0)
                    next_sector();
            } while (reg.count != 0);
            command_ok();
        }

        public void write_sectors()
        {
            intrq = true;
            //printf(" [write] ");
            if (!seek())
                return;

            if (LogIo)
            {
                Logger.Info("IDE HDD WRITE SECTOR ***************************************************************");
            }
            if (!ata_p.WriteSector(transbf, 0))
            {
                reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DSC | HD_STATUS.STATUS_ERR;
                reg.err = HD_ERROR.ERR_UNC;
                state = HD_STATE.S_IDLE;
                return;
            }

            if (--reg.count == 0)
            {
                command_ok();
                return;
            }
            next_sector();

            transptr = 0;
            transcount = 0x100;
            state = HD_STATE.S_WRITE_SECTORS;
            reg.err = HD_ERROR.ERR_NONE;
            // Alex: DRDY added for SPRINTER 
            ///Missing DRDY produce not ready error on write operation
            reg.status = HD_STATUS.STATUS_DRDY | HD_STATUS.STATUS_DRQ | HD_STATUS.STATUS_DSC;
        }

        public void format_track()
        {
            intrq = true;
            if (!seek())
                return;

            command_ok();
            return;
        }

        public void handle_atapi_packet()
        {
            Logger.Error("handle_atapi_packet: method not implemented");
        }

        public void handle_atapi_packet_emulate()
        {
            Logger.Error("handle_atapi_packet_emulate: method not implemented");
        }

        public void exec_mode_select()
        {
        }

        #region Utils

        private static void make_ata_string(byte[] dst, int dstOffset, int n_words, string srcText)
        {
            byte[] srcArray = Encoding.ASCII.GetBytes(srcText);
            for (int i = 0; i < n_words * 2; i++)
            {
                byte value = i < srcArray.Length ? srcArray[i] : (byte)0x20;
                if (value < 0x20 || value > 0x7E)
                    value = 0x20;
                dst[dstOffset + i] = value;
            }
            for (int i = 0; i < n_words * 2; i += 2)
            {
                dst[dstOffset + i] ^= dst[dstOffset + i + 1];
                dst[dstOffset + i + 1] ^= dst[dstOffset + i];
                dst[dstOffset + i] ^= dst[dstOffset + i + 1];
            }
        }

        private static void setUInt16(byte[] transbf, int index, UInt16 value)
        {
            transbf[index] = (byte)value;
            transbf[index + 1] = (byte)(value >> 8);
        }

        private static void setUInt32(byte[] transbf, int index, UInt32 value)
        {
            transbf[index] = (byte)value;
            transbf[index + 1] = (byte)(value >> 8);
            transbf[index + 2] = (byte)(value >> 16);
            transbf[index + 3] = (byte)(value >> 24);
        }

        #endregion
    }

    #region enums

    public enum RESET_TYPE { RESET_HARD, RESET_SOFT, RESET_SRST };
    public enum ATAPI_INT_REASON : byte
    {
        INT_COD = 0x01,
        INT_IO = 0x02,
        INT_RELEASE = 0x04
    };

    public enum HD_STATUS : byte
    {
        STATUS_BSY = 0x80,
        STATUS_DRDY = 0x40,
        STATUS_DF = 0x20,
        STATUS_DSC = 0x10,
        STATUS_DRQ = 0x08,
        STATUS_CORR = 0x04,
        STATUS_IDX = 0x02,
        STATUS_ERR = 0x01,

        STATUS_NONE = 0,
    };

    public enum HD_ERROR : byte
    {
        ERR_BBK = 0x80,
        ERR_UNC = 0x40,
        ERR_MC = 0x20,
        ERR_IDNF = 0x10,
        ERR_MCR = 0x08,
        ERR_ABRT = 0x04,
        ERR_TK0NF = 0x02,
        ERR_AMNF = 0x01,

        ERR_NONE = 0,
    };

    public enum HD_CONTROL : byte
    {
        CONTROL_SRST = 0x04,
        CONTROL_nIEN = 0x02
    };

    public enum HD_STATE
    {
        S_IDLE = 0, S_READ_ID,
        S_READ_SECTORS, S_VERIFY_SECTORS, S_WRITE_SECTORS, S_FORMAT_TRACK,
        S_RECV_PACKET, S_READ_ATAPI,
        S_MODE_SELECT
    };

    #endregion

    public class AtaRegsUnion
    {
        public byte[] __regs = new byte[12];

        public byte data { get { return __regs[0]; } set { __regs[0] = value; } }
        // for write, features
        public HD_ERROR err { get { return (HD_ERROR)__regs[1]; } set { __regs[1] = (byte)value; } }
        public byte count { get { return __regs[2]; } set { __regs[2] = value; } }
        public ATAPI_INT_REASON intreason { get { return (ATAPI_INT_REASON)__regs[2]; } set { __regs[2] = (byte)value; } }
        public byte sec { get { return __regs[3]; } set { __regs[3] = value; } }
        public UInt16 cyl
        {
            get { return (UInt16)(__regs[4] | (__regs[5] << 8)); }
            set { __regs[4] = (byte)value; __regs[5] = (byte)(value >> 8); }
        }
        public UInt16 atapi_count
        {
            get { return (UInt16)(__regs[4] | (__regs[5] << 8)); }
            set { __regs[4] = (byte)value; __regs[5] = (byte)(value >> 8); }
        }
        public byte cyl_l { get { return __regs[4]; } set { __regs[4] = value; } }
        public byte cyl_h { get { return __regs[5]; } set { __regs[5] = value; } }
        public byte devhead { get { return __regs[6]; } set { __regs[6] = value; } }
        // for write, cmd
        public HD_STATUS status { get { return (HD_STATUS)__regs[7]; } set { __regs[7] = (byte)value; } }
        /*                  */
        // reg8 - control (CS1,DA=6)
        public HD_CONTROL control { get { return (HD_CONTROL)__regs[8]; } set { __regs[8] = (byte)value; } }
        public byte feat { get { return __regs[9]; } set { __regs[9] = value; } }
        public byte cmd { get { return __regs[10]; } set { __regs[10] = value; } }
        // reserved
        public byte reserved { get { return __regs[11]; } set { __regs[11] = value; } }

        public AtaRegsUnion()
        {
            __regs[(int)AtaReg.FeatureError] = 0x00;
            __regs[(int)AtaReg.SectorCount] = 0x01;
            __regs[(int)AtaReg.SectorNumber] = 0x01;
            __regs[(int)AtaReg.CylinderLow] = 0x00;
            __regs[(int)AtaReg.CylinderHigh] = 0x00;
            __regs[(int)AtaReg.HeadAndDrive] = 0xA0;
            __regs[(int)AtaReg.CommandStatus] = 0x00;
            __regs[(int)AtaReg.ControlAltStatus] = 0x00;
        }
    }

    public class IdeDiskDescriptor
    {
        public string image = string.Empty;
        public uint c = 20, h = 16, s = 63, lba = 20160;
        public bool readOnly = true;
        public bool cd = false;

        public void Save(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            XmlNode root = xml.AppendChild(xml.CreateElement("IdeDiskDescriptor"));
            XmlNode imageNode = root.AppendChild(xml.CreateElement("Image"));
            string imageFile = image ?? string.Empty;
            if (imageFile != string.Empty &&
                Path.GetDirectoryName(imageFile) == Path.GetDirectoryName(fileName))
            {
                imageFile = Path.GetFileName(imageFile);
            }
            Utils.SetXmlAttribute(imageNode, "fileName", imageFile);
            Utils.SetXmlAttribute(imageNode, "isCdrom", cd);
            Utils.SetXmlAttribute(imageNode, "isReadOnly", readOnly);
            XmlNode geometryNode = root.AppendChild(xml.CreateElement("Geometry"));
            Utils.SetXmlAttribute(geometryNode, "cylinders", c);
            Utils.SetXmlAttribute(geometryNode, "heads", h);
            Utils.SetXmlAttribute(geometryNode, "sectors", s);
            Utils.SetXmlAttribute(geometryNode, "lba", lba);
            xml.Save(fileName);
        }

        public void Load(string fileName)
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(fileName);
            XmlNode root = xml["IdeDiskDescriptor"];
            XmlNode imageNode = root["Image"];
            XmlNode geometryNode = root["Geometry"];
            image = Utils.GetXmlAttributeAsString(imageNode, "fileName", image ?? string.Empty);
            if (image != string.Empty && !Path.IsPathRooted(image))
                image = Utils.GetFullPathFromRelativePath(image, Path.GetDirectoryName(fileName));
            cd = Utils.GetXmlAttributeAsBool(imageNode, "isCdrom", false);
            readOnly = Utils.GetXmlAttributeAsBool(imageNode, "isReadOnly", true);
            c = Utils.GetXmlAttributeAsUInt32(geometryNode, "cylinders", c);
            h = Utils.GetXmlAttributeAsUInt32(geometryNode, "heads", h);
            s = Utils.GetXmlAttributeAsUInt32(geometryNode, "sectors", s);
            lba = Utils.GetXmlAttributeAsUInt32(geometryNode, "lba", lba);
        }
    }


    public enum DEVTYPE { ATA_NONE, ATA_FILEHDD, /*ATA_NTHDD, ATA_SPTI_CD, ATA_ASPI_CD,*/ ATA_FILECD };
    public enum DEVUSAGE { ATA_OP_ENUM_ONLY, ATA_OP_USE };

    public class PhysicalDeviceInfo
    {
        public DEVTYPE type;
        public DEVUSAGE usage;
        public uint hdd_size;
        public uint spti_id;
        public uint adapterid, targetid; // ASPI
        public byte[] idsector = new byte[512];
        public string filename;
        public string viewname;
    };


    public class AtaPasser : IDisposable
    {
        private FileStream _hDevice;
        private PhysicalDeviceInfo _deviceInfo;
        private bool _isReadOnly;


        public void Dispose()
        {
            Close();
        }

        
        public bool Open(PhysicalDeviceInfo deviceInfo, bool isReadOnly)
        {
            try
            {
                _deviceInfo = deviceInfo;
                _isReadOnly = isReadOnly;
                _hDevice = new FileStream(
                    _deviceInfo.filename,
                    _isReadOnly ? FileMode.Open : FileMode.OpenOrCreate,
                    _isReadOnly ? FileAccess.Read : FileAccess.ReadWrite,
                    FileShare.ReadWrite);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public void Close()
        {
            try
            {
                if (_hDevice != null)
                {
                    _hDevice.Dispose();
                }
                _hDevice = null;
                _deviceInfo = null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public bool IsLoaded()
        {
            return _hDevice != null;
        }

        public bool Flush()
        {
            try
            {
                if (_isReadOnly)
                {
                    return true;
                }
                if (_hDevice != null)
                {
                    _hDevice.Flush();
                }
                return _hDevice != null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public bool Seek(uint nsector)
        {
            try
            {
                if (_hDevice == null)
                {
                    return false;
                }
                var offset = ((long)nsector) << 9;
                var newOffset = _hDevice.Seek(offset, SeekOrigin.Begin);
                return newOffset == offset && offset >= 0;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public bool ReadSector(byte[] dst, int offset)
        {
            try
            {
                if (_hDevice == null)
                {
                    return false;
                }
                var read = _hDevice.Read(dst, offset, 512);
                for (var i = read; i < 512; i++)
                {
                    dst[i + offset] = 0;
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }

        public bool WriteSector(byte[] src, int offset)
        {
            try
            {
                if (_isReadOnly)
                {
                    return true;
                }
                if (_hDevice != null)
                {
                    _hDevice.Write(src, offset, 512);
                }
                return _hDevice != null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }
        }
    }
}
