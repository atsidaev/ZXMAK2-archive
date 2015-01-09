using System;
using System.Collections.Generic;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;


namespace ZXMAK2.Presentation
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
            var service = m_resolver.TryResolve<IUserMessage>();
            try
            {
                var view = m_viewResolver.Resolve<IMainView>();
                if (view==null)
                {
                    if(service != null)
                    {
                        service.Error("Cannot create IMainView");
                    }
                    return;
                }
                using (view)
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
                Logger.Error(ex);
                if (service != null)
                {
                    service.ErrorDetails(ex);
                }
            }
        }
    }
}
