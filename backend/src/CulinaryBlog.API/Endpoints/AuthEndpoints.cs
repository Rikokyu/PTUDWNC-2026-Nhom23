using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.DTOs.Auth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CulinaryBlog.API.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterDto request, IIdentityService identityService) =>
        {
            var result = await identityService.RegisterAsync(request.Email, request.Password, request.DisplayName);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            return Results.Ok(new { userId = result.UserId, token = result.Token });
        });

        group.MapPost("/login", async (LoginDto request, IIdentityService identityService) =>
        {
            var result = await identityService.LoginAsync(request.Email, request.Password);
            if (!result.IsSuccess)
            {
                return Results.BadRequest(new { errors = result.Errors });
            }

            return Results.Ok(new { userId = result.UserId, token = result.Token });
        });
    }
}