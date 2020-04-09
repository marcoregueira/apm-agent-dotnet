using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Windows.Metrics.Ingest.Data
{
	public class MetricCrud
	{
		public IConfiguration Configuration { get; }

		private readonly string _connectionString;

		public MetricCrud(IConfiguration configuration)
		{
			Configuration = configuration;
			_connectionString = Configuration.GetConnectionString("default");
		}

		public void Insert(MetricData data)
		{
			/*
				-- Table: public.metrics
				-- DROP TABLE public.metrics;
				CREATE TABLE public.metrics
				(
				"timestamp" timestamp without time zone,
				host text,
				data jsonb
				)
				WITH (
				OIDS=FALSE
				);
				ALTER TABLE public.metrics
				OWNER TO postgres;
			 */


			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();
				connection.Execute("Insert into public.metrics (time, host, data) values (@time, @host, CAST(@metrics AS jsonb));", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}

		public void Insert(TransactionData transaction)
		{
			/*
					CREATE TABLE public.transaction
					(
					  "time" timestamp without time zone,
					  host text,
					  app text,
					  type text,
					  id text,
					  transactionid text,
					  parentid text,
					  data jsonb
					)
					WITH (
					  OIDS=FALSE
					);

			 */
			using (var connection = new NpgsqlConnection(_connectionString))
			{
				connection.Open();
				connection.Execute(@"
						Insert into public.transaction (time, host, app, type, id, transactionid, parentid, duration, data, userid, result, name)
						values (@time, @host, @app, @type, @id, @transactionid, @parentid, @duration, CAST(@data AS jsonb), @user, @result, @name);", transaction);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}

		public void Insert(ErrorData data)
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
					Insert into public.errors
					(time, host, data, transactionid, errorid, app)
					values
					(@time, @host, CAST(@errorInfo AS jsonb),  @transactionid, @errorid, @app);", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}

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
