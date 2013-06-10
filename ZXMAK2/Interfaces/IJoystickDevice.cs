﻿using System;


namespace ZXMAK2.Interfaces
{
    public interface IJoystickDevice
    {
        string HostId { get; set; }
        IJoystickState JoystickState { get; set; }
    }

    public interface IJoystickState
    {
        bool IsLeft { get; }
        bool IsRight { get; }
        bool IsUp { get; }
        bool IsDown { get; }
        bool IsFire { get; }
    }
}