using CulinaryBlog.API.Endpoints;

namespace CulinaryBlog.API.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapRecipeEndpoints();
        endpoints.MapRecipeImageEndpoints();
        endpoints.MapCategoryEndpoints();
        endpoints.MapHealthEndpoints();

        return endpoints;
    }
}
