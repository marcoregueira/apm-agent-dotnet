using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Windows.Metrics.Ingest.Ef
{
	public class BaseTable
	{
		[Key()]
		public long LineId { get; set; }
	}

	[Table("client_config")]
	public class ClientConfig
	{
		[Column("client")]
		public string Client { get; set; }

		[Column("app")]
		public string App { get; set; }

		[Column("logsqlenabled")]
		public bool LogSqlEnabled { get; set; }

		[Column("metricsenabled")]
		public bool MetricsEnabled { get; set; }

		[Column("traceenabled")]
		public bool TraceEnabled { get; set; }

		[Column("loglevel")]
		public string LogLevel { get; set; } = "Debug";
	}

	[Table("errors")]
	public class ErrorEntity : BaseTable
	{
		[Column("time", TypeName = "timestamp")]
		public DateTime Time { get; set; }

		[Column("host")]
		public string Host { get; set; }

		[Column("app")]
		public string App { get; set; }

		[Column("errorid")]
		public string ErrorId { get; set; }

		[Column("transactionid")]
		public string TransactionId { get; set; }

		[Column("data", TypeName = "jsonb")]
		public string Data { get; set; }
	}

	[Table("metrics")]
	public class MetricEntity : BaseTable
	{
		[Column("time", TypeName = "timestamp")]
		public DateTime Time { get; set; }

		[Column("host")]
		public string Host { get; set; }

		[Column("data", TypeName = "jsonb")]
		public string Data { get; set; }
	}

	[Table("log")]
	public class LogEntity : BaseTable
	{
		[Column("time", TypeName = "timestamp")]
		public DateTime Time { get; set; }

		[Column("host")]
		public string Host { get; set; }

		[Column("level")]
		public string Level { get; set; }

		[Column("database")]
		public string Database { get; set; }

		[Column("remotehost")]
		public string RemoteHost { get; set; }

		[Column("userid")]
		public string UserId { get; set; }

		[Column("duration")]
		public decimal Duration { get; set; }

		[Column("logId")]
		public string LogId { get; set; }

		[Column("message")]
		public string Message { get; set; }

		[Column("transactionid")]
		public string TransactionId { get; set; }

		[Column("data", TypeName = "jsonb")]
		public string Data { get; set; }
	}

	[Table("transaction")]
	public class TransactionEntity : BaseTable
	{
		[Column("time", TypeName = "timestamp")]
		public DateTime Time { get; set; }

		[Column("host")]
		public string Host { get; set; }

		[Column("name")]
		public string Name { get; set; }

		[Column("result")]
		public string Result { get; set; }

		[Column("type")]
		public string TransactionType { get; set; }

		[Column("database")]
		public string Database { get; set; }

		[Column("remotehost")]
		public string RemoteHost { get; set; }

		[Column("userid")]
		public string UserId { get; set; }

		[Column("duration")]
		public decimal Duration { get; set; }

		[Column("id")]
		public string Id { get; set; }

		[Column("parentId")]
		public string ParentId { get; set; }

		[Column("transactionId")]
		public string TransactionId { get; set; }

		[Column("data", TypeName = "jsonb")]
		public string Data { get; set; }
	}


}
