using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using ZXMAK2.Host.Entities;

namespace ZXMAK2.Host.Interfaces
{
    public interface IOpenFileDialog : IDisposable
    {
        event CancelEventHandler FileOk;
        
        string Title { get; set; }
        string Filter { get; set; }
        string FileName { get; set; }
        bool ShowReadOnly { get; set; }
        bool ReadOnlyChecked { get; set; }
        bool CheckFileExists { get; set; }
        bool Multiselect { get; set; }

        DlgResult ShowDialog(object owner);
    }
}
