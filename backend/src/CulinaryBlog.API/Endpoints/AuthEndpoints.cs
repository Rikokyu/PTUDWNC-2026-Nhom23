using System.Security.Claims;
using System.Text.Json;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Features.Auth.Commands.Login;
using CulinaryBlog.Application.Features.Auth.Commands.Logout;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using CulinaryBlog.Application.Features.Auth.Commands.ChangePassword;
using CulinaryBlog.Application.Features.Auth.Commands.ForgotPassword;
using CulinaryBlog.Application.Features.Auth.Commands.ResetPassword;
using CulinaryBlog.Application.Features.Auth.Commands.UpdateUserProfile;
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

        group.MapPost("/refresh", RefreshAsync)
            .RequireRateLimiting("Auth").AllowAnonymous()
            .WithName($"{routeVersion}Refresh")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/google", GoogleLoginAsync)
            .RequireRateLimiting("Auth").AllowAnonymous()
            .WithName($"{routeVersion}GoogleLogin")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", LogoutAsync)
            .WithName($"{routeVersion}Logout")
            .WithSummary("Revoke the current user's refresh tokens.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
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

        group.MapPut("/me", UpdateProfileAsync)
            .WithName($"{routeVersion}UpdateProfile")
            .WithSummary("Update the authenticated user's profile.")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPatch("/me", PatchProfileAsync)
            .RequireRateLimiting("Api")
            .WithName($"{routeVersion}PatchProfile")
            .RequireAuthorization()
            .Produces<UserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/change-password", ChangePasswordAsync)
            .WithName($"{routeVersion}ChangePassword")
            .WithSummary("Change the authenticated user's password.")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/forgot-password", ForgotPasswordAsync)
            .WithName($"{routeVersion}ForgotPassword")
            .WithSummary("Send a password reset link if the account exists.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/reset-password", ResetPasswordAsync)
            .WithName($"{routeVersion}ResetPassword")
            .WithSummary("Reset a password using a valid reset link.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
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

    private static async Task<IResult> RefreshAsync(RefreshRequest request,
        IIdentityService identityService, CancellationToken cancellationToken)
    {
        var session = await identityService.RefreshAsync(request.RefreshToken, cancellationToken);
        return session is null ? Results.Unauthorized() : Results.Ok(session);
    }

    private static async Task<IResult> GoogleLoginAsync(GoogleLoginRequest request,
        IIdentityService identityService, CancellationToken cancellationToken)
    {
        var session = await identityService.GoogleLoginAsync(request.IdToken, cancellationToken);
        return Results.Ok(session);
    }

    /// <summary>Revokes all active refresh tokens for the authenticated user.</summary>
    private static async Task<IResult> LogoutAsync(
        ClaimsPrincipal principal,
        HttpRequest request,
        IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Results.Unauthorized();
        }

        string? refreshToken = null;
        var hasBody = request.ContentLength > 0
            || (request.ContentLength is null && request.HasJsonContentType());
        if (hasBody)
        {
            var body = await request.ReadFromJsonAsync<LogoutRequest>(cancellationToken);
            refreshToken = body?.RefreshToken;
        }
        if (!hasBody)
            await identityService.RevokeRefreshTokensAsync(userId, cancellationToken);
        else if (refreshToken is not null)
            await identityService.LogoutAsync(userId, refreshToken, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> PatchProfileAsync(JsonElement request,
        ClaimsPrincipal principal, IIdentityService identityService,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId)) return Results.Unauthorized();
        if (request.ValueKind != JsonValueKind.Object)
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = ["Profile must be an object."] }, statusCode: 422);
        string? name = null, avatar = null, bio = null;
        var hasName = false; var hasAvatar = false; var hasBio = false;
        foreach (var property in request.EnumerateObject())
        {
            if (property.NameEquals("displayName") || property.NameEquals("fullName"))
            {
                hasName = true;
                if (property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["displayName"] = ["Must be a string."] }, statusCode: 422);
                name = property.Value.GetString();
            }
            else if (property.NameEquals("avatarUrl"))
            {
                hasAvatar = true;
                if (property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["avatarUrl"] = ["Must be a string or null."] }, statusCode: 422);
                avatar = property.Value.GetString();
            }
            else if (property.NameEquals("bio"))
            {
                hasBio = true;
                if (property.Value.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
                    return Results.ValidationProblem(new Dictionary<string, string[]> { ["bio"] = ["Must be a string or null."] }, statusCode: 422);
                bio = property.Value.GetString();
            }
            else
                return Results.ValidationProblem(new Dictionary<string, string[]> { [property.Name] = ["This field cannot be updated."] }, statusCode: 422);
        }
        var user = await identityService.PatchProfileAsync(userId, hasName, name,
            hasAvatar, avatar, hasBio, bio, cancellationToken);
        return Results.Ok(user);
    }

    private static async Task<IResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        await sender.Send(
            new ChangePasswordCommand(
                userId,
                request.CurrentPassword,
                request.NewPassword),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ForgotPasswordCommand(request.Email),
            cancellationToken);

        return Results.Ok(new
        {
            message = "If an account exists for that email, password reset instructions will be sent."
        });
    }

    private static async Task<IResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ResetPasswordCommand(
                request.Token,
                request.NewPassword),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateProfileAsync(
        UpdateProfileRequest request,
        ClaimsPrincipal principal,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (!TryGetUserId(principal, out var userId))
        {
            return Results.Unauthorized();
        }

        var user = await sender.Send(
            new UpdateUserProfileCommand(
                userId,
                request.DisplayName,
                request.Bio,
                request.AvatarUrl),
            cancellationToken);

        return Results.Ok(user);
    }

    private static bool TryGetUserId(ClaimsPrincipal principal, out Guid userId)
    {
        var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue("sub");
        return Guid.TryParse(userIdValue, out userId);
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

public sealed record RefreshRequest(string RefreshToken);
public sealed record GoogleLoginRequest(string IdToken);
public sealed record LogoutRequest(string? RefreshToken);

public sealed record UpdateProfileRequest(
    string DisplayName,
    string? AvatarUrl,
    string? Bio);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

/// <summary>Email address for password reset instructions.</summary>
public sealed record ForgotPasswordRequest(string Email);

/// <summary>Email, reset token, and replacement password.</summary>
public sealed record ResetPasswordRequest(
    string Token,
    string NewPassword);

/// <summary>Editable public profile fields.</summary>
