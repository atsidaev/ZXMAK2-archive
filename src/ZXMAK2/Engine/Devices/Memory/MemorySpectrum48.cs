using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Engine.Interfaces;

namespace ZXMAK2.Engine.Devices.Memory
{
    public class MemorySpectrum48 : MemorySpectrum128
    {
        #region IBusDevice

        public override string Name { get { return "ZX Spectrum 48"; } }
        public override string Description { get { return "Spectrum 48K Memory Module"; } }

        #endregion

        #region MemoryBase

		public override bool IsMap48 { get { return true; } }

        public override byte CMR0
        {
            get { return 0x30; }
            set { UpdateMapping(); }
        }

        public override byte CMR1
        {
            get { return 0x00; }
            set { UpdateMapping(); }
        }

        protected override void LoadRom()
        {
            base.LoadRom();
			LoadRomPack("ZX48");
		}

        #endregion
    }
}
