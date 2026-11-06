namespace CulinaryBlog.Domain.Entities;

public sealed class SavedRecipe
{
    public Guid UserId { get; set; }

    public Guid RecipeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Recipe Recipe { get; set; } = null!;
}
