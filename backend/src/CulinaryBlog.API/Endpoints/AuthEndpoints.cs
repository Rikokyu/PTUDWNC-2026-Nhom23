using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Application.Features.Auth.Commands.Login;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MediatR;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterDto? request, IIdentityService identityService) =>
        {
            if (request is null || string.IsNullOrWhiteSpace(request.DisplayName))
            {
                return Results.BadRequest(new { error = "Display name is required." });
            }

            var result = await identityService.RegisterAsync(request.Email, request.Password, request.DisplayName);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            return Results.Ok(new { userId = result.UserId, token = result.Token });
        });

        group.MapPost("/login", async (LoginCommand request, ISender sender, CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(request, cancellationToken);

            if (!result.IsSuccess)
            {
                return Results.Unauthorized();
            }

            return Results.Ok(new { userId = result.UserId, token = result.Token });
        });
    }
}