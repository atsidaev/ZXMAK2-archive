using System;


namespace ZXMAK2.Interfaces
{
    public interface IHostJoystick
    {
        void CaptureHostDevice(string hostId);
        void ReleaseHostDevice(string hostId);
        void Scan();
        IJoystickState GetState(string hostId);
        IKeyboardState KeyboardState { set; }
        bool IsKeyboardStateRequired { get; }
    }
}
