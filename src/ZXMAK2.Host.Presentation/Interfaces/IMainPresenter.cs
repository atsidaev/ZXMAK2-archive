using System;
using System.ComponentModel;
using ZXMAK2.Mvvm;


namespace ZXMAK2.Host.Presentation.Interfaces
{
    public interface IMainPresenter : IDisposable
    {
        void Run();
        void Attach(ISynchronizeInvoke synchronizeInvoke);
    }
}
