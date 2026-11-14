using CulinaryBlog.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    private const int MinimumCategoryCount = 20;
    private const int MinimumRecipeCount = 100;

    public static async Task SeedAsync(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        CancellationToken cancellationToken = default)
    {
        await context.Database.MigrateAsync(cancellationToken);

        await EnsureUsersAsync(
            context,
            passwordHasher,
            cancellationToken);
        await SeedCuratedContentAsync(context, cancellationToken);
    }

    private static async Task SeedCuratedContentAsync(
        CulinaryBlogDbContext context,
        CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            foreach (var fileName in new[] { "real-content.sql", "author-recipes.sql" })
            {
                var resourceName =
                    $"CulinaryBlog.Infrastructure.Persistence.Seed.{fileName}";
                await using var stream = typeof(DatabaseSeeder).Assembly
                    .GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Missing seed resource: {resourceName}");
                using var reader = new StreamReader(stream);
                var sql = await reader.ReadToEndAsync(cancellationToken);
                await using var command = context.Database.GetDbConnection().CreateCommand();
                command.CommandText = sql;
                await command.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    private static async Task EnsureUsersAsync(
        CulinaryBlogDbContext context,
        IPasswordHasher<ApplicationUser> passwordHasher,
        CancellationToken cancellationToken)
    {
        var seedUsers = new (string Email, string UserName, string DisplayName, string Password, string Role)[]
        {
            ("admin@culinaryblog.com", "admin", "System Admin", "Admin@123", "Admin"),
            ("author@culinaryblog.com", "author", "Sample Author", "Author@123", "Author"),
            ("user@culinaryblog.com", "user", "Sample User", "User@123", "User")
        };

        foreach (var seedUser in seedUsers)
        {
            var accountExists = await context.ApplicationUsers.AnyAsync(
                user => user.Email.ToLower() == seedUser.Email
                    || user.UserName.ToLower() == seedUser.UserName,
                cancellationToken);

            if (accountExists)
            {
                continue;
            }

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                Email = seedUser.Email,
                UserName = seedUser.UserName,
                DisplayName = seedUser.DisplayName,
                Role = seedUser.Role,
                CreatedAt = DateTime.UtcNow
            };
            user.PasswordHash = passwordHasher.HashPassword(user, seedUser.Password);

            await context.ApplicationUsers.AddAsync(user, cancellationToken);
        }

        if (context.ChangeTracker.HasChanges())
        {
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
