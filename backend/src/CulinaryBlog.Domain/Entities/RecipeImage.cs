namespace CulinaryBlog.Domain.Entities;

public class RecipeImage : BaseEntity
{
    public Guid RecipeId { get; set; }

    public Recipe Recipe { get; set; } = null!;

    public string OriginalUrl { get; set; } = string.Empty;

    public string? MediumUrl { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    public int OrderIndex { get; set; }

    public static RecipeImage Create(
        Guid recipeId,
        string originalUrl,
        string? altText,
        bool isPrimary,
        int orderIndex)
    {
        return new RecipeImage
        {
            Id = Guid.NewGuid(),
            RecipeId = recipeId,
            OriginalUrl = originalUrl,
            AltText = altText,
            IsPrimary = isPrimary,
            OrderIndex = orderIndex,
            CreatedAt = DateTime.UtcNow
        };
    }
}
