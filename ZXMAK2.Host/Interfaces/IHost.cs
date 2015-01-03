using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Host.Interfaces
{
    public interface IHost : IDisposable
    {
        IHostUi HostUi { get; }
        IHostVideo Video { get; }
        IHostSound Sound { get; }
        IHostKeyboard Keyboard { get; }
        IHostMouse Mouse { get; }
        IHostJoystick Joystick { get; }
    }
}
