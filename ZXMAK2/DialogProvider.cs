using System;
using ZXMAK2.Controls;


namespace ZXMAK2
{
    public static class DialogProvider
    {
        public static void ShowFatalError(Exception ex)
        {
            Show(
                ex.Message, 
                "FATAL ERROR",
                DlgButtonSet.OK,
                DlgIcon.Error);
        }

        public static DlgResult Show(
            String message,
            String caption,
            DlgButtonSet buttonSet,
            DlgIcon icon)
        {
            return (DlgResult)System.Windows.Forms.MessageBox.Show(
                message, caption,
                (System.Windows.Forms.MessageBoxButtons)buttonSet,
                (System.Windows.Forms.MessageBoxIcon)icon);
        }

        public static object ObjectSelector(object[] objArray, string caption)
        {
            if (objArray.Length < 1)
                return null;
            if (objArray.Length == 1)
                return objArray[0];
            using (ObjectSelectorDialog form = new ObjectSelectorDialog())
            {
                form.ItemArray = objArray;
                form.ItemSelected = objArray[0];
                form.Text = caption;
                form.ShowDialog();
                if (form.DialogResult==System.Windows.Forms.DialogResult.OK)
                    return form.ItemSelected;
                return null;
            }
        }
    }

    public enum DlgResult
    {
        Abort = System.Windows.Forms.DialogResult.Abort,
        Cancel = System.Windows.Forms.DialogResult.Cancel,
        Ignore = System.Windows.Forms.DialogResult.Ignore,
        No = System.Windows.Forms.DialogResult.No,
        None = System.Windows.Forms.DialogResult.None,
        OK = System.Windows.Forms.DialogResult.OK,
        Retry = System.Windows.Forms.DialogResult.Retry,
        Yes = System.Windows.Forms.DialogResult.Yes,
    }

    public enum DlgButtonSet
    {
        AbortRetryIgnore = System.Windows.Forms.MessageBoxButtons.AbortRetryIgnore,
        OK = System.Windows.Forms.MessageBoxButtons.OK,
        OKCancel = System.Windows.Forms.MessageBoxButtons.OKCancel,
        RetryCancel = System.Windows.Forms.MessageBoxButtons.RetryCancel,
        YesNo = System.Windows.Forms.MessageBoxButtons.YesNo,
        YesNoCancel = System.Windows.Forms.MessageBoxButtons.YesNoCancel,
    }

    public enum DlgIcon
    {
        Information = System.Windows.Forms.MessageBoxIcon.Information,
        Warning = System.Windows.Forms.MessageBoxIcon.Warning,
        Error = System.Windows.Forms.MessageBoxIcon.Error,
        Question = System.Windows.Forms.MessageBoxIcon.Question,
    }
}
