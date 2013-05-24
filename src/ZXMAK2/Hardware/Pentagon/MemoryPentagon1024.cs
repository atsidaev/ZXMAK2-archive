using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZXMAK2.Interfaces;
using System.Xml;

namespace ZXMAK2.Hardware.Pentagon
{
    public class MemoryPentagon1024 : MemoryBase
    {
        #region Fields

        private bool m_enableShadow;
        private byte[][] m_ramPages = new byte[64][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;
        
        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "Pentagon 1024K"; } }
        public override string Description { get { return "Pentagon 1024K Memory Module"; } }

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0xC002, 0x4000, writePort7FFD);
            bmgr.SubscribeWrIo(0xF008, 0xE000, writePortEFF7);

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

        protected override void OnConfigLoad(XmlNode itemNode)
        {
            base.OnConfigLoad(itemNode);
            EnableShadow = Utils.GetXmlAttributeAsBool(itemNode, "enableShadow", EnableShadow);
        }

        protected override void OnConfigSave(XmlNode itemNode)
        {
            base.OnConfigSave(itemNode);
            Utils.SetXmlAttribute(itemNode, "enableShadow", EnableShadow);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return m_lock; } }

        protected override void UpdateMapping()
        {
            bool extMode = (CMR1 & 0x04) == 0;			// D2 - 0=extended memory mode; 1=lock 128K mode
            bool norom = (CMR1 & 0x10) != 0;			// D3 - ram0 at 0000...3FFF

            m_lock = !extMode && (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomName.ROM_SOS) :
                GetRomIndex(RomName.ROM_128);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
                romPage = GetRomIndex(RomName.ROM_DOS);
            if (SYSEN)
                romPage = GetRomIndex(RomName.ROM_SYS);

            if (extMode)
            {
                int sega = ((CMR0 & 0xC0) >> 6) | ((CMR0 & 0x20) >> 3);	//PENT1024: D5,D7,D6,D2,D1,D0
                ramPage |= sega << 3;
            }

            m_ula.SetPageMapping(videoPage, norom ? 0 : -1, 5, 2, ramPage);
            MapRead0000 = norom ? RamPages[5] : RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = norom ? MapRead0000 : m_trashPage;
            MapWrite4000 = MapRead4000;
            MapWrite8000 = MapRead8000;
            MapWriteC000 = MapReadC000;

            Map48[0] = romPage;
            Map48[1] = 5;
            Map48[2] = 2;
            Map48[3] = ramPage;
        }

        public override int GetRomIndex(RomName romId)
        {
            switch (romId)
            {
                case RomName.ROM_128: return 0;
                case RomName.ROM_SOS: return 1;
                case RomName.ROM_DOS: return 2;
                case RomName.ROM_SYS: return 3;
            }
            LogAgent.Error("Unknown RomName: {0}", romId);
            throw new InvalidOperationException("Unknown RomName");
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

        #endregion

        #region Bus Handlers

        private void writePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!m_lock)
                CMR0 = value;
        }

        private void writePortEFF7(ushort addr, byte value, ref bool iorqge)
        {
            CMR1 = value;
        }

        private void BusNmiRq(BusCancelArgs e)
        {
            // check DOSEN to avoid conflict with BDI
            e.Cancel = EnableShadow ? DOSEN : !IsRom48;
        }

        private void BusNmiAck()
        {
            // enable shadow rom
            SYSEN = EnableShadow;
            DOSEN = !EnableShadow;
        }

        private void BusReset()
        {
            CMR0 = 0;
            CMR1 = 0;
            SYSEN = EnableShadow;
            DOSEN = false;
        }

        #endregion


        public MemoryPentagon1024()
            : base("Pentagon")
        {
            EnableShadow = true;
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }

        public bool EnableShadow
        {
            get { return m_enableShadow; }
            set { m_enableShadow = value; OnConfigChanged(); }
        }
    }
}
