﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace ZXMAK2.MVP.Interfaces
{
    public interface IView : IDisposable
    {
        void Show(IMainView parent);
        void Hide();
        void Close();

        event EventHandler ViewClosed;
        event CancelEventHandler ViewClosing;
    }
}