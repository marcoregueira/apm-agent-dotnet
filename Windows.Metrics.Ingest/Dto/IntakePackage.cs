using System;
using System.Collections.Generic;
using Elastic.Apm.Api;
using Newtonsoft.Json.Linq;
using static Elastic.Apm.Api.Service;

namespace Windows.Metrics.Ingest.Dto
{
	public class MetaDataDto
	{
		public MetadataInternal Metadata { get; set; }
		public class MetadataInternal
		{
			public Service Service { get; set; }
			public Framework Framework { get; set; }
			public AgentC Agent { get; set; }
			public Elastic.Apm.Api.System System { get; set; }
		}
	}

	public class MetricsetDto
	{
		public MetricsetInternal Metricset { get; set; }
		public class MetricsetInternal
		{
			public Dictionary<string, Sample> Samples { get; set; }
			public DateTime Timestamp { get; set; }
		}
	}

	public class LogDto
	{
		public JToken LogInfo { get; set; }

		public LogDtoInternal Log { get; set; } 

		public class LogDtoInternal
		{
			public string Id { get; set; }
			public string Culprit { get; set; }
			public string TraceId { get; set; }
			public string ParentId { get; set; }
			public string Transaction_Id { get; set; }
			public DateTime Timestamp { get; set; }
			public string Level { get; set; }
			public string Message { get; set; }
		}
	}

	public class ErrorsetDto
	{
		public JToken Error { get; set; }
		public class ErrorDtoInternal
		{

			public string Id { get; set; }
			public string Culprit { get; set; }
			public string TraceId { get; set; }
			public string ParentId { get; set; }
			public string Transaction_Id { get; set; }
			public DateTime Timestamp { get; set; }

		}
	}

	public class Sample
	{
		public decimal Value { get; set; }
	}
}
