namespace CulinaryBlog.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}