using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Email;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using CulinaryBlog.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string? contentRootPath = null)
    {
        services.AddDbContext<CulinaryBlogDbContext>(options =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"));
        });

        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = false;
            })
            .AddUserStore<ApplicationUserStore>();

        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<IEmailService, MailKitEmailService>();

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
            services.AddScoped<
                IWelcomeEmailJobScheduler,
                HangfireWelcomeEmailJobScheduler>();
        }
        else
        {
            services.AddScoped<
                IImageResizeJobScheduler,
                DisabledImageResizeJobScheduler>();
            services.AddScoped<
                IWelcomeEmailJobScheduler,
                DisabledWelcomeEmailJobScheduler>();
        }

        services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var uploadRoot = Path.Combine(
            contentRootPath ?? Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads");
        var publicBasePath = configuration["FileStorage:PublicBasePath"]
            ?? "/uploads";
        services.AddScoped<IFileStorageService>(
            _ => new LocalFileStorageService(uploadRoot, publicBasePath));

        return services;
    }
}
