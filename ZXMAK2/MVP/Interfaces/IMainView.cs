using ZXMAK2.Interfaces;
using System.Drawing;
using System;
using System.Windows.Forms;


namespace ZXMAK2.MVP.Interfaces
{
    public interface IMainView
    {
        IWin32Window Window { get; }    // TODO: remove
        
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
