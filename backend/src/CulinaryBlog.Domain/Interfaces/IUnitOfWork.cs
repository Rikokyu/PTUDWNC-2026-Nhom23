using CulinaryBlog.Domain.Interfaces;

public interface IUnitOfWork
{
    IRecipeRepository Recipes { get; }

    ICategoryRepository Categories { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}