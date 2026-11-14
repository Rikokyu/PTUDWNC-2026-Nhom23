using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Commands.RemoveSavedRecipe;

public sealed class RemoveSavedRecipeCommandHandler
    : IRequestHandler<RemoveSavedRecipeCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public RemoveSavedRecipeCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(
        RemoveSavedRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!_currentUser.IsAuthenticated || userId is null)
        {
            throw new UnauthorizedException("Sign in to manage saved recipes.");
        }

        var savedRecipe = await _unitOfWork.SavedRecipes.GetAsync(
            userId.Value,
            request.RecipeId,
            cancellationToken);
        if (savedRecipe is not null)
        {
            _unitOfWork.SavedRecipes.Remove(savedRecipe);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
