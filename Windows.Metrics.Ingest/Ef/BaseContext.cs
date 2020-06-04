using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Windows.Metrics.Ingest.Ef
{
	public class BaseContext : DbContext
	{
		public BaseContext(DbContextOptions options) : base(options) { }

		public DbSet<ClientConfig> Config { get; set; }
		public DbSet<MetricEntity> Metrics { get; set; }
		public DbSet<LogEntity> Logs { get; set; }
		public DbSet<ErrorEntity> Errors { get; set; }
		public DbSet<TransactionEntity> Transactions { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.UseHiLo();

			modelBuilder.Entity<ClientConfig>().HasKey(x => new { x.App, x.Client });
			modelBuilder.Entity<LogEntity>().HasIndex(x => x.Time);
			modelBuilder.Entity<MetricEntity>().HasIndex(x => x.Time);
			modelBuilder.Entity<ErrorEntity>().HasIndex(x => x.Time);
			modelBuilder.Entity<TransactionEntity>().HasIndex(x => x.Time);
		}
	}

	public class PostgresContext : BaseContext
	{

		/*
		 dotnet ef migrations add InitialCreate --context PostgresContext --output-dir Migrations/PostgresDatabase
		 dotnet ef database update --context PostgresContext
		*/

		private readonly IConfiguration configuration;

		public PostgresContext(
			DbContextOptions options,
			IConfiguration configuration) : base(options) => this.configuration = configuration;

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql(configuration.GetConnectionString("default"));
			base.OnConfiguring(optionsBuilder);
		}
	}
}
