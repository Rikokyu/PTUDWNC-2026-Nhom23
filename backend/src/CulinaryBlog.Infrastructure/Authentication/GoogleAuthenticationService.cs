using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Exceptions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace CulinaryBlog.Infrastructure.Authentication;

public sealed class GoogleAuthenticationService(IConfiguration configuration) : IGoogleTokenVerifier
{
    private static readonly ConfigurationManager<OpenIdConnectConfiguration> Metadata = new(
        "https://accounts.google.com/.well-known/openid-configuration",
        new OpenIdConnectConfigurationRetriever());

    public async Task<VerifiedGoogleUser?> VerifyAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = configuration["Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new InvalidOperationException("Google:ClientId is not configured.");
        if (string.IsNullOrWhiteSpace(idToken) || idToken.Length > 8192)
            return null;

        OpenIdConnectConfiguration metadata;
        try
        {
            metadata = await Metadata.GetConfigurationAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or InvalidOperationException)
        {
            throw new GoogleProviderUnavailableException(ex);
        }
        try
        {
            var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(
                idToken,
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = ["https://accounts.google.com", "accounts.google.com"],
                    ValidateAudience = true,
                    ValidAudience = clientId,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = metadata.SigningKeys,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ClockSkew = TimeSpan.FromSeconds(30)
                }, out _);
            var subject = principal.FindFirstValue("sub");
            var email = principal.FindFirstValue("email");
            var verified = principal.FindFirstValue("email_verified");
            var authorizedParty = principal.FindFirstValue("azp");
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(email)
                || !string.Equals(verified, "true", StringComparison.OrdinalIgnoreCase)
                || (authorizedParty is not null && authorizedParty != clientId))
                return null;

            return new VerifiedGoogleUser(subject, email.Trim().ToLowerInvariant(),
                principal.FindFirstValue("name") ?? email, principal.FindFirstValue("picture"));
        }
        catch (SecurityTokenException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
