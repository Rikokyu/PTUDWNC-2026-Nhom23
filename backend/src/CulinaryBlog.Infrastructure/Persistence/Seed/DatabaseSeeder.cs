using CulinaryBlog.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
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
            foreach (var fileName in new[]
                     {
                         "real-content.sql",
                         "author-recipes.sql",
                         "additional-recipes.sql",
                         "commons-photo-assignments.sql"
                     })
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
        var seedUsers = new (string Email, string UserName, string DisplayName, string Password, string Role, string? Bio)[]
        {
            ("admin@culinaryblog.com", "admin", "Quản trị viên", "Admin@123", "Admin", "Quản lý nội dung và vận hành Culinary Blog."),
            ("author@culinaryblog.com", "author", "Nguyễn Lan", "Author@123", "Author", "Chia sẻ những món ăn gia đình và công thức quen thuộc từ căn bếp Việt."),
            ("minhtran@culinaryblog.com", "minhtran", "Trần Minh", "Minh@123", "Author", "Yêu thích các món ăn thường ngày, món xào và cách nấu gọn nhẹ."),
            ("phuong@culinaryblog.com", "phuong", "Bếp nhà Phương", "Phuong@123", "Author", "Ghi lại những món Việt thân thuộc và mẹo nấu ăn trong căn bếp gia đình."),
            ("thanh@culinaryblog.com", "thanh", "Phạm Thanh Hà", "Thanh@123", "Author", "Chia sẻ món ăn theo mùa, món ngọt và cách chuẩn bị bữa cơm nhà."),
            ("user@culinaryblog.com", "user", "Nguyễn Gia Bảo", "User@123", "User", null),
            ("an@culinaryblog.com", "an", "Lê Khánh An", "An@123", "User", null)
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
                Bio = seedUser.Bio,
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
