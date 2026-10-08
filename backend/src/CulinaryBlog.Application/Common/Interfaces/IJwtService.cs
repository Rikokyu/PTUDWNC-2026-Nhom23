using CulinaryBlog.Application.DTOs.Auth;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IJwtService
{
    JwtTokenDto GenerateToken(ApplicationUser user);
}
