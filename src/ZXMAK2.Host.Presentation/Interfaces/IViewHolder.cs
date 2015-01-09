using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;


namespace ZXMAK2.Host.Interfaces
{
    public interface IViewHolder
    {
        ICommand CommandOpen { get; }
        Argument[] Arguments { get; set; }

        void Show();
        void Close();
    }
}
