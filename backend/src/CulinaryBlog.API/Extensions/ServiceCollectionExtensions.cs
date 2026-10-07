using CulinaryBlog.API.CurrentUser;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using MediatR;

namespace CulinaryBlog.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(
            cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(
                        CulinaryBlog.Application
                            .Features.Recipes.Queries
                            .GetRecipes.GetRecipesQuery)
                        .Assembly));

        services.AddHttpContextAccessor();

        services.AddMemoryCache();

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddTransient<
            IRequestValidator<CreateCategoryCommand>,
            CreateCategoryCommandValidator>();

        services.AddTransient<
            IRequestValidator<UpdateCategoryCommand>,
            UpdateCategoryCommandValidator>();

        services.AddScoped<
            ICurrentUser,
            CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(
                "Frontend",
                policy => policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod());
        });

        return services;
    }
}
