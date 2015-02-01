using System;


namespace ZXMAK2.Host.Interfaces
{
    public interface IHostKeyboard : IDisposable
    {
        void LoadConfiguration(string fileName);
        void Scan();
        IKeyboardState State { get; }
    }
}
