using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using CulinaryBlog.Infrastructure.Persistence;

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
                    image.IsPrimary));

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

            "-title" =>
                query.OrderByDescending(x => x.Title),

            "categoryName" =>
                query.OrderBy(x => x.Category.Name),

            "-categoryName" =>
                query.OrderByDescending(x => x.Category.Name),

            "prepTime" =>
                query.OrderBy(x => x.PrepTimeMinutes),

            "-prepTime" =>
                query.OrderByDescending(x => x.PrepTimeMinutes),

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
                x => x.Slug == slug,
                cancellationToken);
    }

    public Task<Recipe?> GetByIdWithDetailsForUpdateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _context.Recipes
            .AsSplitQuery()
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Author)
            .Include(recipe => recipe.Ingredients)
            .Include(recipe => recipe.Steps)
            .Include(recipe => recipe.Images)
            .FirstOrDefaultAsync(recipe => recipe.Id == id, cancellationToken);
    }

    public void AddImage(RecipeImage image)
    {
        _context.RecipeImages.Add(image);
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return _context.Recipes.AnyAsync(recipe => recipe.Slug == slug, cancellationToken);
    }

    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Recipe> query = _context.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.Status == RecipeStatus.Published)
            .Where(recipe =>
                EF.Functions.ToTsVector(
                    "simple",
                    EF.Functions.Unaccent(
                        recipe.Title + " " + recipe.Description + " " + recipe.Instructions))
                .Matches(EF.Functions.PlainToTsQuery(
                    "simple",
                    EF.Functions.Unaccent(searchTerm))))
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images.Where(image => image.IsPrimary));

        var totalCount = await query.CountAsync(cancellationToken);
        query = sort switch
        {
            "createdAt" => query.OrderBy(recipe => recipe.CreatedAt),
            "title" => query.OrderBy(recipe => recipe.Title),
            "-title" => query.OrderByDescending(recipe => recipe.Title),
            "cookTime" => query.OrderBy(recipe => recipe.CookTimeMinutes),
            "-cookTime" => query.OrderByDescending(recipe => recipe.CookTimeMinutes),
            "prepTime" => query.OrderBy(recipe => recipe.PrepTimeMinutes),
            "-prepTime" => query.OrderByDescending(recipe => recipe.PrepTimeMinutes),
            "categoryName" => query.OrderBy(recipe => recipe.Category.Name),
            "-categoryName" => query.OrderByDescending(recipe => recipe.Category.Name),
            _ => query.OrderByDescending(recipe => recipe.CreatedAt)
        };
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}