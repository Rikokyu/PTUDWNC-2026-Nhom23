using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
	private readonly CulinaryBlogDbContext _context;

	public CategoryRepository(CulinaryBlogDbContext context)
		: base(context)
	{
		_context = context;
	}

	public Task<Category?> GetBySlugAsync(
		string slug,
		CancellationToken cancellationToken = default)
	{
		return DbSet
			.AsNoTracking()
			.FirstOrDefaultAsync(
				category => category.Slug == slug,
				cancellationToken);
	}

	public Task<bool> HasRecipesAsync(
		Guid categoryId,
		CancellationToken cancellationToken = default)
	{
		return _context.Recipes
			.IgnoreQueryFilters()
			.AnyAsync(
				recipe => recipe.CategoryId == categoryId,
				cancellationToken);
	}

	public async Task<IReadOnlyList<(Category Category, int RecipeCount)>>
		GetAllWithRecipeCountAsync(CancellationToken cancellationToken = default)
	{
		var categories = await DbSet
			.AsNoTracking()
			.OrderBy(category => category.OrderIndex)
			.ThenBy(category => category.Name)
			.Select(category => new
			{
				Category = category,
				RecipeCount = category.Recipes.Count(recipe =>
					recipe.Status == CulinaryBlog.Domain.Enums.RecipeStatus.Published)
			})
			.ToListAsync(cancellationToken);

		return categories.Select(item => (item.Category, item.RecipeCount)).ToList();
	}
}
