using System;
using System.Collections.Generic;


namespace ZXMAK2.Host.Interfaces
{
    public interface IJoystickState
    {
        bool IsLeft { get; }
        bool IsRight { get; }
        bool IsUp { get; }
        bool IsDown { get; }
        bool IsFire { get; }
    }
}
