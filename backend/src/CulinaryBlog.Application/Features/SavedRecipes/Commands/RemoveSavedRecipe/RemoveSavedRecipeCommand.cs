using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Commands.RemoveSavedRecipe;

public sealed record RemoveSavedRecipeCommand(Guid RecipeId) : IRequest<Unit>;
