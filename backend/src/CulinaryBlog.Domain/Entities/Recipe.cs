namespace CulinaryBlog.Domain.Entities;

public class Recipe : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
    public ICollection<RecipeStep> InstructionSteps { get; set; } = new List<RecipeStep>();
    public ICollection<RecipeImage> Images { get; set; } = new List<RecipeImage>();
}