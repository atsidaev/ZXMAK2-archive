using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Interfaces
{
    public interface IJoystickDevice
    {
        string HostId { get; set; }
        IJoystickState JoystickState { get; set; }
    }
}
