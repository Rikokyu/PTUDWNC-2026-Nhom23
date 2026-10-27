using System.Security.Claims;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Users;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class AdminUserEndpoints
{
    public static IEndpointRouteBuilder MapAdminUserEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/admin/users")
            .WithTags("Admin - Users")
            .RequireAuthorization("Admin")
            .RequireRateLimiting("Api");

        group.MapGet("", GetUsersAsync)
            .WithSummary("List user accounts.");

        group.MapPatch("/{userId:guid}/status", SetStatusAsync)
            .WithSummary("Enable or disable an Author account.");

        group.MapPatch("/{userId:guid}/role", SetRoleAsync)
            .WithSummary("Change an account role.");

        return endpoints;
    }

    private static async Task<IResult> GetUsersAsync(
        HttpRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var page = ParsePositiveInt(request.Query["page"], 1, "page");
        var pageSize = ParsePositiveInt(
            request.Query["pageSize"],
            20,
            "pageSize");
        var users = await sender.Send(
            new GetAdminUsersQuery(
                page,
                pageSize,
                request.Query["search"].FirstOrDefault(),
                request.Query["role"].FirstOrDefault()),
            cancellationToken);

        return Results.Ok(users);
    }

    private static async Task<IResult> SetStatusAsync(
        Guid userId,
        SetUserStatusRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetUserActiveCommand(
                GetCurrentUserId(principal),
                userId,
                request.IsActive),
            cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> SetRoleAsync(
        Guid userId,
        SetUserRoleRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new SetUserRoleCommand(
                GetCurrentUserId(principal),
                userId,
                request.Role),
            cancellationToken);
        return Results.Ok(result);
    }

    private static Guid GetCurrentUserId(ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        return Guid.TryParse(value, out var userId)
            ? userId
            : throw new ForbiddenException(
                "Administrator identity is invalid.");
    }

    private static int ParsePositiveInt(
        string? value,
        int defaultValue,
        string field)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, out var result) || result <= 0)
        {
            throw new ValidationException(
                $"{field} must be a positive integer.");
        }

        return result;
    }
}

public sealed record SetUserStatusRequest(bool IsActive);

public sealed record SetUserRoleRequest(string Role);
