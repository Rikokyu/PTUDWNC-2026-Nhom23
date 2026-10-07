using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.Interfaces;

public interface IRecipeRepository : IRepository<Recipe>
{
    Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
        GetPagedAsync(
            int page,
            int pageSize,
            Guid? categoryId,
            DifficultyLevel? difficulty,
            int? maxCookTime,
            string sort,
            Guid? currentUserId,
            bool isAuthenticated,
            bool isAdmin,
            bool ownRecipesOnly = false,
            bool includeAllStatuses = false,
            CancellationToken cancellationToken = default);

    Task<Recipe?> GetBySlugWithDetailsAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Recipe?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingRecipeId = null,
        CancellationToken cancellationToken = default);
}
