using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class UserDirectoryTests
{
    private sealed record User(bool IsAuthenticated, bool IsAuthor, bool IsAdmin) : ICurrentUser
    {
        public Guid? UserId => null;
    }

    private static CulinaryBlogDbContext Database() => new(new DbContextOptionsBuilder<CulinaryBlogDbContext>()
        .UseNpgsql("Host=127.0.0.1;Port=1;Database=unused;Username=unused;Timeout=1").Options);

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    public async Task UnauthorizedAccessFailsBeforeDatabase(bool authenticated, bool author, bool admin)
    {
        await using var db = Database();
        var directory = new UserDirectory(db, new User(authenticated, author, admin));
        await Assert.ThrowsAsync<ForbiddenException>(() => directory.GetAsync(1, 12, null, null, default));
    }

    [Theory]
    [InlineData(0, 12)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    [InlineData(int.MaxValue, 50)]
    public async Task InvalidPaginationFailsBeforeDatabase(int page, int size)
    {
        await using var db = Database();
        var directory = new UserDirectory(db, new User(true, true, false));
        await Assert.ThrowsAsync<ValidationException>(() => directory.GetAsync(page, size, null, null, default));
    }
}
