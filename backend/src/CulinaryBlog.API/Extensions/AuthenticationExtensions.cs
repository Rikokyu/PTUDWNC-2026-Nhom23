using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var subject = context.Principal?.FindFirst("sub")?.Value;
                        if (!Guid.TryParse(subject, out var id)) { context.Fail("Invalid session."); return; }
                        var db = context.HttpContext.RequestServices.GetRequiredService<CulinaryBlog.Infrastructure.Persistence.CulinaryBlogDbContext>();
                        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.SingleOrDefaultAsync(
                            db.ApplicationUsers, x => x.Id == id, context.HttpContext.RequestAborted);
                        if (user is null || context.Principal?.FindFirst("pwdv")?.Value != CulinaryBlog.Infrastructure.Identity.AccountService.PasswordVersion(user.PasswordHash))
                            context.Fail("Session expired. Sign in again.");
                    }
                };
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
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                "Admin",
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole("Admin"));

        return services;
    }
}
