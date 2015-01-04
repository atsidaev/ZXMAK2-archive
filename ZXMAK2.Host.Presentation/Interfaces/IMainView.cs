using System;
using System.Drawing;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Presentation.Interfaces
{
    public interface IMainView : IDisposable
    {
        string Title { get; set; }
        bool IsFullScreen { get; set; }
        IHost Host { get; }
        Func<IVideoData> GetVideoData { get; set; }

        event EventHandler ViewOpened;
        event EventHandler ViewClosed;
        event EventHandler ViewInvalidate;

        void Run();
        void Bind(IMainPresenter presenter);
        void Close();
        void ShowHelp(object obj);
    }
}
