using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Windows.Metrics.Ingest.Data;
using Windows.Metrics.Ingest.Dto;
using Windows.Metrics.Ingest.Ef;

namespace Windows.Metrics.Ingest.Controllers
{
	public class ConfigurationController : Controller
	{
		private readonly BaseContext context;
		private readonly ILogger<ConfigurationController> _logger;
		private ConfigCrud _crud { get; }

		public ConfigurationController(
			BaseContext context,
			ILogger<ConfigurationController> logger,
			ConfigCrud crud)
		{
			this.context = context;
			_logger = logger;
			_crud = crud;
		}

		[HttpPost("/station/configuration")]
		public async Task<IActionResult> PostDocument([FromBody]ClientInfoRequest request)
		{
			var config = _crud.GetConfig(request);
			await context.SaveChangesAsync();
			return Ok(config);
		}
	}
}
