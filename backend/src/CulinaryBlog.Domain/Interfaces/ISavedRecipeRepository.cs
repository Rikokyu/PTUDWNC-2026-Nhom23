using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Interfaces;

public interface ISavedRecipeRepository
{
    Task<bool> IsPublishedRecipeAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default);

    Task<SavedRecipe?> GetAsync(
        Guid userId,
        Guid recipeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        Guid userId,
        Guid recipeId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SavedRecipe savedRecipe,
        CancellationToken cancellationToken = default);

    void Remove(SavedRecipe savedRecipe);

    Task<(IReadOnlyList<SavedRecipe> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetVisibleRecipeIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<int> CountForPublishedRecipeAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default);
}
