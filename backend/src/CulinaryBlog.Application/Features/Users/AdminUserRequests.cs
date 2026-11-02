using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.DTOs.Users;
using MediatR;

namespace CulinaryBlog.Application.Features.Users;

public sealed record GetAdminUsersQuery(
    int Page,
    int PageSize,
    string? Search,
    string? Role) : IRequest<PagedResult<AdminUserDto>>;

public sealed record SetUserActiveCommand(
    Guid ActingAdminId,
    Guid UserId,
    bool IsActive) : IRequest<AdminUserDto>;

public sealed record SetUserRoleCommand(
    Guid ActingAdminId,
    Guid UserId,
    string Role) : IRequest<AdminUserDto>;

public sealed class AdminUserRequestHandlers :
    IRequestHandler<GetAdminUsersQuery, PagedResult<AdminUserDto>>,
    IRequestHandler<SetUserActiveCommand, AdminUserDto>,
    IRequestHandler<SetUserRoleCommand, AdminUserDto>
{
    private readonly IAdminUserService _users;

    public AdminUserRequestHandlers(IAdminUserService users)
    {
        _users = users;
    }

    public Task<PagedResult<AdminUserDto>> Handle(
        GetAdminUsersQuery request,
        CancellationToken cancellationToken) =>
        _users.GetUsersAsync(
            request.Page,
            request.PageSize,
            request.Search,
            request.Role,
            cancellationToken);

    public Task<AdminUserDto> Handle(
        SetUserActiveCommand request,
        CancellationToken cancellationToken) =>
        _users.SetActiveAsync(
            request.ActingAdminId,
            request.UserId,
            request.IsActive,
            cancellationToken);

    public Task<AdminUserDto> Handle(
        SetUserRoleCommand request,
        CancellationToken cancellationToken) =>
        _users.SetRoleAsync(
            request.ActingAdminId,
            request.UserId,
            request.Role,
            cancellationToken);
}
