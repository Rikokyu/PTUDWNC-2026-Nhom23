using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using CulinaryBlog.Domain.Exceptions;

public class UnitOfWork : IUnitOfWork
{
    private readonly CulinaryBlogDbContext _context;

    public UnitOfWork(CulinaryBlogDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return SaveChangesCoreAsync(cancellationToken);
    }

    private async Task<int> SaveChangesCoreAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new BusinessRuleException(
                "The resource was modified by another request. Reload it and try again.");
        }
    }
}