using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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
            string? search,
            string sort,
            Guid? currentUserId,
            bool isAuthenticated,
            bool isAdmin,
            bool ownRecipesOnly = false,
            bool includeAllStatuses = false,
            CancellationToken cancellationToken = default,
            int? minServings = null)
    {
        IQueryable<Recipe> query =
            _context.Recipes
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Include(x => x.Category)
                .Include(x => x.Images.Where(image =>
                    image.IsPrimary))
                .Where(recipe => !recipe.Category.IsDeleted);

        // Authorization
        if (ownRecipesOnly)
        {
            query = query.Where(x =>
                isAuthenticated
                && currentUserId.HasValue
                && x.AuthorId == currentUserId.Value);
        }
        else if (isAdmin && includeAllStatuses)
        {
            query = query.Where(x =>
                x.Status == RecipeStatus.Published
                || (currentUserId.HasValue
                    && x.AuthorId == currentUserId.Value));
        }
        else if (isAuthenticated && currentUserId.HasValue)
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

        if (minServings.HasValue)
            query = query.Where(x => x.Servings >= minServings.Value);

        // Max cook time
        if (maxCookTime.HasValue)
        {
            query = query.Where(x =>
                x.CookTimeMinutes <= maxCookTime.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Keep list and dedicated search endpoints consistent, and use the
            // indexed Vietnamese-aware vector instead of scanning text columns.
            var tsQuery = VietnameseSearchQuery.Build(search);
            query = tsQuery.Length == 0
                ? query.Where(_ => false)
                : query.Where(recipe =>
                    EF.Property<NpgsqlTypes.NpgsqlTsVector>(recipe, "SearchVector")
                        .Matches(EF.Functions.ToTsQuery("vietnamese", tsQuery)));
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
            await ((IOrderedQueryable<Recipe>)query)
                .ThenBy(recipe => recipe.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

        return (items, totalCount);
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
        var query = _context.Recipes
            .AsNoTracking()
            .Where(recipe =>
                recipe.CategoryId == categoryId
                && !recipe.IsDeleted
                && !recipe.Category.IsDeleted)
            .Where(CreateCategoryVisibilityPredicate(
                currentUserId,
                isAuthenticated && !isAdmin))
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ThenBy(recipe => recipe.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public static Expression<Func<Recipe, bool>>
        CreateCategoryVisibilityPredicate(
            Guid? currentUserId,
            bool includeOwnDrafts)
    {
        return recipe =>
            recipe.Status == RecipeStatus.Published
            || (includeOwnDrafts
                && currentUserId.HasValue
                && recipe.Status == RecipeStatus.Draft
                && recipe.AuthorId == currentUserId.Value);
    }

    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
        SearchPublishedAsync(
            string searchTerm,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default,
            Guid? categoryId = null,
            DifficultyLevel? difficulty = null,
            int? maxCookTime = null,
            int? minServings = null)
    {
        var tsQuery = VietnameseSearchQuery.Build(searchTerm);
        if (tsQuery.Length == 0) return (Array.Empty<Recipe>(), 0);
        IQueryable<Recipe> query = _context.Recipes.AsNoTracking()
            .Where(recipe => !recipe.IsDeleted && !recipe.Category.IsDeleted
                && recipe.Status == RecipeStatus.Published
                && EF.Property<NpgsqlTypes.NpgsqlTsVector>(recipe, "SearchVector")
                    .Matches(EF.Functions.ToTsQuery("vietnamese", tsQuery)))
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images.Where(image => image.IsPrimary));
        if (categoryId.HasValue) query = query.Where(x => x.CategoryId == categoryId.Value);
        if (difficulty.HasValue) query = query.Where(x => x.Difficulty == difficulty.Value);
        if (maxCookTime.HasValue) query = query.Where(x => x.CookTimeMinutes <= maxCookTime.Value);
        if (minServings.HasValue) query = query.Where(x => x.Servings >= minServings.Value);
        var totalCount = await query.CountAsync(cancellationToken);
        var ranked = await query
            .OrderByDescending(recipe => EF.Property<NpgsqlTypes.NpgsqlTsVector>(recipe, "SearchVector")
                .Rank(EF.Functions.ToTsQuery("vietnamese", tsQuery)))
            .ThenByDescending(recipe => recipe.CreatedAt).ThenBy(recipe => recipe.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(recipe => new { Recipe = recipe,
                Score = EF.Property<NpgsqlTypes.NpgsqlTsVector>(recipe, "SearchVector")
                    .Rank(EF.Functions.ToTsQuery("vietnamese", tsQuery)) })
            .ToListAsync(cancellationToken);
        foreach (var row in ranked) row.Recipe.SearchRelevanceScore = row.Score;
        var items = ranked.Select(row => row.Recipe).ToList();

        return (items, totalCount);
    }

    public async Task<Recipe?> GetBySlugWithDetailsAsync(
        string slug,
        CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsNoTracking()
            .AsSplitQuery()
            .Where(x => !x.IsDeleted)
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
                    && !recipe.IsDeleted
                    && !recipe.Category.IsDeleted,
                cancellationToken);
    }

    public async Task<Recipe?> GetByIdWithDetailsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _context.Recipes
            .AsSplitQuery()
            .Include(x => x.Category)
            .Include(x => x.Ingredients)
            .Include(x => x.Steps)
            .Include(x => x.Images)
            .FirstOrDefaultAsync(
                x => x.Id == id && !x.IsDeleted,
                cancellationToken);
    }

    public Task<bool> SlugExistsAsync(
        string slug,
        Guid? excludingRecipeId = null,
        CancellationToken cancellationToken = default)
    {
        return _context.Recipes
            .AnyAsync(
                recipe => recipe.Slug == slug
                    && (!excludingRecipeId.HasValue
                        || recipe.Id != excludingRecipeId.Value),
                cancellationToken);
    }
}
