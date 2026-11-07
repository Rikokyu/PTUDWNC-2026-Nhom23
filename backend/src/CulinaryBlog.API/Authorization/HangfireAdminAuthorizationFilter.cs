using System.Net;
using System.Text;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.API.Authorization;

public sealed class HangfireAdminAuthorizationFilter
    : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (!authorization.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return Challenge(httpContext);
        }

        string credentials;
        try
        {
            credentials = Encoding.UTF8.GetString(
                Convert.FromBase64String(authorization[6..].Trim()));
        }
        catch (FormatException)
        {
            return Challenge(httpContext);
        }

        var separator = credentials.IndexOf(':');
        if (separator <= 0 || separator == credentials.Length - 1)
        {
            return Challenge(httpContext);
        }

        var userNameOrEmail = credentials[..separator];
        var password = credentials[(separator + 1)..];
        var database = httpContext.RequestServices
            .GetRequiredService<CulinaryBlogDbContext>();
        var user = database.ApplicationUsers
            .AsNoTracking()
            .FirstOrDefault(account =>
                account.Email == userNameOrEmail
                || account.UserName == userNameOrEmail);

        if (user is null
            || !user.IsActive
            || user.Role != UserRoles.Admin)
        {
            return Challenge(httpContext);
        }

        var passwordHasher = httpContext.RequestServices
            .GetRequiredService<IPasswordHasher<ApplicationUser>>();
        var verification = passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        if (verification == PasswordVerificationResult.Failed)
        {
            return Challenge(httpContext);
        }

        return true;
    }

    private static bool Challenge(HttpContext httpContext)
    {
        httpContext.Response.Headers.WWWAuthenticate =
            "Basic realm=\"Hangfire Dashboard\", charset=\"UTF-8\"";
        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return false;
    }
}
