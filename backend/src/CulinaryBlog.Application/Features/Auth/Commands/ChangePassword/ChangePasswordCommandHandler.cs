using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandHandler(
    IIdentityService identityService)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        await identityService.ChangePasswordAsync(
            request.UserId,
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
    }
}