using System;

namespace ZXMAK2.Hardware.Spectrum
{
    public class MemorySpectrum48 : MemorySpectrum128
    {
        #region IBusDevice

        public override string Name { get { return "ZX Spectrum 48"; } }
        public override string Description { get { return "Spectrum 48K Memory Module"; } }

        #endregion

        #region MemoryBase

        public MemorySpectrum48()
            : base("ZX48")
        {
        }

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

        #endregion
    }
}
