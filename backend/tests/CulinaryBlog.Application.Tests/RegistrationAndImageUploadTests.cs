using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Recipes.Commands;
using CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;
using CulinaryBlog.Application.Features.Recipes.Queries.GetRecipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.Application.Tests;

public sealed class RegistrationAndImageUploadTests
{
    [Fact]
    public async Task UploadImage_FirstValidImageBecomesPrimary()
    {
        var userId = Guid.NewGuid();
        var recipe = CreateRecipe(userId);
        var repository = new FakeRecipeRepository(recipe);
        var unitOfWork = new FakeUnitOfWork(repository);
        var storage = new FakeFileStorageService();
        var jobs = new FakeImageResizeJobScheduler();
        var handler = new UploadRecipeImageCommandHandler(
            unitOfWork,
            new FakeCurrentUser(userId),
            storage,
            jobs,
            NullLogger<UploadRecipeImageCommandHandler>.Instance);

        var result = await handler.Handle(
            new UploadRecipeImageCommand(
                recipe.Id,
                ValidPngBytes(),
                "image/png",
                "Test image",
                null),
            CancellationToken.None);

        Assert.True(result.IsPrimary);
        Assert.Equal(1, result.OrderIndex);
        Assert.Single(recipe.Images);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(1, storage.UploadCount);
        Assert.Equal(result.Id, Assert.Single(jobs.EnqueuedImageIds));
    }

    [Fact]
    public async Task UploadImage_RejectsInvalidMagicBytes()
    {
        var userId = Guid.NewGuid();
        var handler = new UploadRecipeImageCommandHandler(
            new FakeUnitOfWork(
                new FakeRecipeRepository(CreateRecipe(userId))),
            new FakeCurrentUser(userId),
            new FakeFileStorageService(),
            new FakeImageResizeJobScheduler(),
            NullLogger<UploadRecipeImageCommandHandler>.Instance);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.Handle(
                new UploadRecipeImageCommand(
                    Guid.NewGuid(),
                    [1, 2, 3, 4],
                    "image/png",
                    null,
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task UploadImage_RejectsNonOwner()
    {
        var recipe = CreateRecipe(Guid.NewGuid());
        var handler = new UploadRecipeImageCommandHandler(
            new FakeUnitOfWork(new FakeRecipeRepository(recipe)),
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeFileStorageService(),
            new FakeImageResizeJobScheduler(),
            NullLogger<UploadRecipeImageCommandHandler>.Instance);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                new UploadRecipeImageCommand(
                    recipe.Id,
                    ValidPngBytes(),
                    "image/png",
                    null,
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task RecipeList_DoesNotPassPrivateOwnerScopeForNonAuthors()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeRecipeRepository(
            CreateRecipe(userId));
        var handler = new GetRecipesQueryHandler(
            repository,
            new FakeCurrentUser(userId, isAuthor: false));

        await handler.Handle(
            new GetRecipesQuery(
                1,
                12,
                null,
                null,
                null,
                "-createdAt"),
            CancellationToken.None);

        Assert.Null(repository.LastPagedCurrentUserId);
    }

    [Fact]
    public async Task RecipeList_PassesOwnerScopeForAuthors()
    {
        var userId = Guid.NewGuid();
        var repository = new FakeRecipeRepository(
            CreateRecipe(userId));
        var handler = new GetRecipesQueryHandler(
            repository,
            new FakeCurrentUser(userId));

        await handler.Handle(
            new GetRecipesQuery(
                1,
                12,
                null,
                null,
                null,
                "-createdAt"),
            CancellationToken.None);

        Assert.Equal(userId, repository.LastPagedCurrentUserId);
    }

    [Fact]
    public async Task RecipeList_AdminIncludesAllStatusesByDefault()
    {
        var repository = new FakeRecipeRepository(
            CreateRecipe(Guid.NewGuid()));
        var handler = new GetRecipesQueryHandler(
            repository,
            new FakeCurrentUser(Guid.NewGuid(), isAuthor: false, isAdmin: true));

        await handler.Handle(
            new GetRecipesQuery(
                1,
                12,
                null,
                null,
                null,
                "-createdAt"),
            CancellationToken.None);

        Assert.True(repository.LastPagedIncludeAllStatuses);
    }

    [Fact]
    public async Task UpdateRecipe_RejectsNonOwner()
    {
        var recipe = CreateRecipe(Guid.NewGuid());
        var repository = new FakeRecipeRepository(recipe);
        var handler = new RecipeCommandHandler(
            repository,
            new FakeCategoryStore(),
            repository,
            new FakeUnitOfWork(repository),
            new FakeCurrentUser(Guid.NewGuid()));

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                UpdateCommand(recipe, recipe.RowVersion),
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateRecipe_AllowsAdminToUpdateAnotherAuthorsRecipe()
    {
        var recipe = CreateRecipe(Guid.NewGuid());
        var repository = new FakeRecipeRepository(recipe);
        var unitOfWork = new FakeUnitOfWork(repository);
        var handler = new RecipeCommandHandler(
            repository,
            new FakeCategoryStore(),
            repository,
            unitOfWork,
            new FakeCurrentUser(Guid.NewGuid(), isAuthor: false, isAdmin: true));

        await handler.Handle(
            UpdateCommand(recipe, recipe.RowVersion),
            CancellationToken.None);

        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateRecipe_RejectsStaleRowVersion()
    {
        var ownerId = Guid.NewGuid();
        var recipe = CreateRecipe(ownerId);
        recipe.RowVersion = 5;
        var repository = new FakeRecipeRepository(recipe);
        var handler = new RecipeCommandHandler(
            repository,
            new FakeCategoryStore(),
            repository,
            new FakeUnitOfWork(repository),
            new FakeCurrentUser(ownerId));

        await Assert.ThrowsAsync<
            CulinaryBlog.Domain.Exceptions.BusinessRuleException>(() =>
            handler.Handle(
                UpdateCommand(recipe, rowVersion: 4),
                CancellationToken.None));
    }

    private static UpdateRecipeCommand UpdateCommand(
        Recipe recipe,
        uint rowVersion) =>
        new(
            recipe.Id,
            "Updated recipe",
            "Updated description",
            null,
            recipe.CategoryId,
            10,
            20,
            2,
            DifficultyLevel.Easy,
            rowVersion,
            null);

    private static Recipe CreateRecipe(Guid authorId) =>
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Test recipe",
            Slug = "test-recipe",
            Description = "Test",
            Instructions = "Test",
            CategoryId = Guid.NewGuid(),
            AuthorId = authorId,
            Status = RecipeStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

    private static byte[] ValidPngBytes() =>
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    private sealed class FakeCurrentUser(
        Guid userId,
        bool isAuthor = true,
        bool isAdmin = false) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
        public bool IsAuthor { get; } = isAuthor;
        public bool IsAdmin { get; } = isAdmin;
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public int UploadCount { get; private set; }

        public Task<byte[]> ReadAsync(
            string fileUrl,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Array.Empty<byte>());

        public Task<string> UploadAsync(
            ReadOnlyMemory<byte> content,
            string extension,
            string folder,
            CancellationToken cancellationToken = default)
        {
            UploadCount++;
            return Task.FromResult(
                $"/uploads/{folder}/test{extension}");
        }

        public Task DeleteAsync(
            string fileUrl,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeImageResizeJobScheduler
        : IImageResizeJobScheduler
    {
        public List<Guid> EnqueuedImageIds { get; } = [];

        public void Enqueue(Guid imageId) => EnqueuedImageIds.Add(imageId);
    }

    private sealed class FakeUnitOfWork(IRecipeRepository recipes)
        : IUnitOfWork
    {
        public IRecipeRepository Recipes { get; } = recipes;
        public ICategoryRepository Categories { get; } = null!;
        public int SaveCount { get; private set; }
        public void AddRecipeImage(RecipeImage image) { }

        public Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeCategoryStore : IRepository<Category>
    {
        public Task<Category?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Category?>(new Category { Id = id });

        public Task<IReadOnlyList<Category>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Category>>([]);

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(true);

        public Task AddAsync(
            Category entity,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Category entity) { }
        public void Delete(Category entity) { }
        public void Remove(Category entity) { }
    }

    private sealed class FakeRecipeRepository(Recipe recipe)
        : IRecipeRepository
    {
        public Task<(IReadOnlyList<Recipe> Items, int TotalCount)> SearchAsync(
            string searchTerm, int page, int pageSize, string sort,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Recipe>, int)>(([], 0));

        public Guid? LastPagedCurrentUserId { get; private set; }
        public bool LastPagedIncludeAllStatuses { get; private set; }

        public Task<Recipe?> GetByIdWithImagesAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(id == recipe.Id ? recipe : null);

        public Task<Recipe?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            GetByIdWithImagesAsync(id, cancellationToken);

        public Task<IReadOnlyList<Recipe>> GetAllAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Recipe>>([recipe]);

        public Task<bool> ExistsAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(id == recipe.Id);

        public Task AddAsync(
            Recipe entity,
            CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public void Update(Recipe entity)
        {
        }

        public void Delete(Recipe entity)
        {
        }

        public void Remove(Recipe entity)
        {
        }

        public Task<Recipe?> GetBySlugWithDetailsAsync(
            string slug,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<Recipe?>(null);

        public Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
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
                LastPagedCurrentUserId = currentUserId;
                LastPagedIncludeAllStatuses = includeAllStatuses;
                return Task.FromResult<(IReadOnlyList<Recipe>, int)>(([], 0));
        }

        public Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
            GetPagedForCategoryAsync(
                Guid categoryId,
                int page,
                int pageSize,
                Guid? currentUserId,
                bool isAuthenticated,
                bool isAdmin,
                CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Recipe>, int)>(([], 0));

        public Task<Recipe?> GetByIdWithDetailsAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(id == recipe.Id ? recipe : null);

        public Task<bool> SlugExistsAsync(
            string slug,
            Guid? excludingRecipeId = null,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(
                recipe.Slug == slug
                && (!excludingRecipeId.HasValue
                    || excludingRecipeId.Value != recipe.Id));
    }
}
