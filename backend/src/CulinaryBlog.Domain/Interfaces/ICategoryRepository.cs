using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
	Task<Category?> GetBySlugAsync(
		string slug,
		CancellationToken cancellationToken = default);
}
