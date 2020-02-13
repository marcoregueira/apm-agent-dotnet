using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public void Info(string message) => InternalLogger.Info(message);

		public void Warn(string message) => InternalLogger.Warn(message);

		public void Debug(string message)
		{
			try
			{
				InternalLogger.Debug(message);
			}
			catch (Exception e)
			{
				var ex = e.Message;
			}
		}

		public void Error(string message)
		{
			try
			{
				InternalLogger.Error(message);

			}
			catch (Exception e)
			{
				var ex = e.Message;
			}
		}

		public void Error(Exception x) => InternalLogger.Error(x, x.Message);

		public void Error(string message, Exception x) => InternalLogger.Error(x, message);

		public void Fatal(string message) => InternalLogger.Fatal(message);

		public void Fatal(Exception x) => InternalLogger.Error(x);
	}
}
