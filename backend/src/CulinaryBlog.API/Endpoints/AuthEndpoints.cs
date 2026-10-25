using System.Security.Claims;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
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
                "Creates an account after validating the email and password.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync)
            .WithName($"{routeVersion}Login")
            .WithSummary("Sign in using an email address and password.")
            .WithDescription(
                "Returns a JWT access token and a refresh token.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
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

        group.MapPatch("/me", UpdateProfileAsync)
            .WithName($"{routeVersion}UpdateProfile")
            .WithSummary("Update the authenticated user's profile.")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK);

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithName($"{routeVersion}ChangePassword")
            .WithSummary("Change the authenticated user's password.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/forgot-password", RequestPasswordResetAsync)
            .WithName($"{routeVersion}ForgotPassword")
            .WithSummary("Send a password reset link without revealing whether an account exists.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status202Accepted);

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName($"{routeVersion}ResetPassword")
            .WithSummary("Reset a password using a time-limited email token.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent);
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

    private static async Task<IResult> UpdateProfileAsync(
        UpdateProfileRequest request,
        ClaimsPrincipal principal,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(principal);
        var user = await identityService.UpdateProfileAsync(
            userId,
            request.DisplayName,
            request.AvatarUrl,
            request.Bio,
            cancellationToken);
        return Results.Ok(user);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        await identityService.ChangePasswordAsync(
            GetUserId(principal),
            request.CurrentPassword,
            request.NewPassword,
            cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        await identityService.RequestPasswordResetAsync(
            request.Email,
            cancellationToken);
        return Results.Accepted(
            value: new
            {
                message = "Nếu email tồn tại, hướng dẫn đặt lại mật khẩu sẽ được gửi."
            });
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        await identityService.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            cancellationToken);
        return Results.NoContent();
    }

    private static Guid GetUserId(ClaimsPrincipal principal)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            throw new UnauthorizedException(
                "The authenticated user identifier is invalid.");
        }

        return userId;
    }

    /// <summary>Creates a user account and returns its public profile.</summary>
    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await sender.Send(
                new RegisterCommand(
                    request.DisplayName,
                    request.Email,
                    request.Password),
                cancellationToken);

            return Results.Created("/api/auth/register", user);
        }
        catch (ValidationException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
    }

    /// <summary>Validates credentials and issues an access/refresh token pair.</summary>
    private static async Task<IResult> LoginAsync(
        LoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        try
        {
            var session = await sender.Send(
                new LoginCommand(request.Email, request.Password),
                cancellationToken);

            return Results.Ok(session);
        }
        catch (ValidationException exception)
        {
            return Results.BadRequest(new { error = exception.Message });
        }
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

public sealed record UpdateProfileRequest(
    string DisplayName,
    string? AvatarUrl,
    string? Bio);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);
