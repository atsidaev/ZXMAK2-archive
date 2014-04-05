using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;

namespace ZXMAK2.MVP.Interfaces
{
    public interface IViewHolder
    {
        ICommand CommandOpen { get; }
        object[] Arguments { get; set; }

        void Show();
        void Close();
    }
}
