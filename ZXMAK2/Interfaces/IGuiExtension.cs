using System;
using System.Windows.Forms;


namespace ZXMAK2.Engine.Interfaces
{
    public interface IGuiExtension
    {
        void AttachGui(GuiData guiData);
        void DetachGui();
    }

    public class GuiData
    {
        private object m_mainWindow;
        private object m_menuItem;

        public GuiData(object mainWindow, object menuItem)
        {
            m_mainWindow = mainWindow;
            m_menuItem = menuItem;
        }

        public object MainWindow { get { return m_mainWindow; } }
        public object MenuItem { get { return m_menuItem; } }
    }
}
