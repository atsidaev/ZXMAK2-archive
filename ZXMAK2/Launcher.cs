using System;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.MVP;
using ZXMAK2.Interfaces;


namespace ZXMAK2
{
    public class Launcher
    {
        public static void Run<T>(string[] args)
            where T : IMainView, new()
        {
            LogAgent.Start();
            try
            {
                var view = new T();
                using (var presenter = new MainPresenter(view, args))
                {
                    presenter.Run();
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                try
                {
                    Locator.Resolve<IUserMessage>().ErrorDetails(ex);
                }
                catch (Exception ex2)
                {
                    LogAgent.Error(ex2);
                }
            }
            finally
            {
                LogAgent.Finish();
            }
        }
    }
}
