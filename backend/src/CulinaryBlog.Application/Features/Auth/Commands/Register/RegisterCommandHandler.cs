using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler
    : IRequestHandler<RegisterCommand, AuthResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(
        IIdentityService identityService,
        IJwtService jwtService)
    {
        _identityService = identityService;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenHash = _jwtService.HashRefreshToken(refreshToken);
        var refreshTokenExpiresAt =
            _jwtService.GetRefreshTokenExpiration();

        var user = await _identityService.RegisterAsync(
            request.FullName.Trim(),
            request.Email.Trim().ToLowerInvariant(),
            request.UserName.Trim(),
            request.Password,
            refreshTokenHash,
            refreshTokenExpiresAt,
            cancellationToken);

        var accessToken = _jwtService.GenerateAccessToken(user);

        return new AuthResponseDto(
            accessToken.Token,
            refreshToken,
            accessToken.ExpiresAt,
            new UserDto(
                user.Id,
                user.DisplayName,
                user.Email,
                user.UserName,
                user.AvatarUrl,
                [user.Role]));
    }
}
