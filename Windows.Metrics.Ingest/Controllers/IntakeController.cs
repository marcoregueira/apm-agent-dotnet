using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Windows.Metrics.Ingest.Data;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Metrics.Ingest.Controllers
{
	[ApiController]
	public class IntakeController : ControllerBase
	{
		private readonly ILogger<IntakeController> _logger;
		public MetricCrud Crud { get; }

		public IntakeController(ILogger<IntakeController> logger, MetricCrud crud)
		{
			_logger = logger;
			Crud = crud;
		}



		[HttpPost("/{index}/_doc")]
		[HttpPost("/{index}/_doc/id")]
		[HttpPost("/{index}/_create")]
		[HttpPost("/{index}/_create/{id}")]
		public async Task<IActionResult> PostDocument([FromRoute]string index, [FromRoute] string id)
		{
			var reader = new StreamReader(HttpContext.Request.Body);
			var package = await reader.ReadToEndAsync() ?? "";
			if (!package.StartsWith("{")) return BadRequest();
			return Ok();
		}

		[HttpPost("/intake/v2/events")]
		[Consumes("application/x-ndjson")]
		public async Task<IActionResult> PostAsync()
		{
			var reader = new StreamReader(HttpContext.Request.Body);
			var package = await reader.ReadToEndAsync() ?? "";
			if (!package.StartsWith("{")) return BadRequest();

			MetaDataDto metadata = null;
			var time = DateTime.Now;

			foreach (var part in package.ReadLines())
			{
				if (part.StartsWith("{\"metadata\":"))
				{
					metadata = JsonConvert.DeserializeObject<MetaDataDto>(part);
					continue;
				}

				if (metadata == null)
				{
					Console.WriteLine("no hay metadatos");
					continue;
				}

				if (part.StartsWith("{\"metricset\":"))
				{
					var metricset = JsonConvert.DeserializeObject<MetricsetDto>(part, new ApmDateTimeConverter());
					var metricValues = metricset.Metricset.Samples.Select(x => new { x.Key, x.Value.Value }).ToDictionary(x => x.Key, x => x.Value);
					time = metricset.Metricset.Timestamp;

					var dataDb = new MetricData()
					{
						Time = time,
						Host = metadata?.Metadata?.System.HostName,
						Metrics = JsonConvert.SerializeObject(metricValues)
					};
					Crud.Insert(dataDb);
					continue;
				}
				else
				if (part.StartsWith("{\"error\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<ErrorsetDto>(part, new ApmDateTimeConverter());
					var errorDetailsJson = errorSet.Error.ToString();
					var errorInfo = JsonConvert.DeserializeObject<ErrorsetDto.ErrorDtoInternal>(errorDetailsJson, new ApmDateTimeConverter());
					time = errorInfo.Timestamp;
					var dataDb = new ErrorData()
					{
						Time = time,
						Host = metadata?.Metadata?.System.HostName,
						ErrorInfo = errorSet.Error.ToString(),
						TransactionId = errorInfo.Transaction_Id,
						App = errorInfo.Culprit,
						ParentId = errorInfo.ParentId,
						ErrorId = errorInfo.Id,
					};

					Crud.Insert(dataDb);
					continue;
				}
				else
				if (part.StartsWith("{\"log\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<LogDto>(part, new ApmDateTimeConverter());
					var errorDetailsJson = errorSet.LogInfo?.ToString();
					var errorInfo = errorSet.Log; //JsonConvert.DeserializeObject<LogDto.LogDtoInternal>(errorDetailsJson, new MyDateTimeConverter());
					time = errorInfo.Timestamp;

					var dataDb = new LogData()
					{
						Time = time,
						Host = metadata?.Metadata?.System.HostName,
						Message = errorInfo.Message,
						Level = errorInfo.Level,
						//ErrorInfo = errorSet.LogInfo.ToString(),
						TransactionId = errorInfo.Transaction_Id,
						App = errorInfo.Culprit ?? metadata?.Metadata?.Service?.Name,
						ParentId = errorInfo.ParentId,
						LogId = errorInfo.Id,
					};

					if (errorInfo.LogInfo != null)
					{
						if (errorInfo.LogInfo.ContainsKey("host"))
							dataDb.Host = errorInfo.LogInfo["host"];

						if (errorInfo.LogInfo.ContainsKey("user"))
							dataDb.User = errorInfo.LogInfo["user"];
					}

					Crud.Insert(dataDb);
					continue;
				}
				else
				if (part.StartsWith("{\"transaction\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<TransactionDto>(part, new ApmDateTimeConverter());
					var errorDetailsJson = errorSet.Transaction?.ToString();
					var transactionInfo = JsonConvert.DeserializeObject<TransactionDto.TransactionDtoInternal>(errorDetailsJson, new ApmDateTimeConverter());
					time = transactionInfo.Timestamp;
					var dataDb = new TransactionData()
					{
						Time = time,
						Host = metadata?.Metadata?.System.HostName,
						App = metadata?.Metadata?.Service?.Name,
						Type = transactionInfo.Type,
						Id = transactionInfo.Trace_id,
						TransactionId = transactionInfo.Trace_id,
						Duration = transactionInfo.duration,
						ParentId = null,
						Data = JsonConvert.SerializeObject(errorSet.Transaction)
					};

					Crud.Insert(dataDb);
					continue;
				}
				else
				if (part.StartsWith("{\"span\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<SpanDto>(part, new ApmDateTimeConverter());
					var errorDetailsJson = errorSet.Span?.ToString();
					var spanInfo = JsonConvert.DeserializeObject<SpanDto.TransactionDtoInternal>(errorDetailsJson, new ApmDateTimeConverter());
					time = spanInfo.Timestamp;
					var dataDb = new TransactionData()
					{
						Time = time,
						Host = metadata?.Metadata?.System.HostName,
						App = metadata?.Metadata?.Service?.Name,
						Type = spanInfo.Type,
						Id = spanInfo.Trace_id,
						TransactionId = spanInfo.Trace_id,
						Duration = spanInfo.duration,
						ParentId = spanInfo.Parent_id,
						Data = JsonConvert.SerializeObject(errorSet.Span)
					};

					Crud.Insert(dataDb);
					continue;
				}
				else
				{
					Console.WriteLine(part);
				}
			}

			return Ok();
		}

		[Route("/")]
		public IActionResult Root() => Ok(
			   new
			   {
				   name = "localhost",
				   cluster_name = "elasticsearch",
				   cluster_uuid = "DoZwos0YR26WsHSZYO4O2A",
				   version = new
				   {
					   number = "7.2.0",
					   build_flavor = "default",
					   build_type = "tar",
					   build_hash = "508c38a",
					   build_date = "2019-06-20T15:54:18.811730Z",
					   build_snapshot = false,
					   lucene_version = "8.0.0",
					   minimum_wire_compatibility_version = "6.8.0",
					   minimum_index_compatibility_version = "6.0.0-beta1"
				   },
				   tagline = "You Know, for Search"
			   });

		[Route("/_xpack")]
		public IActionResult Xpack() => BadRequest();

		[Route("/_template")]
		[Route("/_template/{tmpl}")]
		public IActionResult Template() => Ok(new { });

		[Route("/_bulk")]
		[Route("/{index}/_bulk")]
		public IActionResult Bulk()
		{
			var reader = new StreamReader(HttpContext.Request.Body);
			var package = reader.ReadToEndAsync().Result ?? "";
			return Ok(new { });
		}

	}

	public class ApmDateTimeConverter : Newtonsoft.Json.JsonConverter
	{
		private static readonly DateTime BaseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		public override bool CanConvert(Type objectType) => objectType == typeof(DateTime);

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var t = (long)reader.Value;
			return BaseDate.AddMilliseconds(t / 1000);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new NotImplementedException();
	}


}
