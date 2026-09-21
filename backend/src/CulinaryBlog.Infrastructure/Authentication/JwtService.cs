using CulinaryBlog.Application.Common.Interfaces;

namespace CulinaryBlog.Infrastructure.Authentication;

public class JwtService : IJwtService
{
    public string GenerateToken(string userId, string email, string displayName)
    {
        // TODO: Viết logic tạo JWT Token thực tế ở đây
        return "SAMPLE_JWT_TOKEN";
    }
}