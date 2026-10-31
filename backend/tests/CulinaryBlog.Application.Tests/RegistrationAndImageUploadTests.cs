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
                null,
                "-createdAt"),
            CancellationToken.None);

        Assert.Equal(userId, repository.LastPagedCurrentUserId);
    }

    [Fact]
    public async Task RecipeList_AdminDoesNotIncludeOtherAuthorsPrivateStatuses()
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
                null,
                "-createdAt"),
            CancellationToken.None);

        Assert.False(repository.LastPagedIncludeAllStatuses);
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
            new FakeCurrentUser(Guid.NewGuid()),
            new FakeImageCleanupJobScheduler(),
            NullLogger<RecipeCommandHandler>.Instance);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                UpdateCommand(recipe, recipe.RowVersion),
                CancellationToken.None));
    }

    [Fact]
    public async Task UpdateRecipe_AllowsAdminToUpdateAnotherAuthorsRecipe()
    {
        var recipe = CreateRecipe(Guid.NewGuid());
        recipe.Status = RecipeStatus.Published;
        var repository = new FakeRecipeRepository(recipe);
        var unitOfWork = new FakeUnitOfWork(repository);
        var handler = new RecipeCommandHandler(
            repository,
            new FakeCategoryStore(),
            repository,
            unitOfWork,
            new FakeCurrentUser(Guid.NewGuid(), isAuthor: false, isAdmin: true),
            new FakeImageCleanupJobScheduler(),
            NullLogger<RecipeCommandHandler>.Instance);

        await handler.Handle(
            UpdateCommand(recipe, recipe.RowVersion),
            CancellationToken.None);

        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task UpdateRecipe_RejectsAdminForAnotherAuthorsDraft()
    {
        var recipe = CreateRecipe(Guid.NewGuid());
        var repository = new FakeRecipeRepository(recipe);
        var handler = new RecipeCommandHandler(
            repository,
            new FakeCategoryStore(),
            repository,
            new FakeUnitOfWork(repository),
            new FakeCurrentUser(Guid.NewGuid(), isAuthor: false, isAdmin: true),
            new FakeImageCleanupJobScheduler(),
            NullLogger<RecipeCommandHandler>.Instance);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(
                UpdateCommand(recipe, recipe.RowVersion),
                CancellationToken.None));
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
            new FakeCurrentUser(ownerId),
            new FakeImageCleanupJobScheduler(),
            NullLogger<RecipeCommandHandler>.Instance);

        await Assert.ThrowsAsync<
            CulinaryBlog.Domain.Exceptions.BusinessRuleException>(() =>
            handler.Handle(
                UpdateCommand(recipe, rowVersion: 4),
                CancellationToken.None));
    }

    [Fact]
    public async Task AddIngredient_AcceptsFreeFormQuantity()
    {
        var ownerId = Guid.NewGuid();
        var recipe = CreateRecipe(ownerId);
        var repository = new FakeRecipeRepository(recipe);
        var handler = CreateRecipeCommandHandler(
            repository,
            new FakeUnitOfWork(repository),
            ownerId);

        await handler.Handle(
            new AddRecipeIngredientCommand(
                recipe.Id,
                "Olive oil",
                "1/2",
                "tablespoon",
                null),
            CancellationToken.None);

        var ingredient = Assert.Single(recipe.Ingredients);
        Assert.Equal("1/2", ingredient.Quantity);
        Assert.Equal("tablespoon", ingredient.Unit);
    }

    [Fact]
    public async Task AddStep_RenumbersStepsContiguously()
    {
        var ownerId = Guid.NewGuid();
        var recipe = CreateRecipe(ownerId);
        var existingStep = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            StepNumber = 4,
            Description = "Prepare the pan."
        };
        recipe.Steps.Add(existingStep);
        var repository = new FakeRecipeRepository(recipe);
        var handler = CreateRecipeCommandHandler(
            repository,
            new FakeUnitOfWork(repository),
            ownerId);

        await handler.Handle(
            new AddRecipeStepCommand(
                recipe.Id,
                "Cook",
                "Cook until tender.",
                null,
                null),
            CancellationToken.None);

        Assert.Equal(
            new[] { 1, 2 },
            recipe.Steps.OrderBy(step => step.StepNumber)
                .Select(step => step.StepNumber));
    }

    [Fact]
    public async Task ReorderSteps_UsesRequestedOrderAndConsecutiveNumbers()
    {
        var ownerId = Guid.NewGuid();
        var recipe = CreateRecipe(ownerId);
        var first = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            StepNumber = 1,
            Description = "First."
        };
        var second = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            StepNumber = 2,
            Description = "Second."
        };
        recipe.Steps.Add(first);
        recipe.Steps.Add(second);
        var repository = new FakeRecipeRepository(recipe);
        var handler = CreateRecipeCommandHandler(
            repository,
            new FakeUnitOfWork(repository),
            ownerId);

        await handler.Handle(
            new ReorderRecipeStepsCommand(
                recipe.Id,
                [second.Id, first.Id]),
            CancellationToken.None);

        Assert.Equal(1, second.StepNumber);
        Assert.Equal(2, first.StepNumber);
    }

    [Fact]
    public async Task UpdateStep_RejectsDescriptionOverSrsLimit()
    {
        var ownerId = Guid.NewGuid();
        var recipe = CreateRecipe(ownerId);
        var step = new RecipeStep
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            StepNumber = 1,
            Description = "Original."
        };
        recipe.Steps.Add(step);
        var repository = new FakeRecipeRepository(recipe);
        var handler = CreateRecipeCommandHandler(
            repository,
            new FakeUnitOfWork(repository),
            ownerId);

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(
                new UpdateRecipeStepCommand(
                    recipe.Id,
                    step.Id,
                    "Step",
                    new string('x', 2001),
                    null,
                    null),
                CancellationToken.None));
    }

    [Fact]
    public async Task DeletePrimaryImage_QueuesFilesAfterSavingAndPromotesNextImage()
    {
        var ownerId = Guid.NewGuid();
        var recipe = CreateRecipe(ownerId);
        var primary = new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            OriginalUrl = "/uploads/primary.png",
            MediumUrl = "/uploads/primary-medium.webp",
            ThumbnailUrl = "/uploads/primary-thumb.webp",
            IsPrimary = true,
            OrderIndex = 1
        };
        var replacement = new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = recipe.Id,
            OriginalUrl = "/uploads/replacement.png",
            IsPrimary = false,
            OrderIndex = 2
        };
        recipe.Images.Add(primary);
        recipe.Images.Add(replacement);
        var repository = new FakeRecipeRepository(recipe);
        var unitOfWork = new FakeUnitOfWork(repository);
        var cleanupJobs = new FakeImageCleanupJobScheduler();
        var handler = CreateRecipeCommandHandler(
            repository,
            unitOfWork,
            ownerId,
            cleanupJobs);

        await handler.Handle(
            new DeleteRecipeImageCommand(recipe.Id, primary.Id),
            CancellationToken.None);

        Assert.DoesNotContain(primary, recipe.Images);
        Assert.True(replacement.IsPrimary);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(
            new[]
            {
                primary.OriginalUrl,
                primary.MediumUrl,
                primary.ThumbnailUrl
            },
            cleanupJobs.EnqueuedFileUrls);
    }

    private static RecipeCommandHandler CreateRecipeCommandHandler(
        FakeRecipeRepository repository,
        FakeUnitOfWork unitOfWork,
        Guid userId,
        FakeImageCleanupJobScheduler? cleanupJobs = null) =>
        new(
            repository,
            new FakeCategoryStore(),
            repository,
            unitOfWork,
            new FakeCurrentUser(userId),
            cleanupJobs ?? new FakeImageCleanupJobScheduler(),
            NullLogger<RecipeCommandHandler>.Instance);

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

    private sealed class FakeImageCleanupJobScheduler
        : IImageCleanupJobScheduler
    {
        public List<string> EnqueuedFileUrls { get; } = [];

        public void Enqueue(string fileUrl) => EnqueuedFileUrls.Add(fileUrl);
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

        public Task ExecuteInTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default) =>
            operation(cancellationToken);
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
                string? search,
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

        public Task<(IReadOnlyList<Recipe> Items, int TotalCount)>
                SearchPublishedAsync(
                    string searchTerm,
                    int page,
                    int pageSize,
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
