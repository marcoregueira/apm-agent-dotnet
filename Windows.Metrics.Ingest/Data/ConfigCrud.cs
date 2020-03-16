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

		public void Insert(ClientInfoRequest data)
		{
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();
				connection.Execute("Insert into public.metrics (time, host, data) values (@time, @host, CAST(@metrics AS jsonb));", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
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

		private void InsertConfig(ClientInfoResponse config) => throw new NotImplementedException();

		public void Insert(LogData data)
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
					Insert into public.log
					(time, host, data, transactionid,
					   database, remotehost, duration,
					   logid, app, level, message, userid)
					values
					(	@time,
						@host,
						CAST(@logInfo AS jsonb),
						@transactionid,
						@database,
						@remotehost,
						@duration,
						@logid,
						@app,
						@level,
						@message,
						@user);", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}
	}
}
