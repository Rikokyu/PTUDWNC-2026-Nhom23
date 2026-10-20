using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
	public CategoryRepository(CulinaryBlogDbContext context)
		: base(context)
	{
	}

	public Task<Category?> GetActiveByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default)
	{
		return DbSet.FirstOrDefaultAsync(
			category => category.Id == id && !category.IsDeleted,
			cancellationToken);
	}

	public Task<Category?> GetBySlugAsync(
		string slug,
		CancellationToken cancellationToken = default)
	{
		return DbSet
			.AsNoTracking()
			.FirstOrDefaultAsync(
				category =>
					category.Slug == slug
					&& !category.IsDeleted,
				cancellationToken);
	}

    public async Task<IReadOnlyList<CategoryWithRecipeCount>>
        GetAllWithPublishedRecipeCountAsync(
            CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AsNoTracking()
            .Where(category => !category.IsDeleted)
            .OrderBy(category => category.Name)
            .Select(category => new CategoryWithRecipeCount(
                category,
                category.Recipes.Count(recipe =>
                    recipe.Status == RecipeStatus.Published)))
            .ToListAsync(cancellationToken);
    }

    public Task<int> GetPublishedRecipeCountAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .Where(category =>
                category.Id == categoryId
                && !category.IsDeleted)
            .SelectMany(category => category.Recipes)
            .CountAsync(
                recipe => recipe.Status == RecipeStatus.Published,
                cancellationToken);
    }

    public Task<int> GetRecipeCountAsync(
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .Where(category =>
                category.Id == categoryId
                && !category.IsDeleted)
            .SelectMany(category => category.Recipes)
            .CountAsync(cancellationToken);
    }

    public Task<bool> NameExistsAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.ToUpper();

        return DbSet
            .AnyAsync(
                category =>
                    category.Name.ToUpper() == normalizedName
                    && (!excludingId.HasValue
                        || category.Id != excludingId.Value),
                cancellationToken);
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return DbSet
            .AnyAsync(
                category => category.Slug == slug,
                cancellationToken);
    }
}
