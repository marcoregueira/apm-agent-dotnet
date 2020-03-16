using System;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Windows.Metrics.Ingest.Dto;

namespace Windows.Metrics.Ingest.Data
{
	public class ConfigCrud
	{
		public IConfiguration Configuration { get; }

		private readonly string _connectionString;

		public ConfigCrud(IConfiguration configuration)
		{
			Configuration = configuration;
			_connectionString = Configuration.GetConnectionString("default");
		}

		internal ClientInfoResponse GetConfig(ClientInfoRequest request)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();
				var config = connection.Query<ClientInfoResponse>(
					@"SELECT * FROM client_config
					  WHERE client= @client and app= @app and @app > ''", request)
					.FirstOrDefault();

				if (config == null)
				{
					config = new ClientInfoResponse() { Client = request.Client, App = request.App };
					InsertConfig(config);
				}
				return config;
			}
		}

		private void InsertConfig(ClientInfoResponse config)
		{
			/*

				CREATE TABLE public.errors
				(
				  "time" timestamp without time zone,
				  host text,
				  app text,
				  errorid text,
				  transactionid text,
				  data jsonb
				)
				WITH (
				  OIDS=FALSE
				);
				ALTER TABLE public.errors
				  OWNER TO postgres;

			 */

			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();
				connection.Execute(@"
					Insert into public.client_config
					(client, app, logsqlenabled, metricsenabled, traceenabled)
					values
					(	@Client,
						@App,
						@LogSqlEnabled,
						@MetricsEnabled,
						@TraceEnabled
					)", config);
			}
		}
	}
}
