using Microsoft.EntityFrameworkCore.Migrations;

namespace Windows.Metrics.Ingest.Migrations.PostgresDatabase
{
    public partial class EnableTimescale : Migration
    {
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			CustomMigration.EnableTimescaleDbIfAvailable(migrationBuilder);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// No way. If Timescaledb is enabled, drop the database and start again
		}
	}
}
