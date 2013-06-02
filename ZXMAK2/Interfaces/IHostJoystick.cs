

namespace ZXMAK2.Interfaces
{
    public interface IHostJoystick
    {
        void Scan();
        IJoystickState State { get; }
    }
}
