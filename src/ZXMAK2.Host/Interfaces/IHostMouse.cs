using System;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHostMouse : IDisposable
    {
        void Scan();
        IMouseState MouseState { get; }
    }
}
