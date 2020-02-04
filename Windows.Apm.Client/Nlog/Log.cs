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
		private static readonly Lazy<NLog.Logger> LazyNLogger = new Lazy<NLog.Logger>(NLog.LogManager.GetCurrentClassLogger);

		public static ILogger Instance => LazyLogger.Value;

		public Logger()
		{
		}

		public void Info(string message) => LazyNLogger.Value.Info(message);

		public void Warn(string message) => LazyNLogger.Value.Warn(message);

		public void Debug(string message)
		{
			try
			{
				LazyNLogger.Value.Debug(message);
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
				LazyNLogger.Value.Error(message);

			}
			catch (Exception e)
			{
				var ex = e.Message;
			}
		}

		public void Error(Exception x) => LazyNLogger.Value.Error(x, x.Message);

		public void Error(string message, Exception x) => LazyNLogger.Value.Error(x, message);

		public void Fatal(string message) => LazyNLogger.Value.Fatal(message);

		public void Fatal(Exception x) => LazyNLogger.Value.Error(x);
	}
}
