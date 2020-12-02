﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Elastic.Apm;
using Elastic.Apm.Api;
using Elastic.Apm.AspNetFullFramework.Extensions;
using Elastic.Apm.Helpers;
using Elastic.Apm.Model;
using Elastic.Apm.Report;
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
			if (Disposed)
				return;
		}

		public ExecutionSegment GetCurrentTransaction() => HttpContext.Current.Items["__APM_TRANSACTION"] as ExecutionSegment;
		private void SetCurrentTransaction(ExecutionSegment trace) => HttpContext.Current.Items["__APM_TRANSACTION"] = trace;
		private void SetCurrentSpan(AutoFinishingSpan transaction) => HttpContext.Current.Items["__APM_TRANSACTION_SPAN"] = transaction;
		private AutoFinishingSpan GetCurrentSpan() => HttpContext.Current.Items["__APM_TRANSACTION_SPAN"] as AutoFinishingSpan;


		public AutoFinishingSpan InitTrasaction(string name, string type, bool getSpan = false)
		{
			if (!IsEnabled)
				return new AutoFinishingSpan(null);
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
			var trans = GetCurrentTransaction();
			if (trans?.CurrentTransaction != null)
			{
				foreach (var pair in trans.CurrentTransaction.Labels)
				{
					//customvalues permite objetos, no queremos sobreescribirlos con el nombre de un tipo...
					if (!CustomValues.ContainsKey(pair.Key))
						CustomValues[pair.Key] = pair.Value;
				}
			}

			if (logInfo != null)
				foreach (var pair in logInfo)
				{
					CustomValues[pair.Key] = pair.Value;
				}

			if (!IsEnabled)
				return;
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
				logInfo: CustomValues);

			(Agent.Instance.PayloadSender as LocalPayloadSenderV2)?.EnqueueEvent(errorLog, "log");
		}

		public void LogExceptionToApm(Exception ex, string name, string transactionId = null, string host = null, string appName = null)
		{
			if (!IsEnabled)
				return;
			var culprit = appName ?? ApplicationName;

			var transaction = GetCurrentTransaction();
			if (transaction != null)
			{
				transaction.CurrentTransaction.CaptureException(ex, culprit);
			}
			else
				using (var span = InitTrasaction(name, "exception", true))
				{
					span.CaptureException(ex, culprit);
				}
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
			catch (Exception)
			{
				Console.WriteLine("Processing BeginRequest event failed");
				//_logger.Error()?.LogException(ex, "Processing BeginRequest event failed");
			}
		}

		private void ProcessBeginRequest(object eventSender)
		{
			CustomValues = new Dictionary<string, object>();
			var httpApp = (HttpApplication)eventSender;
			var httpRequest = httpApp.Context.Request;

			var transactionName = $"{httpRequest.HttpMethod} {httpRequest.Path}";

			var soapAction = httpRequest.ExtractSoapAction(null);
			if (soapAction != null)
				transactionName = $" {soapAction}";

			var transaction = InitTrasaction(transactionName, ApiConstants.TypeRequest, true);
			SetCurrentSpan(transaction);
		}


		private void OnEndRequest(object eventSender, EventArgs eventArgs)
		{
			ProcessEndRequest(eventSender);

			var currentSpan = GetCurrentSpan();
			SetCurrentSpan(null);
			currentSpan?.Dispose();
			SetCurrentTransaction(null);
		}

		private Dictionary<string, object> CustomValues { get; set; } = new Dictionary<string, object>();

		public void AddCustomData(string key, object value)
		{
			var transaction = GetCurrentTransaction();
			if (transaction != null)
			{
				transaction.CurrentTransaction.Labels[key] = value.ToString();
			}

			if (CustomValues == null)
				CustomValues = new Dictionary<string, object>();
			CustomValues[key] = value;
		}

		private void ProcessEndRequest(object eventSender)
		{
			var httpApp = (HttpApplication)eventSender;
			var httpCtx = httpApp.Context;
			var httpResponse = httpCtx.Response;

			var _currentTransaction = GetCurrentTransaction()?.CurrentTransaction;

			if (_currentTransaction == null)
				return;

			SendErrorEventIfPresent(httpCtx);

			_currentTransaction.Result = Transaction.StatusCodeToResult("HTTP", httpResponse.StatusCode);

			if (_currentTransaction.IsSampled)
			{
				FillSampledTransactionContextResponse(httpResponse, _currentTransaction);
				FillSampledTransactionContextUser(httpCtx, _currentTransaction);
			}

			_currentTransaction.End();
		}

		private void SendErrorEventIfPresent(HttpContext httpCtx)
		{
			var lastError = httpCtx.Server.GetLastError();
			var _currentTransaction = GetCurrentTransaction().CurrentTransaction;
			if (lastError != null)
				_currentTransaction.CaptureException(lastError);
		}

		private static void FillSampledTransactionContextResponse(HttpResponse httpResponse, ITransaction transaction) =>
			transaction.Context.Response = new Response
			{
				Finished = true,
				StatusCode = httpResponse.StatusCode,
				//Headers = _isCaptureHeadersEnabled ? ConvertHeaders(httpResponse.Headers) : null
			};

		private void FillSampledTransactionContextUser(HttpContext httpCtx, ITransaction transaction)
		{
			var userIdentity = httpCtx.User?.Identity;
			if (userIdentity == null || !userIdentity.IsAuthenticated)
				return;

			transaction.Context.User = new User { UserName = userIdentity.Name };

			//_logger.Debug()?.Log("Captured user - {CapturedUser}", transaction.Context.User);
		}
	}
}