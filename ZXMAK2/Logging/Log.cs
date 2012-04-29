using System;


namespace ZXMAK2.Logging
{
	internal abstract class Log
	{
		public virtual void LogMessage(string message)
		{
			LogMessage(LogLevel.Message, message, null);
		}
		public virtual void LogMessage(Exception ex)
		{
			LogMessage(LogLevel.Message, null, ex);
		}
		public virtual void LogMessage(string message, Exception ex)
		{
			LogMessage(LogLevel.Message, message, ex);
		}
		public virtual void LogWarning(string message)
		{
			LogMessage(LogLevel.Warning, message, null);
		}
		public virtual void LogWarning(Exception ex)
		{
			LogMessage(LogLevel.Warning, null, ex);
		}
		public virtual void LogWarning(string message, Exception ex)
		{
			LogMessage(LogLevel.Warning, message, ex);
		}
		public virtual void LogError(string message)
		{
			LogMessage(LogLevel.Error, message, null);
		}
		public virtual void LogError(Exception ex)
		{
			LogMessage(LogLevel.Error, null, ex);
		}
		public virtual void LogError(string message, Exception ex)
		{
			LogMessage(LogLevel.Error, message, ex);
		}
		public virtual void LogFatal(string message)
		{
			LogMessage(LogLevel.Fatal, message, null);
		}
		public virtual void LogFatal(string message, Exception ex)
		{
			LogMessage(LogLevel.Fatal, message, ex);
		}
		public virtual void LogFatal(Exception ex)
		{
			LogMessage(LogLevel.Fatal, null, ex);
		}
		public virtual void LogTrace(string message)
		{
			LogMessage(LogLevel.Debug, message, null);
		}
		public virtual void LogTrace(Exception ex)
		{
			LogMessage(LogLevel.Debug, null, ex);
		}
		public virtual void LogTrace(string message, Exception ex)
		{
			LogMessage(LogLevel.Debug, message, ex);
		}


		protected abstract void LogMessage(LogLevel level, string message, Exception ex);
	}
}
