using System.Security.Claims;
using CulinaryBlog.Application.Features.Auth.Commands.Login;
using CulinaryBlog.Application.Features.Auth.Commands.Logout;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using CulinaryBlog.Application.DTOs.Users;
using CulinaryBlog.Application.Features.Auth.Queries.GetCurrentUser;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication")
            .RequireRateLimiting("Auth");

        MapEndpoints(group, "Auth");
        MapEndpoints(
            endpoints.MapGroup("/api/v1/auth")
                .WithTags("Authentication")
                .RequireRateLimiting("Auth")
                .WithDescription("Version 1 authentication routes."),
            "V1Auth");

        return endpoints;
    }

    private static void MapEndpoints(
        RouteGroupBuilder group,
        string routeVersion)
    {
        group.MapPost("/register", RegisterAsync)
            .WithName($"{routeVersion}Register")
            .WithSummary("Register a new user account.")
            .WithDescription(
                "Creates an Author account, returns an access/refresh token pair, and queues a welcome email.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync)
            .WithName($"{routeVersion}Login")
            .WithSummary("Sign in using an email address and password.")
            .WithDescription(
                "Returns a JWT access token and a refresh token.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .WithName($"{routeVersion}Logout")
            .WithSummary("Revoke the current user's refresh tokens.")
            .RequireAuthorization()
            .Produces<string>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", GetCurrentUserAsync)
            .WithName($"{routeVersion}GetCurrentUser")
            .WithSummary("Get the authenticated user's profile.")
            .WithDescription(
                "Returns the profile belonging to the subject in the current JWT.")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>Returns the profile associated with the authenticated JWT.</summary>
    private static async Task<IResult> GetCurrentUserAsync(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var user = await sender.Send(
            new GetCurrentUserQuery(),
            cancellationToken);

        return Results.Ok(user);
    }

    /// <summary>Creates a user account and immediately returns an authenticated session.</summary>
    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var session = await sender.Send(
            new RegisterCommand(
                request.DisplayName,
                request.Email,
                request.Password),
            cancellationToken);

        return Results.Created("/api/v1/auth/register", session);
    }

    /// <summary>Validates credentials and issues an access/refresh token pair.</summary>
    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var session = await sender.Send(
            new LoginCommand(request.Email, request.Password),
            cancellationToken);

        return Results.Ok(session);
    }

    /// <summary>Revokes all active refresh tokens for the authenticated user.</summary>
    private static async Task<IResult> LogoutAsync(
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        await sender.Send(new LogoutCommand(userId), cancellationToken);
        return Results.Ok("Đăng xuất thành công");
    }
}

/// <summary>Credentials and public name used to register an account.</summary>
public sealed record RegisterRequest(
    string DisplayName,
    string Email,
    string Password);

/// <summary>Email and password credentials.</summary>
public sealed record LoginRequest(
    string Email,
    string Password);
