using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
	public CategoryRepository(CulinaryBlogDbContext context)
		: base(context)
	{
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
}
