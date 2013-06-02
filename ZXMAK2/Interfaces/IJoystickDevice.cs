

namespace ZXMAK2.Interfaces
{
    public interface IJoystickDevice
    {
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
