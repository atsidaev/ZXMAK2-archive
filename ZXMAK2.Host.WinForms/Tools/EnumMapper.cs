using System;
using ZXMAK2.Host.Entities;
using System.Windows.Forms;
using System.Globalization;


namespace ZXMAK2.Host.WinForms.Tools
{
    public static class EnumMapper
    {
        public static DlgResult GetDlgResult(DialogResult value)
        {
            switch (value)
            {
                case DialogResult.Abort: return DlgResult.Abort;
                case DialogResult.Cancel: return DlgResult.Cancel;
                case DialogResult.Ignore: return DlgResult.Ignore;
                case DialogResult.No: return DlgResult.No;
                case DialogResult.None: return DlgResult.None;
                case DialogResult.OK: return DlgResult.OK;
                case DialogResult.Retry: return DlgResult.Retry;
                case DialogResult.Yes: return DlgResult.Yes;
                default: return ThrowArgumentError<DlgResult>(value);
            }
        }

        private static T ThrowArgumentError<T>(Enum value)
        {
            throw new ArgumentException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Unexpected enum value: {0}.{1}",
                    typeof(T).Name,
                    value));
        }
    }
}
