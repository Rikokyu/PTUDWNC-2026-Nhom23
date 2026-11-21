using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthSessionDto?>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<AuthSessionDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken) =>
        _identityService.RefreshAsync(request.RefreshToken, cancellationToken);
}
