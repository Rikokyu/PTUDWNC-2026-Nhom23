using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Authentication;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using CulinaryBlog.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CulinaryBlogDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));
        });

        if (configuration.GetValue("Hangfire:Enabled", true))
        {
            var connectionString = configuration.GetConnectionString(
                "HangfireConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "A PostgreSQL connection string is required for Hangfire.");

            var hangfireStorageOptions = new PostgreSqlStorageOptions
            {
                StartupConnectionMaxRetries = 0,
                AllowDegradedModeWithoutStorage = true,
                StartupConnectionBaseDelay = TimeSpan.FromMilliseconds(250),
                StartupConnectionMaxDelay = TimeSpan.FromSeconds(1)
            };

            services.AddHangfire((_, hangfire) =>
                hangfire.UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(connectionString),
                    hangfireStorageOptions));
            services.AddHangfireServer();
            services.AddScoped<
                IImageResizeJobScheduler,
                HangfireImageResizeJobScheduler>();
        }
        else
        {
            services.AddScoped<
                IImageResizeJobScheduler,
                DisabledImageResizeJobScheduler>();
        }

        services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
