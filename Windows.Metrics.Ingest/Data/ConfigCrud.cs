using System.Linq;
using Microsoft.Extensions.Configuration;
using Windows.Metrics.Ingest.Dto;
using Windows.Metrics.Ingest.Ef;

namespace Windows.Metrics.Ingest.Data
{
	public class ConfigCrud
	{
		public IConfiguration Configuration { get; }

		private readonly string _connectionString;
		private readonly BaseContext context;

		public ConfigCrud(IConfiguration configuration, BaseContext context)
		{
			Configuration = configuration;
			this.context = context;
			_connectionString = Configuration.GetConnectionString("default");
		}

		internal ClientInfoResponse GetConfig(ClientInfoRequest request)
		{
			var config = context.Config
				.Where(x => x.Client == request.Client && x.App == request.App)
				.FirstOrDefault();

			if (config == null)
			{
				config = new ClientConfig() { Client = request.Client, App = request.App };
				context.Add(config);
			}

			var response = new ClientInfoResponse()
			{
				App = config.App,
				Client = config.Client,
				MetricsEnabled = config.MetricsEnabled,
				LogSqlEnabled = config.LogSqlEnabled,
				LogLevel = config.LogLevel,
				TraceEnabled = config.TraceEnabled
			};

			return response;
		}
	}
}
