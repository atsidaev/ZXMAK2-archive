using System;
using System.Text;
using System.Diagnostics;
using ZXMAK2.Logging;
using System.IO;

namespace ZXMAK2
{
    public static class LogAgent
    {
        internal static void Start()
        {
            LoggerInternal.Start();
        }

        internal static void Finish()
        {
            LoggerInternal.Finish();
        }
        
        public static void Debug(string fmt, params object[] args)
        {
            try
            {
                string msg = string.Format(fmt, args);
                LoggerInternal.GetLogger().LogTrace(msg);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static void Info(string fmt, params object[] args)
        {
            try
            {
                string msg = string.Format(fmt, args);
                LoggerInternal.GetLogger().LogMessage(msg);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static void Warn(string fmt, params object[] args)
        {
            try
            {
                string msg = string.Format(fmt, args);
                LoggerInternal.GetLogger().LogWarning(msg);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static void Error(string fmt, params object[] args)
        {
            try
            {
                string stack = new StackTrace().ToString();
                string msg = string.Format(fmt, args);
                msg = string.Format("{0}{1}{2}", msg, Environment.NewLine, stack);
                LoggerInternal.GetLogger().LogError(msg);
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public static void Error(Exception ex)
        {
            try
            {
                string stack = new StackTrace().ToString();
                string msg = format(ex);
                msg = string.Format("{0}{1}---{1}Full StackTrace:{1}{2}", msg, Environment.NewLine, stack);
                LoggerInternal.GetLogger().LogError(msg);
            }
            catch (Exception ex2)
            {
                LoggerInternal.GetLogger().LogError(ex2);
            }
        }

        private static string format(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Exception ");
            if (ex != null)
            {
                sb.Append(ex.GetType().ToString());
                sb.Append(": ");
                sb.Append(ex.Message);
                sb.Append(Environment.NewLine);
                sb.Append(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    sb.Append(string.Format("{0}Inner{1}", Environment.NewLine, format(ex.InnerException)));
                }
            }
            else
            {
                sb.Append("[null]");
            }
            return sb.ToString();
        }

        [Obsolete("remove call to LogAgent.DumpArray")]
        public static void DumpArray<T>(string fileName, T[] array)
        {
            using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var wr = new StreamWriter(fs))
            {
                for (var i = 0; i < array.Length; i++)
                {
                    wr.WriteLine("{0} = {1}", i, array[i]);
                }
            }
        }
    }
}
