using System;
using ZXMAK2.MVP.Interfaces;
using ZXMAK2.MVP;
using ZXMAK2.Interfaces;
using System.Collections.Generic;
using ZXMAK2.Dependency;


namespace ZXMAK2
{
    public class Launcher : ILauncher
    {
        private readonly IResolver m_resolver;
        private readonly IViewResolver m_viewResolver;
        
        public Launcher(IResolver resolver, IViewResolver viewResolver)
        {
            m_resolver = resolver;
            m_viewResolver = viewResolver;
        }
        
        public void Run(string[] args)
        {
            try
            {
                using (var view = m_viewResolver.Resolve<IMainView>())
                {
                    var list = new List<Argument>();
                    list.Add(new Argument("view", view));
                    list.Add(new Argument("args", args));
                    using (var presenter = m_resolver.Resolve<IMainPresenter>(list.ToArray()))
                    {
                        presenter.Run();
                    }
                }
            }
            catch (Exception ex)
            {
                LogAgent.Error(ex);
                var service = m_resolver.TryResolve<IUserMessage>();
                if (service != null)
                {
                    service.ErrorDetails(ex);
                }
            }
        }
    }
}
