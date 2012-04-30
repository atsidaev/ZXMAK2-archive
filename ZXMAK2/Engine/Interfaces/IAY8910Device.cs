using System;



namespace ZXMAK2.Engine.Interfaces
{
    #region Comment
    /// <summary>
    /// Used to save/load state of AY8910 devices
    /// </summary>
    #endregion
    public interface IAY8910Device //: BusDeviceBase
    {
        byte ADDR_REG { get; set; }
        byte DATA_REG { get; set; }
    }
}
