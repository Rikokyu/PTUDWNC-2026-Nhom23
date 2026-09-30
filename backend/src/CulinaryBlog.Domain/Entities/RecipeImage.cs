namespace CulinaryBlog.Domain.Entities;

public class RecipeImage : BaseEntity
{
    public Guid RecipeId { get; set; }
    public string Url { get; set; } = string.Empty;
    public bool IsMain { get; set; }

    public Recipe Recipe { get; set; } = null!;
}