using System;
using System.Drawing;
using ZXMAK2.Interfaces;


namespace ZXMAK2.MVP.Interfaces
{
    public interface IMainView
    {
        string Title { get; set; }
        bool IsFullScreen { get; set; }
        IHost Host { get; }
        Func<Size> GetVideoSize { get; set; }
        Func<float> GetVideoRatio { get; set; }

        event EventHandler ViewOpened;
        event EventHandler ViewClosed;
        event EventHandler ViewInvalidate;

        void Run();
        void Bind(MainPresenter presenter);
        void Close();
    }
}
