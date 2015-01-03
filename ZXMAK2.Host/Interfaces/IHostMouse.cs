using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Host.Interfaces
{
    public interface IHostMouse
    {
        void Scan();
        IMouseState MouseState { get; }
    }
}
