using System;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine.Z80;

namespace ZXMAK2.Hardware.Quorum
{
    public class MemoryQuorum256 : MemoryBase
    {
        #region IBusDevice

        public override string Name { get { return "QUORUM 256K"; } }
        public override string Description { get { return "QUORUM 256K Memory Manager"; } }

        public override void BusInit(IBusManager bmgr)
        {
            base.BusInit(bmgr);
            m_cpu = bmgr.CPU;

            bmgr.SubscribeWrIo(0x801A, 0x7FFD & 0x801A, busWritePort7FFD);
            bmgr.SubscribeWrIo(0x0099, 0x0000 & 0x0099, busWritePort0000);
            bmgr.SubscribeReset(BusReset);
            bmgr.SubscribeNmiRq(BusNmiRq);
            bmgr.SubscribeNmiAck(BusNmiAck);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return false; } }

        private const int Q_F_RAM = 0x01;
        private const int Q_RAM_8 = 0x08;
        private const int Q_B_ROM = 0x20;
        private const int Q_BLK_WR = 0x40;
        private const int Q_TR_DOS = 0x80;

        protected override void UpdateMapping()
        {
            m_lock = false;// (CMR0 & 0x20) != 0;

            int ramPage = CMR0 & 7;
            ramPage |= ((CMR0 & 0xC0) >> 3);
            ramPage &= 0x0F;     //256K

            int romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomName.ROM_SOS) :
                GetRomIndex(RomName.ROM_128);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            bool blkwr = (CMR1 & Q_BLK_WR) != 0;
            int ramPage0000 = ((CMR1 & Q_RAM_8) != 0) ? 8 : 0;
            bool norom = (CMR1 & Q_F_RAM) != 0;
            //bool dosPort = (CMR1 & Q_TR_DOS) == 0;

            if (DOSEN)      // trdos or 48/128
                romPage = GetRomIndex(RomName.ROM_DOS);
            if (SYSEN)
                romPage = GetRomIndex(RomName.ROM_SYS);

            m_ula.SetPageMapping(videoPage, (norom && !blkwr) ? ramPage0000 : -1, 5, 2, ramPage);
            MapRead0000 = norom ? RamPages[ramPage0000] : RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = (norom && !blkwr) ? RamPages[ramPage0000] : m_trashPage;
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

        #region Bus Handlers

        private void busWritePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            //LogAgent.Info("PC: #{0:X4}  CMR0 <- #{1:X2} {2}", m_cpu.regs.PC, value, m_lock ? "[locked]" : string.Empty);
            if (!m_lock)
                CMR0 = value;
        }

        private void busWritePort0000(ushort addr, byte value, ref bool iorqge)
        {
            //LogAgent.Info("PC: #{0:X4}  CMR1 <- #{1:X2}", m_cpu.regs.PC, value);
            CMR1 = value;
        }

        private void BusReset()
        {
            CMR0 = 0;
            CMR1 = 0;
        }

        private void BusNmiRq(BusCancelArgs e)
        {
            e.Cancel = (m_cpu.regs.PC&0xC000) == 0;
        }
        
        private void BusNmiAck()
        {
            CMR1 = 0;
        }

        #endregion


        private byte[][] m_ramPages = new byte[16][];
        private byte[] m_trashPage = new byte[0x4000];
        private Z80CPU m_cpu;
        private bool m_lock = false;


        public MemoryQuorum256()
            : base("Quorum")
        {
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }
    }
}
