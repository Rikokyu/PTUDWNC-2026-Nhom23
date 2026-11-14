using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Commands.SaveRecipe;

public sealed record SaveRecipeCommand(Guid RecipeId) : IRequest<Unit>;
