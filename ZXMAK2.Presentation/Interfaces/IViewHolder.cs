using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Presentation.Interfaces
{
    public interface IViewHolder
    {
        ICommand CommandOpen { get; }
        Argument[] Arguments { get; set; }

        void Show();
        void Close();
    }
}
