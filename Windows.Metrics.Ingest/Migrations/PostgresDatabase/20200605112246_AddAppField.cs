using Microsoft.EntityFrameworkCore.Migrations;

namespace Windows.Metrics.Ingest.Migrations.PostgresDatabase
{
    public partial class AddAppField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "app",
                table: "log",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "app",
                table: "log");
        }
    }
}
