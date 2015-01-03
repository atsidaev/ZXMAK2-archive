using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Interfaces
{
    public interface IUserMessage
    {
        void ErrorDetails(Exception ex);
        
        void Error(Exception ex);
        void Error(string fmt, params object[] args);
        
        void Warning(Exception ex);
        void Warning(string fmt, params object[] args);

        void Info(string fmt, params object[] args);
    }
}
