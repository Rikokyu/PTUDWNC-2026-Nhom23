using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler
    : IRequestHandler<LoginCommand, AuthSessionDto>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<AuthSessionDto> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        var session = await _identityService.LoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        return session
            ?? throw new UnauthorizedException("Invalid email or password.");
    }
}