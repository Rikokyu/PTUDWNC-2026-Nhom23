using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.SavedRecipes.Queries.GetSavedRecipeIds;

public sealed class GetSavedRecipeIdsQueryHandler
    : IRequestHandler<GetSavedRecipeIdsQuery, IReadOnlyList<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;

    public GetSavedRecipeIdsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<Guid>> Handle(
        GetSavedRecipeIdsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!_currentUser.IsAuthenticated || userId is null)
        {
            throw new UnauthorizedException("Sign in to view saved recipes.");
        }

        return _unitOfWork.SavedRecipes.GetVisibleRecipeIdsAsync(
            userId.Value,
            cancellationToken);
    }
}
