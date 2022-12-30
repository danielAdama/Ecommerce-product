using EcommerceMVC.Data;
using EcommerceMVC.Services.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceMVC.Services.Infrastructure.Auth
{
    public static class Extensions
    {
        public static IServiceCollection RegisterIdentity(this IServiceCollection services)
        {
            services.AddIdentity<EcommerceUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                //options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                //options.Lockout.MaxFailedAccessAttempts = 5;
                //options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<EcommerceDbContext>()
            .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(15);
                options.LoginPath = "/Account/Identity/Login";
                options.LogoutPath = "/Account/Identity/LogOut";
                options.AccessDeniedPath = "/Account/Identity/AccessDenied";
                options.SlidingExpiration = true;
            });

            return services;
        }
    }
}
