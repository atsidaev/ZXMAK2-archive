using System;
using System.ComponentModel;


namespace ZXMAK2.Presentation.Interfaces
{
    public interface IHostUi
    {
        void ClearCommandsUi();
        void AddCommandUi(ICommand command);
    }
}
