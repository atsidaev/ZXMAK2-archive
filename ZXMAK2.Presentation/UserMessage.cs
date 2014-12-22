﻿using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Dependency;

namespace ZXMAK2.Presentation
{
    public class UserMessage : IUserMessage
    {
        private readonly IResolver m_resolver;
        
        public UserMessage(IResolver resolver)
        {
            m_resolver = resolver;
        }
        
        public void ErrorDetails(Exception ex)
        {
            var msg = string.Format("{0}\n\n{1}", ex.GetType(), ex.Message);
            Show(msg, "EXCEPTION", DlgIcon.Error);
        }

        public void Error(Exception ex)
        {
            Error(ex.Message);
        }

        public void Error(string fmt, params object[] args)
        {
            var msg = args != null && args.Length > 0 ?
                string.Format(fmt, args) :
                fmt;
            Show(msg, "ERROR", DlgIcon.Error);
        }

        public void Warning(Exception ex)
        {
            Warning(ex.Message);
        }

        public void Warning(string fmt, params object[] args)
        {
            var msg = args != null && args.Length>0 ?  
                string.Format(fmt, args) :
                fmt;
            Show(msg, "WARNING", DlgIcon.Warning);
        }

        public void Info(string fmt, params object[] args)
        {
            var msg = args != null && args.Length > 0 ?
                string.Format(fmt, args) :
                fmt;
            Show(msg, "INFO", DlgIcon.Information);
        }


        private void Show(string msg, string caption, DlgIcon icon)
        {
            var service = m_resolver.TryResolve<IUserQuery>();
            if (service != null)
            {
                service.Show(
                    msg,
                    caption,
                    DlgButtonSet.OK,
                    icon);
            }
        }
    }
}