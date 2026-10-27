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
                "ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Set ConnectionStrings__DefaultConnection for EF design-time operations.");
        }

        var options = new DbContextOptionsBuilder<CulinaryBlogDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CulinaryBlogDbContext(options);
    }
}
