using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Models;

namespace CulinaryBlog.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
	Task<Category?> GetActiveByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default);

	Task<Category?> GetActiveByIdReadOnlyAsync(
		Guid id,
		CancellationToken cancellationToken = default);

	Task<Category?> GetBySlugAsync(
		string slug,
		CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CategoryWithRecipeCount>>
        GetAllWithPublishedRecipeCountAsync(
            CancellationToken cancellationToken = default);

    Task<int> GetPublishedRecipeCountAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<int> GetRecipeCountAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default);
}
