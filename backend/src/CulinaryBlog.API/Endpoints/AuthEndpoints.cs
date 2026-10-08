using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.Features.Auth.Commands.Login;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost(
                "/register",
                async Task<IResult> (
                    RegisterRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new RegisterCommand(
                            request.DisplayName,
                            request.Email,
                            request.Password),
                        cancellationToken);

                    return Results.Created(
                        $"/api/users/{result.Id}",
                        result);
                })
            .WithName("Register")
                .WithSummary("Register a new account")
                .WithDescription("Creates an account after validating its display name, email, and password.")
                .Produces<AuthUserDto>(StatusCodes.Status201Created)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost(
                "/login",
                async Task<IResult> (
                    LoginRequest request,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new LoginCommand(
                            request.Email,
                            request.Password),
                        cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("Login")
                .WithSummary("Log in and receive a bearer token")
                .WithDescription("Validates the supplied credentials and returns an HS256-signed JWT.")
                .Produces<AuthResponseDto>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        return endpoints;
    }
}

/// <summary>Account registration payload.</summary>
public sealed record RegisterRequest(
    string? DisplayName,
    string? Email,
    string? Password);

/// <summary>Account login payload.</summary>
public sealed record LoginRequest(
    string? Email,
    string? Password);
