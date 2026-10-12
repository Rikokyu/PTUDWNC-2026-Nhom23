namespace CulinaryBlog.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public static ApplicationUser Create(
        string fullName,
        string email,
        string userName)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            DisplayName = fullName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            UserName = userName.Trim(),
            Role = string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string Role { get; set; } = "Author";

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}
