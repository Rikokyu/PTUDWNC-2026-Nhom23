namespace CulinaryBlog.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
	public Guid UserId { get; set; }

	public ApplicationUser User { get; set; } = null!;

	public string TokenHash { get; set; } = string.Empty;

	public DateTime ExpiresAt { get; set; }

	public DateTime? RevokedAt { get; set; }
}
