using System;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;


namespace ZXMAK2.Host.Presentation.Interfaces
{
    public interface IMainView : IDisposable
    {
        string Title { get; set; }
        bool IsFullScreen { get; set; }
        IHost Host { get; }
        ICommandManager CommandManager { get; }

        event EventHandler ViewOpened;
        event EventHandler ViewClosed;
        event EventHandler RequestFrame;

        void Run();
        void Bind(IMainPresenter presenter);
        void Close();
        void ShowHelp(object obj);
    }
}
