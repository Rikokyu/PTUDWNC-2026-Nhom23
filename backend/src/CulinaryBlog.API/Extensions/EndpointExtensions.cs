using CulinaryBlog.API.Endpoints;

namespace CulinaryBlog.API.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthEndpoints();
        endpoints.MapCategoryEndpoints();
        endpoints.MapRecipeEndpoints();

        return endpoints;
    }
}
