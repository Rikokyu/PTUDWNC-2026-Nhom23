using MediatR;

namespace CulinaryBlog.Application.Features.Categories.Commands.BulkManageRecipes;

public enum BulkCategoryRecipeAction
{
    Move,
    Delete
}

public sealed record BulkManageCategoryRecipesCommand(
    Guid CategoryId,
    IReadOnlyList<Guid> RecipeIds,
    BulkCategoryRecipeAction Action,
    Guid? TargetCategoryId = null) : IRequest<int>;
