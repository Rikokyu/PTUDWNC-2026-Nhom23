using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.Entities;

public class Recipe : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Instructions { get; set; } = string.Empty;

    public int PrepTimeMinutes { get; set; }

    public int CookTimeMinutes { get; set; }

    public int Servings { get; set; }

    public DifficultyLevel Difficulty { get; set; }

    public RecipeStatus Status { get; set; }

    public bool IsDeleted { get; set; }

    public Guid CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public RecipeNutrition? Nutrition { get; set; }

    public Guid? AuthorId { get; set; }

    public ApplicationUser? Author { get; set; }

    public ICollection<RecipeIngredient> Ingredients { get; set; }
        = new List<RecipeIngredient>();

    public ICollection<RecipeStep> Steps { get; set; }
        = new List<RecipeStep>();

    public ICollection<RecipeImage> Images { get; set; }
        = new List<RecipeImage>();
}