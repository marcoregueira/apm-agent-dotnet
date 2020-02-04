using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.Helpers;
using Elastic.Apm.Report;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.Apm.Client.Nlog;

namespace WMS_Infrastructure.Instrumentation
{
	public class ApmLogger : IDisposable
	{
		public static ApmLogger Default { get; }
		static ApmLogger() => Default = new ApmLogger();

		private bool Disposed { get; set; }
		public static ConditionalWeakTable<Thread, ExecutionSegment> Traces { get; } = new ConditionalWeakTable<Thread, ExecutionSegment>();
		public Thread CurrentTread { get; set; }

		public static string ApplicationName { get; } = AppDomain.CurrentDomain.FriendlyName;

		public bool IsEnabled { get; set; } = true;

		public ApmLogger()
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
			if (Disposed) return;
		}

		public static AutoFinishingSpan InitTrasaction(string name, string type, bool getSpan = false)
		{
			if (!Default.IsEnabled) return new AutoFinishingSpan(null);

			ITransaction transaction = null;
			lock (Traces)
			{
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
					if (!getSpan)
						return new AutoFinishingSpan(transaction, onFinish:
							() =>
							{
								Traces.Remove(thread);
							});
				}

				var currentSegment = trace.Spans.LastOrDefault() ?? trace.CurrentTransaction as IExecutionSegment;
				var newSegment = currentSegment.StartSpan(name, type);
				trace.Spans.Push(newSegment);

				return new AutoFinishingSpan(newSegment, transaction, () =>
				{
					trace.Spans.Pop();
					if (trace.ImplicitSpan && trace.Spans.Count == 0)
					{
						trace.CurrentTransaction.End();
						Traces.Remove(thread);
					}
				});
			}
		}

		public static void AgentSetup()
		{
			var comp = new AgentComponents();
			Agent.Setup(comp);
		}

		private static void FinishCommand(DbCommand command, ISpan newSpan, Stopwatch stop)
		{
			if (!Default.IsEnabled) return;

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

		private static ISpan GetSpan(DbCommand command)
		{
			ISpan newSpan;
			var _tracer = Agent.Tracer;
			//.StartTransaction("MyTransaction", ApiConstants.TypeRequest);
			var currentExecutionSegment = _tracer.CurrentSpan ?? (IExecutionSegment)_tracer.CurrentTransaction;
			newSpan = currentExecutionSegment.StartSpan(command.CommandText, ApiConstants.TypeDb);
			return newSpan;
		}

		public ExecutionSegment CurrentTransaction()
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
			if (!Default.IsEnabled)
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


		public static void LogTraceToApm(string message, string transactionId = null, string host = null, string appName = null, object logInfo = null, string level = null)
		{
			if (!Default.IsEnabled) return;
			var culprit = appName ?? ApmLogger.ApplicationName;
			var now = TimeUtils.TimestampNow();
			var errorLog = new LogEntry(
				culprit: culprit,
				id: null,
				parentId: null,
				timestamp: now,
				traceId: null,
				transactionId: transactionId,
				transaction: null,
				level: level,
				message: message,
				logInfo: logInfo);

			(Agent.Instance.PayloadSender as LocalPayloadSenderV2)?.EnqueueEvent(errorLog, "log");
		}

		public static void LogExceptionToApm(Exception ex, string name, string transactionId = null, string host = null, string appName = null)
		{
			if (!Default.IsEnabled) return;
			var culprit = appName ?? ApmLogger.ApplicationName;

			using (var span = InitTrasaction(name, "exception", true))
				span.CaptureException(ex, culprit);
		}
	}

	public class ExecutionSegment
	{
		public ExecutionSegment(ITransaction transaction)
		{
			CurrentTransaction = transaction;
		}

		public bool ImplicitSpan { get; set; }
		public Stack<ISpan> Spans { get; } = new Stack<ISpan>();
		public ITransaction CurrentTransaction { get; }
	}

	public class AutoFinishingSpan : IDisposable
	{
		private readonly IExecutionSegment span;
		private readonly ITransaction parent;
		private readonly bool m_Disposed = false;

		public Action OnFinish { get; }

		public IExecutionSegment Span => span;

		public AutoFinishingSpan(IExecutionSegment span, ITransaction parent = null, Action onFinish = null)
		{
			this.span = span;
			OnFinish = onFinish;
			this.parent = parent;
		}

		public void Dispose()
		{
			if (m_Disposed) return;
			OnFinish?.Invoke();
			span?.End();
			parent?.End();
		}

		public void CaptureException(Exception exception, string culprit = null, bool isHandled = false, string parentId = null) =>
			span?.CaptureException(exception, culprit, isHandled, parentId);
	}
}

