using CulinaryBlog.API.Endpoints;

namespace CulinaryBlog.API.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapRecipeEndpoints();
        endpoints.MapRecipeIngredientEndpoints();
        endpoints.MapRecipeStepEndpoints();
        endpoints.MapRecipeImageEndpoints();

        return endpoints;
    }
}
