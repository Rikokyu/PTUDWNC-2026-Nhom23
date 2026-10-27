using CulinaryBlog.Application.Common.Interfaces;
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
            .RequireRateLimiting("Api");

        MapEndpoints(group, "Auth");
        MapEndpoints(
            endpoints.MapGroup("/api/v1/auth")
                .WithTags("Authentication")
                .RequireRateLimiting("Api")
                .WithDescription("Version 1 authentication routes."),
            "V1Auth");

        return endpoints;
    }

    private static void MapEndpoints(
        RouteGroupBuilder group,
        string routeVersion)
    {
        group.MapPost("/me/avatar", async (HttpRequest request, ClaimsPrincipal principal, IConfiguration configuration,
            IFileStorageService storage, IAccountService accounts, CancellationToken ct) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id)) return Results.Unauthorized();
            if (!request.HasFormContentType) throw new CulinaryBlog.Application.Common.Exceptions.ValidationException("A multipart image is required.");
            var form = await request.ReadFormAsync(ct);
            var file = form.Files.GetFile("file") ?? throw new CulinaryBlog.Application.Common.Exceptions.ValidationException("file is required.");
            var limit = configuration.GetValue<long>("FileUpload:MaxFileSizeBytes", 5 * 1024 * 1024);
            if (file.Length <= 0 || file.Length > limit) throw new CulinaryBlog.Application.Common.Exceptions.ValidationException("Invalid image size.");
            using var buffer = new MemoryStream();
            await file.CopyToAsync(buffer, ct);
            var content = buffer.ToArray();
            var extension = RecipeImageUploadValidator.Validate(file.FileName, file.ContentType, file.Length, limit, content);
            var url = await storage.UploadAsync(content, extension, $"avatars/{id}", ct);
            return Results.Ok(await accounts.UpdateAvatarAsync(id, url, ct));
        }).RequireAuthorization().RequireRateLimiting("Upload");
        group.MapPost("/change-password", async (ChangePasswordRequest request, ClaimsPrincipal principal, IAccountService accounts, CancellationToken ct) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id)) return Results.Unauthorized();
            await accounts.ChangePasswordAsync(id, request.CurrentPassword, request.NewPassword, request.ConfirmPassword, ct);
            return Results.Ok(new { message = "Password changed. Sign in again." });
        }).RequireAuthorization();
        group.MapPost("/forgot-password", async (ForgotPasswordRequest request, IAccountService accounts, CancellationToken ct) =>
        {
            await accounts.ForgotPasswordAsync(request.Email, ct);
            return Results.Ok(new { message = "If the account exists, a reset link will be sent. Check your email." });
        }).AllowAnonymous().RequireRateLimiting("PasswordReset");
        group.MapPost("/reset-password", async (ResetPasswordRequest request, IAccountService accounts, CancellationToken ct) =>
        {
            await accounts.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, request.ConfirmPassword, ct);
            return Results.Ok(new { message = "Password reset. Sign in again." });
        }).AllowAnonymous().RequireRateLimiting("PasswordReset");
        group.MapPatch("/me", async (UpdateProfileRequest request, ClaimsPrincipal principal, IAccountService accounts, CancellationToken ct) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id)) return Results.Unauthorized();
            return Results.Ok(await accounts.UpdateProfileAsync(id, request.DisplayName, request.Bio, request.AvatarUrl, ct));
        }).RequireAuthorization();

        // Compatibility with clients from develop that update profiles using PUT.
        group.MapPut("/me", async (UpdateProfileRequest request, ClaimsPrincipal principal, IAccountService accounts, CancellationToken ct) =>
        {
            if (!Guid.TryParse(principal.FindFirstValue("sub"), out var id)) return Results.Unauthorized();
            return Results.Ok(await accounts.UpdateProfileAsync(id, request.DisplayName, request.Bio, request.AvatarUrl, ct));
        }).RequireAuthorization();

        group.MapPost("/register", RegisterAsync).RequireRateLimiting("Auth")
            .WithName($"{routeVersion}Register")
            .WithSummary("Register a new user account.")
            .WithDescription(
                "Creates an Author account, returns an access/refresh token pair, and queues a welcome email.")
            .AllowAnonymous()
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/login", LoginAsync).RequireRateLimiting("Auth")
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

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword, string ConfirmPassword);
public sealed record UpdateProfileRequest(string DisplayName, string? Bio, string? AvatarUrl);
