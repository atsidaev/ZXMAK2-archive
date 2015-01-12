using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Interfaces
{
    public interface IMouseDevice
    {
        IMouseState MouseState { get; set; }
    }
}
