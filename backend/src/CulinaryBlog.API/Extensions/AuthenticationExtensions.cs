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
                        IssuerSigningKey = string.IsNullOrWhiteSpace(secret)
                            ? null
                            : new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(secret)),
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
