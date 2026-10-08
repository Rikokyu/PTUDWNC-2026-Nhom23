using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string? DisplayName,
    string? Email,
    string? Password) : IRequest<AuthUserDto>;
