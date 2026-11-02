using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Users;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.UpdateUserProfile;

public sealed class UpdateUserProfileCommandHandler(
    IIdentityService identityService)
    : IRequestHandler<UpdateUserProfileCommand, UserDto>
{
    public Task<UserDto> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken) =>
        identityService.UpdateProfileAsync(
            request.UserId,
            request.DisplayName,
            request.Bio,
            request.AvatarUrl,
            cancellationToken);
}