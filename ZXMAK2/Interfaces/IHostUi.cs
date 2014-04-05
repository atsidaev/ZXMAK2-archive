using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace ZXMAK2.Interfaces
{
    public interface IHostUi
    {
        void ClearCommandsUi();
        void AddCommandUi(ICommand command);
    }
}
