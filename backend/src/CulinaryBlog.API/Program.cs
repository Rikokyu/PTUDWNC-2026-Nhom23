using CulinaryBlog.API.Endpoints;
using CulinaryBlog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ cho Minimal API
builder.Services.AddEndpointsApiExplorer();

// Đăng ký dịch vụ Authentication & Authorization
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Đăng ký các dịch vụ từ tầng Infrastructure
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Đăng ký Auth Endpoints (/api/auth/register, /api/auth/login)
app.MapAuthEndpoints();

app.Run();