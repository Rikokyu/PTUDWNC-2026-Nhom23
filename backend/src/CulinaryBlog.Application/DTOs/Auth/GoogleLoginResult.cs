namespace CulinaryBlog.Application.DTOs.Auth;

public sealed record GoogleLoginResult(AuthSessionDto Session, bool IsNewUser);
