using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Domain.Entities;

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

    public void AddRecipeImage(RecipeImage image) =>
        _context.RecipeImages.Add(image);

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}
