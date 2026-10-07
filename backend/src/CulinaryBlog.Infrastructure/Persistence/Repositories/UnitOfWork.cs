using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Exceptions;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
        return SaveChangesCoreAsync(cancellationToken);
    }

    private async Task<int> SaveChangesCoreAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessRuleException(
                "The resource was modified by another request. Reload it and try again.");
        }
    }
}