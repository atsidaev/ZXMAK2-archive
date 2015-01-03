using System;
using ZXMAK2.Dependency;
using ZXMAK2.Interfaces;


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
                var resolver = new ResolverUnity();
                //var resolver = new Resolver();
                //resolver.Load("ZXMAK2.Dependency.xml");
                Locator.Instance = resolver;

                var launcher = Locator.Instance.Resolve<ILauncher>();
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
