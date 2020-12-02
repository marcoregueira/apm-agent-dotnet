﻿namespace Windows.Metrics.Ingest.Dto
{
	public class ClientInfoResponse : ClientInfoRequest
	{
		public bool LogSqlEnabled { get; set; }
		public bool MetricsEnabled { get; set; }
		public bool TraceEnabled { get; set; }
		public int MinSqlDuration { get; set; }
		public string LogLevel { get; set; } = "Debug";
	}
}