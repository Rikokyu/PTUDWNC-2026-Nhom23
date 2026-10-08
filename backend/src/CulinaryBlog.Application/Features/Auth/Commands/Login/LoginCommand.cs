using CulinaryBlog.Application.DTOs.Auth;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(
    string? Email,
    string? Password) : IRequest<AuthResponseDto>;