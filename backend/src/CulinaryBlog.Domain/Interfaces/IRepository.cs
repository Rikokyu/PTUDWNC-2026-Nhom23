using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Domain.Interfaces;

public interface IRepository<TEntity>
	where TEntity : BaseEntity
{
	Task<TEntity?> GetByIdAsync(
		Guid id,
		CancellationToken cancellationToken = default);

	Task<IReadOnlyList<TEntity>> GetAllAsync(
		CancellationToken cancellationToken = default);

	Task AddAsync(
		TEntity entity,
		CancellationToken cancellationToken = default);

	void Update(TEntity entity);

	void Remove(TEntity entity);
}
