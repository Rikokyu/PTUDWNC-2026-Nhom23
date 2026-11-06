using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public sealed class SavedRecipeRepository : ISavedRecipeRepository
{
    private readonly CulinaryBlogDbContext _context;

    public SavedRecipeRepository(CulinaryBlogDbContext context)
    {
        _context = context;
    }

    public Task<bool> IsPublishedRecipeAsync(
        Guid recipeId,
        CancellationToken cancellationToken = default) =>
        _context.Recipes.AnyAsync(
            recipe => recipe.Id == recipeId
                && !recipe.IsDeleted
                && recipe.Status == RecipeStatus.Published
                && !recipe.Category.IsDeleted,
            cancellationToken);

    public Task<SavedRecipe?> GetAsync(
        Guid userId,
        Guid recipeId,
        CancellationToken cancellationToken = default) =>
        _context.SavedRecipes.FirstOrDefaultAsync(
            savedRecipe => savedRecipe.UserId == userId
                && savedRecipe.RecipeId == recipeId,
            cancellationToken);

    public Task<bool> ExistsAsync(
        Guid userId,
        Guid recipeId,
        CancellationToken cancellationToken = default) =>
        _context.SavedRecipes.AnyAsync(
            savedRecipe => savedRecipe.UserId == userId
                && savedRecipe.RecipeId == recipeId,
            cancellationToken);

    public async Task AddAsync(
        SavedRecipe savedRecipe,
        CancellationToken cancellationToken = default)
    {
        await _context.SavedRecipes.AddAsync(savedRecipe, cancellationToken);
    }

    public void Remove(SavedRecipe savedRecipe) =>
        _context.SavedRecipes.Remove(savedRecipe);

    public async Task<(IReadOnlyList<SavedRecipe> Items, int TotalCount)> GetPagedAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = VisibleSavedRecipes(userId)
            .Include(savedRecipe => savedRecipe.Recipe)
                .ThenInclude(recipe => recipe.Category)
            .Include(savedRecipe => savedRecipe.Recipe)
                .ThenInclude(recipe => recipe.Images.Where(image => image.IsPrimary));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(savedRecipe => savedRecipe.CreatedAt)
            .ThenBy(savedRecipe => savedRecipe.RecipeId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Guid>> GetVisibleRecipeIdsAsync(
        Guid userId,
        CancellationToken cancellationToken = default) =>
        await VisibleSavedRecipes(userId)
            .OrderByDescending(savedRecipe => savedRecipe.CreatedAt)
            .Select(savedRecipe => savedRecipe.RecipeId)
            .ToListAsync(cancellationToken);

    private IQueryable<SavedRecipe> VisibleSavedRecipes(Guid userId) =>
        _context.SavedRecipes
            .AsNoTracking()
            .Where(savedRecipe => savedRecipe.UserId == userId
                && !savedRecipe.Recipe.IsDeleted
                && savedRecipe.Recipe.Status == RecipeStatus.Published
                && !savedRecipe.Recipe.Category.IsDeleted);
}
