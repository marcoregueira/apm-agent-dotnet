using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Windows.Metrics.Ingest.Migrations.PostgresDatabase
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "EntityFrameworkHiLoSequence",
                incrementBy: 10);

            migrationBuilder.CreateTable(
                name: "client_config",
                columns: table => new
                {
                    client = table.Column<string>(nullable: false),
                    app = table.Column<string>(nullable: false),
                    logsqlenabled = table.Column<bool>(nullable: false),
                    metricsenabled = table.Column<bool>(nullable: false),
                    traceenabled = table.Column<bool>(nullable: false),
                    loglevel = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_client_config", x => new { x.app, x.client });
                });

            migrationBuilder.CreateTable(
                name: "errors",
                columns: table => new
                {
                    LineId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    time = table.Column<DateTime>(type: "timestamp", nullable: false),
                    host = table.Column<string>(nullable: true),
                    app = table.Column<string>(nullable: true),
                    errorid = table.Column<string>(nullable: true),
                    transactionid = table.Column<string>(nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_errors", x => x.LineId);
                });

            migrationBuilder.CreateTable(
                name: "log",
                columns: table => new
                {
                    LineId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    time = table.Column<DateTime>(type: "timestamp", nullable: false),
                    host = table.Column<string>(nullable: true),
                    level = table.Column<string>(nullable: true),
                    database = table.Column<string>(nullable: true),
                    remotehost = table.Column<string>(nullable: true),
                    userid = table.Column<string>(nullable: true),
                    duration = table.Column<decimal>(nullable: false),
                    logId = table.Column<string>(nullable: true),
                    message = table.Column<string>(nullable: true),
                    transactionid = table.Column<string>(nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_log", x => x.LineId);
                });

            migrationBuilder.CreateTable(
                name: "metrics",
                columns: table => new
                {
                    LineId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    time = table.Column<DateTime>(type: "timestamp", nullable: false),
                    host = table.Column<string>(nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metrics", x => x.LineId);
                });

            migrationBuilder.CreateTable(
                name: "transaction",
                columns: table => new
                {
                    LineId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SequenceHiLo),
                    time = table.Column<DateTime>(type: "timestamp", nullable: false),
                    host = table.Column<string>(nullable: true),
                    name = table.Column<string>(nullable: true),
                    result = table.Column<string>(nullable: true),
                    type = table.Column<string>(nullable: true),
                    database = table.Column<string>(nullable: true),
                    remotehost = table.Column<string>(nullable: true),
                    userid = table.Column<string>(nullable: true),
                    duration = table.Column<decimal>(nullable: false),
                    id = table.Column<string>(nullable: true),
                    parentId = table.Column<string>(nullable: true),
                    transactionId = table.Column<string>(nullable: true),
                    data = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => x.LineId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_errors_time",
                table: "errors",
                column: "time");

            migrationBuilder.CreateIndex(
                name: "IX_log_time",
                table: "log",
                column: "time");

            migrationBuilder.CreateIndex(
                name: "IX_metrics_time",
                table: "metrics",
                column: "time");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_time",
                table: "transaction",
                column: "time");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "client_config");

            migrationBuilder.DropTable(
                name: "errors");

            migrationBuilder.DropTable(
                name: "log");

            migrationBuilder.DropTable(
                name: "metrics");

            migrationBuilder.DropTable(
                name: "transaction");

            migrationBuilder.DropSequence(
                name: "EntityFrameworkHiLoSequence");
        }
    }
}
