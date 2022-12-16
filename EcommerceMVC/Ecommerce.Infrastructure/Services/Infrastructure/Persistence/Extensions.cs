using EcommerceMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceMVC.Services.Infrastructure.Persistence
{
	public static class Extensions
	{
		public static IServiceCollection RegisterPersistence(this IServiceCollection services, IConfiguration configuration)
		{

			services.AddDbContext<EcommerceDbContext>(option =>
			{
				option.UseNpgsql(configuration.GetConnectionString("Ecommerce"));
				//option.EnableSensitiveDataLogging();
			});

			return services;
		}

		public static void Seed(this ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Category>()
			   .HasData(
			   new Category
			   {
				   Id = 1,
				   Name = "Asus Alienware",
				   DisplayOrder = 1,
				   TimeUpdated = DateTimeOffset.UtcNow,
				   TimeCreated = DateTimeOffset.UtcNow
			   });
		}
	}
}
