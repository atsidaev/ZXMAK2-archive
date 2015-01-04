using System;
using ZXMAK2.Dependency;
using ZXMAK2.Presentation.Interfaces;


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
            Logger.Start();
            try
            {
                var launcher = Locator.Resolve<ILauncher>();
                launcher.Run(args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                Logger.Finish();
            }
        }
    }
}
