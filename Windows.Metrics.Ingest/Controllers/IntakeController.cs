using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Windows.Metrics.Ingest.Data;
using Windows.Metrics.Ingest.Dto;
using Windows.Metrics.Ingest.Ef;

namespace Windows.Metrics.Ingest.Controllers
{
	[ApiController]
	public class IntakeController : Controller
	{
		private readonly BaseContext context;
		private readonly ILogger<IntakeController> _logger;
		private MetricCrud _crud { get; }

		public IntakeController(
			BaseContext context,
			ILogger<IntakeController> logger,
			MetricCrud crud)
		{
			this.context = context;
			_logger = logger;
			_crud = crud;
		}

		[HttpPost("/{index}/_doc")]
		[HttpPost("/{index}/_doc/id")]
		[HttpPost("/{index}/_create")]
		[HttpPost("/{index}/_create/{id}")]
		public async Task<IActionResult> PostDocument([FromRoute]string index, [FromRoute] string id)
		{
			var reader = new StreamReader(HttpContext.Request.Body);
			var package = await reader.ReadToEndAsync() ?? "";
			if (!package.StartsWith("{"))
				return BadRequest();
			return Ok();
		}

		[HttpPost("/intake/v2/events")]
		[Consumes("application/x-ndjson")]
		public async Task<IActionResult> PostAsync()
		{
			var reader = new StreamReader(HttpContext.Request.Body);
			var package = await reader.ReadToEndAsync() ?? "";
			if (!package.StartsWith("{"))
				return BadRequest();

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
					_crud.Insert(dataDb);
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
						//App = errorInfo.Culprit,
						App = metadata?.Metadata?.Service?.Name ?? errorInfo.Culprit,
						ParentId = errorInfo.ParentId,
						ErrorId = errorInfo.Id,
					};

					_crud.Insert(dataDb);
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
						App = metadata?.Metadata?.Service?.Name ?? errorInfo.Culprit,
						ParentId = errorInfo.ParentId,
						LogId = errorInfo.Id,
						LogInfo = JsonConvert.SerializeObject(errorInfo.LogInfo)
					};

					if (errorInfo.LogInfo != null)
					{
						if (errorInfo.LogInfo.ContainsKey("host") && errorInfo.LogInfo["host"] != null)
							dataDb.Host = errorInfo.LogInfo["host"];

						if (errorInfo.LogInfo.ContainsKey("userid") && errorInfo.LogInfo["userid"] != null)
							dataDb.User = errorInfo.LogInfo["userid"];

						if (errorInfo.LogInfo.ContainsKey("remotehost") && errorInfo.LogInfo["remotehost"] != null)
							dataDb.RemoteHost = errorInfo.LogInfo["remotehost"];

						if (errorInfo.LogInfo.ContainsKey("database") && errorInfo.LogInfo["database"] != null)
							dataDb.Database = errorInfo.LogInfo["database"];

						if (errorInfo.LogInfo.ContainsKey("app") && errorInfo.LogInfo["app"] != null)
							dataDb.App = errorInfo.LogInfo["app"];

						if (errorInfo.LogInfo.ContainsKey("transaction") && errorInfo.LogInfo["transaction"] != null)
							dataDb.TransactionId = errorInfo.LogInfo["transaction"];

						if (errorInfo.LogInfo.ContainsKey("duration") && errorInfo.LogInfo["duration"] != null)
						{
							var duration = errorInfo.LogInfo["duration"];
							if (decimal.TryParse(duration, NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
							{
								dataDb.Duration = ms;
							}
						}
					}

					if (string.IsNullOrEmpty(dataDb.User))
						dataDb.User = "(Vacío)";

					if (string.IsNullOrEmpty(dataDb.Database))
						dataDb.Database = "(Vacío)";

					dataDb.Duration = dataDb.Duration ?? 0;

					_crud.Insert(dataDb);
					continue;
				}
				else
				if (part.StartsWith("{\"transaction\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<TransactionDto>(part, new ApmDateTimeConverter());
					var errorDetailsJson = errorSet.Transaction?.ToString();
					var transactionInfo = JsonConvert.DeserializeObject<TransactionDto.TransactionDtoInternal>(errorDetailsJson, new ApmDateTimeConverter());
					time = transactionInfo.Timestamp;
					var tags = transactionInfo.Context?.Tags ?? new Dictionary<string, string>();
					var dataDb = new TransactionData()
					{
						Time = time,
						Host = tags.ContainsKey("host") ? tags["host"] : metadata?.Metadata?.System.HostName,
						User = tags.ContainsKey("user") ? tags["user"] : "(Vacío)",
						Database = tags.ContainsKey("database") ? tags["database"] : "(Vacío)",
						RemoteHost = tags.ContainsKey("remotehost") ? tags["remotehost"] : "(Vacío)",

						App = metadata?.Metadata?.Service?.Name,
						Type = string.IsNullOrWhiteSpace(transactionInfo.Type) ? "(Vacío)" : transactionInfo.Type,
						Id = transactionInfo.Trace_id,
						TransactionId = transactionInfo.id,
						Duration = Math.Round(transactionInfo.duration),
						Result = string.IsNullOrWhiteSpace(transactionInfo.Result) ? "(Vacío)" : transactionInfo.Result,
						Name = string.IsNullOrWhiteSpace(transactionInfo.name) ? "(Vacío)" : transactionInfo.name,
						ParentId = "",
						Data = JsonConvert.SerializeObject(errorSet.Transaction)
					};

					dataDb.App = string.IsNullOrWhiteSpace(dataDb.App) ? "(Vacío)" : dataDb.App;
					dataDb.User = string.IsNullOrWhiteSpace(dataDb.User) ? "(Vacío)" : dataDb.User;
					dataDb.RemoteHost = string.IsNullOrWhiteSpace(dataDb.RemoteHost) ? "(Vacío)" : dataDb.RemoteHost;

					_crud.Insert(dataDb);
					continue;
				}
				else
				if (part.StartsWith("{\"span\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<SpanDto>(part, new ApmDateTimeConverter());
					var errorDetailsJson = errorSet.Span?.ToString();
					var spanInfo = JsonConvert.DeserializeObject<SpanDto.TransactionDtoInternal>(errorDetailsJson, new ApmDateTimeConverter());
					time = spanInfo.Timestamp;
					var tags = spanInfo.Context?.Tags ?? new Dictionary<string, string>();

					var dataDb = new TransactionData()
					{
						Time = time,
						Host = tags.ContainsKey("host") ? tags["host"] : metadata?.Metadata?.System.HostName,
						User = tags.ContainsKey("user") ? tags["user"] : "(Vacío)",
						Database = tags.ContainsKey("database") ? tags["database"] : "(Vacío)",
						RemoteHost = tags.ContainsKey("remotehost") ? tags["remotehost"] : "(Vacío)",
						App = metadata?.Metadata?.Service?.Name,
						Type = string.IsNullOrWhiteSpace(spanInfo.Type) ? "(Vacío)" : spanInfo.Type,
						Id = spanInfo.Trace_id,
						TransactionId = spanInfo.Trace_id,
						Duration = Math.Round(spanInfo.duration),
						ParentId = spanInfo.Parent_id,
						Data = JsonConvert.SerializeObject(errorSet.Span)
					};

					dataDb.App = string.IsNullOrWhiteSpace(dataDb.App) ? "(Vacío)" : dataDb.App;
					dataDb.User= string.IsNullOrWhiteSpace(dataDb.User) ? "(Vacío)" : dataDb.User;
					dataDb.RemoteHost= string.IsNullOrWhiteSpace(dataDb.RemoteHost) ? "(Vacío)" : dataDb.RemoteHost;

					_crud.Insert(dataDb);
					continue;
				}
				else
				{
					Console.WriteLine(part);
				}
			}

			await context.SaveChangesAsync();
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
