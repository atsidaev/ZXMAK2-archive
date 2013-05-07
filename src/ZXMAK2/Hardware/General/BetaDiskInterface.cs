using System;
using System.IO;
using System.Xml;

using ZXMAK2.Engine;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;
using ZXMAK2.Serializers;
using ZXMAK2.Entities;
using ZXMAK2.Crc;



namespace ZXMAK2.Hardware.General
{
    #region enums
    public enum WDSTATE : byte
    {
        S_IDLE = 0,
        S_WAIT,

        S_DELAY_BEFORE_CMD,
        S_CMD_RW,
        S_FOUND_NEXT_ID,
        S_RDSEC,
        S_READ,
        S_WRSEC,
        S_WRITE,
        S_WRTRACK,
        S_WR_TRACK_DATA,

        S_TYPE1_CMD,
        S_STEP,
        S_SEEKSTART,
        S_SEEK,
        S_VERIFY,

        S_RESET,
    }

    public enum BETA_STATUS : byte
    {
        NONE = 0x00,
        DRQ = 0x40,
        INTRQ = 0x80,
    }

    [Flags]
    public enum WD_STATUS
    {
        WDS_NONE = 0,
        WDS_BUSY = 0x01,
        WDS_INDEX = 0x02,
        WDS_DRQ = 0x02,
        WDS_TRK00 = 0x04,
        WDS_LOST = 0x04,
        WDS_CRCERR = 0x08,
        WDS_NOTFOUND = 0x10,
        WDS_SEEKERR = 0x10,
        WDS_RECORDT = 0x20,
        WDS_HEADL = 0x20,
        WDS_WRFAULT = 0x20,
        WDS_WRITEP = 0x40,
        WDS_NOTRDY = 0x80
    }

    [Flags]
    public enum WD_SYS
    {
        SYS_HLT = 0x08,
        SYS_RST = 0x04,
    };

    #endregion


    public class BetaDiskInterface : BusDeviceBase, IConfigurable, IBetaDiskDevice, IGuiExtension
    {
        #region IBusDevice

        public override string Name { get { return "BDI"; } }
        public override string Description { get { return "Beta Disk Interface"; } }
        public override BusDeviceCategory Category { get { return BusDeviceCategory.Disk; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_cpu = bmgr.CPU;
            m_sandbox = bmgr.IsSandbox;
            m_memory = bmgr.FindDevice<IMemoryDevice>();

            bmgr.SubscribeRDMEM_M1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x4000, BusReadMemRam);
            bmgr.SubscribeRDMEM_M1(0xC000, 0x8000, BusReadMemRam);
            bmgr.SubscribeRDMEM_M1(0xC000, 0xC000, BusReadMemRam);
            bmgr.SubscribeBeginFrame(BusBeginFrame);
            bmgr.SubscribeEndFrame(BusEndFrame);

            BusSubscribeWD93IO(bmgr);

            bmgr.SubscribeRESET(BusReset);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);

            foreach (var fs in FDD[0].SerializeManager.GetSerializers())
            {
                bmgr.AddSerializer(fs);
            }
            bmgr.RegisterIcon(m_iconRd);
            bmgr.RegisterIcon(m_iconWr);
        }

        public override void BusConnect()
        {
            if (!m_sandbox)
            {
                foreach (var di in FDD)
                {
                    di.Connect();
                }
            }
        }

        public override void BusDisconnect()
        {
            if (!m_sandbox)
            {
                foreach (var di in FDD)
                {
                    di.Disconnect();
                }
            }
            if (m_memory != null)
                m_memory.DOSEN = false;
        }

        #endregion

        #region IConfigurable

        public void LoadConfig(XmlNode itemNode)
        {
            NoDelay = Utils.GetXmlAttributeAsBool(itemNode, "noDelay", false);
            LogIO = Utils.GetXmlAttributeAsBool(itemNode, "logIO", false);
            for (int i = 0; i < 4; i++)
            {
                FDD[i].FileName = string.Empty;
                FDD[i].IsWP = true;
                FDD[i].Present = false;
            }
            foreach (XmlNode node in itemNode.SelectNodes("Drive"))
            {
                int index = Utils.GetXmlAttributeAsInt32(node, "index", 0);
                string fileName = Utils.GetXmlAttributeAsString(node, "fileName", string.Empty);
                bool inserted = Utils.GetXmlAttributeAsBool(node, "inserted", false);
                bool readOnly = Utils.GetXmlAttributeAsBool(node, "readOnly", true);
                if (index >= 0 && index <= 3)
                {
                    // will be opened on Connect
                    FDD[index].FileName = fileName;
                    FDD[index].IsWP = readOnly;
                    FDD[index].Present = inserted;
                }
            }
        }

        public void SaveConfig(XmlNode itemNode)
        {
            Utils.SetXmlAttribute(itemNode, "noDelay", NoDelay);
            Utils.SetXmlAttribute(itemNode, "logIO", LogIO);
            for (int i = 0; i < 4; i++)
            {
                XmlNode xn = itemNode.AppendChild(itemNode.OwnerDocument.CreateElement("Drive"));
                Utils.SetXmlAttribute(xn, "index", i);
                if (!string.IsNullOrEmpty(FDD[i].FileName))
                    Utils.SetXmlAttribute(xn, "fileName", FDD[i].FileName);
                Utils.SetXmlAttribute(xn, "readOnly", FDD[i].IsWP.ToString());
                Utils.SetXmlAttribute(xn, "inserted", FDD[i].Present.ToString());
            }
        }

        #endregion

        #region IBetaDiskInterface

        public bool DOSEN
        {
            get { return m_dosen; }
            set
            {
                m_dosen = value;
                m_memory.DOSEN = value;
            }
        }

        public DiskImage[] FDD
        {
            get { return fdd; }
        }

        public string DumpState()
        {
            string s1 = string.Format(
                "CMD:    #{0:X2}\nSTATUS: #{1:X2} [{2}]\nTRK:    #{3:X2}\nSEC:    #{4:X2}\nDATA:   #{5:X2}",
                cmd, (int)status, status, track, sector, data);
            string s2 = string.Format(
               "beta:   #{0:X2} [{1}]\nsystem: #{2:X2}\nstate:  {3}\nstate2: {4}\n" +
               "drive:  {5}\nside:   {6}\ntime:   {7}\nnext:   {8}\n" +
               "tshift: {9}\nrwptr:  {10}\nrwlen:  {11}",
               (int)rqs, rqs, system, state, state2,
               drive, side, time, next,
               tshift, rwptr, rwlen);
            string s3 = string.Format(
               "CYL COUNT: {0}\nHEAD POS:  {1}\n" +
               "READY:     {2}\nTR00:      {3}",
               fdd[drive].CylynderCount, fdd[drive].HeadCylynder,
               fdd[drive].IsREADY, fdd[drive].IsTRK00);
            return string.Format(
                "{0}\n--------------------------\n" +
                "{1}\n--------------------------\n" +
                "{2}",
                s1, s2, s3);
        }

        #endregion

        #region Bus Handlers

        protected virtual void BusSubscribeWD93IO(IBusManager bmgr)
        {
            bmgr.SubscribeWRIO(0x83, 0x1F & 0x83, BusWritePortFdc);
            bmgr.SubscribeRDIO(0x83, 0x1F & 0x83, BusReadPortFdc);
            bmgr.SubscribeWRIO(0x83, 0xFF & 0x83, BusWritePortSys);
            bmgr.SubscribeRDIO(0x83, 0xFF & 0x83, BusReadPortSys);
        }

        protected virtual void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (!DOSEN && m_memory.IsRom48)
                DOSEN = true;
        }

        protected virtual void BusReadMemRam(ushort addr, ref byte value)
        {
            if (DOSEN)
                DOSEN = false;
        }

        protected virtual void BusReset()
        {
            DOSEN = false;
        }

        protected virtual void BusNmiRq(BusCancelArgs e)
        {
            e.Cancel = !m_memory.IsRom48;
        }

        protected virtual void BusNmiAck()
        {
            DOSEN = true;
        }

        protected virtual void BusWritePortFdc(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                SetReg((WD93REG)fdcReg, value);
            }
        }

        protected virtual void BusReadPortFdc(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN)
            {
                iorqge = false;
                int fdcReg = (addr & 0x60) >> 5;
                value = GetReg((WD93REG)fdcReg);
            }
        }

        protected virtual void BusWritePortSys(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN)
            {
                iorqge = false;
                SetReg(WD93REG.SYS, value);
            }
        }

        protected virtual void BusReadPortSys(ushort addr, ref byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            if (DOSEN)
            {
                iorqge = false;
                value = GetReg(WD93REG.SYS);
            }
        }

        protected virtual void BusBeginFrame()
        {
            m_ledDiskRd = false;
            m_ledDiskWr = false;
        }

        protected virtual void BusEndFrame()
        {
            m_iconWr.Visible = m_ledDiskWr;
            m_iconRd.Visible = !m_ledDiskWr && m_ledDiskRd;
        }

        #endregion


        protected Z80CPU m_cpu;
        protected IMemoryDevice m_memory;

        private bool m_sandbox = false;
        private bool m_dosen = false;
        private bool m_ledDiskRd = false;
        private bool m_ledDiskWr = false;
        private bool m_logIO = false;
        private IconDescriptor m_iconRd = new IconDescriptor("FDDRD", Utils.GetIconStream("Fdd.png"));
        private IconDescriptor m_iconWr = new IconDescriptor("FDDWR", Utils.GetIconStream("FddWr.png"));

        #region Constructor
        public BetaDiskInterface()
        {
            drive = 0;
            fdd = new DiskImage[4];
            for (int i = 0; i < fdd.Length; i++)
            {
                DiskImage di = new DiskImage();
                di.Init(Z80FQ / FDD_RPS);
                //di.Format();  // take ~1 second (long delay on show options)
                fdd[i] = di;
            }
            fdd[drive].t = fdd[drive].CurrentTrack;

            //fdd[i].set_appendboot(NULL);

            wd93_nodelay = false;
        }
        #endregion

        public bool NoDelay
        {
            get { return wd93_nodelay; }
            set { wd93_nodelay = value; }
        }

        public bool LogIO
        {
            get { return m_logIO; }
            set { m_logIO = value; }
        }



        public void SetReg(WD93REG reg, byte value)
        {
            long tact = m_cpu.Tact;
            if (LogIO)
            {
                LogAgent.Debug(
                    "WD93 {0} <== #{1:X2} [PC=#{2:X4}, T={3}]",
                    reg,
                    value,
                    m_cpu.regs.PC,
                    tact);
            }
            process(tact);
            switch (reg)
            {
                case WD93REG.CMD:  // COMMAND/STATUS
                    m_ledDiskRd = true;
                    // force interrupt
                    if ((value & 0xF0) == 0xD0)
                    {
                        int cond = value & 0xF;
                        next = tact;
                        idx_cnt = 0;
                        idx_tmo = next + 15 * Z80FQ / FDD_RPS; // 15 disk turns
                        cmd = value;

                        if (cond == 0)
                        {
                            state = WDSTATE.S_IDLE; rqs = BETA_STATUS.NONE;
                            status &= ~WD_STATUS.WDS_BUSY;
                            break;
                        }

                        if ((cond & 8) != 0) // Immediate Interrupt
                        {
                            state = WDSTATE.S_IDLE; rqs = BETA_STATUS.INTRQ;
                            status &= ~WD_STATUS.WDS_BUSY;
                            break;
                        }

                        if ((cond & 4) != 0) // Index Pulse (unimplemented yet)
                        {
                            state = WDSTATE.S_IDLE; rqs = BETA_STATUS.INTRQ;
                            status &= ~WD_STATUS.WDS_BUSY;
                            break;
                        }

                        if ((cond & 2) != 0) // Ready to Not-Ready Transition (unimplemented yet)
                        {
                            state = WDSTATE.S_IDLE; rqs = BETA_STATUS.INTRQ;
                            status &= ~WD_STATUS.WDS_BUSY;
                            break;
                        }

                        if ((cond & 1) != 0) // Not-Ready to Ready Transition (unimplemented yet)
                        {
                            state = WDSTATE.S_IDLE; rqs = BETA_STATUS.INTRQ;
                            status &= ~WD_STATUS.WDS_BUSY;
                            break;
                        }
                        break;
                    }

                    if ((status & WD_STATUS.WDS_BUSY) != 0)
                        break;

                    cmd = value;
                    next = tact;
                    status |= WD_STATUS.WDS_BUSY;
                    rqs = BETA_STATUS.NONE;
                    idx_cnt = 0;
                    idx_tmo = long.MaxValue;

                    //-----------------------------------------------------------------------

                    if ((cmd & 0x80) != 0) // read/write command
                    {
                        // abort if no disk
                        if ((status & WD_STATUS.WDS_NOTRDY) != 0)
                        {
                            state2 = WDSTATE.S_IDLE;
                            state = WDSTATE.S_WAIT;
                            next = tact + Z80FQ / FDD_RPS;
                            rqs = BETA_STATUS.INTRQ;
                            break;
                        }

                        // continue disk spinning
                        if (fdd[drive].motor > 0 || wd93_nodelay)
                            fdd[drive].motor = next + 2 * Z80FQ;

                        state = WDSTATE.S_DELAY_BEFORE_CMD;
                        break;
                    }

                    // else seek/step command
                    state = WDSTATE.S_TYPE1_CMD;
                    break;

                case WD93REG.TRK:
                    track = value;
                    break;

                case WD93REG.SEC:
                    sector = value;
                    break;

                case WD93REG.DAT:
                    m_ledDiskRd = true;
                    data = value;
                    rqs &= ~BETA_STATUS.DRQ;
                    status &= ~WD_STATUS.WDS_DRQ;
                    break;

                case WD93REG.SYS:
                    m_ledDiskRd = true;
                    //system = value;
                    drive = value & 3;
                    side = 1 & ~(value >> 4);
                    fdd[drive].HeadSide = side;
                    //seldrive->t.clear();
                    fdd[drive].t = fdd[drive].CurrentTrack;
                    if ((value & (int)WD_SYS.SYS_RST) == 0) // reset
                    {
                        status = WD_STATUS.WDS_NOTRDY;
                        rqs = BETA_STATUS.INTRQ;
                        fdd[drive].motor = 0;
                        state = WDSTATE.S_IDLE;
                        idx_cnt = 0;
                        idx_status = 0;
#if NO_COMPILE // move head to trk00
               steptime = 6 * (Z80FQ / 1000); // 6ms
               next += 1*Z80FQ/1000; // 1ms before command
               state = S_RESET;
               //seldrive->track = 0;
#endif
                    }
                    else if (((system ^ value) & (int)WD_SYS.SYS_HLT) != 0) // hlt 0->1
                    {
                        if ((status & WD_STATUS.WDS_BUSY) == 0)
                        {
                            idx_cnt++;
                        }
                    }
                    system = value;
                    break;

                default:
                    throw new Exception("WD1793.SetReg: Invalid register");
            }
            process(tact);
        }

        public byte GetReg(WD93REG reg)
        {
            long tact = m_cpu.Tact;
            process(tact);
            byte value = 0xFF;
            switch (reg)
            {
                case WD93REG.CMD: // COMMAND/STATUS #1F
                    m_ledDiskRd = true;
                    rqs &= ~BETA_STATUS.INTRQ;
                    value = (byte)status;
                    if ((system & (int)WD_SYS.SYS_HLT) == 0)
                        value &= (byte)(value & (int)~WD_STATUS.WDS_HEADL);
                    if (!fdd[drive].Present)
                        value = (byte)(value & ~(byte)WD_STATUS.WDS_INDEX);    // No disk emulation
                    break;

                case WD93REG.TRK:   // #3F
                    value = track;
                    break;

                case WD93REG.SEC:  // #5F
                    value = sector;
                    break;

                case WD93REG.DAT:    // #7F
                    m_ledDiskRd = true;
                    status &= ~WD_STATUS.WDS_DRQ;
                    rqs &= ~BETA_STATUS.DRQ;
                    value = data;
                    break;

                case WD93REG.SYS: // #FF
                    m_ledDiskRd = true;
                    value = (byte)((byte)rqs | 0x3F);
                    break;

                default:
                    throw new InvalidOperationException();
            }
            if (LogIO)
            {
                LogAgent.Debug(
                    "WD93 {0} ==> #{1:X2} [PC=#{2:X4}, T={3}]",
                    reg,
                    value,
                    m_cpu.regs.PC,
                    tact);
            }
            return value;
        }

        #region consts
        public const int Z80FQ = 3500000; // todo: #define as (conf.frame*conf.intfq)
        public const int FDD_RPS = 5;//15; // rotation speed

        #region CMDBITS
        const byte CMD_SEEK_RATE = 0x03;
        const byte CMD_SEEK_VERIFY = 0x04;
        const byte CMD_SEEK_HEADLOAD = 0x08;
        const byte CMD_SEEK_TRKUPD = 0x10;
        const byte CMD_SEEK_DIR = 0x20;

        const byte CMD_WRITE_DEL = 0x01;
        const byte CMD_SIDE_CMP_FLAG = 0x02;
        const byte CMD_DELAY = 0x04;
        const byte CMD_SIDE = 0x08;
        const byte CMD_SIDE_SHIFT = 3;
        const byte CMD_MULTIPLE = 0x10;
        #endregion
        #endregion

        #region private data

        //private Track trkcache = null;
        private DiskImage[] fdd;

        private long next;
        private long time;
        private long tshift;

        private byte cmd;
        private WDSTATE state, state2;

        private int drive = 0, side = 0;                // update this with changing 'system'
        private int stepdirection = 1;

        private byte system;                // beta128 system register
        private byte data, track, sector;
        private BETA_STATUS rqs;
        private WD_STATUS status;


        // read/write sector(s) data
        private long end_waiting_am;
        private int foundid;                    // index in trkcache.hdr for next encountered ID and bytes before this ID

        private int rwptr;   // pointer to data in track image for read/write op
        private int rwlen;

        // format track data   TODO: check it is may be local?
        private int start_crc;

        private bool wd93_nodelay;

        #endregion
        //trkcache = fdd[drive & 3].CurrentTrack;

        #region private methods

        private long idx_tmo = long.MaxValue;
        private uint idx_cnt = 0; // idx counter
        private WD_STATUS idx_status = 0;

        private void process(long toTact)
        {
            /*time = wd93_get_time()*/
            /*comp.t_states + cpu.t*/
            ;
            time = toTact;

            // inactive drives disregard HLT bit
            if (time > fdd[drive].motor && (system & 0x08) != 0)
                fdd[drive].motor = 0;

            if (state != WDSTATE.S_WAIT)  // KLUDGE: motor emulation to fix SCORPION 128 TRDOS dead lock
            {
                if (fdd[drive].IsREADY)  // RESET
                    status &= ~WD_STATUS.WDS_NOTRDY;
                else
                    status |= WD_STATUS.WDS_NOTRDY;
                //status |= WD_STATUS.WDS_RECORDT; 
            }


            if ((cmd & 0x80) == 0 || (cmd & 0xF0) == 0xD0) // seek / step commands
            {
                WD_STATUS old_idx_status = idx_status;
                idx_status &= ~WD_STATUS.WDS_INDEX;
                status &= ~WD_STATUS.WDS_INDEX;
                if (state != WDSTATE.S_IDLE)
                {
                    status &= ~(WD_STATUS.WDS_TRK00 | WD_STATUS.WDS_INDEX);
                    if (fdd[drive].motor > 0 && (system & 0x08) != 0) status |= WD_STATUS.WDS_HEADL;
                    if (fdd[drive].IsTRK00) status |= WD_STATUS.WDS_TRK00;
                }

                // todo: test spinning
                if (fdd[drive].IsREADY && fdd[drive].motor > 0 && ((time + tshift) % (Z80FQ / FDD_RPS) < (Z80FQ * 4 / 1000)))
                {
                    if (state == WDSTATE.S_IDLE)
                    {
                        if (time < idx_tmo)
                            status |= WD_STATUS.WDS_INDEX;
                    }
                    else
                    {
                        status |= WD_STATUS.WDS_INDEX;
                    }
                    idx_status |= WD_STATUS.WDS_INDEX; // index every turn, len=4ms (if disk present)
                }
            }

            for (; ; )
            {
                switch (state)
                {
                    // ----------------------------------------------------
                    case WDSTATE.S_IDLE:
                        status &= ~WD_STATUS.WDS_BUSY;
                        if (idx_cnt >= 15 || time > idx_tmo)
                        {
                            idx_cnt = 15;
                            status &= ~WD_STATUS.WDS_NOTRDY;
                            status |= WD_STATUS.WDS_NOTRDY;
                            fdd[drive].motor = 0;
                        }
                        rqs = BETA_STATUS.INTRQ;
                        return;

                    case WDSTATE.S_WAIT:
                        if (time < next)
                            return;
                        state = state2;
                        break;
                    // ----------------------------------------------------

                    case WDSTATE.S_DELAY_BEFORE_CMD:
                        if (!wd93_nodelay && (cmd & CMD_DELAY) != 0)
                        {
                            next += (Z80FQ * 15 / 1000); // 15ms delay

                            // this flag should indicate motor state, but we dont have it :( 
                            // so, simulate motor off->on delay when program specify CMD_DELAY
                            status |= WD_STATUS.WDS_NOTRDY;
                        }
                        status = (status | WD_STATUS.WDS_BUSY) & ~(WD_STATUS.WDS_DRQ | WD_STATUS.WDS_LOST | WD_STATUS.WDS_NOTFOUND | WD_STATUS.WDS_RECORDT | WD_STATUS.WDS_WRITEP);
                        state2 = WDSTATE.S_CMD_RW;
                        state = WDSTATE.S_WAIT;
                        break;

                    case WDSTATE.S_CMD_RW:
                        if (((cmd & 0xE0) == 0xA0 || (cmd & 0xF0) == 0xF0) && fdd[drive].IsWP)
                        {
                            status |= WD_STATUS.WDS_WRITEP;
                            state = WDSTATE.S_IDLE;
                            break;
                        }

                        if ((cmd & 0xC0) == 0x80 || (cmd & 0xF8) == 0xC0)
                        {
                            // read/write sectors or read am - find next AM
                            end_waiting_am = next + 5 * Z80FQ / FDD_RPS; // max wait disk 5 turns
                            find_marker(toTact);
                            break;
                        }

                        if ((cmd & 0xF8) == 0xF0) // write track
                        {
                            rqs = BETA_STATUS.DRQ;
                            status |= WD_STATUS.WDS_DRQ;
                            next += 3 * fdd[drive].t.ts_byte;
                            state2 = WDSTATE.S_WRTRACK;
                            state = WDSTATE.S_WAIT;
                            break;
                        }

                        if ((cmd & 0xF8) == 0xE0) // read track
                        {
                            load();
                            rwptr = 0;
                            rwlen = fdd[drive].t.trklen;
                            state2 = WDSTATE.S_READ;
                            getindex();
                            break;
                        }

                        // else unknown command
                        state = WDSTATE.S_IDLE;
                        break;

                    case WDSTATE.S_FOUND_NEXT_ID:
                        if (!fdd[drive].IsREADY)
                        { // no disk - wait again
                            end_waiting_am = next + 5 * Z80FQ / FDD_RPS;
                            //         nextmk:
                            find_marker(toTact);
                            break;
                        }
                        if (next >= end_waiting_am || foundid == -1)
                        {
                            status |= WD_STATUS.WDS_NOTFOUND;
                            state = WDSTATE.S_IDLE;
                            break;
                        }

                        status &= ~WD_STATUS.WDS_CRCERR;
                        load();

                        if ((cmd & 0x80) == 0) // verify after seek
                        {
                            if (fdd[drive].t.HeaderList[foundid].c != track)
                            {
                                find_marker(toTact);
                                break;
                            }
                            if (!fdd[drive].t.HeaderList[foundid].c1)
                            {
                                status |= WD_STATUS.WDS_CRCERR;
                                find_marker(toTact);
                                break;
                            }
                            state = WDSTATE.S_IDLE;
                            break;
                        }

                        if ((cmd & 0xF0) == 0xC0) // read AM
                        {
                            rwptr = fdd[drive].t.HeaderList[foundid].idOffset;
                            rwlen = 6;
                            //         read_first_byte:
                            data = fdd[drive].t.RawRead(rwptr++);
                            rwlen--;
                            rqs = BETA_STATUS.DRQ; status |= WD_STATUS.WDS_DRQ;
                            next += fdd[drive].t.ts_byte;
                            state = WDSTATE.S_WAIT;
                            state2 = WDSTATE.S_READ;
                            break;
                        }

                        // else R/W sector(s)
                        if (fdd[drive].t.HeaderList[foundid].c != track || fdd[drive].t.HeaderList[foundid].n != sector)
                        {
                            find_marker(toTact);
                            break;
                        }
                        if ((cmd & CMD_SIDE_CMP_FLAG) != 0 && (((cmd >> CMD_SIDE_SHIFT) ^ fdd[drive].t.HeaderList[foundid].s) & 1) != 0)
                        {
                            find_marker(toTact);
                            break;
                        }
                        if (!fdd[drive].t.HeaderList[foundid].c1)
                        {
                            status |= WD_STATUS.WDS_CRCERR;
                            find_marker(toTact);
                            break;
                        }

                        if ((cmd & 0x20) != 0) // write sector(s)
                        {
                            rqs = BETA_STATUS.DRQ;
                            status |= WD_STATUS.WDS_DRQ;
                            next += fdd[drive].t.ts_byte * 9;
                            state = WDSTATE.S_WAIT;
                            state2 = WDSTATE.S_WRSEC;
                            break;
                        }

                        // read sector(s)
                        if (fdd[drive].t.HeaderList[foundid].dataOffset < 0) // no data block
                        {
                            find_marker(toTact);
                            break;
                        }
                        if (!wd93_nodelay)
                            next += fdd[drive].t.ts_byte * (fdd[drive].t.HeaderList[foundid].dataOffset - fdd[drive].t.HeaderList[foundid].idOffset); // «адержка на пропуск заголовка сектора и пробела между заголовком и зоной данных
                        state = WDSTATE.S_WAIT; state2 = WDSTATE.S_RDSEC;
                        break;

                    case WDSTATE.S_RDSEC:
                        //// TODO: —делать поиск массива данных как и поиск массива заголовка!

                        if (fdd[drive].t.RawRead(fdd[drive].t.HeaderList[foundid].dataOffset - 1) == 0xF8)  // TODO: check dataOffset>=1
                            status |= WD_STATUS.WDS_RECORDT;
                        else
                            status &= ~WD_STATUS.WDS_RECORDT;
                        rwptr = fdd[drive].t.HeaderList[foundid].dataOffset; // —мещение зоны данных сектора (в байтах) относительно начала трека
                        rwlen = 128 << (fdd[drive].t.HeaderList[foundid].l & 3); // [vv]
                        //goto read_first_byte;

                        #region us374
                        data = fdd[drive].t.RawRead(rwptr++);
                        rwlen--;
                        rqs = BETA_STATUS.DRQ;
                        status |= WD_STATUS.WDS_DRQ;
                        next += fdd[drive].t.ts_byte;
                        state = WDSTATE.S_WAIT;
                        state2 = WDSTATE.S_READ;
                        #endregion

                        #region ZXMAK
                        //without WDSTATE.S_RDSEC (end of WDSTATE.S_FOUND_NEXT_ID)
                        #region fixed by me (timing fix)
                        //// not fixed:
                        ////data = fdd[drive].t.trkd[rwptr++];
                        ////rwlen--;
                        ////rqs = BETA_STATUS.DRQ;
                        ////status |= WD_STATUS.WDS_DRQ;
                        ////next += fdd[drive].t.ts_byte;
                        ////state = WDSTATE.S_WAIT;
                        ////state2 = WDSTATE.S_READ;

                        //// fixed:
                        //long div = fdd[drive].t.trklen * fdd[drive].t.ts_byte;
                        //int i = (int)(((next + tshift) % div) / fdd[drive].t.ts_byte);  // номер байта который пролетает под головкой
                        //int pos = fdd[drive].t.HeaderList[foundid].dataOffset;
                        //int dist = (pos > i) ? pos - i : fdd[drive].t.trklen + pos - i;
                        //next += dist * fdd[drive].t.ts_byte;
                        //state = WDSTATE.S_WAIT;
                        //state2 = WDSTATE.S_READ;
                        #endregion
                        #endregion
                        break;


                    case WDSTATE.S_READ:
                        if (notready())
                            break;
                        load();

                        // TODO: really need?
                        if (!fdd[drive].Present) //if(!seldrive->t.trkd)
                        {
                            status |= WD_STATUS.WDS_NOTFOUND;
                            state = WDSTATE.S_IDLE;
                            break;
                        }

                        if (rwlen > 0)
                        {
                            if ((rqs & BETA_STATUS.DRQ) != 0)
                                status |= WD_STATUS.WDS_LOST;
                            data = fdd[drive].t.RawRead(rwptr++);
                            rwlen--;
                            rqs = BETA_STATUS.DRQ;
                            status |= WD_STATUS.WDS_DRQ;
                            if (!wd93_nodelay)
                                next += fdd[drive].t.ts_byte;
                            else
                                next = time + 1;
                            state = WDSTATE.S_WAIT;
                            state2 = WDSTATE.S_READ;
                        }
                        else
                        {
                            if ((cmd & 0xE0) == 0x80) // read sector
                            {
                                if (!fdd[drive].t.HeaderList[foundid].c2)
                                    status |= WD_STATUS.WDS_CRCERR;
                                if ((cmd & CMD_MULTIPLE) != 0)
                                {
                                    sector++;
                                    state = WDSTATE.S_CMD_RW;
                                    break;
                                }
                            }
                            if ((cmd & 0xF0) == 0xC0) // read address
                                if (!fdd[drive].t.HeaderList[foundid].c1)
                                    status |= WD_STATUS.WDS_CRCERR;
                            state = WDSTATE.S_IDLE;
                        }
                        break;


                    case WDSTATE.S_WRSEC:
                        load();

                        if ((rqs & BETA_STATUS.DRQ) != 0)
                        {
                            status |= WD_STATUS.WDS_LOST;
                            state = WDSTATE.S_IDLE;
                            break;
                        }
                        fdd[drive].ModifyFlag |= ModifyFlag.SectorLevel;
                        rwptr = (int)(fdd[drive].t.HeaderList[foundid].idOffset + 6 + 11 + 11);
                        m_ledDiskWr = true;
                        for (rwlen = 0; rwlen < 12; rwlen++)
                            fdd[drive].t.RawWrite(rwptr++, 0x00, false);
                        for (rwlen = 0; rwlen < 3; rwlen++)
                            fdd[drive].t.RawWrite(rwptr++, 0xA1, true);
                        fdd[drive].t.RawWrite(rwptr++, (byte)(((cmd & CMD_WRITE_DEL) != 0) ? 0xF8 : 0xFB), false);
                        rwlen = 128 << (fdd[drive].t.HeaderList[foundid].l & 3);    // [vv]
                        state = WDSTATE.S_WRITE;
                        break;

                    case WDSTATE.S_WRITE:
                        if (notready()) break;
                        if ((rqs & BETA_STATUS.DRQ) != 0)
                        {
                            status |= WD_STATUS.WDS_LOST;
                            data = 0;
                        }
                        fdd[drive].t.RawWrite(rwptr++, data, false);
                        rwlen--;
                        if (rwptr == fdd[drive].t.trklen) rwptr = 0;

                        //fdd[drive].t.sf = SEEK_MODE.JUST_SEEK; // invalidate sectors
                        fdd[drive].t.sf = true;     // REFRESH!!!

                        if (rwlen > 0)
                        {
                            if (!wd93_nodelay) next += fdd[drive].t.ts_byte;
                            state = WDSTATE.S_WAIT;
                            state2 = WDSTATE.S_WRITE;
                            rqs = BETA_STATUS.DRQ;
                            status |= WD_STATUS.WDS_DRQ;
                        }
                        else
                        {
                            int len = (128 << (fdd[drive].t.HeaderList[foundid].l & 3)) + 1;
                            byte[] sc = new byte[2056];
                            if (rwptr < len)
                            {
                                for (int memi = 0; memi < rwptr; memi++)
                                    sc[memi] = fdd[drive].t.RawRead(fdd[drive].t.trklen - rwptr + memi);
                                for (int memi = 0; memi < len - rwptr; memi++)
                                    sc[rwptr + memi] = fdd[drive].t.RawRead(memi);
                                //memcpy(sc, trkcache.trkd, 0, (int)(trkcache.trklen - rwptr), rwptr);
                                //memcpy(sc, trkcache.trkd, rwptr, 0, len - rwptr);
                            }
                            else
                            {
                                for (int memi = 0; memi < len; memi++)
                                    sc[memi] = fdd[drive].t.RawRead(rwptr - len + memi);
                                //memcpy(sc, trkcache.trkd, 0, rwptr - len, len);
                            }
                            uint crc = CrcVg93.Calc3xA1(sc, 0, len);
                            fdd[drive].t.RawWrite(rwptr++, (byte)crc, false);
                            fdd[drive].t.RawWrite(rwptr++, (byte)(crc >> 8), false);
                            fdd[drive].t.RawWrite(rwptr, 0xFF, false);
                            if ((cmd & CMD_MULTIPLE) != 0)
                            {
                                sector++;
                                state = WDSTATE.S_CMD_RW;
                                break;
                            }
                            state = WDSTATE.S_IDLE;
                        }
                        break;

                    case WDSTATE.S_WRTRACK:
                        if ((rqs & BETA_STATUS.DRQ) != 0)
                        {
                            status |= WD_STATUS.WDS_LOST;
                            state = WDSTATE.S_IDLE;
                            break;
                        }
                        fdd[drive].ModifyFlag |= ModifyFlag.TrackLevel;
                        state2 = WDSTATE.S_WR_TRACK_DATA;
                        start_crc = 0;
                        getindex();
                        end_waiting_am = next + 5 * Z80FQ / FDD_RPS;
                        break;

                    case WDSTATE.S_WR_TRACK_DATA:
                        if (notready())
                            break;
                        if ((rqs & BETA_STATUS.DRQ) != 0)
                        {
                            status |= WD_STATUS.WDS_LOST;
                            data = 0;
                        }
                        //trkcache.seek(fdd[drive], fdd[drive].CurrentTrack, side, SEEK_MODE.JUST_SEEK);
                        //trkcache.sf = SEEK_MODE.JUST_SEEK; // invalidate sectors
                        //                  if (trkcache.sf)
                        //                     trkcache.RefreshHeaders();
                        fdd[drive].t = fdd[drive].CurrentTrack;
                        fdd[drive].t.sf = true;     // REFRESH!!!

                        bool marker = false;
                        byte _byte = data;
                        uint _crc = 0;
                        if (data == 0xF5)
                        {
                            _byte = 0xA1;
                            marker = true;
                            start_crc = rwptr + 1;
                        }
                        if (data == 0xF6)
                        {
                            _byte = 0xC2;
                            marker = true;
                        }
                        if (data == 0xF7)
                        {
                            _crc = fdd[drive].t.MakeCrc(start_crc, rwptr - start_crc);
                            _byte = (byte)(_crc & 0xFF);
                        }
                        fdd[drive].t.RawWrite(rwptr++, _byte, marker);
                        rwlen--;
                        if (data == 0xF7)
                        {
                            fdd[drive].t.RawWrite(rwptr++, (byte)(_crc >> 8), marker); // second byte of CRC16
                            rwlen--;
                        }
                        if ((int)rwlen > 0)
                        {
                            if (!wd93_nodelay) next += fdd[drive].t.ts_byte;
                            state2 = WDSTATE.S_WR_TRACK_DATA;
                            state = WDSTATE.S_WAIT;
                            rqs = BETA_STATUS.DRQ;
                            status |= WD_STATUS.WDS_DRQ;
                            break;
                        }
                        state = WDSTATE.S_IDLE;
                        break;

                    // ----------------------------------------------------

                    case WDSTATE.S_TYPE1_CMD:
                        status = (status | WD_STATUS.WDS_BUSY) & ~(WD_STATUS.WDS_DRQ | WD_STATUS.WDS_CRCERR | WD_STATUS.WDS_SEEKERR | WD_STATUS.WDS_WRITEP);
                        rqs = BETA_STATUS.NONE;

                        if (fdd[drive].IsWP)
                            status |= WD_STATUS.WDS_WRITEP;
                        fdd[drive].motor = next + 2 * Z80FQ;

                        state2 = WDSTATE.S_SEEKSTART; // default is seek/restore
                        if ((cmd & 0xE0) != 0) // single step
                        {
                            if ((cmd & 0x40) != 0) stepdirection = (sbyte)(((cmd & CMD_SEEK_DIR) != 0) ? -1 : 1);
                            state2 = WDSTATE.S_STEP;
                            // TODO: check!!! break required? error in emulator?
                        }
                        if (!wd93_nodelay)
                        {
                            //next += 1 * Z80FQ / 1000;
                            next += 32;
                        }
                        state = WDSTATE.S_WAIT;
                        break;


                    case WDSTATE.S_STEP:
                        // TRK00 sampled only in RESTORE command
                        if (fdd[drive].IsTRK00 && (cmd & 0xF0) == 0)
                        {
                            track = 0;
                            state = WDSTATE.S_VERIFY;
                            break;
                        }

                        if ((cmd & 0xE0) == 0 || (cmd & CMD_SEEK_TRKUPD) != 0) track = (byte)((int)track + stepdirection);
                        fdd[drive].HeadCylynder += stepdirection;
                        //                  if (fdd[drive].HeadCylynder == -1) fdd[drive].CurrentTrack = 0;
                        if (fdd[drive].HeadCylynder >= (fdd[drive].CylynderCount - 1)) fdd[drive].HeadCylynder = fdd[drive].CylynderCount - 1;
                        //trkcache.clear();
                        fdd[drive].t = fdd[drive].CurrentTrack;

                        uint[] steps = new uint[4] { 6, 12, 20, 30 };   // TODO: static
                        if (!wd93_nodelay)
                            next += steps[cmd & CMD_SEEK_RATE] * Z80FQ / 1000;

                        /* ?TODO? -- fdd noise
                         #ifndef MOD_9X
                        if (!wd93_nodelay && conf.fdd_noise) Beep((stepdirection > 0)? 600 : 800, 2);
                        #endif*/

                        state2 = ((cmd & 0xE0) != 0) ? WDSTATE.S_VERIFY : WDSTATE.S_SEEK;
                        state = WDSTATE.S_WAIT;
                        break;

                    case WDSTATE.S_SEEKSTART:
                        if ((cmd & 0x10) == 0)
                        {
                            track = 0xFF;
                            data = 0;
                        }
                        // state = S_SEEK; break;

                        //TODO: проверить!!! не ошибка ли это - дальше выполн€ть блок WDSTATE.SEEK?:
                        // в исходном варианте нет брейка и далее сразу следует case WDSTATE.S_SEEK:
                        if (data == track)
                        {
                            state = WDSTATE.S_VERIFY;
                            break;
                        }
                        stepdirection = (data < track) ? -1 : 1;
                        state = WDSTATE.S_STEP;
                        break;

                    case WDSTATE.S_SEEK:
                        if (data == track)
                        {
                            state = WDSTATE.S_VERIFY;
                            break;
                        }
                        stepdirection = (data < track) ? -1 : 1;
                        state = WDSTATE.S_STEP;
                        break;

                    case WDSTATE.S_VERIFY:
                        if ((cmd & CMD_SEEK_VERIFY) == 0)
                        {
                            status |= WD_STATUS.WDS_BUSY;
                            state2 = WDSTATE.S_IDLE;
                            state = WDSTATE.S_WAIT;
                            next += 128; //next = time + 1;  // do not use time - CHORDOUT issue
                            idx_tmo = next + 15 * Z80FQ / FDD_RPS; // 15 disk turns
                            break;
                        }
                        end_waiting_am = next + 6 * Z80FQ / FDD_RPS; // max wait disk 6 turns
                        load();
                        find_marker(toTact);
                        break;


                    // ----------------------------------------------------

                    case WDSTATE.S_RESET: // seek to trk0, but don't be busy
                        if (fdd[drive].IsTRK00)
                            state = WDSTATE.S_IDLE;
                        else
                        {
                            fdd[drive].HeadCylynder--;
                            //trkcache.clear();
                            fdd[drive].t = fdd[drive].CurrentTrack;
                        }
                        // if (seldrive.TRK00) track = 0;
                        next += 6 * Z80FQ / 1000;
                        break;

                    default:
                        throw new Exception("WD1793.process - WD1793 in wrong state");
                }
            }
        }




        private void find_marker(long toTact)
        {
            if (wd93_nodelay && fdd[drive].HeadCylynder != track)
                fdd[drive].HeadCylynder = track;
            load();

            foundid = -1;
            if (fdd[drive].motor > 0 && fdd[drive].IsREADY)
            {
                long div = fdd[drive].t.trklen * fdd[drive].t.ts_byte;    // ƒлина дорожки в тактах cpu
                int i = (int)(((next + tshift) % div) / fdd[drive].t.ts_byte);  // ѕозици€ байта соответствующего текущему такту на дорожке (байт пролетающий под головкой?)
                long wait = long.MaxValue;

                // ѕоиск заголовка минимально отсто€щего от текущего байта
                for (int _is = 0; _is < fdd[drive].t.HeaderList.Count; _is++)
                {
                    int pos = fdd[drive].t.HeaderList[_is].idOffset;    // —мещение (в байтах) заголовка относительно начала дорожки
                    int dist = (pos > i) ? pos - i : fdd[drive].t.trklen + pos - i; // –ассто€ние (в байтах) от заголовка до текущего байта
                    if (dist < wait)
                    {
                        wait = dist;
                        foundid = _is;
                    }
                }

                if (foundid != -1)
                    wait *= fdd[drive].t.ts_byte;   // «адержка в тактах от текущего такта до такта чтени€ первого байта заголовка
                else
                    wait = 10 * Z80FQ / FDD_RPS;

                if (wd93_nodelay && foundid != -1)
                {
                    // adjust tshift, that id appares right under head
                    int pos = fdd[drive].t.HeaderList[foundid].idOffset + 2;
                    tshift = ((pos * fdd[drive].t.ts_byte) - (next % div) + div) % div;
                    wait = 100; // delay=0 causes fdc to search infinitely, when no matched id on track
                }

                next += wait;
            } // else no index pulses - infinite wait
            else
            {
                next = toTact + 1; //comp.t_states + cpu.t + 1;
            }

            if (fdd[drive].IsREADY && next > end_waiting_am)
            {
                next = end_waiting_am;
                foundid = -1;
            }
            state = WDSTATE.S_WAIT;
            state2 = WDSTATE.S_FOUND_NEXT_ID;
        }

        private bool notready()
        {
            // fdc is too fast in no-delay mode, wait until cpu handles DRQ, but not more 'end_waiting_am'
            if (!wd93_nodelay || (rqs & BETA_STATUS.DRQ) == 0)
                return false;
            if (next > end_waiting_am)
                return false;
            state2 = state;
            state = WDSTATE.S_WAIT;
            next += fdd[drive].t.ts_byte;
            return true;
        }

        private void load()
        {
            if (fdd[drive].t.sf)
                fdd[drive].t.RefreshHeaders();
            fdd[drive].t.sf = false;

            //trkcache.seek(fdd[drive], fdd[drive].CurrentTrack, side, SEEK_MODE.LOAD_SECTORS);
            fdd[drive].t = fdd[drive].CurrentTrack;
        }

        private void getindex()
        {
            long trlen = fdd[drive].t.trklen * fdd[drive].t.ts_byte;
            long ticks = (next + tshift) % trlen;
            if (!wd93_nodelay)
                next += (trlen - ticks);
            rwptr = 0;
            rwlen = fdd[drive].t.trklen;
            state = WDSTATE.S_WAIT;
        }

        #endregion

        #region IGuiExtension Members

        private GuiData m_guiData;
        private System.Windows.Forms.MenuItem m_subMenuItem;
        private Controls.Debugger.dbgWD1793 m_form;

        public void AttachGui(GuiData guiData)
        {
            m_guiData = guiData;
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                System.Windows.Forms.MenuItem menuItem = guiData.MenuItem as System.Windows.Forms.MenuItem;
                if (menuItem != null)
                {
                    m_subMenuItem = new System.Windows.Forms.MenuItem("WD1793", menu_Click);
                    menuItem.MenuItems.Add(m_subMenuItem);
                }
            }
        }

        public void DetachGui()
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_subMenuItem != null)
                {
                    m_subMenuItem.Parent.MenuItems.Remove(m_subMenuItem);
                    m_subMenuItem.Dispose();
                    m_subMenuItem = null;
                }
                if (m_form != null)
                {
                    m_form.Close();
                    m_form = null;
                }
            }
            m_guiData = null;
        }

        private void menu_Click(object sender, EventArgs e)
        {
            if (m_guiData.MainWindow is System.Windows.Forms.Form)
            {
                if (m_form == null)
                {
                    m_form = new Controls.Debugger.dbgWD1793(this);
                    m_form.FormClosed += delegate(object obj, System.Windows.Forms.FormClosedEventArgs arg) { m_form = null; };
                    m_form.Show((System.Windows.Forms.Form)m_guiData.MainWindow);
                }
                else
                {
                    m_form.Show();
                    m_form.Activate();
                }
            }
        }

        #endregion
    }
}
