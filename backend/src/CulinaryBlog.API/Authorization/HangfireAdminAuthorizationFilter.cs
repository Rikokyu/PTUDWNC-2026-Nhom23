using Hangfire.Dashboard;

namespace CulinaryBlog.API.Authorization;

public sealed class HangfireAdminAuthorizationFilter
    : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) =>
        context.GetHttpContext().User.IsInRole("Admin");
}
