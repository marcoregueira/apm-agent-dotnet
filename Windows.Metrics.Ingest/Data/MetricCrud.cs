using Dapper;
using Npgsql;

namespace Windows.Metrics.Ingest.Data
{
	public static class MetricCrud
	{
		public static void Insert(MetricData data)
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

			using (var connection = new NpgsqlConnection("Host=vmware.localhost.com;Username=postgres;Password=listas;Database=metrics"))
			{
				connection.Open();
				connection.Execute("Insert into public.metrics (time, host, data) values (@time, @host, CAST(@metrics AS json));", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}

		public static void Insert(ErrorData data)
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

			using (var connection = new NpgsqlConnection("Host=vmware.localhost.com;Username=postgres;Password=listas;Database=metrics"))
			{
				connection.Open();
				connection.Execute(@"
					Insert into public.errors
					(time, host, data, transactionid, errorid, app)
					values
					(@time, @host, CAST(@errorInfo AS json),  @transactionid, @errorid, @app);", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}

		public static void Insert(LogData data)
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

			using (var connection = new NpgsqlConnection("Host=vmware.localhost.com;Username=postgres;Password=listas;Database=metrics"))
			{
				connection.Open();
				connection.Execute(@"
					Insert into public.log
					(time, host, data, transactionid, logid, app, level, message)
					values
					(@time, @host, CAST(@logInfo AS json),  @transactionid, @logid, @app, @level,  @message);", data);
				//var value = connection.Query<string>("Select data ->> 'first_name' from Employee;");
				//Console.WriteLine(value.First());
			}
		}
	}
}
