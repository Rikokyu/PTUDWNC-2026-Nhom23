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
            CancellationToken cancellationToken = default);

    Task<Recipe?> GetBySlugWithDetailsAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Recipe?> GetByIdWithDetailsForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    void AddImage(RecipeImage image);

    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Recipe> Items, int TotalCount)> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default);
}