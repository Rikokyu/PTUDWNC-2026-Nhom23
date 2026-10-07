using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Authorization;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Categories.Queries.GetCategories;
using CulinaryBlog.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace CulinaryBlog.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler
    : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUser _currentUser;

    public DeleteCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task Handle(
        DeleteCategoryCommand request,
        CancellationToken cancellationToken)
    {
        AuthorizationGuard.EnsureAdmin(_currentUser);

        var category = await _unitOfWork.Categories.GetActiveByIdAsync(
            request.Id,
            cancellationToken);

        if (category is null)
        {
            throw new NotFoundException(
                $"Category with id '{request.Id}' was not found.");
        }

        var recipeCount = await _unitOfWork.Categories
            .GetRecipeCountAsync(
                category.Id,
                cancellationToken);

        if (recipeCount > 0)
        {
            throw new CulinaryBlog.Domain.Exceptions.ConflictException(
                $"Category still contains {recipeCount} recipe(s). Move them to another category before deleting it.");
        }

        category.SoftDelete();
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _cache.Remove(GetCategoriesQueryHandler.CacheKey);
    }
}
