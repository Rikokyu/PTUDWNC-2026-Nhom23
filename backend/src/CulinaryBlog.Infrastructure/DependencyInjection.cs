using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Authentication;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using CulinaryBlog.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddScoped(
            typeof(IRepository<>),
            typeof(Repository<>));

        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IFileStorageService, MinioFileStorageService>();
        
        return services;
    }
}