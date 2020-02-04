using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Apm.Api;
using Elastic.Apm.Helpers;
using Elastic.Apm.Logging;
using Elastic.Apm.Model;
using Elastic.Apm.Report.Serialization;
using Newtonsoft.Json;

namespace Windows.Apm.Client.Nlog
{
	public class LogEntry
	{
		internal LogEntry(Transaction transaction, string parentId, IApmLogger loggerArg)
		{
			Timestamp = TimeUtils.TimestampNow();
			Id = RandomGenerator.GenerateRandomBytesAsString(new byte[16]);

			TraceId = transaction.TraceId;
			TransactionId = transaction.Id;
			ParentId = parentId;
			Transaction = new TransactionData(transaction.IsSampled, transaction.Type);

			if (transaction.IsSampled) Context = transaction.Context;

			IApmLogger logger = loggerArg?.Scoped($"{nameof(Error)}.{Id}");
			logger.Trace()
				?.Log("New Error instance created: {Error}. Time: {Time} (as timestamp: {Timestamp})",
					this, TimeUtils.FormatTimestampForLog(Timestamp), Timestamp);
		}

		// This constructor is meant for serialization
		[JsonConstructor]
		internal LogEntry(string culprit, string id, string parentId, long timestamp, string traceId,
			string transactionId, TransactionData transaction, string level, string message, object logInfo
		)
		{
			Culprit = culprit;
			Id = id;
			ParentId = parentId;
			Timestamp = timestamp;
			TraceId = traceId;
			TransactionId = transactionId;
			Transaction = transaction;
			Level = level;
			Message = message;
			LogInfo = logInfo;
		}

		/// <summary>
		/// <seealso cref="ShouldSerializeContext" />
		/// </summary>
		public Context Context { get; set; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		public string Culprit { get; set; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		public string Message { get; set; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		public string Level { get; set; }

		public object LogInfo { get; set; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		public string Id { get; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		[JsonProperty("parent_id")]
		public string ParentId { get; set; }

		/// <summary>
		/// Recorded time of the event, UTC based and formatted as microseconds since Unix epoch
		/// </summary>
		public long Timestamp { get; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		[JsonProperty("trace_id")]
		public string TraceId { get; set; }

		public TransactionData Transaction { get; }

		[JsonConverter(typeof(TrimmedStringJsonConverter))]
		[JsonProperty("transaction_id")]
		public string TransactionId { get; set; }

		/// <summary>
		/// Method to conditionally serialize <see cref="Context" /> because context should be serialized only when the transaction
		/// is sampled.
		/// See
		/// <a href="https://www.newtonsoft.com/json/help/html/ConditionalProperties.htm">the relevant Json.NET Documentation</a>
		/// </summary>
		public bool ShouldSerializeContext() => Transaction?.IsSampled ?? false;

		public override string ToString() => new ToStringBuilder(nameof(Error))
		{
			{ nameof(Id), Id }, { nameof(TraceId), TraceId }, { nameof(ParentId), ParentId }, { nameof(TransactionId), TransactionId }
		}.ToString();

		public class TransactionData
		{
			[JsonConstructor]
			internal TransactionData(bool isSampled, string type)
			{
				IsSampled = isSampled;
				Type = type;
			}

			[JsonProperty("sampled")]
			public bool IsSampled { get; }

			[JsonConverter(typeof(TrimmedStringJsonConverter))]
			public string Type { get; }

			public override string ToString() =>
				new ToStringBuilder(nameof(TransactionData)) { { "IsSampled", IsSampled }, { "Type", Type } }.ToString();
		}
	}
}

