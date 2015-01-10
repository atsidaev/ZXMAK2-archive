using System;
using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;


namespace ZXMAK2.Host.Services
{
    public class UserHelpProxy : IUserHelp
    {
        private readonly IResolver m_resolver;

        public UserHelpProxy(IResolver resolver)
        {
            m_resolver = resolver;
        }

        public bool CanShow(object uiControl)
        {
            var service = GetService();
            if (service == null)
            {
                return false;
            }
            return service.CanShow(uiControl);
        }

        public void ShowHelp(object uiControl)
        {
            var service = GetService();
            if (service == null)
            {
                return;
            }
            service.ShowHelp(uiControl);
        }

        public void ShowHelp(object uiControl, string keyword)
        {
            var service = GetService();
            if (service == null)
            {
                return;
            }
            service.ShowHelp(uiControl, keyword);
        }


        private IUserHelp GetService()
        {
            var viewResolver = m_resolver.TryResolve<IResolver>("View");
            if (viewResolver == null)
            {
                return null;
            }
            var service = viewResolver.TryResolve<IUserHelp>();
            if (service != null && service.GetType() == GetType())
            {
                Logger.Error("UserHelpProxy: circular dependency detected");
                return null;
            }
            return service;
        }
    }
}
