using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Recipes;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes.Queries.GetAuthorProfile;

public sealed class GetAuthorProfileQueryHandler(IRecipeRepository recipes)
    : IRequestHandler<GetAuthorProfileQuery, AuthorProfileDto>
{
    public async Task<AuthorProfileDto> Handle(
        GetAuthorProfileQuery request,
        CancellationToken cancellationToken)
    {
        if (request.Page < 1 || request.PageSize is < 1 or > 50)
            throw new ValidationException("page must be positive and pageSize must be between 1 and 50.");

        if (request.AuthorId is null)
        {
            var unattributed = await recipes.GetUnattributedPublishedAsync(
                request.Page, request.PageSize, cancellationToken);
            return new AuthorProfileDto(
                Guid.Empty, "Culinary Blog", null, null,
                new PagedResult<RecipeSummaryDto>(
                    unattributed.Items.Select(recipe => RecipeSummaryDto.FromRecipe(recipe)).ToList(),
                    unattributed.TotalCount, request.Page, request.PageSize));
        }

        var author = await recipes.GetAuthorByIdAsync(request.AuthorId.Value, cancellationToken)
            ?? throw new NotFoundException("Author was not found.");
        var result = await recipes.GetPublishedByAuthorAsync(
            request.AuthorId.Value, request.Page, request.PageSize, cancellationToken);
        var items = result.Items.Select(recipe => RecipeSummaryDto.FromRecipe(recipe)).ToList();

        return new AuthorProfileDto(
            author.Id,
            author.DisplayName,
            author.AvatarUrl,
            author.Bio,
            new PagedResult<RecipeSummaryDto>(
                items, result.TotalCount, request.Page, request.PageSize));
    }
}
