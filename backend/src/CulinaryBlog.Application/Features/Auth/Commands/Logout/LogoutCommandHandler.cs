using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IIdentityService _identityService;

    public LogoutCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        await _identityService.RevokeRefreshTokensAsync(
            request.UserId,
            cancellationToken);
    }
}