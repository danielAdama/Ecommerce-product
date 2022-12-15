using EcommerceMVC.Data;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMVC.Services.Infrastructure.Persistence
{
	public class EcommerceDbContext : DbContext
	{
#nullable disable
		public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) :
			base(options)
		{ }

		public DbSet<Category> Categories { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			// Add the Postgres Extension for UUID generation
			modelBuilder.HasPostgresExtension("uuid-ossp");
			modelBuilder.Seed();
		}
	}

}
