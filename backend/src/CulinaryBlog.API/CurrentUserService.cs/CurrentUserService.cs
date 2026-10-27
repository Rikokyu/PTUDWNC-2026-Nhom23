using System.Security.Claims;
using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.API.CurrentUser;

public class CurrentUserService : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User =>
        _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    public bool IsAuthor
    {
        get
        {
            if (User == null)
                return false;

            return User.IsInRole("Author")
                || User.FindFirst("role")?.Value == "Author";
        }
    }

    public Guid? UserId
    {
        get
        {
            var claim =
                User?.FindFirst(ClaimTypes.NameIdentifier)
                ?? User?.FindFirst("sub");

            if (claim == null)
                return null;

            return Guid.TryParse(
                claim.Value,
                out var userId)
                ? userId
                : null;
        }
    }

    public bool IsAdmin
    {
        get
        {
            if (User == null)
                return false;

            return User.IsInRole("Admin")
                || User.FindFirst("role")?.Value == "Admin";
        }
    }
}