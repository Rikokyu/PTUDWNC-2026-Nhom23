using CulinaryBlog.API.Endpoints;
using Asp.Versioning;

namespace CulinaryBlog.API.Extensions;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapApplicationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapAuthEndpoints();
        endpoints.MapCategoryEndpoints();
        endpoints.MapHealthEndpoints();

        var versionSet = endpoints
            .NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        endpoints.MapRecipeEndpoints(versionSet);
        endpoints.MapRecipeIngredientEndpoints(versionSet);
        endpoints.MapRecipeStepEndpoints(versionSet);
        endpoints.MapRecipeImageEndpoints(versionSet);

        return endpoints;
    }
}
