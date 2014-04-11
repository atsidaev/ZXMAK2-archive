using System;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.MVP;


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
                    DialogService.Show(
                        string.Format("{0}\n\n{1}", ex.GetType(), ex.Message),
                        "ZXMAK2",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
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
