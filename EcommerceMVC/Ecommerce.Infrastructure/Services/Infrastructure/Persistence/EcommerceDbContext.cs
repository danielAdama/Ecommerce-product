using Ecommerce.Infrastructure.Data;
using EcommerceMVC.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMVC.Services.Infrastructure.Persistence
{
	public class EcommerceDbContext : IdentityDbContext<EcommerceUser, ApplicationRole, long>
	{
#nullable disable
		public EcommerceDbContext(DbContextOptions<EcommerceDbContext> options) :
			base(options)
		{ }

		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<EcommerceUser> EcommerceUsers { get; set; }
		public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			// Add the Postgres Extension for UUID generation
			modelBuilder.HasPostgresExtension("uuid-ossp");
			modelBuilder.Seed();
		}
	}

}
