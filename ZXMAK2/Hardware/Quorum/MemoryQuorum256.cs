using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Cpu;

namespace ZXMAK2.Hardware.Quorum
{
    public class MemoryQuorum256 : MemoryBase
    {
        #region Constants

        private const int Q_F_RAM = 0x01;
        private const int Q_RAM_8 = 0x08;
        private const int Q_B_ROM = 0x20;
        private const int Q_BLK_WR = 0x40;
        private const int Q_TR_DOS = 0x80;

        #endregion Constants


        #region Fields

        private CpuUnit m_cpu;
        private bool m_lock;

        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "QUORUM 256K"; } }
        public override string Description { get { return "QUORUM 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            m_cpu = bmgr.CPU;

            bmgr.SubscribeWrIo(0x801A, 0x7FFD & 0x801A, BusWritePort7FFD);
            bmgr.SubscribeWrIo(0x0099, 0x0000 & 0x0099, BusWritePort0000);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3DXX_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRam);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRam);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRam);

            bmgr.SubscribeReset(BusReset);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region MemoryBase

        public override bool IsMap48
        {
            get { return false; }
        }

        public override bool IsRom48
        {
            get { return (CMR0 & 0x10) != 0; }
        }

        protected override void UpdateMapping()
        {
            var ramPage = CMR0 & 7;
            ramPage |= ((CMR0 & 0xC0) >> 3);
            ramPage &= 0x0F;     //256K

            var romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomName.ROM_SOS) :
                GetRomIndex(RomName.ROM_128);
            var videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            var ramPage0000 = ((CMR1 & Q_RAM_8) != 0) ? 8 : 0;
            var isBlkWr = (CMR1 & Q_BLK_WR) != 0;
            var isNoRom = (CMR1 & Q_F_RAM) != 0;
            var isDosRom = (CMR1 & Q_TR_DOS) != 0;

            m_lock = isBlkWr;

            if (SYSEN && !isDosRom)
            {
                romPage = GetRomIndex(RomName.ROM_SYS);
            }
            if (DOSEN)      // trdos or 48/128
            {
                romPage = GetRomIndex(RomName.ROM_DOS);
                isNoRom = !isDosRom;
            }

            m_ula.SetPageMapping(
                videoPage,
                !isBlkWr ? ramPage0000 : -1,
                5,
                2,
                ramPage);

            MapRead0000 = isNoRom ? RamPages[ramPage0000] : RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = isBlkWr ? m_trashPage : RamPages[ramPage0000];
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;
        }

        public override bool SYSEN
        {
            get { return (CMR1 & Q_B_ROM) == 0; }
            set
            {
                if (value)
                    CMR1 |= Q_B_ROM;
                else
                    CMR1 &= Q_B_ROM ^ 0xFF;
                UpdateMapping();
            }
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


        #region Private

        protected virtual void BusWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            //LogAgent.Info("PC: #{0:X4}  CMR0 <- #{1:X2} {2}", m_cpu.regs.PC, value, m_lock ? "[locked]" : string.Empty);
            if (!m_lock)
            {
                CMR0 = value;
            }
        }

        protected virtual void BusWritePort0000(ushort addr, byte value, ref bool iorqge)
        {
            //LogAgent.Info("PC: #{0:X4}  CMR1 <- #{1:X2}", m_cpu.regs.PC, value);
            CMR1 = value;
        }

        protected virtual void BusReadMem3DXX_M1(ushort addr, ref byte value)
        {
            if (IsRom48)
            {
                DOSEN = true;
            }
        }

        protected virtual void BusReadMemRam(ushort addr, ref byte value)
        {
            if (DOSEN)
            {
                DOSEN = false;
            }
        }

        protected virtual void BusReset()
        {
            DOSEN = false;
            CMR0 = 0;
            CMR1 = 0;
        }

        protected virtual void BusNmiRq(BusCancelArgs e)
        {
            //e.Cancel = DOSEN;
        }

        protected virtual void BusNmiAck()
        {
            CMR1 = 0x00;
        }

        #endregion


        public MemoryQuorum256()
            : base("Quorum", 4, 16)
        {
        }
    }
}
