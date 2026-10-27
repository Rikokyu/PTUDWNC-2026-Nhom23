using CulinaryBlog.Application.Common.Interfaces;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IIdentityService identityService)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        await identityService.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            cancellationToken);
    }
}
