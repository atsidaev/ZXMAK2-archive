using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Interfaces
{
    public interface IUserQuery
    {
        DlgResult Show(
            String message,
            String caption,
            DlgButtonSet buttonSet,
            DlgIcon icon);
        
        object ObjectSelector(object[] objArray, string caption);

        bool QueryText(
            string caption,
            string text,
            ref string value);

        bool QueryValue(
            string caption,
            string text,
            string format,
            ref int value,
            int min,
            int max);
    }

    public enum DlgResult
    {
        None = 0,
        OK = 1,
        Cancel = 2,
        Abort = 3,
        Retry = 4,
        Ignore = 5,
        Yes = 6,
        No = 7,
    }

    public enum DlgButtonSet
    {
        OK = 0,
        OKCancel = 1,
        AbortRetryIgnore = 2,
        YesNoCancel = 3,
        YesNo = 4,
        RetryCancel = 5,
    }

    public enum DlgIcon
    {
        None = 0,
        Error = 16,
        Question = 32,
        Warning = 48,
        Information = 64,
    }
}
