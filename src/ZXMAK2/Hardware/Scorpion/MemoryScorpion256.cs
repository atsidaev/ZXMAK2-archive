using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Entities;


namespace ZXMAK2.Hardware.Scorpion
{
    public class MemoryScorpion256 : MemoryBase
    {
        #region Fields

        private CpuUnit m_cpu;
        private bool m_lock = false;

        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "Scorpion 256K"; } }
        public override string Description { get { return "Scorpion 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_cpu = bmgr.CPU;
            bmgr.SubscribeWrIo(0xD027, 0x7FFD & 0xD027, BusWritePort7FFD);
            bmgr.SubscribeWrIo(0xD027, 0x1FFD & 0xD027, BusWritePort1FFD);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRamM1);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);
            bmgr.SubscribeReset(BusReset);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region MemoryBase

        public override bool IsMap48 { get { return false; } }

        public override int GetRomIndex(RomId romId)
        {
            switch (romId)
            {
                case RomId.ROM_128: return 0;
                case RomId.ROM_SOS: return 1;
                case RomId.ROM_SYS: return 2;
                case RomId.ROM_DOS: return 3;
            }
            Logger.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;
            bool norom = (CMR1 & 0x01) != 0;

            int romPage = GetRomPage();
            int ramPage = GetRamPage();

            m_ula.SetPageMapping(videoPage, norom ? 0 : -1, 5, 2, ramPage);
            MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        protected virtual int GetRamPage()
        {
            int ramPage = CMR0 & 7;
            int sega = (CMR1 & 0x10) >> 4;
            ramPage |= sega << 3;
            return ramPage;
        }

        protected virtual int GetRomPage()
        {
            int romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomId.ROM_SOS) :
                GetRomIndex(RomId.ROM_128);
            if (DOSEN)      // trdos or 48/128
                romPage = GetRomIndex(RomId.ROM_DOS);
            if (SYSEN)
                romPage = GetRomIndex(RomId.ROM_SYS);
            return romPage;
        }

        public override bool SYSEN
        {
            get { return (CMR1 & 0x02) != 0; }
            set
            {
                if (value)
                    CMR1 |= 0x02;
                else
                    CMR1 &= 0x02 ^ 0xFF;
                UpdateMapping();
            }
        }


        #endregion

        #region Bus Handlers

        private void BusWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            if (!m_lock)
                CMR0 = value;
        }

        private void BusWritePort1FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            CMR1 = value;
        }

        protected virtual void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (IsRom48)
            {
                DOSEN = true;
            }
        }

        protected virtual void BusReadMemRamM1(ushort addr, ref byte value)
        {
            if (DOSEN)
            {
                DOSEN = false;
            }
        }

        private void BusReset()
        {
            DOSEN = false;
            CMR1 = 0;
            CMR0 = 0;
        }

        private void BusNmiRq(BusCancelArgs e)
        {
            e.Cancel = (m_cpu.regs.PC & 0xC000) == 0;
        }

        private void BusNmiAck()
        {
            DOSEN = true;
        }

        #endregion


        public MemoryScorpion256(
            String romSetName, 
            int romPageCount, 
            int ramPageCount)
            : base(romSetName, romPageCount, ramPageCount)
        {
        }

        public MemoryScorpion256()
            : this("Scorpion", 4, 16)
        {
        }
    }

    public class MemoryScorpion1024 : MemoryScorpion256
    {
        #region IBusDevice

        public override string Name { get { return "Scorpion 1024K"; } }
        public override string Description { get { return "Scorpion 1024K Memory Manager"; } }

        #endregion

        public MemoryScorpion1024()
            : base("Scorpion", 4, 64)
        {
        }

        protected override int GetRamPage()
        {
            int ramPage = CMR0 & 7;
            int sega = (CMR1 & 0x10) >> 4;
            sega |= (CMR1 & 0xC0) >> 5;		// 1024 extension
            ramPage |= sega << 3;
            return ramPage;
        }
    }
}
