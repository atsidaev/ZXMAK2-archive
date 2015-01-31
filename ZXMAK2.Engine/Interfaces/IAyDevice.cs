using System;
using ZXMAK2.Engine.Entities;


namespace ZXMAK2.Engine.Interfaces
{
    #region Comment
    /// <summary>
    /// Used to save/load state of AY8910 devices
    /// </summary>
    #endregion
    public interface IAyDevice
    {
        event Action<IAyDevice, PsgPortState> IraHandler;
        event Action<IAyDevice, PsgPortState> IrbHandler;
        
        byte RegAddr { get; set; }
        byte GetReg(int index);
        void SetReg(int index, byte value);
    }
}
