using System;
using System.ComponentModel;


namespace ZXMAK2.Host.Interfaces
{
    public interface ICommand : INotifyPropertyChanged
    {
        event EventHandler CanExecuteChanged;

        bool CanExecute(Object parameter);
        void Execute(Object parameter);

        string Text { get; set; }
        bool Checked { get; set; }
    }
}
