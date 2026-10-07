using System.Text.Json.Serialization;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(
            "DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Missing ConnectionStrings:DefaultConnection configuration.");
        }

        services.AddDbContext<CulinaryBlogDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(
                new JsonStringEnumConverter()));

        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
                policy
                    .WithOrigins(
                        "http://localhost:3000",
                        "http://127.0.0.1:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithExposedHeaders(
                        "ETag",
                        "X-Correlation-ID"));
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
