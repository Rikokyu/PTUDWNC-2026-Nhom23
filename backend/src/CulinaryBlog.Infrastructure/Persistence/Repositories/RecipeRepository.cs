using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Domain.Models;
using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;

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
            bool ownRecipesOnly = false,
            bool includeAllStatuses = false,
            CancellationToken cancellationToken = default)
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

        // Max cook time
        if (maxCookTime.HasValue)
        {
            query = query.Where(x =>
                x.CookTimeMinutes <= maxCookTime.Value);
        }

        // Sorting
        var orderedList = sort switch
        {
            "createdAt" =>
                query.OrderBy(x => x.CreatedAt),

            "-createdAt" =>
                query.OrderByDescending(x => x.CreatedAt),

            "title" =>
                query.OrderBy(x => x.Title),

            "-title" => query.OrderByDescending(x => x.Title),

            "cookTime" =>
                query.OrderBy(x => x.CookTimeMinutes),

            "-cookTime" =>
                query.OrderByDescending(x =>
                    x.CookTimeMinutes),

            _ =>
                query.OrderByDescending(x =>
                    x.CreatedAt)
        };

        query = orderedList.ThenBy(x => x.Id);

        var totalCount =
            await query.CountAsync(cancellationToken);

        var items =
            await query
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
                && !recipe.Category.IsDeleted
                && (recipe.Status == RecipeStatus.Published
                    || isAdmin
                    || (isAuthenticated
                        && currentUserId.HasValue
                        && recipe.AuthorId == currentUserId.Value)))
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

    public async Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
        SearchPublishedAsync(
            string searchTerm,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var pattern = $"%{searchTerm.Trim()}%";
        var query = _context.Recipes
            .AsNoTracking()
            .Where(recipe =>
                !recipe.IsDeleted
                && !recipe.Category.IsDeleted
                && recipe.Status == RecipeStatus.Published
                && (EF.Functions.ILike(recipe.Title, pattern)
                    || EF.Functions.ILike(recipe.Description, pattern)
                    || EF.Functions.ILike(recipe.Instructions, pattern)
                    || recipe.Ingredients.Any(ingredient =>
                        EF.Functions.ILike(ingredient.Name, pattern))))
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images.Where(image => image.IsPrimary));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(recipe => recipe.CreatedAt)
            .ThenBy(recipe => recipe.Id)
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
    public async Task<(IReadOnlyList<RecipeSearchMatch> Items, int TotalCount)> SearchAsync(
        string searchTerm,
        int page,
        int pageSize,
        string sort,
        CancellationToken cancellationToken = default,
        Guid? categoryId = null,
        DifficultyLevel? difficulty = null,
        int? maxCookTime = null)
    {
        var prefixQuery = RecipeSearchText.BuildPrefixQuery(searchTerm);
        if (string.IsNullOrEmpty(prefixQuery))
        {
            return (Array.Empty<RecipeSearchMatch>(), 0);
        }

        IQueryable<Recipe> matches = _context.Recipes
            .AsNoTracking()
            .Where(recipe => !recipe.IsDeleted && !recipe.Category.IsDeleted
                && recipe.Status == RecipeStatus.Published
                && EF.Property<NpgsqlTsVector>(recipe, "SearchVector")
                    .Matches(EF.Functions.ToTsQuery("public.vietnamese", prefixQuery)))
            .Include(recipe => recipe.Category)
            .Include(recipe => recipe.Images.Where(image => image.IsPrimary));

        if (categoryId.HasValue) matches = matches.Where(x => x.CategoryId == categoryId.Value);
        if (difficulty.HasValue) matches = matches.Where(x => x.Difficulty == difficulty.Value);
        if (maxCookTime.HasValue) matches = matches.Where(x => x.CookTimeMinutes <= maxCookTime.Value);

        var totalCount = await matches.CountAsync(cancellationToken);
        var query = matches.Select(recipe => new
        {
            Recipe = recipe,
            RelevanceScore = EF.Property<NpgsqlTsVector>(recipe, "SearchVector")
                .Rank(EF.Functions.ToTsQuery("public.vietnamese", prefixQuery))
        });

        var ordered = sort switch
        {
            "createdAt" => query.OrderBy(match => match.Recipe.CreatedAt),
            "-createdAt" => query.OrderByDescending(match => match.Recipe.CreatedAt),
            "title" => query.OrderBy(match => match.Recipe.Title),
            "-title" => query.OrderByDescending(match => match.Recipe.Title),
            "cookTime" => query.OrderBy(match => match.Recipe.CookTimeMinutes),
            "-cookTime" => query.OrderByDescending(match => match.Recipe.CookTimeMinutes),
            "prepTime" => query.OrderBy(match => match.Recipe.PrepTimeMinutes),
            "-prepTime" => query.OrderByDescending(match => match.Recipe.PrepTimeMinutes),
            "categoryName" => query.OrderBy(match => match.Recipe.Category.Name),
            "-categoryName" => query.OrderByDescending(match => match.Recipe.Category.Name),
            _ => query.OrderByDescending(match => match.RelevanceScore)
        };

        var items = await ordered
            .ThenByDescending(match => match.Recipe.CreatedAt)
            .ThenBy(match => match.Recipe.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.Select(item => new RecipeSearchMatch(item.Recipe, item.RelevanceScore)).ToList(), totalCount);
    }
}
