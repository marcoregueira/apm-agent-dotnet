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
				migrationBuilder.Sql(@"
						drop FUNCTION if exists public.enable_timescale; 
						CREATE FUNCTION enable_timescale() RETURNS text AS
						$$
						begin

							start transaction;
								CREATE EXTENSION IF NOT EXISTS timescaledb cascade;	
							commit;

							ALTER TABLE public.errors DROP CONSTRAINT ""PK_errors"" ;
							ALTER TABLE public.errors ADD CONSTRAINT  ""PK_errors"" PRIMARY KEY(""LineId"", ""time"");
							SELECT create_hypertable('errors', 'time', chunk_time_interval => INTERVAL '1 day');

							ALTER TABLE public.metrics DROP CONSTRAINT ""PK_metrics"" ;
							ALTER TABLE public.metrics ADD CONSTRAINT  ""PK_metrics"" PRIMARY KEY(""LineId"", ""time"");
							SELECT create_hypertable('metrics', 'time', chunk_time_interval => INTERVAL '1 day');

							ALTER TABLE public.transaction DROP CONSTRAINT ""PK_transaction"" ;
							ALTER TABLE public.transaction ADD CONSTRAINT  ""PK_transaction"" PRIMARY KEY(""LineId"", ""time"");
							SELECT create_hypertable('transaction', 'time', chunk_time_interval => INTERVAL '1 day');

							ALTER TABLE public.log DROP CONSTRAINT ""PK_log"" ;
							ALTER TABLE public.log ADD CONSTRAINT  ""PK_log"" PRIMARY KEY(""LineId"", ""time"");
							SELECT create_hypertable('log', 'time', chunk_time_interval => INTERVAL '1 day');

						    return 'SUCCESS';
						    EXCEPTION WHEN OTHERS then
								return 'ERROR ACTIVATING TIMESCALEDB EXTENSION';    
						END;
						$$
						LANGUAGE plpgsql;

						select * from enable_timescale ();
            ", suppressTransaction: true);
			}
			catch
			{
				Console.WriteLine("::: -->TimescaleDb not enabled");
				return;
			}

		}

	}
}
