using System;
using System.Data.Common;
using System.Threading;

namespace WMS_Infrastructure.Instrumentation
{
	public interface IApmLogger
	{
		bool IsEnabled { get; set; }

		ExecutionSegment CurrentTransaction();
		void Dispose();
		void Log(string log);
		void LogCommandToApm(DbCommand command, Action action);
		void LogCommandToApm(DbCommand command, string name, Action action);
		void LogExceptionToApm(Exception ex, string name, string transactionId = null, string host = null, string appName = null);
		void LogTraceToApm(string message, string transactionId = null, string host = null, string appName = null, object logInfo = null, string level = null);
		AutoFinishingSpan InitTrasaction(string name, string type, bool getSpan = false);
	}
}
