using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Windows.Metrics.Ingest.Data;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Metrics.Ingest.Controllers
{
	[ApiController]
	[Route("[controller]/v2")]
	public class IntakeController : ControllerBase
	{
		private readonly ILogger<IntakeController> _logger;
		public MetricCrud Crud { get; }

		public IntakeController(ILogger<IntakeController> logger, MetricCrud crud)
		{
			_logger = logger;
			Crud = crud;
		}


		[HttpPost("events")]
		//[Consumes("application/x-ndjson")]
		public IActionResult Post([FromBody]string package)
		{
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
					var metricset = JsonConvert.DeserializeObject<MetricsetDto>(part, new MyDateTimeConverter());
					var metricValues = metricset.Metricset.Samples.Select(x => new { x.Key, x.Value.Value }).ToDictionary(x => x.Key, x => x.Value);
					time = metricset.Metricset.Timestamp;

					var dataDb = new MetricData() { Time = time, Host = metadata?.Metadata?.System.HostName, Metrics = JsonConvert.SerializeObject(metricValues) };
					Crud.Insert(dataDb);
					continue;
				}

				if (part.StartsWith("{\"error\":"))
				{

					var errorSet = JsonConvert.DeserializeObject<ErrorsetDto>(part, new MyDateTimeConverter());
					var errorDetailsJson = errorSet.Error.ToString();
					var errorInfo = JsonConvert.DeserializeObject<ErrorsetDto.ErrorDtoInternal>(errorDetailsJson, new MyDateTimeConverter());
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

				if (part.StartsWith("{\"log\":"))
				{
					var errorSet = JsonConvert.DeserializeObject<LogDto>(part, new MyDateTimeConverter());
					var errorDetailsJson = errorSet.LogInfo?.ToString();
					var errorInfo = errorSet.Log; //JsonConvert.DeserializeObject<LogDto.LogDtoInternal>(errorDetailsJson, new MyDateTimeConverter());
					var dataDb = new LogData()
					{
						Time = time,
						Host = metadata?.Metadata?.System.HostName,
						Message = errorInfo.Message,
						Level = errorInfo.Level,
						//ErrorInfo = errorSet.LogInfo.ToString(),
						TransactionId = errorInfo.Transaction_Id,
						App = metadata?.Metadata?.Service?.Name,
						ParentId = errorInfo.ParentId,
						LogId = errorInfo.Id
					};

					Crud.Insert(dataDb);
					continue;
				}
			}

			return Ok();
		}
	}

	public class MyDateTimeConverter : Newtonsoft.Json.JsonConverter
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
