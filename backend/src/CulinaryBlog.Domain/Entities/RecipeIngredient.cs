namespace CulinaryBlog.Domain.Entities;

public class RecipeIngredient : BaseEntity
{
    public Guid RecipeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;
}