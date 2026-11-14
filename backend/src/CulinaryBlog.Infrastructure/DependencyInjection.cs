using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.BackgroundJobs;
using CulinaryBlog.Infrastructure.Email;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Authentication;
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
using Microsoft.Extensions.Logging;
using Minio;

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
        services.AddSingleton<IGoogleTokenVerifier, GoogleAuthenticationService>();
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddDataProtection();
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

            services.AddHangfire((provider, hangfire) =>
                hangfire.UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(connectionString),
                    hangfireStorageOptions)
                    .UseFilter(new WelcomeEmailFailureFilter(
                        provider.GetRequiredService<ILogger<WelcomeEmailFailureFilter>>())));
            services.AddHangfireServer();
            services.AddScoped<
                IImageResizeJobScheduler,
                HangfireImageResizeJobScheduler>();
            services.AddScoped<
                IImageCleanupJobScheduler,
                HangfireImageCleanupJobScheduler>();
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
                IImageCleanupJobScheduler,
                DisabledImageCleanupJobScheduler>();
            services.AddScoped<
                IWelcomeEmailJobScheduler,
                DisabledWelcomeEmailJobScheduler>();
        }

        services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISavedRecipeRepository, SavedRecipeRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        var storageProvider = configuration["FileStorage:Provider"] ?? "Local";
        if (storageProvider.Equals("Minio", StringComparison.OrdinalIgnoreCase))
        {
            var endpoint = configuration["Minio:Endpoint"]
                ?? throw new InvalidOperationException("Minio:Endpoint is required.");
            var publicEndpoint = configuration["Minio:PublicEndpoint"]
                ?? throw new InvalidOperationException("Minio:PublicEndpoint is required.");
            var accessKey = configuration["Minio:AccessKey"]
                ?? throw new InvalidOperationException("Minio:AccessKey is required.");
            var secretKey = configuration["Minio:SecretKey"]
                ?? throw new InvalidOperationException("Minio:SecretKey is required.");
            var bucket = configuration["Minio:Bucket"]
                ?? throw new InvalidOperationException("Minio:Bucket is required.");
            var endpointUri = new Uri(endpoint, UriKind.Absolute);
            if (endpointUri.Scheme is not ("http" or "https")
                || endpointUri.AbsolutePath != "/" || endpointUri.Query.Length > 0)
            {
                throw new InvalidOperationException("Minio:Endpoint must be an HTTP(S) origin.");
            }

            services.AddSingleton<IMinioClient>(_ => new MinioClient()
                .WithEndpoint(endpointUri.Host, endpointUri.Port)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(endpointUri.Scheme == "https")
                .Build());
            services.AddSingleton<IFileStorageService>(provider =>
                new MinioObjectStorageService(
                    provider.GetRequiredService<IMinioClient>(), bucket, publicEndpoint));
            return services;
        }

        if (!storageProvider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("FileStorage:Provider must be Local or Minio.");
        }

        var uploadRoot = Path.Combine(
            contentRootPath ?? Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads");
        var publicBasePath = configuration["FileStorage:PublicBasePath"]
            ?? "/uploads";
        var minioEndpoint = configuration["Minio:Endpoint"];
        if (!string.IsNullOrWhiteSpace(minioEndpoint))
        {
            var accessKey = configuration["Minio:AccessKey"] ?? "minioadmin";
            var secretKey = configuration["Minio:SecretKey"] ?? "minioadmin";
            var bucket = configuration["Minio:Bucket"] ?? "culinary-blog";
            var publicEndpoint = configuration["Minio:PublicEndpoint"]
                ?? minioEndpoint;

            var minioUri = new Uri(minioEndpoint, UriKind.Absolute);
            services.AddSingleton<IMinioClient>(_ => new MinioClient()
                .WithEndpoint(minioUri.Host, minioUri.Port)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(minioUri.Scheme.Equals(
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase))
                .Build());
            services.AddScoped<IFileStorageService>(provider =>
                new MinioObjectStorageService(
                    provider.GetRequiredService<IMinioClient>(),
                    bucket,
                    publicEndpoint));
        }
        else
        {
            services.AddScoped<IFileStorageService>(
                _ => new LocalFileStorageService(uploadRoot, publicBasePath));
        }

        return services;
    }
}
