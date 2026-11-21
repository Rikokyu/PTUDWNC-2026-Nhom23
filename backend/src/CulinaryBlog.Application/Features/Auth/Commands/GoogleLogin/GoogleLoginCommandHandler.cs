using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.GoogleLogin;

public sealed class GoogleLoginCommandHandler(
    IIdentityService identityService,
    IWelcomeEmailJobScheduler welcomeEmailJobs)
    : IRequestHandler<GoogleLoginCommand, AuthSessionDto>
{
    public async Task<AuthSessionDto> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken)
    {
        var result = await identityService.GoogleLoginAsync(request.IdToken, cancellationToken);
        if (result.IsNewUser)
            welcomeEmailJobs.Enqueue(result.Session.User.Email, result.Session.User.DisplayName);

        return result.Session;
    }
}
