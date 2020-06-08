using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;

namespace WMS_Infrastructure.Instrumentation
{
	public interface IApmLogger
	{
		bool IsEnabled { get; set; }

		ExecutionSegment GetCurrentTransaction();
		void Dispose();
		void Log(string log);
		void LogCommandToApm(DbCommand command, Action action);
		void LogCommandToApm(DbCommand command, string name, Action action);
		void LogExceptionToApm(Exception ex, string name, string transactionId = null, string host = null, string appName = null);
		void LogTraceToApm(string message, string transactionId = null, string host = null, string appName = null, Dictionary<string, object> logInfo = null, string level = null, DateTime? customDate = null);
		AutoFinishingSpan InitTrasaction(string name, string type, bool getSpan = false);
		void AddCustomData(string key, object value);
	}
}
