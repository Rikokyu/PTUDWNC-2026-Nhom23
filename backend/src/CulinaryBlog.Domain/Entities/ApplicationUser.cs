namespace CulinaryBlog.Domain.Entities;

public class ApplicationUser : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? Bio { get; set; }

    public string Role { get; set; } = Constants.UserRoles.Author;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Incremented whenever authorization-sensitive account data changes.
    /// Existing access tokens stop working immediately when this value changes.
    /// </summary>
    public int TokenVersion { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; }
        = new List<RefreshToken>();
}
