using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Helpers;
using Elastic.Apm.Report;
using Windows.Apm.Client.Nlog;

namespace WMS_Infrastructure.Instrumentation
{
	public class ApmLoggerForms : IDisposable, IApmLogger
	{
		private bool Disposed { get; set; }
		public static ConditionalWeakTable<Thread, ExecutionSegment> Traces { get; } = new ConditionalWeakTable<Thread, ExecutionSegment>();
		public Thread CurrentTread { get; set; }

		public static string ApplicationName { get; set; } = AppDomain.CurrentDomain.FriendlyName;

		public bool IsEnabled { get; set; } = true;

		public ApmLoggerForms()
		{

		}

		//public static void LogSql(DbConnection con, string sql)
		//{
		//    ExecutionSegment sqlTrace;
		//    lock (Traces)
		//    {
		//        Traces.TryGetValue(Thread.CurrentThread, out sqlTrace);
		//        if (sqlTrace == null) return;
		//    }
		//
		//    bool firstTime;
		//    var id = sqlTrace.IdGenerator.GetId(con, out firstTime);
		//    sqlTrace.SqlList.Add("Conexión #" + id + (firstTime ? "(nueva)" : ""));
		//    sqlTrace.Connections[con] = id;
		//
		//    sqlTrace.SqlList.Add(DateTime.Now.ToLongTimeString());
		//    sqlTrace.SqlList.Add(sql);
		//}


		public void Dispose()
		{
			if (Disposed)
				return;
		}

		public static SemaphoreSlim _tracesSemaphore = new SemaphoreSlim(1);

		public AutoFinishingSpan InitTrasaction(string name, string type, bool getSpan = false)
		{
			if (!IsEnabled)
				return new AutoFinishingSpan(null);

			ITransaction transaction = null;
			try
			{
				_tracesSemaphore.Wait();
				// si estamos en un entorno web esto tiene que depender del contexto. Funcionará mal en entornos multitarea
				// en web quitar el lock.
				var thread = Thread.CurrentThread;
				Traces.TryGetValue(thread, out var trace);
				if (trace == null)
				{
					transaction = Agent.Tracer.StartTransaction(name, type);
					trace = new ExecutionSegment(transaction)
					{
						ImplicitSpan = getSpan
					};
					Traces.Add(Thread.CurrentThread, trace);
					return new AutoFinishingSpan(
						transaction,
						onFinish: () => Traces.Remove(thread));
				}

				var currentSegment = trace.Spans.LastOrDefault() ?? trace.CurrentTransaction as IExecutionSegment;
				var newSegment = currentSegment.StartSpan(name, type);
				trace.Spans.Push(newSegment);

				return new AutoFinishingSpan(newSegment, transaction, () =>
				{
					trace.Spans.Pop();
					if (trace.Spans.Count == 0)
					{
						trace.CurrentTransaction.End();
						//traces.Remove(thread);
					}
				});
			}
			finally
			{
				_tracesSemaphore.Release();
			}
		}

		private void FinishCommand(DbCommand command, ISpan newSpan, Stopwatch stop)
		{
			if (!IsEnabled)
				return;

			newSpan.Context.Db = new Database
			{
				Statement = command.CommandText,
				Instance = command.Connection.Database,
				Type = Database.TypeSql
			};

			newSpan.Duration = stop.ElapsedMilliseconds;

			var providerType = command.Connection.GetType().FullName;

			switch (providerType)
			{
				case string str when str.Contains("Npgsql.NpgsqlConnection"):
					newSpan.Subtype = "npg";
					break;
				case string str when str.Contains("Sqlite"):
					newSpan.Subtype = ApiConstants.SubtypeSqLite;
					break;
				case string str when str.Contains("SqlConnection"):
					newSpan.Subtype = ApiConstants.SubtypeMssql;
					break;
				default:
					newSpan.Subtype = providerType; //TODO, TBD: this is an unknown provider
					break;
			}

			switch (command.CommandType)
			{
				case CommandType.Text:
					newSpan.Action = ApiConstants.ActionQuery;
					break;
				case CommandType.StoredProcedure:
					newSpan.Action = ApiConstants.ActionExec;
					break;
				case CommandType.TableDirect:
					newSpan.Action = "tabledirect";
					break;
				default:
					newSpan.Action = command.CommandType.ToString();
					break;
			}

			//newSpan.End(); for reference, we don't do this here...
		}

		private ISpan GetSpan(DbCommand command)
		{

			var transaction = GetCurrentTransaction().CurrentTransaction ?? InitTrasaction(command.CommandText, ApiConstants.TypeDb).Span;
			return transaction.StartSpan(command.CommandText, ApiConstants.TypeDb);
			//var _tracer = Agent.Tracer;
			//var currentExecutionSegment = _tracer.CurrentSpan ?? (IExecutionSegment)_tracer.CurrentTransaction;
			//var newSpan = currentExecutionSegment.StartSpan(command.CommandText, ApiConstants.TypeDb);
			//return newSpan;
		}

		public ExecutionSegment GetCurrentTransaction()
		{
			Traces.TryGetValue(Thread.CurrentThread, out var executionSegment);
			return executionSegment;
		}

		public void Log(string log)
		{
			//var span = CurrentTransaction();
			//span.CurrentTransaction.lo
		}

		public void LogCommandToApm(DbCommand command, Action action) => LogCommandToApm(command, "DbCommand", action);

		public void LogCommandToApm(DbCommand command, string name, Action action)
		{
			if (!IsEnabled)
			{
				action.Invoke();
				return;
			}

			var time = new Stopwatch();
			using (var span = InitTrasaction(name, "db", true))
				try
				{
					time.Start();
					action.Invoke();
				}
				catch (Exception ex)
				{
					span.CaptureException(ex);
					throw;
				}
				finally
				{
					FinishCommand(command, span.Span as ISpan, time);
				}
		}

		public void LogTraceToApm(string message, string transactionId = null, string host = null, string appName = null, Dictionary<string, object> logInfo = null, string level = null, DateTime? customDate = null)
		{
			var info = new Dictionary<string, object>();
			var trans = GetCurrentTransaction();
			if (trans?.CurrentTransaction != null)
			{
				foreach (var pair in trans.CurrentTransaction.Labels)
				{
					info[pair.Key] = pair.Value;
				}
			}

			if (logInfo != null)
			{
				foreach (var pair in logInfo)
				{
					info[pair.Key] = pair.Value;
				}
			}

			if (!string.IsNullOrWhiteSpace(host))
			{
				info[host] = host;
			}

			if (!IsEnabled)
				return;
			var culprit = appName ?? ApplicationName;
			var now = TimeUtils.TimestampNow();
			var errorLog = new LogEntry(
				culprit: culprit,
				id: null,
				parentId: null,
				timestamp: TimeUtils2.ToTimestamp(customDate) ?? TimeUtils.TimestampNow(),
				traceId: null,
				transactionId: transactionId ?? GetCurrentTransaction()?.CurrentTransaction?.Id,
				transaction: null,
				level: level,
				message: message,
				logInfo: info);

			(Agent.Instance.PayloadSender as LocalPayloadSenderV2)?.EnqueueEvent(errorLog, "log");
		}


		public void LogExceptionToApm(Exception ex, string name, string transactionId = null, string host = null, string appName = null)
		{
			if (!IsEnabled)
				return;
			var culprit = appName ?? ApplicationName;
			var transaction = GetCurrentTransaction();

			//if (transaction == null)
			{
				using (var span = InitTrasaction(name, "exception", true))
					span.CaptureException(ex, culprit, parentId: transaction?.CurrentTransaction.Id);
			}
			//else
			{

			}
		}

		public void AddCustomData(string key, object value)
		{
			var labels = GetCurrentTransaction()?.CurrentTransaction.Labels;
			if (labels != null)
				labels[key] = value?.ToString();
		}
	}
}

