using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Domain.Interfaces;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly CulinaryBlogDbContext _context;

    public UnitOfWork(
        CulinaryBlogDbContext context,
        IRecipeRepository recipes,
        ICategoryRepository categories)
    {
        _context = context;
        Recipes = recipes;
        Categories = categories;
    }

    public IRecipeRepository Recipes { get; }

    public ICategoryRepository Categories { get; }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}