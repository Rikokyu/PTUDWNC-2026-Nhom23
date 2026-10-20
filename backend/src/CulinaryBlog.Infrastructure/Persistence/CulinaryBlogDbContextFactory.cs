using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CulinaryBlog.Infrastructure.Persistence;

public sealed class CulinaryBlogDbContextFactory
    : IDesignTimeDbContextFactory<CulinaryBlogDbContext>
{
    public CulinaryBlogDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=culinary_blog;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CulinaryBlogDbContext(options);
    }
}
