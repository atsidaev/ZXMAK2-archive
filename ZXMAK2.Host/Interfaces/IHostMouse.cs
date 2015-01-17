

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostMouse
    {
        void Scan();
        IMouseState MouseState { get; }
    }
}
