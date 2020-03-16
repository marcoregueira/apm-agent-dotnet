using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Windows.Metrics.Ingest.Data;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Metrics.Ingest.Controllers
{
	public class ConfigurationController : Controller
	{
		private readonly ILogger<ConfigurationController> _logger;
		private ConfigCrud _crud { get; }

		public ConfigurationController(ILogger<ConfigurationController> logger, ConfigCrud crud)
		{
			_logger = logger;
			_crud = crud;
		}

		[HttpPost("/station/configuration")]
		public async Task<IActionResult> PostDocument([FromBody]ClientInfoRequest request)
		{
			var config = _crud.GetConfig(request);
			return Ok(config);
		}
	}
}
