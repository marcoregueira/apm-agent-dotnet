using System;

namespace Windows.Apm.Client.Nlog
{
	public class Logger : ILogger
	{
		private static readonly Lazy<Logger> LazyLogger = new Lazy<Logger>(() => new Logger());
		private static readonly NLog.Logger InternalLogger = NLog.LogManager.GetCurrentClassLogger();

		public static ILogger Instance => LazyLogger.Value;

		public Logger()
		{
		}

		public void Trace(string message, object args = null) => InternalLogger.Trace(message, args: args);

		public void Info(string message, object args = null) => InternalLogger.Info(message, args: args);

		public void Warn(string message) => InternalLogger.Warn(message);

		public void Debug(string message, object args = null)
		{
			try
			{
				InternalLogger.Debug(message, args);
			}
			catch (Exception e)
			{
				var ex = e.Message;
			}
		}

		public void Error(string message, Exception ex = null, object args = null)
		{
			try
			{
				InternalLogger.Error(ex, message, args: args);
			}
			catch (Exception e)
			{
				var exc = e.Message;
			}
		}

		public void Error(Exception x) => InternalLogger.Error(x, x.Message);


		public void Fatal(string message) => InternalLogger.Fatal(message);

		public void Fatal(Exception x) => InternalLogger.Error(x);
	}
}
