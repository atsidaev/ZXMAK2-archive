using System;
using System.Text;
using System.Reflection;


namespace ZXMAK2.Logging
{
	internal class LogMessage
	{
		private string _name;
		private DateTime _dateTime;
		private long _tick;
		private LogLevel _level;
		private string _message;
		private Exception _exception;

		public LogMessage(string name, DateTime time, long tick, LogLevel level, string msg, Exception ex)
		{
			_name = name;
			_dateTime = time;
			_tick = tick;
			_level = level;
			_message = msg;
			_exception = ex;
		}

		public string Name { get { return _name; } }
		public DateTime DateTime { get { return _dateTime; } }
		public long Tick { get { return _tick; } }
		public LogLevel Level { get { return _level; } }
		public string Message { get { return _message; } }
		public Exception Exception { get { return _exception; } }

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			if (_name != string.Empty)
				builder.Append(_name + "\t");
			builder.Append(_level.ToString() + "\t");
			builder.Append(_dateTime.ToString("HH:mm:ss.fff") + "\t");
			//builder.Append(_tick.ToString() + "\t");
			if(_message!=null)
				builder.Append(_message.ToString() + "\t");
			string ex = getExceptionString();
			if(ex!=null)
				builder.Append(ex);
			return builder.ToString();
		}

		private string getExceptionString()
		{
            if (_exception != null)
                return Environment.NewLine + 
                    parseException(0, "Exception", _exception);
			return null;
		}

		private string parseException(int tabSize, string name, Exception ex)
		{
			StringBuilder builder = new StringBuilder();
            string tab = new string(' ', tabSize);
			if(!string.IsNullOrEmpty(name))
            {    
                builder.Append(tab);
                builder.Append(string.Format("====={0}======", name));
                builder.Append(Environment.NewLine);
            }
            builder.Append(tab + "Type: " + ex.GetType().ToString() + Environment.NewLine);
			builder.Append(tab + "Message: " + ex.Message + Environment.NewLine);
            ReflectionTypeLoadException rtlex = ex as ReflectionTypeLoadException;
            if (rtlex != null)
                foreach(Exception ldrex in rtlex.LoaderExceptions)
                {
                    builder.Append(parseException(tabSize + 4, "Loader Exception", ldrex));
                }

            if (!string.IsNullOrEmpty(ex.StackTrace))
            {
                builder.Append(tab);
                builder.Append("Stack trace:");
                builder.Append(Environment.NewLine);
                builder.Append(ex.StackTrace);
                builder.Append(Environment.NewLine);
            }
            
            if (ex.InnerException != null)
			{
                builder.Append(parseException(tabSize + 4, "Inner exception", ex.InnerException));
			}
            if (!string.IsNullOrEmpty(name))
            {
                builder.Append(tab);
                builder.Append("===========================");
                builder.Append(Environment.NewLine);
            }
            return builder.ToString();
		}
	}
}
