using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthSessionDto>
{
    private readonly IIdentityService _identityService;
    private readonly IWelcomeEmailJobScheduler _welcomeEmailJobs;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IWelcomeEmailJobScheduler welcomeEmailJobs)
    {
        _identityService = identityService;
        _welcomeEmailJobs = welcomeEmailJobs;
    }

    public async Task<AuthSessionDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _identityService.RegisterAsync(
            request.DisplayName,
            request.Email,
            request.Password,
            cancellationToken);

        _welcomeEmailJobs.Enqueue(
            session.User.Email,
            session.User.DisplayName);

        return session;
    }
}
