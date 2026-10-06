using CulinaryBlog.API.CurrentUser;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.API.Middleware;
using CulinaryBlog.Application.Features.Recipes.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(
            cfg =>
            {
                cfg.RegisterServicesFromAssembly(
                    typeof(
                        CulinaryBlog.Application
                            .Features.Recipes.Queries
                            .GetRecipes.GetRecipesQuery)
                        .Assembly);
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            });

        services.AddHttpContextAccessor();

        services.AddScoped<IPasswordHasher<CulinaryBlog.Domain.Entities.ApplicationUser>,
            PasswordHasher<CulinaryBlog.Domain.Entities.ApplicationUser>>();

        services.AddAuthentication("Bearer")
            .AddScheme<AuthenticationSchemeOptions, JwtAuthenticationHandler>(
                "Bearer",
                _ => { });
        services.AddAuthorization();
        services.AddHttpClient();

        services.AddScoped<
            ICurrentUser,
            CurrentUserService>();

        services.AddScoped<RecipeManagementService>();

        return services;
    }
}