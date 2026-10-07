using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Application.Common.Authorization;

public static class AuthorizationGuard
{
    public static void EnsureAdmin(ICurrentUser currentUser)
    {
        if (!currentUser.IsAuthenticated)
        {
            throw new UnauthorizedException(
                "Authentication is required.");
        }

        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException(
                "Administrator permission is required.");
        }
    }
}
