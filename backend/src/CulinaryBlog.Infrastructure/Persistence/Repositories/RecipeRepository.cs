using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Repositories;

public class RecipeRepository : Repository<Recipe>, IRecipeRepository
{
    private readonly CulinaryBlogDbContext _context;

    public RecipeRepository(
        CulinaryBlogDbContext context)
        : base(context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
        GetPagedAsync(
            int page,
            int pageSize,
            Guid? categoryId,
            DifficultyLevel? difficulty,
            int? maxCookTime,
            string sort,
            Guid? currentUserId,
            bool isAuthenticated,
            bool isAdmin,
            CancellationToken cancellationToken = default)
    {
        IQueryable<Recipe> query =
            _context.Recipes
                .AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.Images.Where(image =>
                    image.IsPrimary))
                .Where(recipe => !recipe.Category.IsDeleted);

        // Authorization
        if (!isAdmin)
        {
            if (isAuthenticated && currentUserId.HasValue)
            {
                query = query.Where(x =>
                    x.Status == RecipeStatus.Published
                    || x.AuthorId == currentUserId.Value);
            }
            else
            {
                query = query.Where(x =>
                    x.Status == RecipeStatus.Published);
            }
        }

        // Category
        if (categoryId.HasValue)
        {
            query = query.Where(x =>
                x.CategoryId == categoryId.Value);
        }

        // Difficulty
        if (difficulty.HasValue)
        {
            query = query.Where(x =>
                x.Difficulty == difficulty.Value);
        }

        // Max cook time
        if (maxCookTime.HasValue)
        {
            query = query.Where(x =>
                x.CookTimeMinutes <= maxCookTime.Value);
        }

        // Sorting
        query = sort switch
        {
            "createdAt" =>
                query.OrderBy(x => x.CreatedAt),

            "-createdAt" =>
                query.OrderByDescending(x => x.CreatedAt),

            "title" =>
                query.OrderBy(x => x.Title),

            "cookTime" =>
                query.OrderBy(x => x.CookTimeMinutes),

            "-cookTime" =>
                query.OrderByDescending(x =>
                    x.CookTimeMinutes),

            _ =>
                query.OrderByDescending(x =>
                    x.CreatedAt)
        };

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Recipe?> GetBySlugWithDetailsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Category)
            .Include(x => x.Author)
            .Include(x => x.Ingredients)
            .Include(x => x.Steps)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(
                x => x.Slug == slug
                    && !x.Category.IsDeleted,
                cancellationToken);
    }

    public Task<Recipe?> GetByIdWithImagesAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Recipes
            .Include(recipe => recipe.Images)
            .FirstOrDefaultAsync(
                recipe => recipe.Id == id
                    && !recipe.Category.IsDeleted,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
        GetPagedForCategoryAsync(
            Guid categoryId,
            int page,
            int pageSize,
            Guid? currentUserId,
            bool isAuthenticated,
            bool isAdmin,
            CancellationToken cancellationToken = default)
    {
        IQueryable<Recipe> query = _context.Recipes
            .AsNoTracking()
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images.Where(image =>
                image.IsPrimary))
            .Where(recipe =>
                recipe.CategoryId == categoryId
                && !recipe.Category.IsDeleted);

        if (!isAdmin)
        {
            query = isAuthenticated && currentUserId.HasValue
                ? query.Where(recipe =>
                    recipe.Status == RecipeStatus.Published
                    || (recipe.Status == RecipeStatus.Draft
                        && recipe.AuthorId == currentUserId.Value))
                : query.Where(recipe =>
                    recipe.Status == RecipeStatus.Published);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ThenBy(recipe => recipe.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
