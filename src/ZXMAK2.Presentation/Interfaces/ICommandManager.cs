using System;
using System.ComponentModel;


namespace ZXMAK2.Presentation.Interfaces
{
    public interface ICommandManager
    {
        void ClearCommandsUi();
        void AddCommandUi(ICommand command);
    }
}
