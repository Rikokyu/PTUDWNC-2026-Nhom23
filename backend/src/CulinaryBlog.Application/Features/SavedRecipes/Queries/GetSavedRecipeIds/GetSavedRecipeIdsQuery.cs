using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Queries.GetSavedRecipeIds;

public sealed record GetSavedRecipeIdsQuery : IRequest<IReadOnlyList<Guid>>;
