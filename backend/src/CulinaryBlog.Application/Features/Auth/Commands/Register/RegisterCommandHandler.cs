using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthUserDto>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<AuthUserDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken) =>
        _identityService.RegisterAsync(
            request.DisplayName,
            request.Email,
            request.Password,
            cancellationToken);
}
