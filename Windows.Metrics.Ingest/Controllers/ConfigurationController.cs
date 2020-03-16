using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Windows.Metrics.Ingest.Data;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Metrics.Ingest.Controllers
{
	public class ConfigurationController : ControllerBase
	{
		[HttpPost("/station/configuration")]
		public async Task<IActionResult> PostDocument([FromBody]ClientInfoRequest request, ConfigCrud crud)
		{
			crud.GetConfig(request);
			return Ok();
		}
	}
}
