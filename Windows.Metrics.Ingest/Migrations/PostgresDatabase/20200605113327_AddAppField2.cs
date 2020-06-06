using Microsoft.EntityFrameworkCore.Migrations;

namespace Windows.Metrics.Ingest.Migrations.PostgresDatabase
{
    public partial class AddAppField2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "app",
                table: "transaction",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "app",
                table: "transaction");
        }
    }
}
