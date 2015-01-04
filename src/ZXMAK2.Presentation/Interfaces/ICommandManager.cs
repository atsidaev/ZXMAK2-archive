using System;
using System.ComponentModel;


namespace ZXMAK2.Presentation.Interfaces
{
    public interface ICommandManager
    {
        void Clear();
        void Add(ICommand command);
    }
}
