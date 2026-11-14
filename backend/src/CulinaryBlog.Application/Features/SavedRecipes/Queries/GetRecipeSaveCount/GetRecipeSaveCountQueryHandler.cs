using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Queries.GetRecipeSaveCount;

public sealed class GetRecipeSaveCountQueryHandler(ISavedRecipeRepository savedRecipes)
    : IRequestHandler<GetRecipeSaveCountQuery, int>
{
    public async Task<int> Handle(
        GetRecipeSaveCountQuery request,
        CancellationToken cancellationToken)
    {
        if (!await savedRecipes.IsPublishedRecipeAsync(request.RecipeId, cancellationToken))
            throw new NotFoundException("Recipe was not found.");

        return await savedRecipes.CountForPublishedRecipeAsync(
            request.RecipeId, cancellationToken);
    }
}
