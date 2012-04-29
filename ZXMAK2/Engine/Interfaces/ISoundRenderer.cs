using System;


namespace ZXMAK2.Engine.Interfaces
{
    #region Comment
    /// <summary>
    /// Provide way to transfer audio data to sound card. AudioBuffer will be taken between BusFrameEnd and BusFrameBegin events.
    /// </summary>
    #endregion
    public interface ISoundRenderer : IBusDevice
    {
        uint[] AudioBuffer { get; }
		int Volume { get; set; }
    }
}
