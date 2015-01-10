using System;
using System.Collections.Generic;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Presentation.Interfaces;
using ZXMAK2.Host.Presentation.Interfaces;


namespace ZXMAK2.Host.Presentation
{
    public class Launcher : ILauncher
    {
        private readonly IResolver m_resolver;
        
        public Launcher(IResolver resolver)
        {
            m_resolver = resolver;
        }
        
        public void Run(string[] args)
        {
            var service = m_resolver.TryResolve<IUserMessage>();
            try
            {
                //m_resolver.RegisterInstance<string>("viewType", "XNA");

                var viewResolver = m_resolver.Resolve<IResolver>("View");
                var view = viewResolver.Resolve<IMainView>();
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
