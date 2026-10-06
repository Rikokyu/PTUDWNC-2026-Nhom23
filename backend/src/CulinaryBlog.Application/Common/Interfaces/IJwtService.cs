using System.Security.Claims;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IJwtService
{
	IssuedAccessToken CreateAccessToken(ApplicationUser user);

	ClaimsPrincipal? ValidateAccessToken(string token);
}

public sealed record IssuedAccessToken(string Value, DateTime ExpiresAt);
