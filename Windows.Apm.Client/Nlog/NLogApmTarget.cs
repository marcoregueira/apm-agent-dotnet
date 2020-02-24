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
		private static Action<LogEventInfo> _event;

		public static void OnTrace(Action<LogEventInfo> onTrace) => _event = onTrace;

		protected override void Write(LogEventInfo logEvent)
		{
			var logMessage = Layout.Render(logEvent);
			var currentTransaction = ApmLogger.Default.GetCurrentTransaction();
			var transactionId = currentTransaction?.CurrentTransaction?.Id;

			_event?.Invoke(logEvent);

			if (logEvent.Exception != null && currentTransaction == null)
			{
				using (var trans = ApmLogger.Default.InitTrasaction("ErrorReport", "LogicGroup"))
				{
					Dictionary<string, object> properties = null;

					if (logEvent.Properties != null)
					{
						properties = new Dictionary<string, object>();
						foreach (var pair in logEvent.Properties)
						{
							properties[pair.Key.ToString()] = pair.Value;
						}
					}

					ApmLogger.Default.LogTraceToApm(
						logMessage,
						level: logEvent.Level.ToString(),
						logInfo: properties);
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
				transactionId: transactionId,
				logInfo: logEvent.Properties.ToDictionary(x => x.Key.ToString(), x => x.Value)); //a lo mejor se podría hacer un casting //puede dar error si la clave no se serializa a string
		}
	}
}
