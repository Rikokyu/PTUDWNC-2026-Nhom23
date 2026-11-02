using System.Text;
using System.Security.Claims;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.API.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"] ?? "CulinaryBlog";
        var audience = configuration["Jwt:Audience"] ?? "CulinaryBlog";
        var secret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(secret)
            || Encoding.UTF8.GetByteCount(secret) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Secret must be configured with at least 32 UTF-8 bytes.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(secret)),
                        ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.FromSeconds(30),
                        NameClaimType = "sub",
                        RoleClaimType = "role"
                    };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var principal = context.Principal;
                        var userIdValue = principal?.FindFirstValue("sub")
                            ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                        var tokenVersionValue = principal?
                            .FindFirstValue("token_version");
                        var tokenRole = principal?.FindFirstValue("role");

                        if (!Guid.TryParse(userIdValue, out var userId)
                            || !int.TryParse(
                                tokenVersionValue,
                                out var tokenVersion))
                        {
                            context.Fail("The access token is invalid.");
                            return;
                        }

                        var database = context.HttpContext.RequestServices
                            .GetRequiredService<CulinaryBlogDbContext>();
                        var account = await database.ApplicationUsers
                            .AsNoTracking()
                            .Where(user => user.Id == userId)
                            .Select(user => new
                            {
                                user.IsActive,
                                user.Role,
                                user.TokenVersion
                            })
                            .SingleOrDefaultAsync(
                                context.HttpContext.RequestAborted);

                        if (account is null
                            || !account.IsActive
                            || account.TokenVersion != tokenVersion
                            || account.Role != tokenRole)
                        {
                            context.Fail(
                                "The account is disabled or its permissions have changed.");
                        }
                    }
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                "Admin",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(UserRoles.Admin))
            .AddPolicy(
                "AuthorOrAdmin",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(UserRoles.Author, UserRoles.Admin));

        return services;
    }
}
