using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Dependency;

namespace ZXMAK2.MVP.Interfaces
{
    public interface IViewHolder
    {
        ICommand CommandOpen { get; }
        Argument[] Arguments { get; set; }

        void Show();
        void Close();
    }
}
