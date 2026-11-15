using CulinaryBlog.API.Endpoints;
using CulinaryBlog.API.Extensions;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CulinaryBlog.API.Tests;

public sealed class AuthRateLimitPolicyTests
{
    [Fact]
    public void AvatarUpload_UsesUploadPolicyInsteadOfAuthGroupPolicy()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddApplicationServices();
        builder.Services.AddApiRateLimiting();
        builder.Services.AddAuthorization();
        builder.Services.AddOutputCache();
        builder.Services.AddSingleton<IIdentityService>(_ => null!);
        builder.Services.AddSingleton<IFileStorageService>(_ => null!);
        using var app = builder.Build();

        app.MapAuthEndpoints();

        var avatars = ((IEndpointRouteBuilder)app).DataSources
            .SelectMany(source => source.Endpoints)
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.RoutePattern.RawText is
                "/api/auth/me/avatar" or "/api/v1/auth/me/avatar")
            .ToArray();

        Assert.Equal(2, avatars.Length);
        Assert.All(avatars, avatar => Assert.Equal(
            "Upload",
            avatar.Metadata.GetMetadata<EnableRateLimitingAttribute>()?.PolicyName));
    }
}
