using MediatR;

namespace CulinaryBlog.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid UserId) : IRequest;