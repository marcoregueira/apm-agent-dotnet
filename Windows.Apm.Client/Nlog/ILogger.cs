using System;

namespace Windows.Apm.Client.Nlog
{
	public interface ILogger
	{
		void Debug(string message, object args = null);
		void Error(Exception x);
		void Error(string message, Exception ex = null, object args = null);
		void Fatal(Exception x);
		void Fatal(string message);
		void Info(string message, object args = null);
		void Trace(string message, object args = null);
		void Warn(string message);
	}
}
