using System;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHost : IDisposable
    {
        IHostVideo Video { get; }
        IHostSound Sound { get; }
        IHostKeyboard Keyboard { get; }
        IHostMouse Mouse { get; }
        IHostJoystick Joystick { get; }
    }
}
