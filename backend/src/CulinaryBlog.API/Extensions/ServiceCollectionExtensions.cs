using CulinaryBlog.API.CurrentUser;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Application.Common.Behaviors;
using CulinaryBlog.Application.Features.Auth.Commands.Login;
using CulinaryBlog.Application.Features.Auth.Commands.ChangePassword;
using CulinaryBlog.Application.Features.Auth.Commands.ForgotPassword;
using CulinaryBlog.Application.Features.Auth.Commands.Register;
using CulinaryBlog.Application.Features.Auth.Commands.ResetPassword;
using CulinaryBlog.Application.Features.Auth.Commands.UpdateUserProfile;
using CulinaryBlog.Application.Features.Categories.Commands.CreateCategory;
using CulinaryBlog.Application.Features.Categories.Commands.UpdateCategory;
using FluentValidation;
using MediatR;
using System.Threading.RateLimiting;

namespace CulinaryBlog.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        services.AddMediatR(
            cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(
                        CulinaryBlog.Application
                            .Features.Recipes.Queries
                            .GetRecipes.GetRecipesQuery)
                        .Assembly));

        services.AddHttpContextAccessor();

        services.AddMemoryCache();

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(FluentValidationBehavior<,>));
        services.AddTransient<
            IValidator<RegisterCommand>,
            RegisterCommandValidator>();
        services.AddTransient<
            IValidator<LoginCommand>,
            LoginCommandValidator>();
        services.AddTransient<
            IValidator<ChangePasswordCommand>,
            ChangePasswordCommandValidator>();
        services.AddTransient<
            IValidator<ForgotPasswordCommand>,
            ForgotPasswordCommandValidator>();
        services.AddTransient<
            IValidator<ResetPasswordCommand>,
            ResetPasswordCommandValidator>();
        services.AddTransient<
            IValidator<UpdateUserProfileCommand>,
            UpdateUserProfileCommandValidator>();

        services.AddTransient<
            IValidator<CreateCategoryCommand>,
            CreateCategoryCommandValidator>();

        services.AddTransient<
            IValidator<UpdateCategoryCommand>,
            UpdateCategoryCommandValidator>();

        services.AddScoped<
            ICurrentUser,
            CurrentUserService>();

        return services;
    }

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
            ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(
                "Frontend",
                policy => policy
                    .WithOrigins(allowedOrigins)
                    .WithHeaders(
                        "Authorization",
                        "Content-Type",
                        "If-Match",
                        "If-None-Match",
                        "api-version")
                    .AllowAnyMethod()
                    .WithExposedHeaders("ETag", "Retry-After"));
        });

        return services;
    }

    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.Headers.RetryAfter = "60";
                context.HttpContext.Response.ContentType =
                    "application/problem+json";
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        type = "https://httpstatuses.com/429",
                        title = "Too many requests",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Please wait before sending another request."
                    },
                    cancellationToken);
            };
            options.AddPolicy("PasswordReset", context => RateLimitPartition.GetFixedWindowLimiter(
                GetClientAddress(context), _ => new FixedWindowRateLimiterOptions
                { PermitLimit = 5, Window = TimeSpan.FromMinutes(15), QueueLimit = 0, AutoReplenishment = true }));
            options.AddPolicy(
                "Auth",
                context => RateLimitPartition.GetFixedWindowLimiter(
                    GetClientAddress(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.AddPolicy(
                "Api",
                context => RateLimitPartition.GetFixedWindowLimiter(
                    GetClientAddress(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
            options.AddPolicy(
                "Upload",
                context => RateLimitPartition.GetFixedWindowLimiter(
                    GetClientAddress(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return services;
    }

    private static string GetClientAddress(HttpContext context) =>
        context.Connection.RemoteIpAddress?.MapToIPv6().ToString()
        ?? "unknown";
}
