using System;
using System.Linq;
using ZXMAK2.XNA4.Views;


namespace ZXMAK2.XNA4
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Launcher.Run<MainView>(args);
        }
    }
}
