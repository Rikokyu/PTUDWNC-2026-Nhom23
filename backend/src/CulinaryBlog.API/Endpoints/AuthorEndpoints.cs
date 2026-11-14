using Asp.Versioning;
using Asp.Versioning.Builder;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Application.Features.Recipes.Queries.GetAuthorProfile;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class AuthorEndpoints
{
    public static IEndpointRouteBuilder MapAuthorEndpoints(
        this IEndpointRouteBuilder endpoints,
        ApiVersionSet versionSet)
    {
        endpoints.MapGet("/api/v1/authors/culinary-blog",
                async (int? page, int? pageSize,
                    ISender sender, CancellationToken cancellationToken) =>
                    Results.Ok(await sender.Send(
                        new GetAuthorProfileQuery(null, page ?? 1, pageSize ?? 12),
                        cancellationToken)))
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Authors")
            .WithName("GetCulinaryBlogProfile")
            .WithSummary("Get public recipes without an assigned author.")
            .RequireRateLimiting("Api")
            .Produces<AuthorProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        endpoints.MapGet("/api/v1/authors/{authorId:guid}",
                async (Guid authorId, int? page, int? pageSize,
                    ISender sender, CancellationToken cancellationToken) =>
                    Results.Ok(await sender.Send(
                        new GetAuthorProfileQuery(authorId, page ?? 1, pageSize ?? 12),
                        cancellationToken)))
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0))
            .WithTags("Authors")
            .WithName("GetAuthorProfile")
            .WithSummary("Get a public author profile and published recipes.")
            .RequireRateLimiting("Api")
            .Produces<AuthorProfileDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }
}
