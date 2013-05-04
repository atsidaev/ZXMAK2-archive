using System;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Reflection;


namespace ZXMAK2.Logging
{
    internal class LoggerInternal
    {
        private static readonly object _syncStart = new object();
        private static bool _isStarted = false;
        private static Hashtable _loggers = new Hashtable();
        private static Queue _logQueue = new Queue();
        private static Thread _logWriteThread = null;
        private static AutoResetEvent _logQueueEvent = new AutoResetEvent(false);

        private static string _path = null;
        private static bool _append = false;

        public static void Start()
        {
            lock (_syncStart)
            {
                if (_isStarted)
                    return;

                try
                {
                    var folderName = Path.Combine(
                        Utils.GetAppDataFolder(),
                        "Logs");
                    var dateTime = DateTime.Now;
                    var salt = Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().Location);
                    salt = string.Format(
                        "{0}-{1:D04}-{2:D02}-{3:D02}-{4:D02}-{5:D02}-{6:D02}",
                        salt,
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute,
                        dateTime.Second);
                    var fileName = Path.Combine(folderName, string.Format("{0}.log", salt));
                    for (var i = 0; File.Exists(fileName) && i < 100; i++)
                    {
                        fileName = Path.Combine(folderName, string.Format("{0}({1:D02}).log", salt, i));
                    }
                    if (File.Exists(fileName))
                    {
                        return;
                    }
                    _path = fileName;
                    _append = false;
                }
                catch (Exception)
                {
                    return;
                }

                _logWriteThread = new Thread(new ThreadStart(logWriteProc));
                _logWriteThread.Name = "Logger thread";
                _logWriteThread.IsBackground = false;
                _isStarted = true;
                _logWriteThread.Start();
            }
        }

        public static void Finish()
        {
            lock (_syncStart)
            {
                if (!_isStarted)
                    return;
                _isStarted = false;
                _logQueueEvent.Set();
                _logWriteThread.Join();
            }
        }


        public static Log GetLogger()
        {
            return GetLogger(string.Empty);
        }

        public static Log GetLogger(string name)
        {
            lock (_loggers.SyncRoot)
            {
                if (_loggers.ContainsKey(name))
                    return _loggers[name] as Log;
                Log log = new LogReceiver(name);
                _loggers.Add(name, log);
                return log;
            }
        }


        private static void logWriteProc()
        {
            try
            {
                Stream stream = null;

                // open log file when first message appears in queue 
                // (to avoid create empty logs)
                _logQueueEvent.WaitOne(-1, false);
                lock (_logQueue.SyncRoot)
                    if (_logQueue.Count == 0 && !_isStarted)
                        return;
                if (!Directory.Exists(Path.GetDirectoryName(_path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(_path));

                if (!_append)
                {
                    stream = new FileStream(_path, FileMode.Create, FileAccess.Write, FileShare.Read);
                }
                else
                {
                    stream = new FileStream(_path, FileMode.Append, FileAccess.Write, FileShare.Read);
                    stream.Seek(0, SeekOrigin.End);
                }
                using (stream)
                {
                    while (_isStarted)
                    {
                        flushQueue(stream);
                        if (!_logQueueEvent.WaitOne(1000, false))
                            stream.Flush();
                    }
                    flushQueue(stream);
                    stream.Flush();
                }
            }
            catch { }
            _isStarted = false;
        }

        private static byte[] _separator = Encoding.UTF8.GetBytes(Environment.NewLine);

        private static void flushQueue(Stream stream)
        {
            lock (_logQueue.SyncRoot)
                while (_logQueue.Count > 0)
                {
                    byte[] msg = Encoding.UTF8.GetBytes((_logQueue.Dequeue() as LogMessage).ToString());
                    stream.Write(msg, 0, msg.Length);
                    stream.Write(_separator, 0, _separator.Length);
                }
        }

        private static void log(string name, LogLevel level, string message, Exception ex)
        {
            lock (_syncStart)
                if (!_isStarted)
                    return;
            lock (_logQueue.SyncRoot)
            {
                _logQueue.Enqueue(new LogMessage(name, DateTime.Now, Stopwatch.GetTimestamp(), level, message, ex));
                _logQueueEvent.Set();
            }
        }

        private class LogReceiver : Log
        {
            private string _name;

            public LogReceiver(string name)
            {
                _name = name;
            }

            protected override void LogMessage(LogLevel level, string message, Exception ex)
            {
                LoggerInternal.log(_name, level, message, ex);
            }
        }
    }
}
