using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Email;
using CulinaryBlog.Infrastructure.Identity;
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

        services.AddScoped<
            IPasswordHasher<ApplicationUser>,
            PasswordHasher<ApplicationUser>>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddDataProtection();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IPasswordResetEmailSender, PasswordResetEmailSender>();
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
                IImageCleanupJobScheduler,
                HangfireImageCleanupJobScheduler>();
            services.AddScoped<IWelcomeEmailJobScheduler, HangfireWelcomeEmailJobScheduler>();
        }
        else
        {
            services.AddScoped<
                IImageResizeJobScheduler,
                DisabledImageResizeJobScheduler>();
            services.AddScoped<
                IImageCleanupJobScheduler,
                DisabledImageCleanupJobScheduler>();
            services.AddScoped<IWelcomeEmailJobScheduler, DisabledWelcomeEmailJobScheduler>();
        }

        services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var storageProvider = configuration["FileStorage:Provider"]
            ?? (string.IsNullOrWhiteSpace(configuration["Minio:AccessKey"]) ? "Local" : "Minio");
        if (string.Equals(storageProvider, "Minio", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, MinioFileStorageService>();
            return services;
        }

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
