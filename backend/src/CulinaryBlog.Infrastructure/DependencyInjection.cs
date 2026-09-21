using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Infrastructure.Authentication;
using CulinaryBlog.Infrastructure.Identity;
using CulinaryBlog.Infrastructure.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Đăng ký DbContext
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<CulinaryBlogDbContext>(options =>
            options.UseSqlServer(connectionString));

        // 2. Đăng ký Identity + SignInManager + EF Stores
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<CulinaryBlogDbContext>();

        // 3. Đăng ký các dịch vụ Auth
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}