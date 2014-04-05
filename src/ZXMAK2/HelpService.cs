using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace ZXMAK2
{
    public static class HelpService
    {
        private static string s_url;

        public static bool CanShow(object uiControl)
        {
            return uiControl is Control;
        }
        
        public static void ShowHelp(object uiControl)
        {
            var control = uiControl as Control;
            if (control == null || !CheckUrl())
            {
                return;
            }
            Help.ShowHelp(control, s_url);
        }

        public static void ShowHelp(object uiControl, HelpNavigator navigator)
        {
            var control = uiControl as Control;
            if (control == null || !CheckUrl())
            {
                return;
            }
            Help.ShowHelp(control, s_url, navigator);
        }

        public static void ShowHelp(object uiControl, string keyword)
        {
            var control = uiControl as Control;
            if (control == null || !CheckUrl())
            {
                return;
            }
            Help.ShowHelp(control, s_url, keyword);
        }

        public static void ShowHelp(Object uiControl, HelpNavigator command, object parameter)
        {
            var control = uiControl as Control;
            if (control == null || !CheckUrl())
            {
                return;
            }
            Help.ShowHelp(control, s_url, command, parameter);
        }

        private static bool CheckUrl()
        {
            if (s_url != null)
            {
                return true;
            }
            var appName = Path.GetFullPath(Assembly.GetExecutingAssembly().Location);
            var helpFile = Path.ChangeExtension(appName, ".chm");
            if (!File.Exists(helpFile))
            {
                DialogService.Show(
                    "Help file is missing",
                    "ERROR",
                    DlgButtonSet.OK,
                    DlgIcon.Error);
                return false;
            }
            if (helpFile.Contains("#")) //Path to .chm file must not contain # - Microsoft bug
            {
                var fileName = Path.GetRandomFileName() + ".chm";
                var tmpFile = Path.Combine(Path.GetTempPath(), fileName);
                File.Copy(helpFile, tmpFile);
                s_url = tmpFile;
                Application.ApplicationExit += (s, e) => File.Delete(tmpFile);
                return true;
            }
            s_url = helpFile;
            return true;
        }
    }
}
