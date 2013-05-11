﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ZXMAK2.Interfaces;
using System.Xml;

namespace ZXMAK2.Hardware.Pentagon
{
    public class MemoryPentagon512 : MemoryBase
    {
        #region Fields

        private byte[][] m_ramPages = new byte[32][];
        private byte[] m_trashPage = new byte[0x4000];
        private bool m_lock = false;

        #endregion Fields


        #region IBusDevice

        public override string Name { get { return "Pentagon 512K"; } }
        public override string Description { get { return "Pentagon 512K Memory Module"; } }

        public override void BusInit(IBusManager bmgr)
        {
            bmgr.SubscribeWrIo(0x8002, 0x0000, writePort7FFD);

            bmgr.SubscribeRdMemM1(0xFF00, 0x3D00, BusReadMem3D00_M1);
            bmgr.SubscribeRdMemM1(0xC000, 0x4000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0x8000, BusReadMemRamM1);
            bmgr.SubscribeRdMemM1(0xC000, 0xC000, BusReadMemRamM1);
            bmgr.SubscribeNmiAck(BusNmiAck);
            bmgr.SubscribeReset(BusReset);

            // Subscribe before MemoryBase.BusInit 
            // to handle memory switches before read
            base.BusInit(bmgr);
        }

        #endregion

        #region MemoryBase

        public override byte[][] RamPages { get { return m_ramPages; } }

        public override bool IsMap48 { get { return m_lock; } }

        protected override void UpdateMapping()
        {
            m_lock = (CMR0 & 0x20) != 0;
            int ramPage = CMR0 & 7;
            int romPage = (CMR0 & 0x10) != 0 ?
                GetRomIndex(RomName.ROM_SOS) :
                GetRomIndex(RomName.ROM_128);
            int videoPage = (CMR0 & 0x08) == 0 ? 5 : 7;

            if (DOSEN)      // trdos or 48/128
                romPage = GetRomIndex(RomName.ROM_DOS);
            if (SYSEN)
                romPage = GetRomIndex(RomName.ROM_SYS);

            int sega = (CMR0 & 0xC0) >> 6; // PENT512: D7,D6,D2,D1,D0

            ramPage |= sega << 3;

            m_ula.SetPageMapping(videoPage, -1, 5, 2, ramPage);
            MapRead0000 = RomPages[romPage];
            MapRead4000 = RamPages[5];
            MapRead8000 = RamPages[2];
            MapReadC000 = RamPages[ramPage];

            MapWrite0000 = m_trashPage;
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

        public override void LoadConfig(XmlNode itemNode)
        {
            base.LoadConfig(itemNode);
            EnableShadow = Utils.GetXmlAttributeAsBool(itemNode, "enableShadow", EnableShadow);
        }

        public override void SaveConfig(XmlNode itemNode)
        {
            base.SaveConfig(itemNode);
            Utils.SetXmlAttribute(itemNode, "enableShadow", EnableShadow);
        }

        #endregion

        #region Bus Handlers

        private void writePort7FFD(ushort addr, byte value, ref bool iorqge)
        {
            if (!m_lock)
                CMR0 = value;
        }

        private void BusNmiRq(BusCancelArgs e)
        {
            // check DOSEN to avoid conflict with BDI
            e.Cancel = DOSEN;
        }

        private void BusNmiAck()
        {
            // enable shadow rom
            SYSEN = EnableShadow;//true;
        }

        private void BusReset()
        {
            SYSEN = EnableShadow;//true;
            CMR0 = 0;
        }

        #endregion Bus Handlers




        public MemoryPentagon512()
            : base("Pentagon")
        {
            EnableShadow = true;
            for (var i = 0; i < m_ramPages.Length; i++)
            {
                m_ramPages[i] = new byte[0x4000];
            }
        }

        public bool EnableShadow { get; set; }
    }
}
