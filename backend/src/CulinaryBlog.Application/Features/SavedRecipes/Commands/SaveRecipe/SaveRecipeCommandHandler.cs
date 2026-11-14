using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Commands.SaveRecipe;

public sealed class SaveRecipeCommandHandler
    : IRequestHandler<SaveRecipeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public SaveRecipeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        SaveRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!_currentUser.IsAuthenticated || userId is null)
        {
            throw new UnauthorizedException("Sign in to save recipes.");
        }

        if (!await _unitOfWork.SavedRecipes.IsPublishedRecipeAsync(
                request.RecipeId,
                cancellationToken))
        {
            throw new NotFoundException("Published recipe was not found.");
        }

        if (await _unitOfWork.SavedRecipes.ExistsAsync(
                userId.Value,
                request.RecipeId,
                cancellationToken))
        {
            return Unit.Value;
        }

        await _unitOfWork.SavedRecipes.AddAsync(
            new SavedRecipe
            {
                UserId = userId.Value,
                RecipeId = request.RecipeId,
                CreatedAt = DateTime.UtcNow
            },
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
