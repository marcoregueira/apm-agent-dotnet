using System;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Windows.Metrics.Ingest.Ef;

namespace Windows.Metrics.Ingest.Data
{
	public class MetricCrud
	{
		public IConfiguration Configuration { get; }

		private readonly string _connectionString;
		private readonly BaseContext context;

		public MetricCrud(IConfiguration configuration, BaseContext context)
		{
			Configuration = configuration;
			this.context = context;
			_connectionString = Configuration.GetConnectionString("default");
		}

		public void Insert(MetricData data)
		{
			var metric = new MetricEntity()
			{
				Data = data.Metrics,
				Host = data.Host,
				Time = data.Time
			};

			context.Add(metric);
		}

		public void Insert(TransactionData transaction)
		{
			var transactionEntity = new TransactionEntity()
			{
				Name = transaction.Name,
				ParentId = transaction.ParentId,
				Time = transaction.Time,
				Duration = transaction.Duration,
				Host = transaction.Host,
				Database = transaction.Database,
				RemoteHost = transaction.RemoteHost,
				Result = transaction.Result,
				TransactionId = transaction.TransactionId,
				TransactionType = transaction.Type,
				Id = transaction.Id,
				Data = transaction.Data,
				UserId = transaction.User,
				App = transaction.App
			};
			context.Add(transactionEntity);
		}

		public void Insert(ErrorData data)
		{
			var errorEntity = new ErrorEntity()
			{
				App = data.App,
				Host = data.Host,
				Time = data.Time,
				ErrorId = data.ErrorId,
				Data = data.ErrorInfo,
				TransactionId = data.TransactionId
			};

			context.Add(errorEntity);
		}

		public void Insert(LogData data)
		{
			var logEntity = new LogEntity()
			{
				Data = data.LogInfo,
				Database = data.Database,
				Duration = data.Duration ?? 0,
				Host = data.Host,
				App = data.App,
				Level = data.Level,
				LogId = string.IsNullOrWhiteSpace(data.LogId) ? Guid.NewGuid().ToString().Replace("-", "") : data.TransactionId,
				Message = data.Message,
				Time = data.Time,
				RemoteHost = string.IsNullOrWhiteSpace(data.RemoteHost) ? "(Vacío)" : data.RemoteHost,
				TransactionId = string.IsNullOrWhiteSpace(data.TransactionId) ? "(Vacío)" : data.TransactionId,
				UserId = data.User,
				ParentId = string.IsNullOrWhiteSpace(data.ParentId) ? "(Vacío)" : data.ParentId,
			};

			context.Add(logEntity);
		}
	}
}
