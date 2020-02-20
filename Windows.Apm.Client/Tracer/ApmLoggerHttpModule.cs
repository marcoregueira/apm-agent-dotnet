using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.AspNetFullFramework;
using Elastic.Apm.AspNetFullFramework.Extensions;
using Elastic.Apm.Helpers;
using Elastic.Apm.Report;
using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Web;
using Windows.Apm.Client.Nlog;

namespace WMS_Infrastructure.Instrumentation
{
	public class ApmLoggerHttpModule : IDisposable, IApmLogger, IHttpModule
	{
		private bool Disposed { get; set; }

		public static string ApplicationName { get; } = AppDomain.CurrentDomain.FriendlyName;

		public bool IsEnabled { get; set; } = true;

		public ApmLoggerHttpModule() { }

		public void Dispose()
		{
			if (Disposed) return;
		}

		public ExecutionSegment GetCurrentTransaction() => HttpContext.Current.Items["__APM_TRANSACTION"] as ExecutionSegment;
		private void SetCurrentTransaction(ExecutionSegment trace) => HttpContext.Current.Items["__APM_TRANSACTION"] = trace;
		private void SetCurrentSpan(AutoFinishingSpan transaction) => HttpContext.Current.Items["__APM_TRANSACTION_SPAN"] = transaction;
		private AutoFinishingSpan GetCurrentSpan() => HttpContext.Current.Items["__APM_TRANSACTION_SPAN"] as AutoFinishingSpan;


		public AutoFinishingSpan InitTrasaction(string name, string type, bool getSpan = false)
		{
			if (!IsEnabled) return new AutoFinishingSpan(null);
			var trace = GetCurrentTransaction();

			ITransaction transaction = null;
			if (trace == null)
			{
				transaction = Agent.Tracer.StartTransaction(name, type);
				trace = new ExecutionSegment(transaction)
				{
					ImplicitSpan = getSpan
				};
				SetCurrentTransaction(trace);

				//if (!getSpan)
				return new AutoFinishingSpan(transaction,
					onFinish: () => SetCurrentTransaction(null));
			}

			var currentSegment = trace.Spans.LastOrDefault() ?? trace.CurrentTransaction as IExecutionSegment;
			var newSegment = currentSegment.StartSpan(name, type);
			trace.Spans.Push(newSegment);

			return new AutoFinishingSpan(newSegment, transaction, () =>
			{
				trace.Spans.Pop();
				if (/*trace.ImplicitSpan && */ trace.Spans.Count == 0)
				{
					trace.CurrentTransaction.End();
					//customModule.ExecutionSegment = null;
				}
			});
		}



		private void FinishCommand(DbCommand command, ISpan newSpan, Stopwatch stop)
		{
			if (!IsEnabled) return;

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


		public void LogTraceToApm(string message, string transactionId = null, string host = null, string appName = null, object logInfo = null, string level = null, DateTime? customDate = null)
		{
			if (!IsEnabled) return;
			var culprit = appName ?? ApplicationName;
			var now = TimeUtils.ToTimestamp(customDate) ?? TimeUtils.TimestampNow();
			var errorLog = new LogEntry(
				culprit: culprit,
				id: null,
				parentId: null,
				timestamp: now,
				traceId: null,
				transactionId: transactionId ?? GetCurrentTransaction()?.CurrentTransaction?.Id,
				transaction: null,
				level: level,
				message: message,
				logInfo: logInfo);

			(Agent.Instance.PayloadSender as LocalPayloadSenderV2)?.EnqueueEvent(errorLog, "log");
		}

		public void LogExceptionToApm(Exception ex, string name, string transactionId = null, string host = null, string appName = null)
		{
			if (!IsEnabled) return;
			var culprit = appName ?? ApplicationName;

			using (var span = InitTrasaction(name, "exception", true))
				span.CaptureException(ex, culprit);
		}

		public void Init(HttpApplication httpApp)
		{
			httpApp.BeginRequest += OnBeginRequest;
			httpApp.EndRequest += OnEndRequest;
		}

		private void OnBeginRequest(object eventSender, EventArgs eventArgs)
		{

			try
			{
				ProcessBeginRequest(eventSender);
			}
			catch (Exception ex)
			{
				//_logger.Error()?.LogException(ex, "Processing BeginRequest event failed");
			}
		}

		private void ProcessBeginRequest(object eventSender)
		{
			var httpApp = (HttpApplication)eventSender;
			var httpRequest = httpApp.Context.Request;

			var transactionName = $"{httpRequest.HttpMethod} {httpRequest.Path}";

			var soapAction = httpRequest.ExtractSoapAction(null);
			if (soapAction != null) transactionName = $" {soapAction}";

			var transaction = InitTrasaction(transactionName, ApiConstants.TypeRequest, true);
			SetCurrentSpan(transaction);
		}


		private void OnEndRequest(object eventSender, EventArgs eventArgs)
		{
			var currentSpan = GetCurrentSpan();
			SetCurrentSpan(null);
			currentSpan?.Dispose();
			SetCurrentTransaction(null);
		}

	}
}
