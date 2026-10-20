using CulinaryBlog.Application.Features.Recipes.Queries.SearchRecipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CULINARYBLOG_TEST_POSTGRES")))
            Skip = "Set CULINARYBLOG_TEST_POSTGRES to run against an isolated PostgreSQL test database.";
    }
}

// Each test creates and removes its own uniquely named database. Never migrate,
// seed, or delete data in the database named by the supplied connection string.
public sealed class RecipeFullTextSearchTests : IAsyncLifetime
{
    private readonly string _databaseName = $"culinary_fts_test_{Guid.NewGuid():N}";
    private NpgsqlConnection? _admin;
    private CulinaryBlogDbContext _db = null!;
    private bool _created;
    private SearchRecipesQueryHandler Handler => new(new RecipeRepository(_db));

    public async Task InitializeAsync()
    {
        var settings = new NpgsqlConnectionStringBuilder(Environment.GetEnvironmentVariable("CULINARYBLOG_TEST_POSTGRES"))
        {
            Database = "postgres",
            Pooling = false,
            Timeout = 5
        };
        _admin = new NpgsqlConnection(settings.ConnectionString);
        await _admin.OpenAsync();
        await new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", _admin).ExecuteNonQueryAsync();
        _created = true;
        settings.Database = _databaseName;
        _db = new CulinaryBlogDbContext(new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(settings.ConnectionString).Options);
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (_db is not null) await _db.DisposeAsync();
        if (_admin is not null)
        {
            if (_created)
                await new NpgsqlCommand($"DROP DATABASE \"{_databaseName}\" WITH (FORCE)", _admin).ExecuteNonQueryAsync();
            await _admin.DisposeAsync();
        }
    }

    private Category AddCategory(bool deleted = false)
    {
        var category = Category.Create($"Món chính {Guid.NewGuid():N}", $"mon-{Guid.NewGuid():N}", null, null, 0);
        category.IsDeleted = deleted;
        _db.Categories.Add(category);
        return category;
    }

    private Recipe AddRecipe(Category category, string title, string description = "Hương vị gia đình",
        RecipeStatus status = RecipeStatus.Published, bool deleted = false, DateTime? createdAt = null)
    {
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            Title = title,
            Slug = $"mon-{Guid.NewGuid():N}",
            Description = description,
            Instructions = "Chuẩn bị nguyên liệu và nấu chín.",
            Category = category,
            CategoryId = category.Id,
            PrepTimeMinutes = 10,
            CookTimeMinutes = 30,
            Servings = 2,
            Difficulty = DifficultyLevel.Easy,
            Status = status,
            IsDeleted = deleted,
            CreatedAt = createdAt ?? DateTime.UtcNow
        };
        _db.Recipes.Add(recipe);
        return recipe;
    }

    [PostgresFact]
    public async Task AccentInsensitivePrefixSearch_RanksBeforePagination_AndExcludesHiddenRecipes()
    {
        var category = AddCategory();
        var olderTitleMatch = AddRecipe(category, "Phở bò đặc biệt", createdAt: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        var newerDescriptionMatch = AddRecipe(category, "Bữa tối gia đình", "Phở bò thơm ngon",
            createdAt: new DateTime(2026, 10, 20, 0, 0, 0, DateTimeKind.Utc));
        AddRecipe(category, "Phở bò nháp", status: RecipeStatus.Draft);
        AddRecipe(category, "Phở bò lưu trữ", status: RecipeStatus.Archived);
        AddRecipe(category, "Phở bò đã xóa", deleted: true);
        AddRecipe(AddCategory(deleted: true), "Phở bò danh mục đã xóa");
        olderTitleMatch.Images.Add(new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = olderTitleMatch.Id,
            OriginalUrl = "/uploads/pho.jpg",
            ThumbnailUrl = "/uploads/pho-thumb.jpg",
            IsPrimary = true
        });
        await _db.SaveChangesAsync();

        var first = await Handler.Handle(new("pho b", 1, 1), CancellationToken.None);
        var second = await Handler.Handle(new("pho b", 2, 1), CancellationToken.None);
        Assert.Equal(2, first.TotalCount);
        Assert.True(first.HasNextPage);
        Assert.Equal(olderTitleMatch.Id, Assert.Single(first.Items).Id);
        Assert.Equal(newerDescriptionMatch.Id, Assert.Single(second.Items).Id);
        Assert.True(first.Items[0].RelevanceScore > second.Items[0].RelevanceScore);
        Assert.True(second.Items[0].RelevanceScore > 0);
        Assert.Equal("/uploads/pho-thumb.jpg", first.Items[0].ThumbnailUrl);
        Assert.Equal(category.Name, first.Items[0].CategoryName);

        var accented = await Handler.Handle(new("PHỞ BÒ", 1, 10), CancellationToken.None);
        Assert.Equal(new[] { olderTitleMatch.Id, newerDescriptionMatch.Id }, accented.Items.Select(item => item.Id));
        var explicitSort = await Handler.Handle(new("pho bo", 1, 10, "-createdAt"), CancellationToken.None);
        Assert.Equal(newerDescriptionMatch.Id, explicitSort.Items[0].Id);
    }

    [PostgresFact]
    public async Task Trigger_RefreshesVectorForTitleDescriptionAndInstructions()
    {
        var recipe = AddRecipe(AddCategory(), "Món ban đầu");
        await _db.SaveChangesAsync();
        Assert.Empty((await Handler.Handle(new("pho", 1, 10), CancellationToken.None)).Items);
        recipe.Title = "Phở gà";
        await _db.SaveChangesAsync();
        Assert.Single((await Handler.Handle(new("pho", 1, 10), CancellationToken.None)).Items);
        recipe.Title = "Món ban đầu";
        recipe.Description = "Phở bò";
        await _db.SaveChangesAsync();
        Assert.Single((await Handler.Handle(new("pho", 1, 10), CancellationToken.None)).Items);
        recipe.Description = "Hương vị gia đình";
        recipe.Instructions = "Nấu phở thật ngon";
        await _db.SaveChangesAsync();
        Assert.Single((await Handler.Handle(new("pho", 1, 10), CancellationToken.None)).Items);
        recipe.Instructions = "Nấu chín";
        await _db.SaveChangesAsync();
        var empty = await Handler.Handle(new("pho", 1, 10), CancellationToken.None);
        Assert.Empty(empty.Items);
        Assert.NotNull(empty.Message);
    }

    [PostgresFact]
    public async Task InjectionShapedInput_IsLiteralAndDoesNotModifyDatabase()
    {
        AddRecipe(AddCategory(), "Phở bò");
        await _db.SaveChangesAsync();
        var result = await Handler.Handle(new("pho'); DROP TABLE Recipes; --", 1, 10), CancellationToken.None);
        Assert.Empty(result.Items);
        Assert.Equal(1, await _db.Recipes.CountAsync());
        var operators = await Handler.Handle(new("pho & bo:* | !", 1, 10), CancellationToken.None);
        Assert.Single(operators.Items);
    }

    [PostgresFact]
    public async Task Migration_UpgradesExistingVectors_AndRollbackPreservesRecipes()
    {
        var migrator = _db.GetService<IMigrator>();
        await migrator.MigrateAsync("20261014173100_AddRecipeSearchVector");
        var recipe = AddRecipe(AddCategory(), "Bánh bột lọc");
        await _db.SaveChangesAsync();
        // The database is disposable; avoid Down(), which drops the pre-existing SearchVector.
        await _db.Database.MigrateAsync();
        var result = await Handler.Handle(new("banh bo", 1, 10), CancellationToken.None);
        Assert.Equal(recipe.Id, Assert.Single(result.Items).Id);
        Assert.False(_db.Database.HasPendingModelChanges());
        await _db.Database.OpenConnectionAsync();
        var connection = (NpgsqlConnection)_db.Database.GetDbConnection();
        Assert.Equal(2L, await new NpgsqlCommand("SELECT count(*) FROM pg_extension WHERE extname IN ('unaccent', 'pg_trgm')", connection).ExecuteScalarAsync());
        Assert.Equal("vietnamese", await new NpgsqlCommand("SELECT cfgname FROM pg_ts_config c JOIN pg_namespace n ON n.oid = c.cfgnamespace WHERE n.nspname = 'public' AND c.cfgname = 'vietnamese'", connection).ExecuteScalarAsync());
        var index = (string?)await new NpgsqlCommand("SELECT indexdef FROM pg_indexes WHERE indexname = 'IX_Recipes_SearchVector'", connection).ExecuteScalarAsync();
        Assert.Contains("USING gin", index);
        await _db.Database.CloseConnectionAsync();
    }
}
