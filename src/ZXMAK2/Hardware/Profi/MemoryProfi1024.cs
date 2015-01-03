﻿using System;

using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using ZXMAK2.Engine.Cpu;
using ZXMAK2.Attributes;


namespace ZXMAK2.Hardware.Profi
{
    public class MemoryProfi1024 : MemoryBase
    {
        #region Fields

        private CpuUnit m_cpu;
        private UlaProfi3XX m_ulaProfi;
        private bool m_lock = false;
        private int m_cmr1mask;
        
        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "PROFI+ 1024K"; } }
        public override string Description { get { return "PROFI+ 1024K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_cpu = bmgr.CPU;
            m_ulaProfi = bmgr.FindDevice<UlaProfi3XX>();

            bmgr.SubscribeWrIo(0x8002, 0x7FFD & 0x8002, BusWritePort7FFD);
            bmgr.SubscribeWrIo(0x2002, 0xDFFD & 0x2002, BusWritePortDFFD);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRamM1);
            bmgr.SubscribeReset(BusReset);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region MemoryBase

        public override bool IsMap48 { get { return false; } }

        [HardwareValue("DS80", Description = "Extended Video")]
        public bool DS80
        {
            get { return (CMR1 & 0x80) != 0; }
            set { CMR1 = (byte)((CMR1 & ~0x80) | (value ? 0x80 : 0)); }
        }

        [HardwareValue("SCR", Description = "Page for window #8000 (0=2,1=6)")]
        public bool SCR
        {
            get { return (CMR1 & 0x40) != 0; }
            set { CMR1 = (byte)((CMR1 & ~0x40) | (value ? 0x40 : 0)); }
        }

        [HardwareValue("CPM", Description = "")]
        public bool CPM
        {
            get { return (CMR1 & 0x20) != 0; }
            set { CMR1 = (byte)((CMR1 & ~0x20) | (value ? 0x20 : 0)); }
        }

        [HardwareValue("NOROM", Description = "Enable RAM cache")]
        public bool NOROM
        {
            get { return (CMR1 & 0x10) != 0; }
            set { CMR1 = (byte)((CMR1 & ~0x10) | (value ? 0x10 : 0)); }
        }

        [HardwareValue("SCO", Description = "Select window pos (0=#C000,1=#4000)")]
        public bool SCO
        {
            get { return (CMR1 & 0x08) != 0; }
            set { CMR1 = (byte)((CMR1 & ~0x08) | (value ? 0x08 : 0)); }
        }

        protected override void UpdateMapping()
        {
            m_lock = ((CMR0 & 0x20) != 0);
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) != 0 ? 
                GetRomIndex(RomName.ROM_SOS) : 
                GetRomIndex(RomName.ROM_128);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
                romPage = GetRomIndex(RomName.ROM_DOS);// 2;
            if (SYSEN)
                romPage = GetRomIndex(RomName.ROM_SYS);// 3;

            int sega = CMR1 & m_cmr1mask;
            bool norom = NOROM;
            bool sco = SCO;   // selectors RAM gates
            bool scr = SCR;   // !??CMR0.D3=1??!
            //bool cpm = CPM;

            if (norom)
                m_lock = false;

            ramPage |= sega << 3;

            if (m_ulaProfi != null)
            {
                m_ulaProfi.SetPageMappingProfi(
                    DS80,
                    videoPage, 
                    norom ? 0 : -1, 
                    sco ? ramPage : 5, 
                    scr ? 6 : 2, 
                    sco ? 7 : ramPage);
            }
            else
            {
                m_ula.SetPageMapping(
                    videoPage, 
                    norom ? 0 : -1, 
                    sco ? ramPage : 5, 
                    scr ? 6 : 2, 
                    sco ? 7 : ramPage);
            }
            MapRead0000 = norom ? RamPages[0] : RomPages[romPage];
            MapRead4000 = sco ? RamPages[ramPage] : RamPages[5];
            MapRead8000 = scr ? RamPages[6] : RamPages[2];
            MapReadC000 = sco ? RamPages[7] : RamPages[ramPage];

            MapWrite0000 = norom ? RamPages[0] : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_128: return 2;
                case RomName.ROM_SOS: return 3;
                case RomName.ROM_DOS: return 1;
                case RomName.ROM_SYS: return 0;
            }
            LogAgent.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
        }

        #endregion

        #region Bus Handlers

        protected virtual void BusWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            if (!m_lock)
            {
                CMR0 = value;
            }
        }

        protected virtual void BusWritePortDFFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!iorqge)
                return;
            iorqge = false;
            CMR1 = value;
        }

        protected virtual void BusReadMem3D00_M1(ushort addr, ref byte value)
        {
            if (!DOSEN && IsRom48)
            {
                DOSEN = true;
            }
        }

        protected virtual void BusReadMemRamM1(ushort addr, ref byte value)
        {
            if (SYSEN)
            {
                SYSEN = false;
            }
            if (DOSEN)
            {
                DOSEN = false;
            }
        }

        protected virtual void BusReset()
        {
            CMR1 = 0;
            CMR0 = 0;
            SYSEN = true;
            DOSEN = false;
        }

        protected virtual void BusNmiRq(BusCancelArgs e)
        {
            e.Cancel = !IsRom48;
        }

        protected virtual void BusNmiAck()
        {
            DOSEN = true;
        }


        #endregion


        public MemoryProfi1024(
            String romSetName, 
            int romPageCount, 
            int ramPageCount)
            : base(romSetName, romPageCount, ramPageCount)
        {
            m_cmr1mask = (ramPageCount / 8) - 1;
        }

        public MemoryProfi1024()
            : this("PROFI", 4, 64)
        {
        }
    }

    public class MemoryProfi512 : MemoryProfi1024
    {
        public override string Name { get { return "PROFI+ 512K"; } }
        public override string Description { get { return "PROFI+ 512K Memory Manager"; } }
        
        public MemoryProfi512()
            : base("PROFI-V03", 4, 32)
        {
        }
    }
}
