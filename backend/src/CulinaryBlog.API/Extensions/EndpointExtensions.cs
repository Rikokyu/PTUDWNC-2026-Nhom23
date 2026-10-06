using CulinaryBlog.API.Endpoints;

namespace CulinaryBlog.API.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapRecipeEndpoints();
        endpoints.MapRecipeImageEndpoints();
        endpoints.MapCategoryEndpoints();
        endpoints.MapAuthEndpoints();
        endpoints.MapHealthEndpoints();

        return endpoints;
    }
}
