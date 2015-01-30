


namespace ZXMAK2.Engine.Interfaces
{
    #region Comment
    /// <summary>
    /// Used to save/load state of AY8910 devices
    /// </summary>
    #endregion
    public interface IAyDevice
    {
        byte RegAddr { get; set; }
        byte RegData { get; set; }
    }
}
