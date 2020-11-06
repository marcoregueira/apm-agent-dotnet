using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Windows.Metrics.Ingest.Migrations
{
	internal static class CustomMigration
	{
		/// <summary>
		/// In this migration we enable TimescaleDb if available
		/// This script won't work if there is data at the tables
		/// We don't provide a Down procedure
		/// 
		/// Most likely, future updates won't work if tables have any data
		/// </summary>
		/// <param name="migrationBuilder"></param>
		internal static void EnableTimescaleDbIfAvailable(MigrationBuilder migrationBuilder)
		{
			// All hypertables are configured using defaults. That is, all chuks are 7 days long.
			// It could be necessary to make them smaller, for instance 1 day.

			try
			{
				migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS timescaledb CASCADE;", suppressTransaction: true);
			}
			catch
			{
				Console.WriteLine("::: -->TimescaleDb not enabled");
				return;
			}

			migrationBuilder.Sql(@"
                ALTER TABLE public.errors DROP CONSTRAINT ""PK_errors"" ;
                ALTER TABLE public.errors ADD CONSTRAINT  ""PK_errors"" PRIMARY KEY(""LineId"", ""Time"");
                SELECT create_hypertable('errors', 'Time', chunk_time_interval => INTERVAL '1 day');

                ALTER TABLE public.metrics DROP CONSTRAINT ""PK_metrics"" ;
                ALTER TABLE public.metrics ADD CONSTRAINT  ""PK_metrics"" PRIMARY KEY(""LineId"", ""Time"");
                SELECT create_hypertable('metrics', 'Time', chunk_time_interval => INTERVAL '1 day');

                ALTER TABLE public.transaction DROP CONSTRAINT ""PK_transaction"" ;
                ALTER TABLE public.transaction ADD CONSTRAINT  ""PK_transaction"" PRIMARY KEY(""LineId"", ""Time"");
                SELECT create_hypertable('transaction', 'Time', chunk_time_interval => INTERVAL '1 day');

                ALTER TABLE public.log DROP CONSTRAINT ""PK_log"" ;
                ALTER TABLE public.log ADD CONSTRAINT  ""PK_log"" PRIMARY KEY(""LineId"", ""Time"");
                SELECT create_hypertable('log', 'Time', chunk_time_interval => INTERVAL '1 day');

            ", suppressTransaction: true);
		}

	}
}
