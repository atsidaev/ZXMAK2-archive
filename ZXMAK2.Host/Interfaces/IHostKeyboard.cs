using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostKeyboard
    {
        void LoadConfiguration(string fileName);
        void Scan();
        IKeyboardState State { get; }
    }
}
