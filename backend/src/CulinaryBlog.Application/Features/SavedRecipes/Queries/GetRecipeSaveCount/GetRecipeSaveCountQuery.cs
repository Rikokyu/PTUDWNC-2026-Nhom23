using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Queries.GetRecipeSaveCount;

public sealed record GetRecipeSaveCountQuery(Guid RecipeId) : IRequest<int>;
