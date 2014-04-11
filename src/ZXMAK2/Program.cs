using System;
using System.Windows.Forms;
using ZXMAK2.MVP.WinForms;


namespace ZXMAK2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Launcher.Run<MainView>(args);
        }
    }
}
