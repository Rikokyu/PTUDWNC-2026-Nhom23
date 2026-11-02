using CulinaryBlog.Application.DTOs.Users;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string DisplayName,
    string? Bio,
    string? AvatarUrl) : IRequest<UserDto>;