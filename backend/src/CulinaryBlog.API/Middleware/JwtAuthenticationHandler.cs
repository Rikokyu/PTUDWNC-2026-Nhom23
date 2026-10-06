using System.Text.Encodings.Web;
using CulinaryBlog.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.API.Middleware;

public sealed class JwtAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IJwtService _jwtService;

    public JwtAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IJwtService jwtService)
        : base(options, logger, encoder)
    {
        _jwtService = jwtService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorization = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Bearer token required."));
        }

        var principal = _jwtService.ValidateAccessToken(authorization[7..].Trim());
        if (principal is null)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid or expired access token."));
        }

        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}