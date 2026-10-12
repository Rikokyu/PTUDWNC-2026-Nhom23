using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        group.MapPost("/register", RegisterAsync)
            .WithName("Register")
            .WithSummary("Đăng ký tài khoản mới và tự động đăng nhập.")
            .AllowAnonymous()
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        return endpoints;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new RegisterCommand(
                request.FullName,
                request.Email,
                request.UserName,
                request.Password),
            cancellationToken);

        return Results.Created(
            $"/api/v1/users/{result.User.Id}",
            result);
    }
}

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string UserName,
    string Password);
