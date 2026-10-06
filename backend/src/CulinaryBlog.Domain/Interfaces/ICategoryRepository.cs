using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
	Task<Category?> GetBySlugAsync(
		string slug,
		CancellationToken cancellationToken = default);

	Task<bool> HasRecipesAsync(
		Guid categoryId,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<(Category Category, int RecipeCount)>> GetAllWithRecipeCountAsync(
		CancellationToken cancellationToken = default);
}
