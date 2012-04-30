using System;

using ZXMAK2.Engine.Devices.Disk;


namespace ZXMAK2.Engine.Interfaces
{
    public interface IBetaDiskDevice //: BusDeviceBase
    {
        bool DOSEN { get; set; }

        void SetReg(WD93REG reg, byte value);
        byte GetReg(WD93REG reg);

        bool LedDiskIO { get; set; }
        DiskImage[] FDD { get; }
        string DumpState();
    }

    public enum WD93REG
    {
        #region Comment
        /// <summary>
        /// COMMAND/STATUS register (port #1F)
        /// </summary>
        #endregion
        CMD = 0,
        #region Comment
        /// <summary>
        /// TRACK register (port #3F)
        /// </summary>
        #endregion
        TRK = 1,
        #region Comment
        /// <summary>
        /// SECTOR register (port #5F)
        /// </summary>
        #endregion
        SEC = 2,
        #region Comment
        /// <summary>
        /// DATA register (port #7F)
        /// </summary>
        #endregion
        DAT = 3,
        #region Comment
        /// <summary>
        /// BETA128 register (port #FF)
        /// </summary>
        #endregion
        SYS = 4,
    }

}
