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
			public Dictionary<string, string> LogInfo { get; set; }
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
	public class TransactionDto
	{
		public JToken Transaction { get; set; }

		public TransactionDtoInternal TransactionInfo { get; set; }


		public class TransactionDtoInternal
		{
			//"context":{},
			public decimal duration { get; set; }             //                373.395,
			public string id { get; set; }                    //  "d562fc8482b4b641",
			public bool sampled { get; set; }                 //        true,
			public string name { get; set; }                  //      " GetPendingOutputMessages",
															  // public span_count			  {get;set;}	  //
			public DateTime Timestamp { get; set; }           //                    1581631012472065,
			public string Trace_id { get; set; }              //              "76d01eaf4700f78dbf06c68b035f9f27",
			public string Type { get; set; }                    //     "request"}
			public string Result { get; set; }                    //     "request"}
			public ContextDto Context { get; set; }
		}

		public class ContextDto
		{
			public Dictionary<string, string> Tags { get; set; }
		}
	}
	public class SpanDto
	{
		public JToken Span { get; set; }

		public TransactionDtoInternal TransactionInfo { get; set; }

		public class TransactionDtoInternal
		{
			//"context":{},
			public decimal duration { get; set; }             //                373.395,
			public string id { get; set; }                    //  "d562fc8482b4b641",
			public bool sampled { get; set; }                 //        true,
			public string name { get; set; }                  //      " GetPendingOutputMessages",
															  // public span_count			  {get;set;}	  //
			public DateTime Timestamp { get; set; }           //                    1581631012472065,
			public string Transaction_id { get; set; }              //              "76d01eaf4700f78dbf06c68b035f9f27",
			public string Parent_id { get; set; }              //              "76d01eaf4700f78dbf06c68b035f9f27",
			public string Trace_id { get; set; }              //              "76d01eaf4700f78dbf06c68b035f9f27",
			public string Type { get; set; }                    //     "request"}
		}
	}
}
