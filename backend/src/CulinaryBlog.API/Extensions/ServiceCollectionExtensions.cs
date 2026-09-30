using CulinaryBlog.API.CurrentUser;
using CulinaryBlog.Application.Common.Interfaces;
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

        services.AddScoped<
            ICurrentUser,
            CurrentUserService>();

        return services;
    }
}