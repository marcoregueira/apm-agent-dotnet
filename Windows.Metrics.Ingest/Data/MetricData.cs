using System;

namespace Windows.Metrics.Ingest.Data
{
	public class MetricData
	{
		public DateTime Time { get; set; }
		public string Host { get; set; }
		public string Metrics { get; set; }
	}

	public class ErrorData
	{
		public DateTime Time { get; set; }
		public string Host { get; set; }
		public string ErrorInfo { get; set; }
		public string ErrorId { get; set; }
		public string ParentId { get; set; }
		public string TransactionId { get; set; }
		public string App { get; set; }
	}

	public class LogData
	{
		public DateTime Time { get; set; }
		public string Host { get; set; }
		public string Level { get; set; }
		public string Message { get; set; }
		public string LogId { get; set; }
		public string ParentId { get; set; }
		public string TransactionId { get; set; }
		public string App { get; set; }
		public object LogInfo { get; set; }
	}
}
