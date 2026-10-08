using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Helpers;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using CulinaryBlog.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace CulinaryBlog.Application.Tests;

public sealed class CategoryModuleTests
{
    [Fact]
    public void SlugHelper_RemovesVietnameseDiacritics()
    {
        var slug = SlugHelper.Generate("  Món Chính Đặc Biệt!  ");

        Assert.Equal("mon-chinh-dac-biet", slug);
    }

    [Fact]
    public void CreateValidator_RejectsHtmlAndShortName()
    {
        var validator = new CreateCategoryCommandValidator();

        var errors = validator.Validate(
            new CreateCategoryCommand("<b>x</b>", null, null));

        Assert.Contains(errors, error => error.Contains("HTML"));
    }

    [Fact]
    public void UpdateValidator_RequiresAtLeastOneField()
    {
        var validator = new UpdateCategoryCommandValidator();

        var errors = validator.Validate(
            new UpdateCategoryCommand(
                Guid.NewGuid(),
                null,
                null,
                null,
                null));

        Assert.Contains(errors, error => error.Contains("At least one"));
    }

    [Fact]
    public async Task CreateHandler_GeneratesUniqueSlugAndInvalidatesCache()
    {
        var repository = new FakeCategoryRepository();
        repository.Categories.Add(
            Category.Create(
                "Món-Chính",
                "mon-chinh",
                null,
                null,
                0));
        var unitOfWork = new FakeUnitOfWork(repository);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        cache.Set(GetCategoriesQueryHandler.CacheKey, new object());
        var handler = new CreateCategoryCommandHandler(
            unitOfWork,
            cache,
            FakeCurrentUser.Admin);

        var result = await handler.Handle(
            new CreateCategoryCommand(
                "Món Chính",
                "Món chính hằng ngày",
                null),
            CancellationToken.None);

        Assert.Equal("mon-chinh-2", result.Slug);
        Assert.Equal(2, repository.Categories.Count);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.False(cache.TryGetValue(
            GetCategoriesQueryHandler.CacheKey,
            out _));
    }

    [Fact]
    public async Task UpdateHandler_KeepsOriginalSlugWhenNameChanges()
    {
        var category = Category.Create(
            "Món cũ",
            "mon-cu",
            null,
            null,
            0);
        var repository = new FakeCategoryRepository();
        repository.Categories.Add(category);
        var handler = new UpdateCategoryCommandHandler(
            new FakeUnitOfWork(repository),
            new MemoryCache(new MemoryCacheOptions()),
            FakeCurrentUser.Admin);

        var result = await handler.Handle(
            new UpdateCategoryCommand(
                category.Id,
                "Món mới",
                "Mô tả mới",
                null,
                3),
            CancellationToken.None);

        Assert.Equal("Món mới", result.Name);
        Assert.Equal("mon-cu", result.Slug);
        Assert.Equal(3, result.OrderIndex);
    }

    [Fact]
    public async Task DeleteHandler_RejectsCategoryContainingRecipes()
    {
        var category = Category.Create(
            "Món chính",
            "mon-chinh",
            null,
            null,
            0);
        var repository = new FakeCategoryRepository();
        repository.Categories.Add(category);
        repository.RecipeCounts[category.Id] = 2;
        var handler = new DeleteCategoryCommandHandler(
            new FakeUnitOfWork(repository),
            new MemoryCache(new MemoryCacheOptions()),
            FakeCurrentUser.Admin);

        var exception = await Assert.ThrowsAsync<
            CulinaryBlog.Domain.Exceptions.ConflictException>(
            () => handler.Handle(
                new DeleteCategoryCommand(category.Id),
                CancellationToken.None));

        Assert.Contains("2", exception.Message);
        Assert.False(category.IsDeleted);
    }

    [Fact]
    public async Task DeleteHandler_SoftDeletesEmptyCategory()
    {
        var category = Category.Create(
            "Món trống",
            "mon-trong",
            null,
            null,
            0);
        var repository = new FakeCategoryRepository();
        repository.Categories.Add(category);
        var unitOfWork = new FakeUnitOfWork(repository);
        var handler = new DeleteCategoryCommandHandler(
            unitOfWork,
            new MemoryCache(new MemoryCacheOptions()),
            FakeCurrentUser.Admin);

        await handler.Handle(
            new DeleteCategoryCommand(category.Id),
            CancellationToken.None);

        Assert.True(category.IsDeleted);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task CreateHandler_RejectsNonAdminAtApplicationLayer()
    {
        var repository = new FakeCategoryRepository();
        var handler = new CreateCategoryCommandHandler(
            new FakeUnitOfWork(repository),
            new MemoryCache(new MemoryCacheOptions()),
            FakeCurrentUser.Author);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(
                new CreateCategoryCommand("Món mới", null, null),
                CancellationToken.None));
    }

    private sealed class FakeCurrentUser : ICurrentUser
    {
        public static FakeCurrentUser Admin { get; } =
            new(true, true);

        public static FakeCurrentUser Author { get; } =
            new(true, false);

        private FakeCurrentUser(
            bool isAuthenticated,
            bool isAdmin)
        {
            IsAuthenticated = isAuthenticated;
            IsAdmin = isAdmin;
        }

        public Guid? UserId { get; } = Guid.NewGuid();
        public bool IsAuthenticated { get; }
        public bool IsAdmin { get; }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public FakeUnitOfWork(ICategoryRepository categories)
        {
            Categories = categories;
        }

        public IRecipeRepository Recipes { get; } = null!;
        public ICategoryRepository Categories { get; }
        public int SaveCount { get; private set; }
        public void AddRecipeImage(RecipeImage image) { }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public List<Category> Categories { get; } = [];
        public Dictionary<Guid, int> RecipeCounts { get; } = [];

        public Task<Category?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.FirstOrDefault(
                category => category.Id == id && !category.IsDeleted));

        public Task<Category?> GetActiveByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);

        public Task<Category?> GetActiveByIdReadOnlyAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            GetByIdAsync(id, cancellationToken);

        public Task<IReadOnlyList<Category>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Category>>(
                Categories.Where(category => !category.IsDeleted).ToList());

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.Any(category => category.Id == id));

        public Task AddAsync(
            Category entity,
            CancellationToken cancellationToken = default)
        {
            Categories.Add(entity);
            return Task.CompletedTask;
        }

        public void Update(Category entity)
        {
        }

        public void Delete(Category entity) => Categories.Remove(entity);
        public void Remove(Category entity) => Delete(entity);

        public Task<Category?> GetBySlugAsync(
            string slug,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.FirstOrDefault(
                category => category.Slug == slug && !category.IsDeleted));

        public Task<IReadOnlyList<CategoryWithRecipeCount>>
            GetAllWithPublishedRecipeCountAsync(
                CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CategoryWithRecipeCount>>(
                Categories
                    .Where(category => !category.IsDeleted)
                    .OrderBy(category => category.Name)
                    .Select(category => new CategoryWithRecipeCount(
                        category,
                        RecipeCounts.GetValueOrDefault(category.Id)))
                    .ToList());

        public Task<int> GetPublishedRecipeCountAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(RecipeCounts.GetValueOrDefault(categoryId));

        public Task<int> GetRecipeCountAsync(
            Guid categoryId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(RecipeCounts.GetValueOrDefault(categoryId));

        public Task<bool> NameExistsAsync(
            string name,
            Guid? excludingId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.Any(category =>
                category.Id != excludingId
                && string.Equals(
                    category.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase)));

        public Task<bool> SlugExistsAsync(
            string slug,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Categories.Any(category =>
                category.Slug == slug));
    }
}
