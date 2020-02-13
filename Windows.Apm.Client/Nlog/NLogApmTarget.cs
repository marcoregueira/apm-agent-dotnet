using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using NLog;
using NLog.Common;
using NLog.Targets;
using WMS_Infrastructure.Instrumentation;

namespace Windows.Apm.Client.Nlog
{
	[Target("apm")]
	public class NLogApmTarget : TargetWithContext
	{
		protected override void Write(LogEventInfo logEvent)
		{
			var logMessage = Layout.Render(logEvent);
			var currentTransaction = ApmLogger.Default.GetCurrentTransaction();
			var transactionId = currentTransaction?.CurrentTransaction?.Id;
			if (logEvent.Exception != null && currentTransaction == null)
			{
				using (var trans = ApmLogger.Default.InitTrasaction("ErrorReport", "LogicGroup"))
				{
					ApmLogger.Default.LogTraceToApm(logMessage, level: logEvent.Level.ToString());
					ApmLogger.Default.LogExceptionToApm(logEvent.Exception, logEvent.Message ?? logEvent.Exception.Message);
				}
				return;
			}

			if (logEvent.Exception != null)
			{
				ApmLogger.Default.LogExceptionToApm(
					logEvent.Exception,
					logEvent.Message ?? logEvent.Exception.Message,
					transactionId: transactionId);
			}

			ApmLogger.Default.LogTraceToApm(
				logMessage,
				level: logEvent.Level.ToString(),
				transactionId: transactionId);
			//var logProperties = GetAllProperties(logEvent);
		}
	}
}
