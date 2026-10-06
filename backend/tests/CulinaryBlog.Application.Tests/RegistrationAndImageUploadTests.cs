using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using CulinaryBlog.Application.Features.Recipes.Commands.UploadRecipeImage;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Domain.Interfaces;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.Application.Tests;

public sealed class RegistrationAndImageUploadTests
{
    [Fact]
    public void RegisterValidator_RejectsWeakInput()
    {
        var validator = new RegisterCommandValidator();

        var errors = validator.Validate(new RegisterCommand(
            "A",
            "not-an-email",
            "bad name",
            "123"));

        Assert.Equal(4, errors.Count);
    }

    [Fact]
    public async Task RegisterHandler_ReturnsTokensAndAuthorUser()
    {
        var identity = new FakeIdentityService();
        var jwt = new FakeJwtService();
        var handler = new RegisterCommandHandler(identity, jwt);

        var result = await handler.Handle(
            new RegisterCommand(
                "Nguyen Van Test",
                "TEST@example.com",
                "test_user",
                "Password@1"),
            CancellationToken.None);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("refresh-token", result.RefreshToken);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.Equal("Author", Assert.Single(result.User.Roles));
        Assert.NotNull(identity.CreatedUser);
        Assert.Equal("refresh-token-hash", identity.RefreshTokenHash);
    }

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

    private sealed class FakeIdentityService : IIdentityService
    {
        public ApplicationUser? CreatedUser { get; private set; }
        public string? RefreshTokenHash { get; private set; }

        public Task<ApplicationUser> RegisterAsync(
            string fullName,
            string email,
            string userName,
            string password,
            string refreshTokenHash,
            DateTime refreshTokenExpiresAt,
            CancellationToken cancellationToken = default)
        {
            RefreshTokenHash = refreshTokenHash;
            CreatedUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                DisplayName = fullName,
                Email = email,
                UserName = userName,
                Role = "Author"
            };

            return Task.FromResult(CreatedUser);
        }
    }

    private sealed class FakeJwtService : IJwtService
    {
        public AccessTokenResult GenerateAccessToken(ApplicationUser user) =>
            new("access-token", DateTime.UtcNow.AddMinutes(15));

        public string GenerateRefreshToken() => "refresh-token";
        public string HashRefreshToken(string refreshToken) =>
            "refresh-token-hash";

        public DateTime GetRefreshTokenExpiration() =>
            DateTime.UtcNow.AddDays(7);
    }

    private sealed class FakeCurrentUser(Guid userId) : ICurrentUser
    {
        public Guid? UserId { get; } = userId;
        public bool IsAuthenticated => true;
        public bool IsAdmin => false;
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

    private sealed class FakeRecipeRepository(Recipe recipe)
        : IRecipeRepository
    {
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
                CancellationToken cancellationToken = default) =>
            Task.FromResult<(IReadOnlyList<Recipe>, int)>(([], 0));

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
    }
}
