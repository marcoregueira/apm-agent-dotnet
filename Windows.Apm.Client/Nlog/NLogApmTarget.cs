using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			if (logEvent.Exception != null)
			{
				ApmLogger.LogExceptionToApm(logEvent.Exception, logEvent.Message ?? logEvent.Exception.Message);
			}

			var logMessage = Layout.Render(logEvent);
			ApmLogger.LogTraceToApm(logMessage, level: logEvent.Level.ToString());
			var logProperties = GetAllProperties(logEvent);
		}
	}
}
