namespace CulinaryBlog.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = "Author";

    public bool EmailConfirmed { get; set; }

    public string? PasswordHash { get; set; }

    public string? GoogleSubject { get; set; }

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}