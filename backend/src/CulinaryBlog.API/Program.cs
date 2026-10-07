using CulinaryBlog.API.Endpoints;
using CulinaryBlog.Application.Features.Auth.Commands.Login;
using CulinaryBlog.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký dịch vụ cho Minimal API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMediatR(configuration =>
	configuration.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly));

// Đăng ký dịch vụ Authentication & Authorization
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
	?? throw new InvalidOperationException("JWT secret is not configured.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
	?? throw new InvalidOperationException("JWT issuer is not configured.");
var jwtAudience = builder.Configuration["JwtSettings:Audience"]
	?? throw new InvalidOperationException("JWT audience is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = new TokenValidationParameters
		{
			ValidateIssuer = true,
			ValidIssuer = jwtIssuer,
			ValidateAudience = true,
			ValidAudience = jwtAudience,
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
			ValidateLifetime = true
		};
	});
builder.Services.AddAuthorization();

// Đăng ký các dịch vụ từ tầng Infrastructure
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Đăng ký Auth Endpoints (/api/auth/register, /api/auth/login)
app.MapAuthEndpoints();
app.MapCategoryEndpoints();
app.MapRecipeEndpoints();

app.Run();