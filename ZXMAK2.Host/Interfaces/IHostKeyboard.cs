

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostKeyboard
    {
        void LoadConfiguration(string fileName);
        void Scan();
        IKeyboardState State { get; }
    }
}
